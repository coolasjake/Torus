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