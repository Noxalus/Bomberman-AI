using UnityEngine;

public class EntitiesMinimap : Minimap
{
    [SerializeField]
    private EEntityTypeSpriteDictionary _sprites = new EEntityTypeSpriteDictionary();

    public void UpdateMinimap()
    {
        for (int y = 0; y < _map.MapSize.y; y++)
        {
            for (int x = 0; x < _map.MapSize.x; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                EEntityType entity = _map.GetEntityType(cellPosition);

                GetCell(x, (_map.MapSize.y - 1) - y).sprite = _sprites[entity];
            }
        }
    }
}
