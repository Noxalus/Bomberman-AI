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
    private List<Vector2Int> _visitedCells = new List<Vector2Int>();
    private Vector2Int _currentCell;

    private Map _map;

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

        //_player.OnBonusDestroy.AddListener(
        //    (player, bonusType) =>
        //    {
        //        if (bonusType == EBonusType.Bad)
        //        {
        //            AddReward(0.1f);
        //        }
        //        else
        //        {
        //            AddReward(-0.1f);
        //        }
        //    }
        //);

        //_player.OnPickUpBonus.AddListener(
        //    (player, bonusType) =>
        //    {
        //        if (bonusType == EBonusType.Bad)
        //        {
        //            AddReward(-0.05f);
        //        }
        //        else
        //        {
        //            AddReward(0.05f);
        //        }
        //    }
        //);
    }

    public override void OnEpisodeBegin()
    {
        _currentCell = Vector2Int.zero;
        _previousPosition = Vector2.zero;
        _previousAction = AgentAction.Nothing;
        _visitedCells.Clear();
    }

    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        if (_player.IsDead)
        {
            actionMasker.SetMask(0, new[] { 
                (int)AgentAction.Up, 
                (int)AgentAction.Right, 
                (int)AgentAction.Down, 
                (int)AgentAction.Left,
                (int)AgentAction.Bomb
            });
        }
        // Prevents the agent from planting a bomb if he can't
        else if (_player.IsInvincible || _player.BombCount == 0)
        {
            actionMasker.SetMask(0, new[] { (int)AgentAction.Bomb });
        }
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
        bool wantedToMove = (int)_previousAction > 0 && (int)_previousAction < 5;
        var cellPosition = _map.CellPosition(_player.transform.position);
        bool hasChangedCell = !_currentCell.Equals(cellPosition);

        // Reward for new visited cells
        //if (hasChangedCell && !_visitedCells.Contains(cellPosition))
        //{
        //    AddReward(0.01f);
        //    _visitedCells.Add(cellPosition);
        //}

        // Check danger
        //var dangerLevel = _map.GetDangerLevel(cellPosition);
        //if (dangerLevel > 0)
        //{
        //    var rewardValue = -((dangerLevel / 3f) * 0.005f);
        //    //Debug.Log($"DANGER: {rewardValue}");

        //    AddReward(rewardValue);
        //}
        //else if (dangerLevel == 0)
        //{
        //    AddReward(0.001f);
        //}

        var movement = Vector2.zero;
        var action = (AgentAction)Mathf.FloorToInt(vectorAction[0]);
        
        // Malus if no action
        //if (action == AgentAction.Nothing && _player.BombCount == _player.MaxBombCount)
        //{
        //    AddReward(-0.01f);
        //}

        //if (wantedToMove)
        //{
        //    // If the agent wanted to move but didn't move => negative reward
        //    if (transform.position.Equals(_previousPosition))
        //    {
        //        //Debug.Log($"DIDN'T MOVE: -0.05");
        //        AddReward(-0.001f);
        //    }
        //    // Reward continuous movement
        //    else if (action == _previousAction)
        //    {
        //        AddReward(0.01f);
        //    }
        //}

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

        _playerMovement.Move(movement);
    }

    private void Update()
    {
        float reward = GetCumulativeReward();

        _rewardText.text = reward.ToString();
    }
}
