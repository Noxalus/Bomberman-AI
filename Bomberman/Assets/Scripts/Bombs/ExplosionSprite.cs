using System;
using UnityEngine;

public class ExplosionSprite : MonoBehaviour
{
    public Action OnExplosionFinished;

    public void ExplosionFinished()
    {
        Destroy();
        OnExplosionFinished?.Invoke();
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
