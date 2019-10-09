using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataView : MonoBehaviour
{
    #region Serialized fields

    [Header("Inner references")]

    [SerializeField] private TextMeshProUGUI _scoreText = null;
    [SerializeField] private Transform _powerSpriteHolder = null;
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

        UpdatePower(player.Power);
        UpdateBomb(player.BombCount);
    }

    public void UpdatePower(int value)
    {
        // Increase
        if (value > _powerSpriteInstances.Count)
        {
            while (_powerSpriteInstances.Count < value)
            {
                GameObject icon = Instantiate(new GameObject("PowerSprite"), _powerSpriteHolder);
                Image iconImage = icon.AddComponent<Image>();
                iconImage.sprite = _powerSprite;

                _powerSpriteInstances.Add(icon);
            }
        }
        // Decrease
        else
        {
            while (_powerSpriteInstances.Count > value)
            {
                var index = _powerSpriteInstances.Count - 1;
                Destroy(_powerSpriteInstances[index].gameObject);
                _powerSpriteInstances.RemoveAt(index);
            }
        }
    }

    public void UpdateBomb(int value)
    {
        // Increase
        if (value > _bombSpriteInstances.Count)
        {
            while (_bombSpriteInstances.Count < value)
            {
                GameObject icon = Instantiate(new GameObject("BombSprite"), _bombSpriteHolder);
                Image iconImage = icon.AddComponent<Image>();
                iconImage.sprite = _bombSprite;

                _bombSpriteInstances.Add(icon);
            }
        }
        // Decrease
        else
        {
            while (_bombSpriteInstances.Count > value)
            {
                var index = _bombSpriteInstances.Count - 1;
                Destroy(_bombSpriteInstances[index].gameObject);
                _bombSpriteInstances.RemoveAt(index);
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
