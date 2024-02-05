using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class HubInput : MonoBehaviour
{
    public PlayerInput playerInput;
    public MultiplayerEventSystem MPEvents;

    private void Reset()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (MPEvents == null)
            MPEvents = GetComponentInChildren<MultiplayerEventSystem>();
    }

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

    public void OnPause(InputAction.CallbackContext context)
    {
        PauseManager.TogglePause();
    }
}
