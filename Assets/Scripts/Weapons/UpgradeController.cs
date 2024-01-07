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

    //Ability lists are shown in inspector for debugging only
    [SerializeField]
    private List<Ability> possibleAbilities = new List<Ability>();
    [SerializeField]
    private List<Ability> availableAbilities = new List<Ability>();
    [SerializeField]
    private List<Ability> chosenAbilities = new List<Ability>();

    private List<Ability> appliedAbilities = new List<Ability>();
    private List<Ability> impossibleAbilities = new List<Ability>();

    private Ability StoreAsImpossible(Ability ability)
    {
        impossibleAbilities.Add(ability);
        print(ability.name + " is now impossible.");
        return ability;
    }

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
        Ability[] allAbilities = Resources.LoadAll<Ability>(abilityDataFolder);
        if (allAbilities.Length == 0)
            Debug.LogError("No abilities found at path: " + abilityDataFolder);

        string abilities = "All Abilities (" + allAbilities.Length + "): ";
        foreach (Ability ability in allAbilities)
        {
            abilities += "\n" + ability.name;
        }
        print(abilities);

        foreach (Ability ability in allAbilities)
        {
            //If the ability is for this weapon, and doesn't have an incompatible type, it belongs in either the possible or available list
            if (ability.allowedWeapons.Includes(targetWeapon.Type()) && targetWeapon.incompatibleDamageTypes.Includes(ability.damageType) == false)
            {
                //Add to available abilities if it has no prerequisites, and is either of a damage type that the weapon has or is allowed to unlock its damage type
                if (ability.preReqAbilities.Count == 0 && (targetWeapon.existingDamageTypes.Includes(ability.damageType) || ability.requireType == false) && ability.minWave <= 1)
                    availableAbilities.Add(ability);
                else
                    possibleAbilities.Add(ability);
            }
        }
        PrintAbilityOptions();
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
        GetAbilities();

        //Must be after get abilities or default type abilities will not be removed when applied to weapons
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
        foreach (Ability ability in chosenAbilities)
        {
            availableAbilities.Add(ability);
        }

        chosenAbilities.Clear();
        PrintAbilityOptions();

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
        string possible = "Possible Abilities for " + targetWeapon.name + " (" + possibleAbilities.Count + "): ";
        foreach (Ability ability in possibleAbilities)
        {
            possible += "\n" + ability.name;
        }
        print(possible);
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

        //Add remaining chosen abilities (abilities that were a button option) back to the pool
        //chosenAbilities.RemoveAt(buttonIndex);
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
        //Apply effects of this ability to weapon
        foreach (AbilityEffect effect in chosenAbility.effects)
        {
            effect.Apply(targetWeapon, chosenAbility.name);
        }

        //Add ability to the list of applied abilities (for checking exlusions / prereqs / repeats)
        appliedAbilities.Add(chosenAbility);

        //If the ability has been repeated less than its max repeats, add it back to the pool, otherwise remove it
        if (chosenAbility.maxRepeats > 0)
        {
            int count = 0;
            foreach (Ability ability in appliedAbilities)
            {
                if (ability.name == chosenAbility.name)
                    count += 1;
            }
            if (count > chosenAbility.maxRepeats)
            {
                availableAbilities.Remove(chosenAbility);
                possibleAbilities.Remove(chosenAbility);
            }
        }
        else
        {
            availableAbilities.Remove(chosenAbility);
            possibleAbilities.Remove(chosenAbility);
        }

        if (chosenAbility.requireType == false && chosenAbility.damageType != DamageType.none)
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

        //Add abilities that are now valid to the available pool, and remove impossible abilites
        for (int i = 0; i < possibleAbilities.Count; ++i)
        {
            if (possibleAbilities[i].incompatibleAbilities.Contains(chosenAbility.name) || BattleController.WaveNumber > possibleAbilities[i].maxWave)
            {
                possibleAbilities.RemoveAt(i--);
                continue;
            }

            //If a possible ability now has all prerequisites and the weapon has it's type unlocked (or it doesn't require the type to be unlocked), add it to the available list
            if ((possibleAbilities[i].requireType == false || targetWeapon.existingDamageTypes.Includes(possibleAbilities[i].damageType))
                && BattleController.WaveNumber >= possibleAbilities[i].minWave)
            {
                bool allPrerequisitesUnlocked = true;
                foreach (string preReqName in possibleAbilities[i].preReqAbilities)
                {
                    if (appliedAbilities.Find(X => X.name == preReqName) == false)
                    {
                        allPrerequisitesUnlocked = false;
                        break;
                    }
                }
                if (allPrerequisitesUnlocked)
                {
                    availableAbilities.Add(possibleAbilities[i]);
                    possibleAbilities.RemoveAt(i--);
                }
                else
                    print(possibleAbilities[i].name + "not possible because of pre-reqs.");
            }
            else
                print(possibleAbilities[i].name + "not possible because of type (" + possibleAbilities[i].damageType + ") or wave number.");
        }

        //Remove abilities that are incompatible with this ability from the available pool
        for (int i = 0; i < availableAbilities.Count; ++i)
        {
            if (availableAbilities[i].incompatibleAbilities.Contains(chosenAbility.name))
                availableAbilities.RemoveAt(i--);
        }
    }
}
