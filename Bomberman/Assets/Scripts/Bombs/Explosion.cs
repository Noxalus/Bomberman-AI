using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Assets references")]

    [SerializeField] ExplosionPart _explosionCenter = null;
    [SerializeField] ExplosionPart _explosionUp = null;
    [SerializeField] ExplosionPart _explosionDown = null;
    [SerializeField] ExplosionPart _explosionLeft = null;
    [SerializeField] ExplosionPart _explosionRight = null;
    [SerializeField] ExplosionPart _explosionVertical = null;
    [SerializeField] ExplosionPart _explosionHorizontal = null;

    private List<ExplosionPart> _explosionPartInstances = new List<ExplosionPart>();

    private void Awake()
    {
        Initialize(5);
    }

    public void Initialize(int power)
    {
        Vector2 cellSize = Vector2.one;
        Instantiate(_explosionCenter, Vector2.zero, Quaternion.identity, transform);

        for (int i = 1; i <= power; i++)
        {
            Vector2 topPosition = new Vector2(0, i * cellSize.y);
            Vector2 bottomPosition = new Vector2(0, -i * cellSize.y);
            Vector2 leftPosition = new Vector2(-i * cellSize.x, 0);
            Vector2 rightPosition = new Vector2(i * cellSize.x, 0);

            // Top
            if (i == power)
            {
                Instantiate(_explosionUp, topPosition, Quaternion.identity, transform);
            }
            else
            {
                Instantiate(_explosionVertical, topPosition, Quaternion.identity, transform);
            }

            Debug.Log($"Top position: {topPosition}");

            // Bottom
            if (i == power)
            {
                Instantiate(_explosionDown, bottomPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(_explosionVertical, bottomPosition, Quaternion.identity);
            }

            // Left
            if (i == power)
            {
                Instantiate(_explosionLeft, leftPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(_explosionHorizontal, leftPosition, Quaternion.identity);
            }

            // Right
            if (i == power)
            {
                Instantiate(_explosionRight, rightPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(_explosionHorizontal, rightPosition, Quaternion.identity);
            }
        }
    }
}
