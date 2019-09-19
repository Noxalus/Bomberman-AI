using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private WallGenerator _wallGenerator = null;

    void Start()
    {
        _wallGenerator.GenerateWalls(1f);
    }

    void Update()
    {
        
    }
}
