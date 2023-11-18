using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image background;
    [SerializeField]
    private Image armour;
    [SerializeField]
    private Image health;
    [SerializeField]
    private Image hit;
    [SerializeField]
    private Image dot;

    [SerializeField]
    private float effectClearRate = 0.2f;
    [SerializeField]
    private float effectDropSpeed = 1f;

    private static readonly int cacheLen = 5;

    private int _dotIndex = 0;
    private float[] _dotDamages = new float[cacheLen];
    private int _hitIndex = 0;
    private float[] _hitDamages = new float[cacheLen];
    private float _lastDam = 0f;

    private float _hitEffectVal = 1f;
    private float _hitLagVal = 0f;
    private float _dotEffectVal = 1f;
    private float _dotLagVal = 0f;

    private void Awake()
    {
        health.fillAmount = 1f;
        gameObject.SetActive(false);
    }

    void Update()
    {
        _hitLagVal = _hitLagVal.Lerp(_hitEffectVal, effectDropSpeed * Time.deltaTime);
        _dotLagVal = _dotLagVal.Lerp(_dotEffectVal, effectDropSpeed * Time.deltaTime);
        UpdateFills();


        if (Time.time > _lastDam + effectClearRate)
        {
            StoreHit(0);
            StoreDOT(0);
        }
    }

    public void Move(Vector2 worldPos)
    {
        background.rectTransform.position = Camera.main.WorldToScreenPoint(worldPos);
    }

    public void SetArmour(int level)
    {
        armour.sprite = StaticRefs.ArmourBorder(level);
    }

    public void Hit(float newHealth01)
    {
        gameObject.SetActive(true);
        float damage = health.fillAmount - newHealth01;
        health.fillAmount = newHealth01;
        _hitLagVal += damage;
        StoreHit(damage);

        _hitLagVal = Mathf.Max(_hitLagVal, _hitEffectVal);
        UpdateFills();
    }

    private void StoreHit(float hitVal)
    {
        _hitDamages[_hitIndex] = hitVal;
        _hitIndex = (_hitIndex + 1) % cacheLen;
        _lastDam = Time.time;

        _hitEffectVal = 0f;
        foreach (float dam in _hitDamages)
            _hitEffectVal += dam;
    }

    public void DOT(float newHealth01)
    {
        gameObject.SetActive(true);
        float damage = health.fillAmount - newHealth01;
        health.fillAmount = newHealth01;
        _dotLagVal += damage;
        StoreDOT(damage);

        _dotLagVal = Mathf.Max(_dotLagVal, _dotEffectVal);
        UpdateFills();
    }

    private void StoreDOT(float dotVal)
    {
        _dotDamages[_hitIndex] = dotVal;
        _dotIndex = (_dotIndex + 1) % cacheLen;
        _lastDam = Time.time;

        _dotEffectVal = 0f;
        foreach (float dam in _dotDamages)
            _dotEffectVal += dam;
    }

    private void UpdateFills()
    {
        hit.fillAmount = health.fillAmount + _hitLagVal;
        dot.fillAmount = health.fillAmount + _hitLagVal + _dotLagVal;
    }
}
