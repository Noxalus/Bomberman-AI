using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] private Grid _grid = null;
    [SerializeField] private Tilemap _tilemap = null;
    [SerializeField] private TilemapCollider2D _collisionMap = null;
    [SerializeField] private List<Transform> _playerSpawns = null;

    public Grid GameGrid => _grid;
    public Tilemap GameTilemap => _tilemap;
    public TilemapCollider2D CollisionMap => _collisionMap;

    public Vector3 GetSpawnPosition(int index)
    {
        return _playerSpawns[index].position;
    }
}
