using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataView : MonoBehaviour
{
    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private Image _portraitImage = null;
    [SerializeField] private TextMeshProUGUI _scoreText = null;
    [SerializeField] private RectTransform _powerSpriteHolder = null;
    [SerializeField] private RectTransform _bombSpriteHolder = null;
    [SerializeField] private RectTransform _speedSpriteHolder = null;

    [Header("Assets references")]

    [SerializeField] private Sprite _powerSprite = null;
    [SerializeField] private Sprite _bombSprite = null;
    [SerializeField] private Sprite _speedSprite = null;

    #endregion

    #region Private fields

    private Player _player = null;
    private List<GameObject> _powerSpriteInstances = new List<GameObject>();
    private List<GameObject> _bombSpriteInstances = new List<GameObject>();
    private List<GameObject> _speedSpriteInstances = new List<GameObject>();

    #endregion

    public void Initialize(Player player)
    {
        Clear();

        UpdatePowerCount(player.Power);
        UpdateBombCount(player.BombCount);
        UpdateSpeedCount(player.SpeedBonus);

        _portraitImage.color = player.Color;
    }

    public void UpdatePowerCount(int value)
    {
        UpdateItemCount(value, _powerSpriteInstances, _powerSprite, _powerSpriteHolder, "PowerSprite");
    }

    public void UpdateBombCount(int value)
    {
        UpdateItemCount(value, _bombSpriteInstances, _bombSprite, _bombSpriteHolder, "BombSprite");
    }

    public void UpdateSpeedCount(int value)
    {
        UpdateItemCount(value, _speedSpriteInstances, _speedSprite, _speedSpriteHolder, "SpeedSprite");
    }

    private void UpdateItemCount(
        int value, 
        List<GameObject> itemInstances, 
        Sprite itemSprite, 
        RectTransform holder, 
        string newInstanceName = "Sprite")
    {
        // Increase
        if (value > itemInstances.Count)
        {
            while (itemInstances.Count < value)
            {
                GameObject icon = Instantiate(new GameObject(newInstanceName), holder);
                Image iconImage = icon.AddComponent<Image>();
                iconImage.sprite = itemSprite;

                itemInstances.Add(icon);
            }
        }
        // Decrease
        else
        {
            while (itemInstances.Count > value)
            {
                var index = itemInstances.Count - 1;
                Destroy(itemInstances[index].gameObject);
                itemInstances.RemoveAt(index);
            }
        }
    }

    private void Clear()
    {
        int powerSpriteChildren = _powerSpriteHolder.transform.childCount;
        for (int i = 0; i < powerSpriteChildren; i++)
        {
            Destroy(_powerSpriteHolder.transform.GetChild(i).gameObject);
        }

        int bombSpriteChildren = _bombSpriteHolder.transform.childCount;
        for (int i = 0; i < bombSpriteChildren; i++)
        {
            Destroy(_bombSpriteHolder.transform.GetChild(i).gameObject);
        }

        int speedSpriteChildren = _speedSpriteHolder.transform.childCount;
        for (int i = 0; i < speedSpriteChildren; i++)
        {
            Destroy(_speedSpriteHolder.transform.GetChild(i).gameObject);
        }

        _powerSpriteInstances.Clear();
        _bombSpriteInstances.Clear();
        _speedSpriteInstances.Clear();
    }
}
