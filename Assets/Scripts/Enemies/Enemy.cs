using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    private bool _frozen = false;
    public bool Frozen
    {
        get { return _frozen; }
        set
        {
            if (value != _frozen)
            {
                if (value == true)
                {
                    //enable frozen effect
                }
                else
                {
                    //diable frozen effect
                }
                _frozen = value;
            }
        }
    }
    private bool _onFire = false;
    public bool OnFire
    {
        get { return _onFire; }
        set
        {
            if (value != _onFire)
            {
                if (value == true)
                {
                    //enable on fire effect
                }
                else
                {
                    //diable on fire effect
                }
                _onFire = value;
            }
        }
    }

    /// <summary> Stack of radiation on this enemy. Radiation deals infrequent DOT and doesn't diminish, but can be completely resisted. </summary>
    [HideInInspector]
    public float radiation = 0;
    /// <summary> Stack of acid on this enemy. Acid deals constant DOT until stack runs out, and explodes when enemy is on fire. </summary>
    [HideInInspector]
    public float acid = 0;
    /// <summary> Set to the higher of the old and new value. </summary>
    public void SetAcidDPS(float dps)
    {
        acidDPS = Mathf.Max(acidDPS, dps);
    }
    /// <summary> Tick damage of the most powerful acid effect applied. </summary>
    [HideInInspector]
    public float acidDPS = 1;
    private float _lastAcidTick = 0;
    private float _lastTempTick = 0;
    private float _lastNanitesTick = 0;
    private float _lastRadTick = 0;
    /// <summary> Stack of nanites on this enemy. Nanites deal DOT that is multiplied by the percentage of health remaining,
    /// and does nothing when health is below 10%. Nanites short when hit by lightining, doubling the effects. </summary>
    [HideInInspector]
    public float nanites = 0;
    /// <summary> Stack of antimatter on this enemy. Antimatter creates an explosion with damage AND radius based on the stack size when hit by matter attacks:
    /// physical, acid, nanites. </summary>
    [HideInInspector]
    public float antimatter = 0;

    [HideInInspector]
    public Weapon lastHitBy;

    public delegate void DestroyEvent(Enemy destroyedEnemy);
    /// <summary> Events that happen when this enemy is destroyed - specifically designed to register deaths in game managers. </summary>
    public DestroyEvent destroyEvents;

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
        NanitesDOT();
        TempDOT();
        AcidDOT();
        RadDOT();

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
        destroyEvents.Invoke(this);
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

    public void SpawnExplosion(float scale, Vector2 pos)
    {
        if (data.explosionPrefab != null)
        {
            GameObject explosion = Instantiate(data.explosionPrefab, pos, transform.rotation, transform.parent);
            explosion.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private void NanitesDOT()
    {
        if (nanites <= 0 || _health / MaxHealth < StaticRefs.NanitesCutoff || StaticRefs.DoRadiationTick(_lastNanitesTick) == false)
            return;

        _lastNanitesTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        _health -= nanites * (_health / MaxHealth);// * ResistanceMult(DamageType.radiation);
        healthBar.DOT(_health / MaxHealth);
    }

    private void TempDOT()
    {
        if (StaticRefs.DoTempTick(_lastTempTick) == false)
            return;

        _lastTempTick = Time.deltaTime;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        if (data.damageFromHot && temperature > data.maxSafeTemp)
        {
            _health -= (temperature - data.maxSafeTemp);// * ResistanceMult(DamageType.heat);
            healthBar.DOT(_health / MaxHealth);
        }
        if (data.damageFromCold && temperature < data.freezeTemp)
        {
            _health -= -(temperature - data.freezeTemp);// * ResistanceMult(DamageType.cold);
            healthBar.DOT(_health / MaxHealth);
        }


        if (temperature > data.restingTemp)
            temperature -= data.baseTempChange;
        else
            temperature += data.baseTempChange;
    }

    private void AcidDOT()
    {
        if (acid <= 0 || StaticRefs.DoAcidTick(_lastAcidTick) == false)
            return;

        _lastAcidTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        _health -= StaticRefs.AcidTickDamage(acidDPS);// * ResistanceMult(DamageType.acid);
        healthBar.DOT(_health / MaxHealth);
        acid -= 1;
    }

    private void RadDOT()
    {
        if (radiation <= 0 || StaticRefs.DoRadiationTick(_lastRadTick) == false)
            return;

        _lastRadTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        _health -= radiation;// * ResistanceMult(DamageType.radiation);
        healthBar.DOT(_health / MaxHealth);
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