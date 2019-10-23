using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private Grid _grid = null;
    [SerializeField] private Tilemap _tilemap = null;
    [SerializeField] private TilemapCollider2D _collisionMap = null;
    [SerializeField] private List<Transform> _playerSpawns = null;

    [Header("Assets")]

    [SerializeField] private GameSettings _gameSettings = null;
    [SerializeField] private AudioClip _music = null;
    [SerializeField] private Bonus _bonusPrefab = null;
    [SerializeField] private DestructibleWall _destructibleWallPrefab = null;

    #endregion

    #region Properties

    public Grid GameGrid => _grid;
    public Tilemap GameTilemap => _tilemap;
    public TilemapCollider2D CollisionMap => _collisionMap;
    public AudioClip Music => _music;
    public Vector2Int MapSize => _mapSize;

    #endregion

    #region Private fields

    private List<Vector2Int> _playerSpawnCells = new List<Vector2Int>();
    private EEntityType[,] _entitiesMap;
    private short[,] _dangerMap;

    private Vector2Int _mapSize = Vector2Int.zero;
    private Vector2Int _mapOrigin = Vector2Int.zero;

    private List<DestructibleWall> _destructibleWalls = new List<DestructibleWall>();
    private List<Bonus> _bonus = new List<Bonus>();
    private List<Bomb> _bombs = new List<Bomb>();

    #endregion

    private void Awake()
    {
        ComputeMapSize();
        InitializeEntitiesMap();
        InitializeDangerMap();
        InitializePlayersSpawn();
    }

    public Vector2Int CellPosition(Vector3 worldPosition)
    {
        var cellPosition = GameTilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPosition.x, cellPosition.y) - _mapOrigin;
    }

    public Vector3 WorldPosition(Vector2Int cellPosition)
    {
        // Update the given normalized cell position to "world" cell position
        cellPosition += _mapOrigin;
        Vector3 worldPosition = GameTilemap.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));

        return worldPosition + GameTilemap.tileAnchor;
    }

    private void ComputeMapSize()
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int y = GameTilemap.origin.y; y < GameTilemap.size.y; y++)
        {
            for (int x = GameTilemap.origin.x; x < GameTilemap.size.x; x++)
            {
                var cellPosition = new Vector3Int(x, -y, 0);
                var tile = GameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    if (cellPosition.x < min.x)
                        min.x = cellPosition.x;
                    else if (cellPosition.x > max.x)
                        max.x = cellPosition.x;

                    if (cellPosition.y < min.y)
                        min.y = cellPosition.y;
                    else if (cellPosition.y > max.y)
                        max.y = cellPosition.y;
                }
            }
        }

        _mapOrigin = min;
        _mapSize = new Vector2Int((max.x - min.x) + 1, (max.y - min.y) + 1);
    }

    public void Clear()
    {
        ClearEntitiesMap();
        ClearDangerMap();
        ClearBonus();
        ClearDestructibleWalls();

        _bombs.Clear();
    }

    public bool OverlapUnbreakableWall(Vector3 worldPosition)
    {
        return CollisionMap.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    public bool IsOutOfBound(Vector2Int cellPosition)
    {
        return cellPosition.x < 0 || cellPosition.x > _mapSize.x - 1 ||
               cellPosition.y < 0 || cellPosition.y > _mapSize.y - 1;
    }

    public bool IsAccessible(Vector2Int cellPosition)
    {
        return !IsOutOfBound(cellPosition) &&
                GetEntityType(cellPosition) != EEntityType.UnbreakableWall &&
                GetEntityType(cellPosition) != EEntityType.DestructibleWall &&
                GetEntityType(cellPosition) != EEntityType.Explosion &&
                GetEntityType(cellPosition) != EEntityType.Bomb;
    }

    #region Player spawn

    private void InitializePlayersSpawn()
    {
        _playerSpawnCells.Clear();

        foreach (Transform spawnTransform in _playerSpawns)
        {
            _playerSpawnCells.Add(CellPosition(spawnTransform.position));
        }
    }

    public Vector3 GetSpawnPosition(int index)
    {
        return _playerSpawns[index].position;
    }

    public bool OverlapPlayerSpawn(Vector2Int cellPosition)
    {
        foreach (Vector2Int spawnCellPosition in _playerSpawnCells)
        {
            if ((cellPosition.x >= spawnCellPosition.x - 1 && cellPosition.x <= spawnCellPosition.x + 1) &&
                (cellPosition.y >= spawnCellPosition.y - 1 && cellPosition.y <= spawnCellPosition.y + 1))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Entities map

    private void InitializeEntitiesMap()
    {
        _entitiesMap = new EEntityType[_mapSize.x, _mapSize.y];
        FindUnbreakableWalls();
    }

    public EEntityType GetEntityType(Vector2Int cellPosition)
    {
        if (IsOutOfBound(cellPosition))
        {
            return EEntityType.UnbreakableWall;
        }

        return _entitiesMap[cellPosition.x, cellPosition.y];
    }

    public EEntityType GetEntityType(Vector3 worldPosition)
    {
        var cellPosition = CellPosition(worldPosition);

        if (IsOutOfBound(cellPosition))
        {
            return EEntityType.UnbreakableWall;
        }

        return _entitiesMap[cellPosition.x, cellPosition.y];
    }

    public void SetEntityType(EEntityType entityType, Vector2Int cellPosition)
    {
        _entitiesMap[cellPosition.x, cellPosition.y] = entityType;
    }

    public void SetEntityType(EEntityType entityType, Vector3 worldPosition)
    {
        Vector2Int cellPosition = CellPosition(worldPosition);
        _entitiesMap[cellPosition.x, cellPosition.y] = entityType;
    }

    private void ClearEntitiesMap()
    {
        for (int y = 0; y < _mapSize.y; y++)
        {
            for (int x = 0; x < _mapSize.x; x++)
            {
                if (_entitiesMap[x, y] != EEntityType.UnbreakableWall)
                    _entitiesMap[x, y] = EEntityType.None;
            }
        }
    }

    #endregion

    #region Danger map

    private void InitializeDangerMap()
    {
        _dangerMap = new short[_mapSize.x, _mapSize.y];
    }

    public bool IsSafe(Vector2Int cellPosition)
    {
        return _dangerMap[cellPosition.x, cellPosition.y] == 0;
    }

    public bool IsSafe(Vector3 worldPosition)
    {
        return IsSafe(CellPosition(worldPosition));
    }

    public short GetDangerLevel(Vector2Int cellPosition)
    {
        return _dangerMap[cellPosition.x, cellPosition.y];
    }

    private void SetDangerLevel(Vector2Int cellPosition, short dangerLevel)
    {
        _dangerMap[cellPosition.x, cellPosition.y] = dangerLevel;
    }

    private void ClearDangerMap()
    {
        for (int y = 0; y < _mapSize.y; y++)
        {
            for (int x = 0; x < _mapSize.x; x++)
            {
                _dangerMap[x, y] = 0;
            }
        }
    }

    #endregion

    #region Destructible walls

    // Copy the unbreakable walls position to the entities map
    private void FindUnbreakableWalls()
    {
        for (int y = 0; y < MapSize.y; y++)
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                TileBase tile = GameTilemap.GetTile(
                    new Vector3Int(cellPosition.x + _mapOrigin.x, cellPosition.y + _mapOrigin.y, 0)
                );

                if (tile)
                {
                    var worldPosition = WorldPosition(cellPosition);

                    if (OverlapUnbreakableWall(worldPosition))
                    {
                        SetEntityType(EEntityType.UnbreakableWall, worldPosition);
                    }
                }
            }
        }
    }

    public void DestroyAllDestructibleWalls()
    {
        List<DestructibleWall> wallsToDestroy = new List<DestructibleWall>();

        foreach (var destructibleWall in _destructibleWalls)
        {
            if (destructibleWall)
                wallsToDestroy.Add(destructibleWall);
        }

        foreach (var wallToDestroy in wallsToDestroy)
        {
            wallToDestroy.Explode();
        }
    }

    public void GenerateDestrucibleWalls(float wallPercentage)
    {
        for (int y = 0; y < MapSize.y; y++)
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                TileBase tile = GameTilemap.GetTile(
                    new Vector3Int(cellPosition.x + _mapOrigin.y, cellPosition.y + _mapOrigin.y, 0)
                );

                if (tile)
                {
                    var worldPosition = WorldPosition(cellPosition);

                    if (OverlapPlayerSpawn(cellPosition) || OverlapUnbreakableWall(worldPosition))
                    {
                        continue;
                    }

                    if (Random.value < wallPercentage)
                    {
                        AddDestructibleWall(worldPosition);
                    }
                }
            }
        }
    }

    public void AddDestructibleWall(Vector3 worldPosition)
    {
        DestructibleWall destructibleWall = Instantiate(
            _destructibleWallPrefab,
            worldPosition,
            Quaternion.identity,
            transform
        );

        destructibleWall.OnDestroy.AddListener(OnDestructibleWallDestroy);

        _destructibleWalls.Add(destructibleWall);

        SetEntityType(EEntityType.DestructibleWall, destructibleWall.transform.position);
    }

    private void OnDestructibleWallDestroy(DestructibleWall destructibleWall)
    {
        destructibleWall.OnDestroy.RemoveListener(OnDestructibleWallDestroy);
        
        SetEntityType(EEntityType.None, destructibleWall.transform.position);

        _destructibleWalls.Remove(destructibleWall);

        // Spanw a bonus?
        if (Random.value < _gameSettings.BonusProbability)
        {
            AddBonus(destructibleWall.transform.position);
        }
    }

    public void ClearDestructibleWalls()
    {
        foreach (var destructibleWall in _destructibleWalls)
        {
            if (destructibleWall && destructibleWall.gameObject)
            {
                Destroy(destructibleWall.gameObject);
            }
        }

        _destructibleWalls.Clear();
    }

    #endregion

    #region Bonus

    private void AddBonus(Vector3 position)
    {
        Bonus bonus = Instantiate(_bonusPrefab, position, Quaternion.identity, transform);
        bonus.Initalize(_gameSettings.GetAvailableBonus());
        bonus.OnDestroy.AddListener(OnBonusDestroy);

        _bonus.Add(bonus);
        
        SetEntityType(EEntityType.Bonus, position);
    }

    private void OnBonusDestroy(Bonus bonus)
    {
        bonus.OnDestroy.RemoveListener(OnBonusDestroy);

        SetEntityType(EEntityType.None, bonus.transform.position);
    }

    public void DestroyAllBonus()
    {
        List<Bonus> bonusToDestroy = new List<Bonus>();

        foreach (var bonus in _bonus)
        {
            if (bonus)
                bonusToDestroy.Add(bonus);
        }

        foreach (Bonus bonus in bonusToDestroy)
        {
            bonus.Explode();
        }
    }

    public void ClearBonus()
    {
        foreach (var bonus in _bonus)
        {
            if (bonus && bonus.gameObject)
            {
                Destroy(bonus.gameObject);
            }
        }

        _bonus.Clear();
    }

    #endregion

    #region Bombs

    public void OnBombAdded(Bomb bomb)
    {
        SetEntityType(EEntityType.Bomb, bomb.transform.position);
        UpdateDangerMap(bomb.FindImpactedCells(), 1);

        bomb.OnWillExplodeSoon.AddListener(OnBombWillExplodeSoon);
        bomb.OnExplosion.AddListener(OnBombExplosion);

        _bombs.Add(bomb);
    }

    private void OnBombWillExplodeSoon(Bomb bomb)
    {
        bomb.OnWillExplodeSoon.RemoveListener(OnBombWillExplodeSoon);
        UpdateDangerMap(bomb.FindImpactedCells(), 2);
    }

    private void OnBombExplosion(Bomb bomb)
    {
        bomb.OnExplosion.RemoveListener(OnBombExplosion);
        _bombs.Remove(bomb);
    }

    #endregion

    #region Explosions

    public void OnExplosionAdded(Explosion explosion)
    {
        SetEntityType(EEntityType.Explosion, explosion.transform.position);

        explosion.OnExplosionFinished.AddListener(OnExplosionFinished);

        foreach (var cell in explosion.ImpactedCells)
            SetEntityType(EEntityType.Explosion, cell);

        UpdateDangerMap(explosion.ImpactedCells, 3);
    }

    public void OnExplosionFinished(Explosion explosion)
    {
        explosion.OnExplosionFinished.RemoveListener(OnExplosionFinished);
      
        foreach (var cellPosition in explosion.ImpactedCells)
        {
            if (GetEntityType(cellPosition) != EEntityType.Bonus)
                SetEntityType(EEntityType.None, cellPosition);
        }

        UpdateDangerMap(explosion.ImpactedCells, 0, true);
    }

    // Warning: the first cells element should be the center position
    public void UpdateDangerMap(List<Vector2Int> cells, short dangerLevel, bool force = false)
    {
        short currentDangerLevel = GetDangerLevel(cells[0]);

        if (!force && currentDangerLevel > dangerLevel)
            dangerLevel = currentDangerLevel;

        foreach (var cell in cells)
        {
            if (GetDangerLevel(cell) == dangerLevel)
                continue;

            if (cell != cells[0])
            {
                var foundBomb = _bombs.Find(b => Equals(CellPosition(b.transform.position), cell));

                if (foundBomb != null)
                {
                    UpdateDangerMap(foundBomb.FindImpactedCells(), dangerLevel);
                }
            }

            SetDangerLevel(cell, dangerLevel);
        }
    }

    #endregion
}
