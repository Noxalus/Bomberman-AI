using System;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class MLAIPlayer : Agent
{
    [SerializeField]
    private Player _player = null;

    [SerializeField]
    private PlayerMovement _playerMovement = null;

    [SerializeField]
    private TMP_Text _rewardText = null;

    public Player Player => _player;

    public enum AgentAction
    {
        Nothing,
        Up,
        Right,
        Down,
        Left,
        Bomb
    }

    private EnvironmentParameters _environmentParameters;
    private Vector3 _previousPosition;
    private AgentAction _previousAction;
    private Vector3 _targetPosition;
    private bool _isMoving = false;
    private List<Vector2Int> _visitedCells = new List<Vector2Int>();
    private Vector2Int _currentCell;

    private List<AgentAction> _actionsHistory = new List<AgentAction>();

    private GameManager _gameManager;
    private Map _map;

    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void SetMap(Map map)
    {
        _map = map;
    }

    public override void Initialize()
    {
        _environmentParameters = Academy.Instance.EnvironmentParameters;

        _player.OnDeath.AddListener(
            (player) =>
            {
                //Debug.Log("DEATH: -0.5");
                AddReward(-0.5f);
                Debug.Log($"Cumulative reward: {GetCumulativeReward()}");
                EndEpisode();
            }
        );

        _player.OnWallDestroy.AddListener(
            (player) =>
            {
                AddReward(0.1f);
            }
        );

        _player.OnPlantBomb.AddListener((player) => AddReward(0.01f));

        _player.OnBonusDestroy.AddListener(
            (player, bonusType) =>
            {
                if (bonusType == EBonusType.Bad)
                {
                    AddReward(0.1f);
                }
                else
                {
                    AddReward(-0.1f);
                }
            }
        );

        _player.OnPickUpBonus.AddListener(
            (player, bonusType) =>
            {
                if (bonusType == EBonusType.Bad)
                {
                    AddReward(-0.05f);
                }
                else
                {
                    AddReward(0.05f);
                }
            }
        );
    }

    public override void OnEpisodeBegin()
    {
        _currentCell = Vector2Int.zero;
        _previousPosition = Vector2.zero;
        _previousAction = AgentAction.Nothing;
        _targetPosition = Vector3.zero;
        _isMoving = false;
        _visitedCells.Clear();
        _actionsHistory.Clear();

        _gameManager.StartRound();
    }

    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        List<AgentAction> disableActions = new List<AgentAction>()
        {
            AgentAction.Up,
            AgentAction.Right,
            AgentAction.Down,
            AgentAction.Left,
            AgentAction.Bomb
        };

        if (!_player.IsDead)
        {
            if (_isMoving)
            {
                // We only allow the same action (so direction) than the previous one
                disableActions.Remove(_previousAction);
                disableActions.Add(AgentAction.Nothing);
            }
            else
            {
                // Check accessible cells around
                var cellPosition = _map.CellPosition(transform.position);
                var neighbours = AIUtils.GetNeighbours(cellPosition, _map, true, true, false);
                
                foreach (var neighbourDirection in neighbours.Keys)
                {
                    switch (neighbourDirection)
                    {
                        case EDirection.None:
                            break;
                        case EDirection.Up:
                            disableActions.Remove(AgentAction.Up);
                            break;
                        case EDirection.Right:
                            disableActions.Remove(AgentAction.Right);
                            break;
                        case EDirection.Down:
                            disableActions.Remove(AgentAction.Down);
                            break;
                        case EDirection.Left:
                            disableActions.Remove(AgentAction.Left);
                            break;
                    }
                }

                // Only allow the agent to plant a bomb if he can
                if (_player.BombCount > 0)
                {
                    disableActions.Remove(AgentAction.Bomb);
                }
            }
        }

        int[] actionsMask = new int[disableActions.Count];

        for (int i = 0; i < disableActions.Count; i++)
        {
            actionsMask[i] = (int)disableActions[i];
        }

        actionMasker.SetMask(0, actionsMask);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = (int)AgentAction.Nothing;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = (int)AgentAction.Right;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = (int)AgentAction.Up;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = (int)AgentAction.Left;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = (int)AgentAction.Down;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            actionsOut[0] = (int)AgentAction.Bomb;
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var cellPosition = _map.CellPosition(_player.transform.position);
        bool hasChangedCell = !_currentCell.Equals(cellPosition);

        // Reward for new visited cells
        //if (hasChangedCell && !_visitedCells.Contains(cellPosition))
        //{
        //    AddReward(0.01f);
        //    _visitedCells.Add(cellPosition);
        //}

        // Check danger
        var dangerLevel = _map.GetDangerLevel(cellPosition);
        if (dangerLevel > 0)
        {
            var rewardValue = -((dangerLevel / 3f) * 0.005f);
            //Debug.Log($"DANGER: {rewardValue}");

            AddReward(rewardValue);
        }
        else if (dangerLevel == 0)
        {
            //AddReward(0.001f);
        }

        var movement = Vector2Int.zero;
        var action = (AgentAction)Mathf.FloorToInt(vectorAction[0]);

        // Malus if no action
        //if (action == AgentAction.Nothing && _player.BombCount == _player.MaxBombCount)
        //{
        //    AddReward(-0.01f);
        //}

        if (_previousAction != action)
        {
            _actionsHistory.Add(action);
        }

        _previousAction = action;
        _previousPosition = transform.position;
        _currentCell = cellPosition;

        switch (action)
        {
            case AgentAction.Nothing:
                // Do nothing
                break;
            case AgentAction.Right:
                movement.x = 1;
                break;
            case AgentAction.Left:
                movement.x = -1;
                break;
            case AgentAction.Up:
                movement.y = 1;
                break;
            case AgentAction.Down:
                movement.y = -1;
                break;
            case AgentAction.Bomb:
                _playerMovement.PlantBomb();
                break;
            default:
                throw new ArgumentException("Invalid action value");
        }

        if (!_isMoving && movement != Vector2Int.zero)
        {
            _targetPosition = _map.WorldPosition(cellPosition + movement);
            _isMoving = true;
        }

        _playerMovement.Move(movement);

        if (HasReachedTarget())
        {
            _isMoving = false;
        }
    }

    private bool HasReachedTarget()
    {
        if (!_isMoving)
        {
            return false;
        }

        var distance = new Vector2(
            Mathf.Abs(transform.position.x - _targetPosition.x),
            Mathf.Abs(transform.position.y - _targetPosition.y)
        );

        return distance.x < (_playerMovement.Speed * Time.fixedDeltaTime) && distance.y < (_playerMovement.Speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        float reward = GetCumulativeReward();

        _rewardText.text = reward.ToString("#.##");
    }
}
