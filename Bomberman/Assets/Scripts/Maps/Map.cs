using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [Header("Inner references")]

    [SerializeField] private Grid _grid = null;
    [SerializeField] private Tilemap _tilemap = null;
    [SerializeField] private TilemapCollider2D _collisionMap = null;
    [SerializeField] private List<Transform> _playerSpawns = null;

    [Header("Assets")]

    [SerializeField] private DestructibleWall _destructibleWallPrefab = null;

    private List<Vector3Int> _playerSpawnCells = new List<Vector3Int>();
    private EEntityType[,] _mapEntities;

    public Grid GameGrid => _grid;
    public Tilemap GameTilemap => _tilemap;
    public TilemapCollider2D CollisionMap => _collisionMap;

    public Vector2Int MapSize => _mapSize;

    private Vector2Int _mapSize = Vector2Int.zero;
    private Vector2Int _mapOrigin = Vector2Int.zero;

    private void Awake()
    {
        _playerSpawnCells.Clear();

        foreach (Transform spawnTransform in _playerSpawns)
        {
            _playerSpawnCells.Add(_grid.WorldToCell(spawnTransform.position));
        }

        ComputeMapSize();
        _mapEntities = new EEntityType[_mapSize.x, _mapSize.y];
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

    public bool OverlapWall(Vector3 worldPosition)
    {
        return CollisionMap.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));
    }

    // Get cell position in world space from normalized cell position (which goes from 0 to MapSize)
    public Vector3Int GetCellPositionFromNormalizedPosition(Vector2Int normalizedCellPosition)
    {
        var position = normalizedCellPosition + _mapOrigin;
        return new Vector3Int(position.x, position.y, 0);
    }

    public void AddDestructibleWall(Vector3 worldPosition)
    {
        DestructibleWall destructibleWall = Instantiate(
            _destructibleWallPrefab,
            worldPosition,
            Quaternion.identity,
            transform
        );

        destructibleWall.OnExplode.AddListener(OnDestructibleWallExplode);
    }

    private void OnDestructibleWallExplode(DestructibleWall destructibleWall)
    {
        destructibleWall.OnExplode.RemoveListener(OnDestructibleWallExplode);
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
}
