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

    public override void Kill(Player killer)
    {
        base.Kill(killer);
        Behaviour.Enable(false);
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
        Behaviour.Enable();
    }
}
