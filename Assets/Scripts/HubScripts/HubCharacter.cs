using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HubCharacter : MonoBehaviour
{
    public Faction faction = Faction.None;

    public Sprite dialogueSprite;
    [TextArea(3, 10)]
    public string dialogue;
    public MissionData mission;
}
