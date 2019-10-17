using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup = null;
    [SerializeField] Minimap _costMap = null;
    [SerializeField] EntitiesMinimap _entitiesMap = null;

    private GameManager _gameManager = null;
    private bool _isInitialized = false;

    private void Awake()
    {
        Show(false);
    }

    public void Initialize(GameManager gameManager)
    {
        _gameManager = gameManager;
        _costMap.Initialize(gameManager.Map);
        _entitiesMap.Initialize(gameManager);

        _isInitialized = true;
    }

    public void Clear()
    {
        _costMap.Clear();
        _entitiesMap.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            Show(_canvasGroup.alpha == 0);

        if (_isInitialized)
        {
            _entitiesMap.UpdateMinimap();
        }
    }

    public void Show(bool show = true)
    {
        _canvasGroup.alpha = show ? 1 : 0;
    }
}