/*using System.Collections.Generic;
using TerraformingTerrain2d;
using UnityEngine;

public class WeaponController : MonoBehaviour, IWeaponUser
{
    public WeaponSO currentWeapon;
    public List<TerraformingTerrain2D> terrains; // הרשימה של חפירה בחול
    public GameObject projectilePrefab;

    private AmmoManager ammoManager;
    private ExplosionHandler explosionHandler;

    private void Start()
    {
        // אתחול מחלקות פונקציונליות
        ammoManager = new AmmoManager(currentWeapon);
        explosionHandler = new ExplosionHandler(currentWeapon, terrains);
    }

    private void Update()
    {
        // בדיקה אם השחקן יורה
        if (Input.GetButtonDown("Fire1"))
        {
            FireWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ammoManager.Reload();
        }
    }
    public void FireWeapon()
    {
        if (ammoManager.CanFire())
        {
            SpawnProjectile();
            

            // הפעלת אפקטים

            // שימוש בתחמושת
            ammoManager.UseAmmo();
        }
    }

    public void SwitchWeapon(WeaponSO newWeapon)
    {
        currentWeapon = newWeapon;

        // עדכון מחלקות
        ammoManager.SetWeapon(newWeapon);
        explosionHandler.SetWeapon(newWeapon);
    }


    private void SpawnProjectile()
    {
        if (projectilePrefab != null)
        {
            Vector2 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projInstance = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.Euler(0, 0, angle)
            );
        }
    }
}
*/