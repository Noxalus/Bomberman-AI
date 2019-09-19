using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private WallGenerator _wallGenerator = null;

    private Map _map = null;

    private void Start()
    {
        StartCoroutine(LoadMapScene());
    }

    IEnumerator LoadMapScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Map1", LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnSceneLoaded();
    }

    private void OnSceneLoaded()
    {
        _map = FindObjectOfType<Map>();

        if (_map != null)
        {
            _wallGenerator.Initialize(_map);
            _wallGenerator.GenerateWalls(1f);
        }
    }
}
