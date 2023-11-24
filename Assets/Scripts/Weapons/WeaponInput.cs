using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInput : MonoBehaviour
{
    public Vector2 Movement => _movementInput;
    private Vector2 _movementInput = Vector2.zero;
    public bool MovementDown
    {
        get
        {
            bool down = _movementDown;
            _movementDown = false;
            return down;
        }
    }
    private bool _movementDown = false;
    public bool Firing => _fireInput;
    private bool _fireInput = false;
    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
        if (context.started)
            _movementDown = true;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        _fireInput = context.action.triggered;
    }
}
