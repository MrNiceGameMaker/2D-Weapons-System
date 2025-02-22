using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AmmoManager
{
    public UnityEvent<bool> OnCanShootChanged = new UnityEvent<bool>(); // אירוע שינוי ירי
    public static event Action<int, int> OnAmmoChanged; // מעדכן את ה-UI על התחמושת
    public static event Action<float> OnReloadingStarted; // מעדכן על טעינה

    private WeaponSO weaponData;
    private MonoBehaviour coroutineRunner;
    private ProjectileManager projectileManager;
    private Transform firePoint; // נקודת הירי

    private int currentAmmo;
    private bool isReloading;
    private bool isShootingBurst = false;
    private bool canShoot = false;

    public AmmoManager(WeaponSO weaponData, MonoBehaviour runner, ProjectileManager projectileManager, Transform firePoint)
    {
        this.weaponData = weaponData;
        this.coroutineRunner = runner;
        this.projectileManager = projectileManager;
        this.firePoint = firePoint;

        if (this.projectileManager == null)
            Debug.LogError("AmmoManager: ProjectileManager is STILL null after assignment!");

        if (this.firePoint == null)
            Debug.LogError("AmmoManager: FirePoint is STILL null after assignment!");

        currentAmmo = weaponData.magazineSize;
        isReloading = false;

        // מאזין להחלפת נשק
        PlayersWeapon.OnWeaponChanged += UpdateWeaponData;

        OnAmmoChanged?.Invoke(currentAmmo, weaponData.magazineSize);
    }

    private void UpdateWeaponData(int playerId, WeaponSO newWeapon)
    {
        weaponData = newWeapon;
        currentAmmo = weaponData.magazineSize;
        isReloading = false;

        // שולח עדכון ל-UI
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.magazineSize);
    }

    public bool CanShoot()
    {
        return !isReloading && currentAmmo > 0;
    }

    public void Shoot()
    {
        if (!CanShoot()) return; // אם אי אפשר לירות, לא מבצע כלום

        currentAmmo--; // גורע ירייה אחת מהמחסנית
        OnCanShootChanged.Invoke(false); // מונע ירי כפול בזמן ההמתנה
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.magazineSize);

        coroutineRunner.StartCoroutine(Fire()); // מפעיל מגבלת ירי לפי fireRate

        if (currentAmmo <= 0)
        {
            Reload();
        }
    }

    private IEnumerator Fire()
    {
        yield return new WaitForSeconds(weaponData.fireRate);
        OnCanShootChanged.Invoke(true); // מחזיר את היכולת לירות אחרי fireRate
    }

    public void Reload()
    {
        if (isReloading) return;
        coroutineRunner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadingStarted?.Invoke(weaponData.magazineReloadTime);

        float elapsedTime = 0f;
        while (elapsedTime < weaponData.magazineReloadTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentAmmo = weaponData.magazineSize;
        isReloading = false;
        OnCanShootChanged.Invoke(true); // מאפשר ירי אחרי טעינה
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.magazineSize);
    }
}
