using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapsDatabases.asset", menuName = "Bomberman/Map/MapsDatabases")]
public class MapsDatabase : ScriptableObject
{
    [SerializeField] private List<MapData> _maps = new List<MapData>();
}
