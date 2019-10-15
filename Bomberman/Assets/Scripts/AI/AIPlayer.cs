using UnityEngine;

public class AIPlayer : Player
{
    [Header("Inner references")]

    [SerializeField] AIBehaviour _behaviour = null;

    public AIBehaviour Behaviour => _behaviour;

    private void OnDestroy()
    {
        _behaviour.Clear();
    }
}
