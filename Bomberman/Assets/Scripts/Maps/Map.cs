using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] private Grid _grid = null;
    [SerializeField] private Tilemap _tilemap = null;
    [SerializeField] private TilemapCollider2D _collisionMap = null;
    [SerializeField] private List<Transform> _playerSpawns = null;

    private List<Vector3Int> _playerSpawnCells = new List<Vector3Int>();

    public Grid GameGrid => _grid;
    public Tilemap GameTilemap => _tilemap;
    public TilemapCollider2D CollisionMap => _collisionMap;

    private void Awake()
    {
        _playerSpawnCells.Clear();

        foreach (Transform spawnTransform in _playerSpawns)
        {
            _playerSpawnCells.Add(_grid.WorldToCell(spawnTransform.position));
        }
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
}
