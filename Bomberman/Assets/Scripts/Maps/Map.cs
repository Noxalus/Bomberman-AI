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

    private List<Vector3Int> _playerSpawnCells = new List<Vector3Int>();
    private EEntityType[,] _entitiesMap;

    private Vector2Int _mapSize = Vector2Int.zero;
    private Vector2Int _mapOrigin = Vector2Int.zero;

    private List<DestructibleWall> _destructibleWalls = new List<DestructibleWall>();
    private List<Bonus> _bonus = new List<Bonus>();

    #endregion

    private void Awake()
    {
        _playerSpawnCells.Clear();

        foreach (Transform spawnTransform in _playerSpawns)
        {
            _playerSpawnCells.Add(_grid.WorldToCell(spawnTransform.position));
        }

        ComputeMapSize();
        InitializeEntitiesMap();
    }

    public Vector3 GetSpawnPosition(int index)
    {
        return _playerSpawns[index].position;
    }

    public bool OverlapPlayerSpawn(Vector3Int cellPosition)
    {
        foreach (Vector3Int spawnCellPosition in _playerSpawnCells)
        {
            if ((cellPosition.x >= spawnCellPosition.x - 1 && cellPosition.x <= spawnCellPosition.x + 1) &&
                (cellPosition.y >= spawnCellPosition.y - 1 && cellPosition.y <= spawnCellPosition.y + 1))
            {
                return true;
            }
        }

        return false;
    }

    public void Clear()
    {
        ClearEntitiesMap();
        ClearBonus();
        ClearDestructibleWalls();
    }

    private void ClearEntitiesMap()
    {
        for (int y = 0; y <= _mapSize.y; y++)
        {
            for (int x = 0; x <= _mapSize.x; x++)
            {
                if (_entitiesMap[x, y] != EEntityType.UnbreakableWall)
                    _entitiesMap[x, y] = EEntityType.None;
            }
        }
    }

    public EEntityType GetEntityType(Vector3Int normalizedCellPosition)
    {
        return _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y];
    }

    public EEntityType GetEntityType(Vector2Int normalizedCellPosition)
    {
        return _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y];
    }

    public EEntityType GetEntityType(Vector3 worldPosition)
    {
        var normalizedCellPosition = GetNormalizedCellPositionFromWorldPosition(worldPosition);

        if (IsNormalizedCellPositionOutOfBound(normalizedCellPosition))
        {
            return EEntityType.UnbreakableWall;
        }

        return _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y];
    }

    public bool OverlapUnbreakableWall(Vector3 worldPosition)
    {
        return CollisionMap.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    public  bool IsNormalizedCellPositionOutOfBound(Vector2Int normalizedCellPosition)
    {
        return (normalizedCellPosition.x < 0 || normalizedCellPosition.x > _mapSize.x ||
                normalizedCellPosition.y < 0 || normalizedCellPosition.y > _mapSize.y);
    }

    public Vector2Int GetNormalizedCellPositionFromWorldPosition(Vector3 worldPosition)
    {
        var cellPosition = GameTilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPosition.x, cellPosition.y) - _mapOrigin;
    }

    public void SetEntityType(EEntityType entityType, Vector3 worldPosition)
    {
        var normalizedCellPosition = GetNormalizedCellPositionFromWorldPosition(worldPosition);
        _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y] = entityType;
    }

    // Get cell position in world space from normalized cell position (which goes from 0 to MapSize)
    public Vector3Int GetCellPositionFromNormalizedPosition(Vector2Int normalizedCellPosition)
    {
        var position = normalizedCellPosition + _mapOrigin;
        return new Vector3Int(position.x, position.y, 0);
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
        for (int y = 0; y <= MapSize.y; y++)
        {
            for (int x = 0; x <= MapSize.x; x++)
            {
                var normalizedCellPosition = new Vector2Int(x, y);
                var cellPosition = GetCellPositionFromNormalizedPosition(normalizedCellPosition);
                var tile = GameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    var worldPosition = GameGrid.CellToWorld(cellPosition) + GameTilemap.tileAnchor;

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

    public void AddDestructibleWall(Vector3 worldPosition)
    {
        DestructibleWall destructibleWall = Instantiate(
            _destructibleWallPrefab,
            worldPosition,
            Quaternion.identity,
            transform
        );

        _destructibleWalls.Add(destructibleWall);

        var normalizedCellPosition = GetNormalizedCellPositionFromWorldPosition(worldPosition);

        if (!IsNormalizedCellPositionOutOfBound(normalizedCellPosition))
        {
            _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y] = EEntityType.DestructibleWall;
        }

        destructibleWall.OnDestroy.AddListener(OnDestructibleWallDestroy);
    }

    void AddBonus(Vector3 position)
    {
        Bonus bonus = Instantiate(_bonusPrefab, position, Quaternion.identity, transform);
        bonus.Initalize(_gameSettings.GetAvailableBonus());

        SetEntityType(EEntityType.Bonus, position);

        _bonus.Add(bonus);
    }

    private void InitializeEntitiesMap()
    {
        _entitiesMap = new EEntityType[_mapSize.x + 1, _mapSize.y + 1];
        FindUnbreakableWalls();
    }

    private void OnDestructibleWallDestroy(DestructibleWall destructibleWall)
    {
        destructibleWall.OnDestroy.RemoveListener(OnDestructibleWallDestroy);

        var normalizedCellPosition = GetNormalizedCellPositionFromWorldPosition(destructibleWall.transform.position);

        if (!IsNormalizedCellPositionOutOfBound(normalizedCellPosition))
        {
            _entitiesMap[normalizedCellPosition.x, normalizedCellPosition.y] = EEntityType.None;
        }

        _destructibleWalls.Remove(destructibleWall);

        // Spanw a bonus?
        if (Random.value < _gameSettings.BonusProbability)
        {
            AddBonus(destructibleWall.transform.position);
        }
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
        _mapSize = new Vector2Int(max.x - min.x, max.y - min.y);
    }

    // Copy the unbreakable walls position to the entities map
    private void FindUnbreakableWalls()
    {
        for (int y = 0; y <= MapSize.y; y++)
        {
            for (int x = 0; x <= MapSize.x; x++)
            {
                var normalizedCellPosition = new Vector2Int(x, y);
                var cellPosition = GetCellPositionFromNormalizedPosition(normalizedCellPosition);
                var tile = GameTilemap.GetTile(cellPosition);

                if (tile)
                {
                    var worldPosition = GameGrid.CellToWorld(cellPosition) + GameTilemap.tileAnchor;

                    if (OverlapUnbreakableWall(worldPosition))
                    {
                        SetEntityType(EEntityType.UnbreakableWall, worldPosition);
                    }
                }
            }
        }
    }
}
