using System.Text;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _logicalMapText = null;
    [SerializeField] private CanvasGroup _canvasGroup = null;
    [SerializeField] Minimap _costMap = null;
    [SerializeField] EntitiesMinimap _entitiesMap = null;

    private Map _map = null;
    private bool _isInitialized = false;

    private void Awake()
    {
        Show(false);
    }

    public void Initialize(Map map)
    {
        _map = map;
        _costMap.Initialize(map);
        _entitiesMap.Initialize(map);

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