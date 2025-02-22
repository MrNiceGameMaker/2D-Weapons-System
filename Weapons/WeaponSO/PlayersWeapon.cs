using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "PlayersWeapon", menuName = "Weapons/PlayersWeapon")]
public class PlayersWeapon : ScriptableObject
{
    public List<Weapons> playerWeapons = new List<Weapons>(); // רשימת שחקנים עם הנשקים שלהם

    public static event Action<int, WeaponSO> OnWeaponChanged; // אירוע שינוי נשק

    public WeaponSO GetCurrentWeapon(int playerId)
    {
        if (playerId < 0 || playerId >= playerWeapons.Count || playerWeapons[playerId].weapons.Count == 0)
            return null;

        return playerWeapons[playerId].weapons[playerWeapons[playerId].currentWeaponIndex];
    }

    public void SwitchWeapon(int playerId, int direction)
    {
        if (playerId < 0 || playerId >= playerWeapons.Count || playerWeapons[playerId].weapons.Count == 0) return;

        Weapons player = playerWeapons[playerId];

        // עדכון אינדקס הנשק
        player.currentWeaponIndex += direction;

        // אם עברנו את הרשימה, נחזור להתחלה או לסוף
        if (player.currentWeaponIndex >= player.weapons.Count)
            player.currentWeaponIndex = 0;
        if (player.currentWeaponIndex < 0)
            player.currentWeaponIndex = player.weapons.Count - 1;

        // שליחת אירוע שינוי נשק
        OnWeaponChanged?.Invoke(playerId, player.weapons[player.currentWeaponIndex]);
    }

    public void AddWeapon(int playerId, WeaponSO newWeapon)
    {
        if (playerId < 0 || playerId >= playerWeapons.Count) return;
        if (!playerWeapons[playerId].weapons.Contains(newWeapon))
        {
            playerWeapons[playerId].weapons.Add(newWeapon);
        }
    }
}

[System.Serializable]
public class Weapons
{
    public List<WeaponSO> weapons = new List<WeaponSO>(); // רשימת הנשקים של כל שחקן
    public int currentWeaponIndex = 0; // הנשק הנוכחי של השחקן
}
