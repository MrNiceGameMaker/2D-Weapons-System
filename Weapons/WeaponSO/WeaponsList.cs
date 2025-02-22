using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsList", menuName = "Weapons/WeaponsList")]
public class WeaponsList : ScriptableObject
{
    public List<WeaponSO> weaponsList;
}
