using UnityEngine;

public class MLAICameraRenderer : MonoBehaviour
{
    [SerializeField]
    private Map _map = null;

    [SerializeField]
    private RenderTexture _renderTexture = null;

    [SerializeField]
    public Renderer _renderer = null; // renderer in which you will apply changed texture

    private Texture2D _texture;

    void Start()
    {
        _texture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
        _texture.filterMode = FilterMode.Point;

        _renderer.material.mainTexture = _texture;
        //make texture2D because you want to "edit" it. 
        //however this is not a way to apply any post rendering effects because
        //this way, you are reading it through CPU(slow).
    }

    void Update()
    {
        Render();
    }

    public void Render()
    {
        RenderTexture.active = _renderTexture;

        //don't forget that you need to specify rendertexture before you call readpixels
        //otherwise it will read screen pixels.
        _texture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);

        int factorX = _renderTexture.width / _map.MapSize.x;
        int factorY = _renderTexture.height / _map.MapSize.y;

        for (int y = 0; y < _renderTexture.height; y += factorX)
        { 
            for (int x = 0; x < _renderTexture.width; x += factorY)
            {
                Vector2Int cellPosition = new Vector2Int(x / factorX, y / factorY);
                EEntityType entity = _map.GetEntityType(cellPosition);
                Color color = Color.black;

                if (entity == EEntityType.None)
                {
                    color = Color.white;
                }
                else if (entity == EEntityType.DestructibleWall)
                {
                    color = Color.black;
                }
                else if (entity == EEntityType.Player)
                {
                    color = Color.green;
                }
                else if (entity == EEntityType.Bomb)
                {
                    color = Color.red;
                }
                else if (entity == EEntityType.Bonus)
                {
                    color = Color.yellow;
                }
                else if (entity == EEntityType.Explosion)
                {
                    color = Color.magenta;
                }

                for (int j = 0; j < factorY; j++)
                {
                    for (int i = 0; i < factorX; i++)
                    {
                        _texture.SetPixel(x + i, y + j, color);
                    }
                }
            }
        }

        _texture.Apply();

        RenderTexture.active = null; //don't forget to set it back to null once you finished playing with it. 
    }
}