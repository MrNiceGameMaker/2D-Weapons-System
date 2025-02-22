using System.Collections;
using UnityEngine;

public class ExplosionManager : IExplosionManager
{
    private WeaponSO weaponData;
    private MonoBehaviour coroutineRunner; // רפרנס לניהול Coroutine
    private EnvironmentalManager environmentalManager; // ניהול השפעה סביבתית
    private ProjectileManager projectileManager;
    private GravityManager gravityManager;
    public SlowMotionEventSO slowMotionEvents;
    public ExplosionManager(WeaponSO weaponData, MonoBehaviour runner, EnvironmentalManager envManager, ProjectileManager projManager, SlowMotionEventSO slowMotionEvents)
    {
        this.weaponData = weaponData;
        this.coroutineRunner = runner;
        this.environmentalManager = envManager;
        this.projectileManager = projManager;
        this.slowMotionEvents = slowMotionEvents; // שמירת ה-SO
    }
    private void SpawnProjectilesOnExplosion(Vector2 position, Vector2 explosionDirection)
    {
        if (!weaponData.hasProjectiles || projectileManager == null) return;

        int numProjectiles = weaponData.hasRandomAmountOfProjectiles
            ? Random.Range(weaponData.minimumAmount, weaponData.maximumAmount + 1)
            : weaponData.amountOfProjectiles;

        for (int i = 0; i < numProjectiles; i++)
        {
            if (weaponData.projectilesAttributes == null || weaponData.projectilesAttributes.weaponsList.Count == 0) continue;

            WeaponSO selectedWeapon = weaponData.projectilesAttributes.weaponsList[
                Random.Range(0, weaponData.projectilesAttributes.weaponsList.Count)
            ];

            float spreadAngle = Random.Range(weaponData.projectilesSpread.minValue, weaponData.projectilesSpread.maxValue);
            Quaternion rotation = Quaternion.Euler(0, 0, spreadAngle) * Quaternion.LookRotation(Vector3.forward, explosionDirection);
            projectileManager.SpawnProjectile(position, rotation, explosionDirection, selectedWeapon, slowMotionEvents);
        }
    }




    public void TriggerExplosion(Vector2 position, Vector2 explosionDirection)
    {
        slowMotionEvents.OnExplosionTriggered.Invoke();
        if (!weaponData.isExploding) return;

        if (weaponData.explosionEffect != null)
        {
            GameObject.Instantiate(weaponData.explosionEffect, position, Quaternion.identity);
        }

        DealExplosionDamage(position);

        if (environmentalManager != null && weaponData.isExplosionDestroyingSand)
        {
            environmentalManager.DigInTerrain(position, true);
        }

        if (environmentalManager != null && weaponData.isExplosionMakingNewSand)
        {
            environmentalManager.CreateNewSand(position);
        }

        if (weaponData.hasShockwave)
        {
            coroutineRunner.StartCoroutine(TriggerShockwave(position));
        }

        if (weaponData.hasProjectiles)
        {
            SpawnProjectilesOnExplosion(position, explosionDirection);
        }
        coroutineRunner.StartCoroutine(EndExplosion());
    }
    private IEnumerator EndExplosion()
    {
        yield return new WaitForSeconds(0.5f); // זמן לדעיכת הפיצוץ
        slowMotionEvents.OnExplosionEnded.Invoke();
    }
    private void DealExplosionDamage(Vector2 position)
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(position, weaponData.explosionRadius);


        foreach (Collider2D hit in hitObjects)
        {
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                float distance = Vector2.Distance(position, hit.transform.position);
                float damageFactor = Mathf.Clamp01(1 - (distance / weaponData.explosionRadius));

                int finalDamage = Mathf.RoundToInt(weaponData.explosionDamage * damageFactor);
                if (finalDamage > 0)
                {
                    damageable.TakeDamage(finalDamage);
                }

                // בדיקה אם יש נזק מתמשך
                if (weaponData.hasDamageOverTime)
                {
                    damageable.TakeDamageOverTime(weaponData.damageOverTime, weaponData.totalDuration, weaponData.tickRate);
                }
            }

        }
    }


    private IEnumerator TriggerShockwave(Vector2 position)
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(position, weaponData.shockwaveSize);
        foreach (Collider2D hit in hitObjects)
        {
            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 forceDirection = (rb.position - position).normalized * weaponData.shockwaveStrength;

                PlayerMovement player = hit.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.ApplyShockwaveForce(forceDirection); // משתמש בפונקציה החדשה של השחקן
                }
                else
                {
                    rb.AddForce(forceDirection, ForceMode2D.Impulse); // אובייקטים אחרים זזים כרגיל
                }
            }
        }
        yield return null;
    }

}
