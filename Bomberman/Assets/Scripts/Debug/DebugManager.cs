using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup = null;
    [SerializeField] Minimap _costMap = null;
    [SerializeField] EntitiesMinimap _entitiesMap = null;
    [SerializeField] private Sprite _pathSprite = null;

    private GameManager _gameManager = null;
    private bool _isInitialized = false;
    private int _currentAIPlayerIndex = 0;
    private Dictionary<int, List<GameObject>> _pathSprites = new Dictionary<int, List<GameObject>>();

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

        foreach (var aiPlayer in _gameManager.AIManager.AIPlayers)
        {
            aiPlayer.Behaviour.OnPathChanged.AddListener(OnPlayerPathChanged);
            aiPlayer.Behaviour.OnTargetReached.AddListener(OnPlayerTargetReached);
        }
    }

    public void Clear()
    {
        _costMap.Clear();
        _entitiesMap.Clear();

        foreach (var aiPlayer in _gameManager.AIManager.AIPlayers)
            aiPlayer.Behaviour.OnPathChanged.RemoveListener(OnPlayerPathChanged);

        foreach (var keyValuePair in _pathSprites)
        {
            foreach (var sprite in keyValuePair.Value)
            {
                Destroy(sprite.gameObject);
            }
        }

        _pathSprites.Clear();
    }

    private void ClearPathSprites(int playerId)
    {
        if (!_pathSprites.ContainsKey(playerId))
            return;

        foreach (var sprite in _pathSprites[playerId])
            Destroy(sprite);

        _pathSprites[playerId].Clear();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            Show(_canvasGroup.alpha == 0);

        if (_isInitialized)
        {
            DrawCostMap();
            _entitiesMap.UpdateMinimap();
        }
    }

    private void OnPlayerPathChanged(Player player)
    {
        ClearPathSprites(player.Id);

        var aiPlayer = player as AIPlayer;

        foreach (var step in aiPlayer.Behaviour.CurrentPath)
        {
            var pathStep = new GameObject("AIPathStep");
            pathStep.transform.position = _gameManager.AIManager.WorldPosition(step);
            pathStep.transform.localScale *= 5f;
            var nextPositionDebugSpriteRenderer = pathStep.AddComponent<SpriteRenderer>();
            nextPositionDebugSpriteRenderer.sprite = _pathSprite;
            nextPositionDebugSpriteRenderer.color = aiPlayer.Color;
            nextPositionDebugSpriteRenderer.sortingLayerName = "Player";

            if (!_pathSprites.ContainsKey(player.Id))
                _pathSprites.Add(player.Id, new List<GameObject>());

            _pathSprites[player.Id].Add(pathStep);
        }
    }

    private void OnPlayerTargetReached(Player player)
    {
        ClearPathSprites(player.Id);
    }

    private void DrawCostMap()
    {
        var currentAIPlayer = _gameManager.AIManager.AIPlayers[_currentAIPlayerIndex];
        var playerCellPosition = _gameManager.AIManager.CellPosition(currentAIPlayer.transform.position);
        var costMatrix = _gameManager.AIManager.ComputeCostMap(playerCellPosition);

        float infiniteValue = (_gameManager.AIManager.AreaSize.x * _gameManager.AIManager.AreaSize.y);
        float maxCostValue = 0;

        for (int x = 0; x < _gameManager.AIManager.AreaSize.x; x++)
        {
            for (int y = 0; y < _gameManager.AIManager.AreaSize.y; y++)
            {
                if (costMatrix[x, y] != infiniteValue && costMatrix[x, y] > maxCostValue)
                    maxCostValue = costMatrix[x, y];
            }
        }

        for (int y = 0; y < _gameManager.AIManager.AreaSize.y; y++)
        {
            for (int x = 0; x < _gameManager.AIManager.AreaSize.x; x++)
            {
                Image currentCell = _costMap.GetCell(x, (_gameManager.AIManager.AreaSize.y - 1) - y);
                float factor = 1f - (costMatrix[x, y] * (255f / maxCostValue) / 255f);

                currentCell.color = new Color(factor, factor, factor, 1f);
            }
        }
    }

    public void Show(bool show = true)
    {
        _canvasGroup.alpha = show ? 1 : 0;
    }
}