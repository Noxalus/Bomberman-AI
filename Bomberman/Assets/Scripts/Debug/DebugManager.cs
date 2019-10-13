using System.Text;
using TMPro;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _logicalMapText = null;
    [SerializeField] private CanvasGroup _canvasGroup = null;

    private Map _map = null;

    public void Initialize(Map map)
    {
        _map = map;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.F1))
            _canvasGroup.alpha = 1;
        else
            _canvasGroup.alpha = 0;

        _canvasGroup.alpha = 1;

        if (_map != null)
        {
            UpdateLogicalMap();
        }
    }

    public void UpdateLogicalMap()
    {
        StringBuilder logicalMapString = new StringBuilder();
        for (int y = 0; y <= _map.MapSize.y; y++)
        {
            for (int x = 0; x <= _map.MapSize.x; x++)
            {
                Vector3Int normalizedCellPosition = new Vector3Int(x, _map.MapSize.y - y, 0); 
                logicalMapString.Append(EntityTypeToString(_map.GetEntityType(normalizedCellPosition)));
            }

            logicalMapString.Append("\n");
        }

        _logicalMapText.text = logicalMapString.ToString();
    }

    private string EntityTypeToString(EEntityType entityType)
    {
        switch (entityType)
        {
            case EEntityType.None:
                return ".";
            case EEntityType.UnbreakableWall:
                return "X";
            case EEntityType.DestructibleWall:
                return "W";
            case EEntityType.Bomb:
                return "B";
            case EEntityType.Explosion:
                return "E";
            case EEntityType.Bonus:
                return "*";
            default:
                return "@";
        }
    }
}