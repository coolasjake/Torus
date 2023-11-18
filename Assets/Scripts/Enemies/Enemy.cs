using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : TorusMotion
{
    public EnemyClass myClass;
    [HideInInspector]
    public EnemyData data;

    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private HealthBar healthBar;

    private float _health;
    public float health
    {
        get { return _health; }
        set
        {
            if (value != _health)
            {
                _health = value;
                healthBar.Hit(_health / MaxHealth);
            }
        }
    }
    [HideInInspector]
    public float temperature = 0f;

    /// <summary> Time that the latest stun effect on this enemy will end. </summary>
    [HideInInspector]
    public float stunEndTime = 0f;
    public bool Stunned => (stunEndTime > Time.time);

    [HideInInspector]
    public int armourDebuffs = 0;

    [HideInInspector]
    public bool frozen = false;

    /// <summary> Stack of poison on this enemy. Poison deals infrequent DOT and doesn't diminish, but can be completely resisted. </summary>
    [HideInInspector]
    public float poison = 0;
    /// <summary> Stack of acid on this enemy. Acid deals constant DOT until stack runs out, and explodes when enemy is on fire. </summary>
    [HideInInspector]
    public float acid = 0;
    /// <summary> Tick damage of the most powerful acid effect applied. </summary>
    [HideInInspector]
    public float acidDPS = 0;
    /// <summary> Stack of nanites on this enemy. Nanites deal DOT that is multiplied by the percentage of health remaining,
    /// and does nothing when health is below 10%. Nanites short when hit by lightining, doubling the effects. </summary>
    [HideInInspector]
    public float nanites = 0;
    /// <summary> Stack of antimatter on this enemy. Antimatter creates an explosion with damage AND radius based on the stack size when hit by matter attacks:
    /// physical, acid, poison, nanites. </summary>
    [HideInInspector]
    public float antimatter = 0;

    [HideInInspector]
    public Weapon lastHitBy;

    void Awake()
    {
        healthBar = StaticRefs.SpawnHealthBar(Armour);
    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged)
            ApplyTransformPos();
    }
#endif

    private void FixedUpdate()
    {
        DOTEffects();
        MoveCloser(CalculateSpeed());
        CheckReachedStation();
        healthBar.Move(transform.position);
    }

    private float CalculateSpeed()
    {
        if (Stunned)
            return 0f;

        float modifier = 1f;

        //Apply cold and frozen modifiers
        if (temperature <= data.freezeTemp)
            modifier -= data.frozenSlow + data.maxColdSlow;
        else if (temperature < 0f)
            modifier -= (temperature / data.freezeTemp) * data.maxColdSlow;

        //Clamp modifier to not be less than the maximum slow value
        modifier = Mathf.Clamp(modifier, 1f - data.maxSlow, 2f);
        return BaseSpeed * modifier * Time.fixedDeltaTime * 0.01f;
    }

    public void CheckReachedStation()
    {
        if (Height <= 0)
        {
            //Damage Station
        }
    }

    private void DOTEffects()
    {
        UpdateTemperature();
        AcidDOT();

        if (_health <= 0)
        {
            if (lastHitBy != null)
                lastHitBy.KillEnemy(this);
            else
            {
                Destroy(gameObject);
                Debug.LogError("Enemy killed before being hit by weapon.");
            }
        }
    }

    public void Destroy()
    {
        Destroy(healthBar.gameObject);
        SpawnExplosion(EffectsScale);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void SpawnExplosion(float scale)
    {
        if (data.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(data.explosionPrefab, transform.position, transform.rotation, transform.parent);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void UpdateTemperature()
    {
        if (data.damageFromHot && temperature > data.maxSafeTemp)
        {
            _health -= (temperature - data.maxSafeTemp) * ResistanceMult(DamageType.heat);
            healthBar.DOT(_health / MaxHealth);
        }
        if (data.damageFromCold && temperature < data.freezeTemp)
        { 
            _health -= -(temperature - data.freezeTemp) * ResistanceMult(DamageType.heat);
            healthBar.DOT(_health / MaxHealth);
        }


        if (temperature > data.restingTemp)
            temperature -= data.baseTempChange;
        else
            temperature += data.baseTempChange;
    }

    private void AcidDOT()
    {
        if (acid <= 0)
            return;

        float acidDamage = acid;

    }

    public int PointsCost => data.Points(myClass);
    public float MaxHealth => data.Health(myClass);
    public int Armour => data.Armour(myClass) - armourDebuffs;
    public float BaseSpeed => data.Speed(myClass);
    public float XPReward => data.Points(myClass);
    public float EffectsScale => data.EffectsScale(myClass);
    public float ResistanceMult(DamageType type) => (1f - data.resistances.GetDamage(type));

    public void SetData(EnemyData enemyData)
    {
        data = enemyData;

        _health = MaxHealth;
        temperature = data.restingTemp;

        spriteRenderer.sprite = data.ClassSprite(myClass);
        animator.runtimeAnimatorController = data.ClassAnimations(myClass);
    }
}

public enum EnemyClass {
    swarm,
    fast,
    dodge,
    tank,
}