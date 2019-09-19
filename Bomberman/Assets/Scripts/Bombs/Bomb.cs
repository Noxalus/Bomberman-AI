using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private Collider2D _collider = null;

    private void Awake()
    {
        _collider.enabled = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            _collider.enabled = true;
        }
    }
}
