using UnityEngine;

public interface IProjectileManager
{
    void SpawnProjectile(Vector2 position, Quaternion rotation, Vector2 shooterMomentum, WeaponSO randomWeapon, SlowMotionEventSO slowMotionEvents); // יצירת קליע חדש
    void StartProjectileLifetime(GameObject projectile, WeaponSO weaponData); // ניהול חיי הקליע
}
