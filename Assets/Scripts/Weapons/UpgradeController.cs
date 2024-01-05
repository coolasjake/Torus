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

    //Groups lists are shown in inspector for debugging only
    [SerializeField]
    private List<Ability> possibleAbilities = new List<Ability>();
    [SerializeField]
    private List<Ability> availableAbilities = new List<Ability>();
    [SerializeField]
    private List<Ability> chosenAbilities = new List<Ability>();

    private List<Ability> appliedAbilities = new List<Ability>();

    private bool _initialized = false;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void GetAbilities()
    {
        Ability[] allGroupData = Resources.LoadAll<Ability>(abilityDataFolder);
        if (allGroupData.Length == 0)
            Debug.LogError("No abilities found at path: " + abilityDataFolder);

        //Calculate incompatible types based on existing types on weapon (mostly useful for lazy testers)
        foreach (DamageType existingType in System.Enum.GetValues(typeof(DamageType)))
        {
            if (targetWeapon.existingDamageTypes.Includes(existingType))
            {
                foreach (DamageType otherType in System.Enum.GetValues(typeof(DamageType)))
                {
                    if (StaticRefs.DamageTypesAreCompatible(existingType, otherType) == false)
                    {
                        targetWeapon.incompatibleDamageTypes = targetWeapon.incompatibleDamageTypes.PlusType(otherType);
                    }
                }
            }
        }

        foreach (Ability ability in allGroupData)
        {
            //If the ability is for this weapon, and doesn't have an incompatible type, it belongs in either the possible or available list
            if (ability.allowedWeapons.Includes(targetWeapon.Type()) && targetWeapon.incompatibleDamageTypes.Includes(ability.damageType) == false)
            {
                //Add to available abilities if it has no prerequisites, and is either of a damage type that the weapon has or is allowed to unlock its damage type
                if (ability.preReqAbilities.Count == 0 && (targetWeapon.existingDamageTypes.Includes(ability.damageType) || ability.requireType == false))
                    availableAbilities.Add(ability);
                else
                    possibleAbilities.Add(ability);
            }
        }
    }

    public void StartUpgrading()
    {
        if (_initialized == false)
            Initialize();

        Show();
        GiveTestPoints();
        ChooseOptions();
    }

    private void Initialize()
    {
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
        GetAbilities();
        _initialized = true;
    }

    private void GiveTestPoints()
    {
        targetWeapon.LevelUp();
    }

    private void ChooseOptions()
    {
        foreach (Ability ability in chosenAbilities)
        {
            availableAbilities.Add(ability);
        }

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
            button.gameObject.SetActive(true);
            chosenAbilities.Add(randomAbility);
            availableAbilities.RemoveAt(randIndex);
        }

        CheckHasPoints();
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
        //Get group
        Ability chosenAbility = chosenAbilities[buttonIndex];

        //Remove an upgrade point from the weapon
        if (targetWeapon.UseUpgradePoint() == false)
            return;

        //Add remaining chosen groups (groups that were a button option) back to the pool
        chosenAbilities.RemoveAt(buttonIndex);
        foreach (Ability ability in chosenAbilities)
        {
            availableAbilities.Add(ability);
        }
        chosenAbilities.Clear();

        ApplyAbility(chosenAbility);

        ChooseOptions();
    }

    public void ApplyAbility(Ability chosenAbility)
    {
        //Apply effects of next ability in group to weapon
        foreach (AbilityEffect effect in chosenAbility.effects)
        {
            effect.Apply(targetWeapon, chosenAbility.name);
        }

        //Add ability to the list of applied abilities (for checking exlusions / prereqs / repeats)
        appliedAbilities.Add(chosenAbility);

        if (chosenAbility.requireType == false)
        {
            //Add to existing damage types flag
            targetWeapon.existingDamageTypes = targetWeapon.existingDamageTypes.PlusType(chosenAbility.damageType);

            //Add incompatible types to incompatible types flag
            foreach (DamageType type in System.Enum.GetValues(typeof(DamageType)))
            {
                if (StaticRefs.DamageTypesAreCompatible(type, chosenAbility.damageType) == false)
                {
                    targetWeapon.incompatibleDamageTypes = targetWeapon.incompatibleDamageTypes.PlusType(type);
                }
            }

            //Remove abilities that are now impossible due to damage type
            for (int i = 0; i < possibleAbilities.Count; ++i)
            {
                if (targetWeapon.incompatibleDamageTypes.Includes(possibleAbilities[i].damageType))
                    possibleAbilities.RemoveAt(i--);
            }
            for (int i = 0; i < availableAbilities.Count; ++i)
            {
                if (targetWeapon.incompatibleDamageTypes.Includes(availableAbilities[i].damageType))
                    availableAbilities.RemoveAt(i--);
            }
        }

        //Remove groups that are incompatible with this ability from the available pool
        for (int i = 0; i < availableAbilities.Count; ++i)
        {
            if (availableAbilities[i].incompatibleAbilities.Contains(chosenAbility.name))
                availableAbilities.RemoveAt(i--);
        }

        //Add groups that are now valid to the available pool, and remove incompatible abilites
        for (int i = 0; i < possibleAbilities.Count; ++i)
        {
            if (possibleAbilities[i].incompatibleAbilities.Contains(chosenAbility.name))
            {
                possibleAbilities.RemoveAt(i--);
                continue;
            }

            //If a possible ability now has all prerequisites and the weapon has it's type unlocked (or it doesn't require the type to be unlocked), add it to the available list
            if (possibleAbilities[i].requireType == false || targetWeapon.existingDamageTypes.Includes(possibleAbilities[i].damageType))
            {
                bool allPrerequisitesUnlocked = true;
                foreach (string preReqName in possibleAbilities[i].preReqAbilities)
                {
                    if (appliedAbilities.Find(X => X.name == preReqName) == false)
                    {
                        allPrerequisitesUnlocked = false;
                        break;
                    }
                    if (allPrerequisitesUnlocked)
                    {
                        availableAbilities.Add(possibleAbilities[i]);
                        possibleAbilities.RemoveAt(i--);
                    }
                }
            }
        }
    }
}
