using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HubCharacter : MonoBehaviour
{
    public Faction faction = Faction.None;

    public Sprite characterSprite;
    public string dialogue;
    public MissionData mission;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMission()
    {

    }

    public void Interact()
    {

    }
}
