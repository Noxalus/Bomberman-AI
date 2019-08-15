using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Rigidbody2D _rigidbody = null;

    private Vector2 _movement = Vector2.zero;

    void Update()
    {
        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _movement * _moveSpeed * Time.fixedDeltaTime);
    }
}
