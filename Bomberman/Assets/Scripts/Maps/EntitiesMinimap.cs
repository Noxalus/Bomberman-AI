using UnityEngine;
using UnityEngine.UI;

public class EntitiesMinimap : Minimap
{
    [SerializeField]
    private EEntityTypeSpriteDictionary _sprites = new EEntityTypeSpriteDictionary();
    
    private GameManager _gameManager;

    public void Initialize(GameManager gameManager)
    {
        base.Initialize(gameManager.Map);

        _gameManager = gameManager;
    }

    public void UpdateMinimap()
    {
        for (int y = 0; y < _map.MapSize.y; y++)
        {
            for (int x = 0; x < _map.MapSize.x; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                EEntityType entity = _map.GetEntityType(cellPosition);

                Image cell = GetCell(x, (_map.MapSize.y - 1) - y);
                cell.sprite = _sprites[entity];
                cell.color = Color.white;

                if (entity == EEntityType.Player)
                {
                    Player player = _gameManager.GetPlayerAt(cellPosition);

                    if (player != null)
                        cell.color = player.Color;
                }
            }
        }
    }
}
