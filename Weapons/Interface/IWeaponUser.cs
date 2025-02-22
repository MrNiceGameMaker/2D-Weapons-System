using UnityEngine;

public interface IWeaponUser
{
    void FireWeapon();
    void SwitchWeapon(WeaponSO newWeapon);
}
