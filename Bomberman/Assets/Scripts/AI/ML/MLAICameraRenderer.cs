using System.Collections;
using UnityEngine;

public class MLAICameraRenderer : MonoBehaviour
{
    [SerializeField]
    private Map _map = null;

    [SerializeField]
    private Camera _camera = null;

    [SerializeField]
    private int _renderTexturePixelSize = 4;

    [SerializeField, Tooltip("Seconds interval between render")]
    private float _renderFrequency = 1f;

    [SerializeField]
    public Renderer _renderer = null; // renderer in which you will apply changed texture

    private Texture2D _texture;
    private RenderTexture _renderTexture = null;

    public RenderTexture RenderTexture => _renderTexture;

    void Start()
    {
        _renderTexture = new RenderTexture(_map.MapSize.x * _renderTexturePixelSize, _map.MapSize.y * _renderTexturePixelSize, 0);

        _texture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
        _texture.filterMode = FilterMode.Point;

        _renderer.material.mainTexture = _texture;
        //make texture2D because you want to "edit" it. 
        //however this is not a way to apply any post rendering effects because
        //this way, you are reading it through CPU(slow).

        _camera.targetTexture = _renderTexture;

        //StartCoroutine(RenderCoroutine());
    }

    private IEnumerator RenderCoroutine()
    {
        while(true)
        {
            Render();
            yield return new WaitForSeconds(_renderFrequency);
        }
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
                EEntityType entityType = _map.GetEntityType(cellPosition);
                Color color = GetEntityColor(entityType);

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

    private Color GetEntityColor(EEntityType entityType)
    {
        Color color = Color.black;

        if (entityType == EEntityType.None)
        {
            color = Color.white;
        }
        else if (entityType == EEntityType.DestructibleWall)
        {
            color = Color.black;
        }
        else if (entityType == EEntityType.Player)
        {
            color = Color.green;
        }
        else if (entityType == EEntityType.Bomb)
        {
            color = Color.red;
        }
        else if (entityType == EEntityType.Bonus)
        {
            color = Color.yellow;
        }
        else if (entityType == EEntityType.Explosion)
        {
            color = Color.magenta;
        }

        return color;
    }
}