using System;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MLAIPlayer : Agent
{
    [SerializeField]
    private Player _player = null;

    [SerializeField]
    private PlayerMovement _playerMovement = null;


    public Player Player => _player;

    public enum PossibleAction
    {
        Nothing,
        Up,
        Right,
        Down,
        Left,
        Bomb
    }

    private EnvironmentParameters _environmentParameters;

    public override void Initialize()
    {
        _environmentParameters = Academy.Instance.EnvironmentParameters;

        _player.OnDeath.AddListener((player) => AddReward(-1f));
        _player.OnPickUpBonus.AddListener(
            (player, bonusType) =>
            {
                if (bonusType == EBonusType.Bad)
                {
                    AddReward(-0.5f);
                }
                else
                {
                    AddReward(0.5f);
                }
            }
        );
        _player.OnPlantBomb.AddListener((player) => AddReward(0.1f));
        _player.OnWallDestroy.AddListener((player) => AddReward(0.75f));
    }

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    base.CollectObservations(sensor);

    //    //sensor.AddObservation(_player.)
    //}

    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        // Prevents the agent from picking an action that would make it collide with a wall
        //var positionX = (int)transform.position.x;
        //var positionZ = (int)transform.position.z;
        //var maxPosition = (int)_environmentParameters.GetWithDefault("gridSize", 5f) - 1;

        //if (positionX == 0)
        //{
        //    actionMasker.SetMask(0, new[] { PossibleAction.Left });
        //}

        //if (positionX == maxPosition)
        //{
        //    actionMasker.SetMask(0, new[] { PossibleAction.Right });
        //}

        //if (positionZ == 0)
        //{
        //    actionMasker.SetMask(0, new[] { PossibleAction.Down });
        //}

        //if (positionZ == maxPosition)
        //{
        //    actionMasker.SetMask(0, new[] { PossibleAction.Up });
        //}
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = (int)PossibleAction.Nothing;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = (int)PossibleAction.Right;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = (int)PossibleAction.Up;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = (int)PossibleAction.Left;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = (int)PossibleAction.Down;
        }
        if (Input.GetKey(KeyCode.Space))
        {
             actionsOut[0] = (int)PossibleAction.Bomb;
        }
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var movement = Vector2.zero;
        var action = (PossibleAction)Mathf.FloorToInt(vectorAction[0]);

        switch (action)
        {
            case PossibleAction.Nothing:
                // do nothing
                break;
            case PossibleAction.Right:
                movement.x = 1;
                break;
            case PossibleAction.Left:
                movement.x = -1;
                break;
            case PossibleAction.Up:
                movement.y = 1;
                break;
            case PossibleAction.Down:
                movement.y = -1;
                break;
            case PossibleAction.Bomb:
                _playerMovement.PlantBomb();
                break;
            default:
                throw new ArgumentException("Invalid action value");
        }

        _playerMovement.Move(movement);
    }
}
