using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : TorusMotion
{
    [SerializeField]
    private EnemyClass _myClass;
    public EnemyClass Class => _myClass;
    [HideInInspector]
    public EnemyData data;

    public SpriteRenderer spriteRenderer;
    public Animator animator;

    private SpriteRenderer _tempEffectSR;
    private GameObject _frozenEffect;
    private GameObject _fireEffect;
    private GameObject _acidEffect;
    private GameObject _nanitesEffect;
    private GameObject _antimatterEffect;

    private HealthBar healthBar;

    private float _health;
    public float Health => _health;

    /// <summary> Reduce the health of the enemy after applying tank ability and rounding up to 0, then show changes on the healthbar. </summary>
    public void ReduceHealthBy(float damage)
    {
        Damage(damage);
        healthBar.Hit(_health / MaxHealth);
    }

    /// <summary> Reduce the health of the enemy after applying tank ability and rounding up to 0, then show changes on the healthbar. </summary>
    public void DOTDamage(float damage)
    {
        Damage(damage);
        healthBar.DOT(_health / MaxHealth);
    }

    /// <summary> Reduce the health of the enemy after applying tank ability. </summary>
    private void Damage(float damage)
    {
        if (damage <= 0)
            return;
        damage = this.AfterAbilityReductions(damage);
        damage = Mathf.Max(damage, 0);
        _health -= damage;
    }

    public void SetHealth(float newHealth)
    {
        _health = newHealth;
        healthBar.Hit(_health / MaxHealth);
    }

    [HideInInspector]
    public float temperature = 0f;
    
    public void ChangeTemp(float byValue)
    {
        temperature += byValue / Size;
    }

    /// <summary> Time that the latest stun effect on this enemy will end. </summary>
    public bool Grounded => Time.time < _lastLightningHit + StaticRefs.GroundedDur;
    private float _lastLightningHit = 0f;
    private bool Stunned => _stunnedUntil > Time.time;
    private float _stunnedUntil = 0;
    private Weapon _lastStunnedBy = null;
    public bool LightningHit(Weapon by)
    {
        if (by == _lastStunnedBy && Grounded)
            return false;

        _lastLightningHit = Time.time;
        _lastStunnedBy = by;
        return true;
    }

    public void Stun(float stunDur)
    {
        _stunnedUntil = Time.time + stunDur;
    }

    //private float _lastLightningStruck = 0f;
    [HideInInspector]
    public bool triggerAntimatter = false;

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
                temperature = Mathf.Min(data.freezeTemp, temperature);
                if (value == true)
                    ShowFrozenEffect();
                else
                    HideFrozenEffect();
                _frozen = value;
                if (OnFire && _frozen)
                    DamageEvents.FreezeWhileOnFire(this);
            }
        }
    }
    private float _onFireUntil = -1;
    public bool OnFire
    {
        get { return _onFireUntil > Time.time; }
    }
    public void SetOnFire(float duration)
    {
        duration = Mathf.Max(duration, Time.fixedDeltaTime);
        duration = Mathf.Max(duration, _onFireUntil - Time.time);
        if (OnFire && _frozen)
            DamageEvents.IgniteWhileFrozen(this);
        if (temperature > data.restingTemp)
        {
            _onFireUntil = Time.time + duration;
            ShowFireEffect();

            if (OnFire && acid > 0)
                DamageEvents.AcidFireExplosion(this);
        }
    }
    public void RemoveFire()
    {
        _onFireUntil = Mathf.Min(_onFireUntil, Time.time);
        HideFireEffect();
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
    public float acidDPS = 1f;
    private float _lastAcidTick = 0;
    /// <summary> Permanent modifier to armour from fire upgrades. </summary>
    [HideInInspector]
    public int meltedArmour = 0;
    /// <summary> Permanent modifier to armour from acid upgrades. </summary>
    [HideInInspector]
    public int dissolvedArmour = 0;

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
        healthBar.Move(transform.position);
    }

    private float CalculateSpeed()
    {
        if (Stunned)
            return 0f;

        float modifier = 1f;

        if (Class == EnemyClass.fast && Height < StaticRefs.BoostStartingHeight)
            modifier += AbilityPower;

        //Apply cold and frozen modifiers
        if (temperature <= data.freezeTemp)
            modifier -= data.frozenSlow + data.maxColdSlow;
        else if (temperature < 0f)
            modifier -= (temperature / data.freezeTemp) * data.maxColdSlow;

        //Clamp modifier to not be less than the maximum slow value
        modifier = Mathf.Clamp(modifier, 1f - data.maxSlow, 3f);
        modifier = DamageEvents.ApplySpecialSpeedModifiers(this, modifier);
        return ClassSpeed * modifier * Time.fixedDeltaTime * StaticRefs.BaseSpeed;
    }

    public bool CheckDodge(Vector2 hitPos)
    {
        if (Class == EnemyClass.dodge && Time.time > _lastDodge + AbilityPower && Stunned == false && Frozen == false)
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

    private void DOTEffects()
    {
        AntimatterCheck();
        NanitesDOT();
        TempDOT();
        AcidDOT();
        RadDOT();
        triggerAntimatter = false;

        UpdateTempEffect();

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

    private void AntimatterCheck()
    {
        if (antimatter > 0)
        {
            ShowAntimatterEffect();

            if (triggerAntimatter || acid > 0 || nanites > 0)
                StaticRefs.SpawnAntimatterExplosion(transform.position, this);
        }
        else
        {
            HideAntimatterEffect();
        }
    }

    private void NanitesDOT()
    {
        if (nanites > 0)
        {
            ShowNanitesEffect();

            if (StaticRefs.DoNanitesTick(_lastNanitesTick, _frozen))
            {
                _lastNanitesTick = Time.time;
                DamageEvents.Nanites.DamageTick(this);
            }
        }
        else
        {
            _lastNanitesTick = Time.time;
            HideNanitesEffect();
        }
    }

    private void TempDOT()
    {
        if (StaticRefs.DoTempTick(_lastTempTick) == false)
            return;

        _lastTempTick = Time.time;

        //Remove frozen/fire if temp is on the wrong side of resting
        if (temperature >= data.restingTemp)
            Frozen = false;
        if (temperature <= data.restingTemp || OnFire == false)
            RemoveFire();

        if (OnFire)
            DamageEvents.Heat.FireDamage(this);

        DamageEvents.Heat.DamageTick(this);
        DamageEvents.Cold.DamageTick(this);

        DamageEvents.BaseTempChange(this);
    }

    private void AcidDOT()
    {
        if (OnFire && acid > 0)
            DamageEvents.AcidFireExplosion(this);

        if (acid > 0)
        {
            ShowAcidEffect();

            if (StaticRefs.DoAcidTick(_lastAcidTick) == false)
                return;

            _lastAcidTick = Time.time;

            if (nanites > 0)
                nanites -= StaticRefs.AcidTickDamage(acidDPS);
            else
                //Note: DOT effects are effected by resistances when applied, not when dealing damage
                DOTDamage(StaticRefs.AcidTickDamage(acidDPS));

            acid -= 1;
            if (acid < 0)
                acid = 0;
        }
        else
        {
            _lastAcidTick = Time.time;
            acidDPS = 1;
            HideAcidEffect();
        }
    }

    private void RadDOT()
    {
        if (radiation > 0)
        {
            ShowRadiationEffect();

            if (radiation <= 0 || StaticRefs.DoRadiationTick(_lastRadTick) == false)
                return;

            _lastRadTick = Time.time;

            //Note: DOT effects are effected by resistances when applied, not when dealing damage
            DOTDamage(radiation);
        }
        else
        {
            HideRadiationEffect();
            _lastRadTick = Time.time;
        }
    }

    public void Destroy()
    {
        Destroy(healthBar.gameObject);
        StaticRefs.SpawnExplosion(Size, transform.position);
        gameObject.SetActive(false);
        destroyEvents.Invoke(this);
        Destroy(gameObject);
    }

    private void UpdateTempEffect()
    {
        float normTemp = NormalisedTemp;
        if (normTemp >= 0)
            _tempEffectSR.color = StaticRefs.HotColour(normTemp);
        else
            _tempEffectSR.color = StaticRefs.ColdColour(-normTemp);
    }

    /// <summary> Temperature of the enemy normalised so that -1 = frozen, 0 = resting, 1 = heat damage (can go above/below 1/-1). </summary>
    public float NormalisedTemp
    {
        get
        {
            if (temperature >= data.restingTemp)
            {
                return (temperature - data.restingTemp) / data.maxSafeTemp;
            }
            return -(temperature - data.restingTemp) / data.freezeTemp;
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
        if (_fireEffect != null)
            _fireEffect.SetActive(false);
    }

    private void ShowAcidEffect()
    {
        if (_acidEffect == null)
            _acidEffect = StaticRefs.SpawnAcidEffect(this);
        else
            _acidEffect.SetActive(true);
    }

    private void HideAcidEffect()
    {
        if (_acidEffect != null)
            _acidEffect.SetActive(false);
    }

    private void ShowNanitesEffect()
    {
        if (_nanitesEffect == null)
            _nanitesEffect = StaticRefs.SpawnNanitesEffect(this);
        else
            _nanitesEffect.SetActive(true);
    }

    private void HideNanitesEffect()
    {
        if (_nanitesEffect != null)
            _nanitesEffect.SetActive(false);
    }

    private void ShowRadiationEffect()
    {
    }

    private void HideRadiationEffect()
    {
    }

    private void ShowAntimatterEffect()
    {
        if (_antimatterEffect == null)
            _antimatterEffect = StaticRefs.SpawnAntimatterEffect(this);
        else
            _antimatterEffect.SetActive(true);
    }

    private void HideAntimatterEffect()
    {
        if (_antimatterEffect != null)
            _antimatterEffect.SetActive(false);
    }

    public int PointsCost => data.Points(Class);
    public float MaxHealth => data.Health(Class);
    public int Armour => data.Armour(Class) - armourDebuffs;
    public float ClassSpeed => data.Speed(Class);
    public float XPReward => data.Points(Class);
    public float Size => data.Size(Class);
    public float AbilityPower => data.Ability(Class);
    public float ResistanceMult(DamageType type) => (1f - data.resistances.GetDamage(type));

    public void SetData(EnemyData enemyData)
    {
        data = enemyData;

        _health = MaxHealth;
        temperature = data.restingTemp;

        spriteRenderer.sprite = data.ClassSprite(Class);

        animator.runtimeAnimatorController = data.ClassAnimations(Class);

        Initialize();
    }
}

public enum EnemyClass {
    swarm,
    fast,
    dodge,
    tank,
}