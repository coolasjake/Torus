using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Image border;
    public Image levelIcon;
    public Button button;

    public void ShowAbility(Ability data, int level)
    {
        nameText.text = data.name;
        descriptionText.text = data.description;
        levelIcon.sprite = StaticRefs.UpgradeLvlIcon(level);
        //Rarity code here <---
        gameObject.SetActive(true);
    }
    public void BecomeBlank()
    {
        nameText.text = "N/A";
        descriptionText.text = "";
        levelIcon.sprite = StaticRefs.UpgradeLvlIcon(0);
        //Rarity code here <---
        gameObject.SetActive(false);
    }
}