using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : TorusMotion
{
    private float _health;
    private int _armourLevel;
    private float _speed;

    private float _temperature = 0f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hit(float damage,  DamageType type)
    {
        
    }


}

public enum DamageType {
    basic,      //default damage, uneffected by resistances or armor
    physical,   //kinetic damage. Applied instantly, heavily effected by armor, deals bonus damage to frozen
    heat,       //positive temperature damage. Damage is applied based on amount over max temp
    cold,       //negative temperature damage. Never effects health. Slows enemy 
    lightning,
    poison,
    acid,
    nanites,
    antimatter
}

public enum EnemyType {
    none,
    tank,
    swarm,
    fast,
    dodge
}

public abstract class Effect
{
    public abstract void Initialise();
    
    
}