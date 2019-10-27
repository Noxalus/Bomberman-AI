using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup = null;
    [SerializeField] Minimap _costMap = null;
    [SerializeField] Minimap _dangerMap = null;
    [SerializeField] Minimap _goalMap = null;
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

        if (_costMap.isActiveAndEnabled)
            _costMap.Initialize(gameManager.Map);

        _dangerMap.Initialize(gameManager.Map);
        _goalMap.Initialize(gameManager.Map);
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
        if (!_isInitialized)
            return;

        if (_costMap.isActiveAndEnabled)
            _costMap.Clear();
        
        _dangerMap.Clear();
        _goalMap.Clear();
        _entitiesMap.Clear();

        foreach (var aiPlayer in _gameManager.AIManager.AIPlayers)
        {
            aiPlayer.Behaviour.OnPathChanged.RemoveListener(OnPlayerPathChanged);
            aiPlayer.Behaviour.OnTargetReached.RemoveListener(OnPlayerTargetReached);
        }

        foreach (var keyValuePair in _pathSprites)
        {
            foreach (var sprite in keyValuePair.Value)
            {
                Destroy(sprite.gameObject);
            }
        }

        _pathSprites.Clear();

        _isInitialized = false;
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
            if (_costMap.isActiveAndEnabled)
                DrawCostMap();

            if (_dangerMap.isActiveAndEnabled)
                DrawDangerMap();

            if (_goalMap.isActiveAndEnabled)
                DrawGoalMap();

            if (_entitiesMap.isActiveAndEnabled)
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
        if (_currentAIPlayerIndex >= _gameManager.AIManager.AIPlayers.Count)
            return;

        var currentAIPlayer = _gameManager.AIManager.AIPlayers[_currentAIPlayerIndex];
        var playerCellPosition = _gameManager.AIManager.CellPosition(currentAIPlayer.transform.position);
        var costMatrix = _gameManager.AIManager.ComputeCostMap(playerCellPosition);

        int infiniteValue = (_gameManager.AIManager.AreaSize.x * _gameManager.AIManager.AreaSize.y);
        int maxCostValue = GetMatrixMaxValue(costMatrix, infiniteValue);

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

    private void DrawDangerMap()
    {
        float maxCostValue = 3;

        for (int y = 0; y < _gameManager.Map.MapSize.y; y++)
        {
            for (int x = 0; x < _gameManager.Map.MapSize.x; x++)
            {
                Image currentCell = _dangerMap.GetCell(x, (_gameManager.Map.MapSize.y - 1) - y);
                float factor = (_gameManager.Map.GetDangerLevel(new Vector2Int(x, y)) * (255f / maxCostValue) / 255f);

                currentCell.color = (factor > 0) ? new Color(factor, 0f, 0f, 1f) : Color.white;
            }
        }
    }

    private void DrawGoalMap()
    {
        if (_currentAIPlayerIndex >= _gameManager.AIManager.AIPlayers.Count)
            return;

        var currentAIPlayer = _gameManager.AIManager.AIPlayers[_currentAIPlayerIndex];
        var playerCellPosition = _gameManager.AIManager.CellPosition(currentAIPlayer.transform.position);
        var goalMatrix = _gameManager.AIManager.ComputeGoalMap(playerCellPosition);

        int infiniteValue = (_gameManager.AIManager.AreaSize.x * _gameManager.AIManager.AreaSize.y);
        int maxCostValue = GetMatrixMaxValue(goalMatrix, infiniteValue);

        for (int y = 0; y < _gameManager.AIManager.AreaSize.y; y++)
        {
            for (int x = 0; x < _gameManager.AIManager.AreaSize.x; x++)
            {
                Image currentCell = _goalMap.GetCell(x, (_gameManager.AIManager.AreaSize.y - 1) - y);
                float factor = (goalMatrix[x, y] * (255f / maxCostValue) / 255f);

                currentCell.color = new Color(factor, factor, factor, 1f);
            }
        }
    }

    public int GetMatrixMaxValue(int[,] matrix, int infiniteValue = -1)
    {
        int max = 0;
        for (int x = 0; x < _gameManager.AIManager.AreaSize.x; x++)
        {
            for (int y = 0; y < _gameManager.AIManager.AreaSize.y; y++)
            {
                if (matrix[x, y] != infiniteValue && matrix[x, y] > max)
                    max = matrix[x, y];
            }
        }

        return max;
    }

    public void Show(bool show = true)
    {
        _canvasGroup.alpha = show ? 1 : 0;
    }
}