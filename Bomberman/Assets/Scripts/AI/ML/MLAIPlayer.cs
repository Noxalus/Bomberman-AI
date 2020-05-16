using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MLAIPlayer : Agent
{
    [SerializeField]
    private Player _player = null;
    
    [SerializeField]
    private PlayerMovement _playerMovement = null;


    EnvironmentParameters _environmentParameters;

    public Player Player => _player;


    public override void Initialize()
    {
        base.Initialize();

        _environmentParameters = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);

        //sensor.AddObservation(_player.)
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        base.OnActionReceived(vectorAction);

        if (vectorAction[2] > 0f)
        {
            _playerMovement.PlantBomb();
        }
        
        if (vectorAction[0] != 0f || vectorAction[1] != 0f)
        {
            _playerMovement.Move(new Vector2(vectorAction[0], vectorAction[1]));
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        base.Heuristic(actionsOut);
    }

    private void SetResetParameters()
    {
        // Setup the player with given parameters (bomb count, speed, etc...)
    }
}
