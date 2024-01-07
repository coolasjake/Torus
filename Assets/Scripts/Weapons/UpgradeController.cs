using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeController : MonoBehaviour
{
    public Weapon targetWeapon;
    public int index = 0;
    public string abilityDataFolder = "/AbilityData";

    public TMP_Text title;

    public List<AbilityUI> buttons = new List<AbilityUI>();

    public static Ability[] allAbilities = null;

    //Ability lists are shown in inspector for debugging only
    [SerializeField]
    private List<Ability> availableAbilities = new List<Ability>();
    [SerializeField]
    private List<Ability> chosenAbilities = new List<Ability>();

    private List<Ability> appliedAbilities = new List<Ability>();
    private List<Ability> impossibleAbilities = new List<Ability>();

    private bool _initialized = false;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void GetAvailableAbilities()
    {
        //Load all abilities
        if (allAbilities == null || allAbilities.Length == 0)
            allAbilities = Resources.LoadAll<Ability>(abilityDataFolder);
        if (allAbilities.Length == 0)
            Debug.LogError("No abilities found at path: " + abilityDataFolder);

        //Add valid abilities to available abilies list.
        availableAbilities.Clear();
        foreach (Ability ability in allAbilities)
        {
            bool isValid =
                IsAllowedOnWeapon(ability) &&
                CanBeUnlockedOrRepeated(ability) &&
                IsAllowedThisWave(ability) &&
                IsValidType(ability) &&
                HasRequirementsMet(ability) &&
                IsNotIncompatible(ability);

            if (isValid)
                availableAbilities.Add(ability);
        }

        PrintAbilityOptions();
    }

    /// <summary> True if the ability has not already been unlocked, and is below max number of repeats. </summary>
    private bool CanBeUnlockedOrRepeated(Ability ability)
    {
        int count = 0;
        foreach (Ability existingAbility in appliedAbilities)
        {
            if (ability.name == existingAbility.name)
                count += 1;
        }
        return count < ability.maxRepeats + 1;
    }

    /// <summary> True if wave number is between the abilities min and max waves. </summary>
    private bool IsAllowedThisWave(Ability ability)
    {
        return BattleController.WaveNumber >= ability.minWave && BattleController.WaveNumber <= ability.maxWave;
    }

    /// <summary> True if the ability doesn't require type and is not an incompatible damage type,
    /// or does require type and is an existing damage type (existing trumps incompatible). </summary>
    private bool IsValidType(Ability ability)
    {
        return (ability.requireType == false && targetWeapon.incompatibleDamageTypes.Includes(ability.damageType) == false)
            || (ability.requireType == true && targetWeapon.existingDamageTypes.Includes(ability.damageType));
    }

    /// <summary> True if the ability doesn't require type and is not an incompatible damage type,
    /// or does require type and is an existing damage type (existing trumps incompatible). </summary>
    private bool IsAllowedOnWeapon(Ability ability)
    {
        return ability.allowedWeapons.Includes(targetWeapon.Type());
    }

    /// <summary> True if the prerequisites of the ability are met. </summary>
    private bool HasRequirementsMet(Ability ability)
    {
        if (ability.preReqAbilities.Count > 0)
        {
            bool[] preReqs = new bool[ability.preReqAbilities.Count];
            bool allMet = false;
            foreach (Ability existingAbility in appliedAbilities)
            {
                allMet = true;
                for (int i = 0; i < ability.preReqAbilities.Count; ++i)
                {
                    if (preReqs[i] || existingAbility.name == ability.preReqAbilities[i])
                        preReqs[i] = true;
                    else
                        allMet = false;
                }
                if (allMet)
                    break;
            }
            return allMet;
        }
        return true;
    }

    /// <summary> True if no abilities that THIS ability is incompatible with have been applied (does not check existing abilities being compatible with this one). </summary>
    private bool IsNotIncompatible(Ability ability)
    {
        if (ability.incompatibleAbilities.Count > 0)
        {
            foreach (Ability existingAbility in appliedAbilities)
            {
                for (int i = 0; i < ability.incompatibleAbilities.Count; ++i)
                {
                    if (existingAbility.name == ability.incompatibleAbilities[i])
                        return false;
                }
            }
        }
        return true;
    }

    public void StartUpgrading()
    {
        Initialize();

        Show();
        GiveTestPoints();
        ChooseOptions();
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        if (targetWeapon == null)
        {
            Weapon[] weapons = FindObjectsOfType<Weapon>();
            foreach (Weapon weapon in weapons)
            {
                if (weapon.gameObject.activeInHierarchy && weapon.playerIndex == index)
                {
                    targetWeapon = weapon;
                    break;
                }
            }
        }

        title.text = targetWeapon.Type().ToString() + " Upgrades";
        SetupDamageTypes();
    }

    private void SetupDamageTypes()
    {
        foreach (DamageType damageType in System.Enum.GetValues(typeof(DamageType)))
        {
            if (damageType == DamageType.none || damageType == DamageType.basic)
                continue;

            targetWeapon.damageStats.ModifyDamage(damageType, "Starting Value", StatChangeOperation.Percentage, -100f);

            if (targetWeapon.existingDamageTypes.Includes(damageType))
                ApplyAbility(StaticRefs.DefaultAbility(damageType));
        }
    }

    private void GiveTestPoints()
    {
        targetWeapon.LevelUp();
    }

    private void ChooseOptions()
    {
        GetAvailableAbilities();

        PrintAbilityOptions();

        chosenAbilities.Clear();
        foreach (AbilityUI button in buttons)
        {
            if (availableAbilities.Count == 0)
            {
                button.BecomeBlank();
                continue;
            }

            int randIndex = Random.Range(0, availableAbilities.Count);
            Ability randomAbility = availableAbilities[randIndex];
            button.ShowAbility(randomAbility);
            if (randomAbility.maxRepeats > 0)
            {
                int count = 0;
                foreach (Ability ability in appliedAbilities)
                {
                    if (ability.name == randomAbility.name)
                        count += 1;
                }
                button.nameText.text = button.nameText.text + (count + 1);
            }
            button.gameObject.SetActive(true);
            chosenAbilities.Add(randomAbility);
            availableAbilities.RemoveAt(randIndex);
        }

        CheckHasPoints();
    }

    private void PrintAbilityOptions()
    {
        string available = "Available Abilities for " + targetWeapon.name + " (" + availableAbilities.Count + "): ";
        foreach (Ability ability in availableAbilities)
        {
            available += "\n" + ability.name;
        }
        print(available);
    }

    private void CheckHasPoints()
    {
        bool hasPoints = targetWeapon.UpgradePoints > 0;
        foreach (AbilityUI button in buttons)
        {
            button.button.interactable = hasPoints;
        }
        
        if (hasPoints == false)
            FinishUpgrading();
    }

    public void FinishUpgrading()
    {
        BattleController.BecomeReady(this);
    }

    public void SelectButton(int buttonIndex)
    {
        //Get Ability
        Ability chosenAbility = chosenAbilities[buttonIndex];

        //Remove an upgrade point from the weapon
        if (targetWeapon.UseUpgradePoint() == false)
            return;

        ApplyAbility(chosenAbility);

        ChooseOptions();
    }

    public void ApplyAbility(Ability chosenAbility)
    {
        //Apply effects of this ability to weapon
        foreach (AbilityEffect effect in chosenAbility.effects)
        {
            effect.Apply(targetWeapon, chosenAbility.name);
        }

        //Add ability to the list of applied abilities (for checking exlusions / prereqs / repeats)
        appliedAbilities.Add(chosenAbility);

        //If the ability doesn't require its type to already be on the weapon, add its type to the weapon.
        if (chosenAbility.requireType == false && chosenAbility.damageType != DamageType.none)
        {
            //Add to existing damage types flag
            targetWeapon.existingDamageTypes = targetWeapon.existingDamageTypes.PlusType(chosenAbility.damageType);

            //Add types incompatible with this damage type to incompatible types flag
            foreach (DamageType type in System.Enum.GetValues(typeof(DamageType)))
            {
                if (StaticRefs.DamageTypesAreCompatible(type, chosenAbility.damageType) == false)
                {
                    targetWeapon.incompatibleDamageTypes = targetWeapon.incompatibleDamageTypes.PlusType(type);
                }
            }
        }
    }
}
