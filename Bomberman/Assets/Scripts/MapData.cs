using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "MapData.asset", menuName = "Bomberman/Map/MapData")]
public class MapData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private Scene _scene;
}
