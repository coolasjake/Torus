using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Image border;
    public Image levelIcon;
    public Button button;

    [HideInInspector]
    public int groupIndex = -1;

    public void ShowAbility(Ability data, int index)
    {
        nameText.text = data.name;
        descriptionText.text = data.description;
        //Rarity code here <---

        groupIndex = index;
        gameObject.SetActive(true);
    }
}

public class UpgradeController : MonoBehaviour
{
    public Weapon targetWeapon;
    public string abilityDataFolder = "/AbiliyData";

    public List<AbilityUI> buttons = new List<AbilityUI>();

    public List<AbilityGroup> possibleAbilityGroups = new List<AbilityGroup>();
    public List<AbilityGroup> validGroupPool = new List<AbilityGroup>();

    void Awake()
    {
        GetAbilityGroups();
    }

    
    private void GetAbilityGroups()
    {
        AbilityGroupData[] allGroupData = Resources.LoadAll<AbilityGroupData>(abilityDataFolder + "/");
        foreach (AbilityGroupData groupData in allGroupData)
        {
            if (groupData.targetType == WeaponType.Any || groupData.targetType == targetWeapon.Type())
            {
                possibleAbilityGroups.Add(new AbilityGroup(groupData));
            }
        }
    }

    public void StartUpgrading()
    {
        ChooseOptions();
    }

    private void ChooseOptions()
    {
        foreach(AbilityUI button in buttons)
        {
            int randIndex = Random.Range(0, validGroupPool.Count);
            AbilityGroup randomGroup = validGroupPool[randIndex];
            if (randomGroup.nextAbility == 0)
            {
                randIndex = Random.Range(0, validGroupPool.Count);
                randomGroup = validGroupPool[randIndex];
            }
            button.ShowAbility(randomGroup.data.abilities[randomGroup.nextAbility], randIndex);
            button.gameObject.SetActive(true);
            button.levelIcon.sprite = StaticRefs.UpgradeLvlIcon(randomGroup.nextAbility + 1);
        }

        CheckHasPoints();
    }

    private void CheckHasPoints()
    {
        bool hasPoints = targetWeapon.upgradePoints > 0;
        foreach (AbilityUI button in buttons)
        {
            button.button.interactable = hasPoints;
        }
    }

    public void BuyAbility(int buttonIndex)
    {
        //Get group from button
        int groupIndex = buttons[buttonIndex].groupIndex;
        AbilityGroup chosenGroup = validGroupPool[groupIndex];

        //Remove an upgrade point from the weapon
        targetWeapon.upgradePoints -= 1;

        //Apply effects of next ability in group to weapon
        foreach (AbilityEffect effect in chosenGroup.data.abilities[chosenGroup.nextAbility].effects)
        {
            effect.Apply(targetWeapon);
        }

        string abilityName = chosenGroup.data.abilities[chosenGroup.nextAbility].name;

        //Add groups that require this ability to the pool
        for (int i = 0; i < possibleAbilityGroups.Count; ++i)
        {
            if (possibleAbilityGroups[i].data.preReqAbilities.Contains(abilityName))
            {
                validGroupPool.Add(possibleAbilityGroups[i]);
                possibleAbilityGroups.RemoveAt(i--);
            }
        }

        //Remove groups that are incompatible with this ability from the pool
        for (int i = 0; i < validGroupPool.Count; ++i)
        {
            if (validGroupPool[i].data.incompatibleAbilities.Contains(abilityName))
                validGroupPool.RemoveAt(i--);
        }

        //Increment the ability index, and remove this group if it was the last ability in it
        if (++chosenGroup.nextAbility >= chosenGroup.data.abilities.Count)
        {
            validGroupPool.RemoveAt(groupIndex);
        }

        ChooseOptions();
    }

    [System.Serializable]
    public class AbilityGroup
    {
        public AbilityGroup(AbilityGroupData Data)
        {
            data = Data;
            nextAbility = 0;
        }

        public AbilityGroupData data;
        public int nextAbility = 0;
    }
}

public class AbilityGroupData : ScriptableObject
{
    public WeaponType targetType;
    public List<string> preReqAbilities = new List<string>();
    public List<string> incompatibleAbilities = new List<string>();
    public List<Ability> abilities = new List<Ability>();
}

public class Ability : ScriptableObject
{
    //public string name;
    public string description;
    public List<AbilityEffect> effects;
}

[System.Serializable]
public class AbilityEffect
{
    [Tooltip("Larger numbers will be applied later, and override effects of lower numbers")]
    public string name = "";
    public int priority = 0;
    public bool isPower = false;
    public string statName = "";
    public StatChangeOperation operation;
    public float change = 1f;

    public void Apply(Weapon weapon)
    {
        if (isPower)
            weapon.UnlockPower(statName, (int)change);
        else
            weapon.AddModifier(statName, name, operation, change);
    }
}

public enum StatChangeOperation
{
    Add,
    Multiply,
    Set
}