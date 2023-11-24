using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeController : MonoBehaviour
{
    public Weapon targetWeapon;
    public string abilityDataFolder = "/AbilityData";

    public TMP_Text title;

    public List<AbilityUI> buttons = new List<AbilityUI>();

    //Groups lists are shown in inspector for debugging only
    [SerializeField]
    private List<AbilityGroup> possibleAbilityGroups = new List<AbilityGroup>();
    [SerializeField]
    private List<AbilityGroup> validGroupPool = new List<AbilityGroup>();
    [SerializeField]
    private List<AbilityGroup> chosenGroups = new List<AbilityGroup>();

    void Awake()
    {
        title.text = targetWeapon.Type().ToString() + " Upgrades";
        GetAbilityGroups();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void GetAbilityGroups()
    {
        AbilityGroupData[] allGroupData = Resources.LoadAll<AbilityGroupData>(abilityDataFolder);
        object[] test = Resources.LoadAll(abilityDataFolder);
        if (allGroupData.Length == 0)
            Debug.LogError("No abilities found at path: " + abilityDataFolder);
        foreach (AbilityGroupData groupData in allGroupData)
        {
            if (groupData.targetType == WeaponType.Any || groupData.targetType == targetWeapon.Type())
            {
                possibleAbilityGroups.Add(new AbilityGroup(groupData));
            }
        }

        for (int i = 0; i < possibleAbilityGroups.Count; ++i)
        {
            if (possibleAbilityGroups[i].data.preReqAbilities.Count == 0)
            {
                validGroupPool.Add(possibleAbilityGroups[i]);
                possibleAbilityGroups.RemoveAt(i--);
            }
        }
    }

    public void StartUpgrading()
    {
        Show();
        GiveTestPoints();
        ChooseOptions();
    }

    private void GiveTestPoints()
    {
        targetWeapon.GainExperience(targetWeapon.ExperienceNeeded);
    }

    private void ChooseOptions()
    {
        foreach (AbilityGroup group in chosenGroups)
        {
            validGroupPool.Add(group);
        }

        chosenGroups.Clear();

        foreach (AbilityUI button in buttons)
        {
            int randIndex = Random.Range(0, validGroupPool.Count);
            AbilityGroup randomGroup = validGroupPool[randIndex];
            if (randomGroup.nextAbility == 0)
            {
                randIndex = Random.Range(0, validGroupPool.Count);
                randomGroup = validGroupPool[randIndex];
            }
            button.ShowAbility(randomGroup.data.abilities[randomGroup.nextAbility]);
            button.gameObject.SetActive(true);
            button.levelIcon.sprite = StaticRefs.UpgradeLvlIcon(randomGroup.nextAbility);
            chosenGroups.Add(randomGroup);
            validGroupPool.RemoveAt(randIndex);
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
        AbilityGroup chosenGroup = chosenGroups[buttonIndex];

        //Remove an upgrade point from the weapon
        if (targetWeapon.UseUpgradePoint() == false)
            return;

        //Apply effects of next ability in group to weapon
        Ability nextAbility = chosenGroup.data.abilities[chosenGroup.nextAbility];
        foreach (AbilityEffect effect in nextAbility.effects)
        {
            effect.Apply(targetWeapon, nextAbility.name);
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

        //Increment the ability index, and remove this group if it was the last ability in it
        if (++chosenGroup.nextAbility >= chosenGroup.data.abilities.Count)
        {
            chosenGroups.RemoveAt(buttonIndex);
        }

        //Add remaining chosen groups (groups that were a button option) back to the pool
        foreach (AbilityGroup group in chosenGroups)
        {
            validGroupPool.Add(group);
        }
        chosenGroups.Clear();

        //Remove groups that are incompatible with this ability from the pool
        for (int i = 0; i < validGroupPool.Count; ++i)
        {
            if (validGroupPool[i].data.incompatibleAbilities.Contains(abilityName))
                validGroupPool.RemoveAt(i--);
        }

        ChooseOptions();
    }

    [System.Serializable]
    public class AbilityGroup
    {
        public AbilityGroup(AbilityGroupData Data)
        {
            data = Data;
            name = data.name;
            nextAbility = 0;
        }

        public string name = "";

        public AbilityGroupData data;
        public int nextAbility = 0;
    }
}
