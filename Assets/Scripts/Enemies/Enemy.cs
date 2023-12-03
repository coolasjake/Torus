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

    private SpriteRenderer _tempEffectSR;
    private GameObject _frozenEffect;
    private GameObject _fireEffect;

    private HealthBar healthBar;

    private float _health;
    public float Health => _health;

    /// <summary> Reduce the health of the enemy after applying tank ability and rounding up to 1, then show changes on the healthbar. </summary>
    public void ReduceHealthBy(float damage, Weapon hitBy)
    {
        lastHitBy = hitBy;
        Damage(damage);
        healthBar.Hit(_health / MaxHealth);
    }

    /// <summary> Reduce the health of the enemy after applying tank ability and rounding up to 1, then show changes on the healthbar. </summary>
    public void DOTDamage(float damage)
    {
        Damage(damage);
        healthBar.DOT(_health / MaxHealth);
    }

    /// <summary> Reduce the health of the enemy after applying tank ability and rounding up to 1. </summary>
    private void Damage(float damage)
    {
        if (myClass == EnemyClass.tank)
            damage -= AbilityPower;
        damage = Mathf.Max(damage, 1);
        _health -= damage;
    }

    [HideInInspector]
    public float temperature = 0f;

    /// <summary> Time that the latest stun effect on this enemy will end. </summary>
    [HideInInspector]
    public float stunEndTime = 0f;
    public bool Stunned => (stunEndTime > Time.time);

    public bool lightningStruck = false;
    //private bool _lastLightningStruck = 0f;

    [HideInInspector]
    public int armourDebuffs = 0;

    private bool _disabled = false;
    public bool Disabled => _disabled || _frozen;

    private bool _frozen = false;
    public bool Frozen
    {
        get { return _frozen; }
        set
        {
            if (value != _frozen)
            {
                if (value == true)
                    ShowFrozenEffect();
                else
                    HideFrozenEffect();
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
                    ShowFireEffect();
                else
                    HideFireEffect();
                _onFire = value;
            }
        }
    }

    /// <summary> Stack of radiation on this enemy. Radiation deals infrequent DOT and doesn't diminish, but can be completely resisted. </summary>
    [HideInInspector]
    private float _radiation = 0;
    public float Radiation => _radiation;
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
    private float _lastDodge = 0;
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

    [HideInInspector]
    public Enemy[] nearbyEnemies = new Enemy[5];

    public delegate void DestroyEvent(Enemy destroyedEnemy);
    /// <summary> Events that happen when this enemy is destroyed - specifically designed to register deaths in game managers. </summary>
    public DestroyEvent destroyEvents;

    void Initialize()
    {
        healthBar = StaticRefs.SpawnHealthBar(Armour);
        _tempEffectSR = StaticRefs.SpawnTempEffect(this);
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
        return ClassSpeed * modifier * Time.fixedDeltaTime * StaticRefs.BaseSpeed;
    }

    public bool CheckDodge(Vector2 hitPos)
    {
        if (myClass == EnemyClass.dodge && Time.time > _lastDodge + AbilityPower)
        {
            float movement = StaticRefs.DodgeDist;
            if (TorusMotion.AngleFromPos(hitPos) > Angle)
                movement *= -1;
            MoveAround(movement);
            _lastDodge = Time.time;
            return true;
        }
        return false;
    }

    public void RadiationHit(float damage)
    {
        if (_radiation <= 0)
            _lastRadTick = Time.time;
        _radiation += damage;
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
        lightningStruck = false;

        if (_health <= 0)
        {
            if (lastHitBy != null)
                lastHitBy.KillEnemy(this);
            else
            {
                Debug.LogError("Enemy killed before being hit by weapon.");
                Destroy();
            }
        }
    }

    private void NanitesDOT()
    {
        if (nanites <= 0 || _health / MaxHealth < StaticRefs.NanitesCutoff || StaticRefs.DoRadiationTick(_lastNanitesTick) == false)
            return;

        _lastNanitesTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        DOTDamage(nanites * (_health / MaxHealth));
    }

    private void TempDOT()
    {
        if (StaticRefs.DoTempTick(_lastTempTick) == false)
            return;

        _lastTempTick = Time.deltaTime;

        if (OnFire)
            temperature += 2f;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        if (data.damageFromHot && temperature > data.maxSafeTemp)
        {
            DOTDamage(temperature - data.maxSafeTemp);
        }
        if (temperature < data.freezeTemp)
        {
            Frozen = true;
            if (data.damageFromCold)
            {
                DOTDamage(-(temperature - data.freezeTemp));
            }
        }
        else
            Frozen = false;

        temperature = temperature.Lerp(data.restingTemp, data.baseTempChange * StaticRefs.TempTickRate);
        UpdateTempEffect();
    }

    private void AcidDOT()
    {
        if (acid <= 0 || StaticRefs.DoAcidTick(_lastAcidTick) == false)
            return;

        _lastAcidTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        DOTDamage(StaticRefs.AcidTickDamage(acidDPS));
        acid -= 1;
    }

    private void RadDOT()
    {
        if (_radiation <= 0 || StaticRefs.DoRadiationTick(_lastRadTick) == false)
            return;

        _lastRadTick = Time.time;

        //Note: DOT effects are effected by resistances when applied, not when dealing damage
        DOTDamage(_radiation);
    }

    public void Destroy()
    {
        Destroy(healthBar.gameObject);
        StaticRefs.SpawnExplosion(EffectsScale, transform.position);
        gameObject.SetActive(false);
        destroyEvents.Invoke(this);
        Destroy(gameObject);
    }

    private void UpdateTempEffect()
    {
        if (temperature >= data.restingTemp)
        {
            _tempEffectSR.color = StaticRefs.HotColour((temperature - data.restingTemp) / data.maxSafeTemp);
        }
        else if (temperature < data.restingTemp)
        {
            _tempEffectSR.color = StaticRefs.ColdColour(((temperature - data.restingTemp) / data.freezeTemp));
        }
    }

    private void ShowFrozenEffect()
    {
        if (_frozenEffect == null)
            _frozenEffect = StaticRefs.SpawnFrozenEffect(this);
        else
            _frozenEffect.SetActive(true);
    }

    private void HideFrozenEffect()
    {
        _frozenEffect.SetActive(false);
    }

    private void ShowFireEffect()
    {
        if (_fireEffect == null)
            _fireEffect = StaticRefs.SpawnFireEffect(this);
        else
            _fireEffect.SetActive(true);
    }

    private void HideFireEffect()
    {
        _fireEffect.SetActive(false);
    }

    public int PointsCost => data.Points(myClass);
    public float MaxHealth => data.Health(myClass);
    public int Armour => data.Armour(myClass) - armourDebuffs;
    public float ClassSpeed => data.Speed(myClass);
    public float XPReward => data.Points(myClass);
    public float EffectsScale => data.EffectsScale(myClass);
    public float AbilityPower => data.Ability(myClass);
    public float ResistanceMult(DamageType type) => (1f - data.resistances.GetDamage(type));

    public void SetData(EnemyData enemyData)
    {
        data = enemyData;

        _health = MaxHealth;
        temperature = data.restingTemp;

        spriteRenderer.sprite = data.ClassSprite(myClass);

        animator.runtimeAnimatorController = data.ClassAnimations(myClass);

        Initialize();
    }
}

public enum EnemyClass {
    swarm,
    fast,
    dodge,
    tank,
}