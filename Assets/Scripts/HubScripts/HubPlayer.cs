using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubPlayer : MonoBehaviour
{
    public int playerIndex = 0;
    public float tileSize = 1f;
    public WeaponInput input;

    // Start is called before the first frame update
    void Start()
    {
        input = StaticRefs.SpawnInputPrefab(transform, playerIndex, "Keyboard_" + playerIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PauseManager.Paused)
            return;

        Move();
    }

    private void Move()
    {
        if (input.MovementDown)
        {
            Vector2 move = input.Movement.SimplifyToDir() * StaticRefs.TileSize;
            if (CheckMove(move))
            {
                transform.position += (Vector3)move;
            }
        }
    }

    private bool CheckMove(Vector2 movement)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + movement, Vector2.one * tileSize, 0);
        foreach (Collider2D hit in hits)
        {
            HubCharacter character = hit.GetComponent<HubCharacter>();
            if (character)
            {
                character.Interact();
            }
        }
        return (hits.Length == 0);
    }

    private void InteractWithCharacter(HubCharacter character)
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position, Vector3.one * tileSize);
    }
}
