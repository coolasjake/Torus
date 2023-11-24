using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityGroupData", menuName = "ScriptableObjects/AbilityGroupData", order = 1)]
public class AbilityGroupData : ScriptableObject
{
    public WeaponType targetType;
    public List<string> preReqAbilities = new List<string>();
    public List<string> incompatibleAbilities = new List<string>();
    public List<Ability> abilities = new List<Ability>();
}
