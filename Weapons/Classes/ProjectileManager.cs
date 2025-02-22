using ExplosionGame;
using System.Collections;
using UnityEngine;

public class ProjectileManager : IProjectileManager
{
    private WeaponSO currentWeapon;
    private MonoBehaviour coroutineRunner; // ????? ?????? Coroutine
    private Transform firePoint; // ????? ???? ?? ????
    private SOVector2 playerMomentumSO; // מומנטום השחקן
    Rigidbody2D rb;
    private TerrainListSO terrainListSO;
    private EnvironmentalManager environmentalManager;
    private GravityManager gravityManager;
    [SerializeField] private SlowMotionEventSO slowMotionEvents;
    public ProjectileManager(WeaponSO weaponData, MonoBehaviour runner, Transform firePoint, SOVector2 playerMomentumSO, TerrainListSO terrainListSO, SlowMotionEventSO slowMotionEvents)
    {
        this.currentWeapon = weaponData;
        this.coroutineRunner = runner;
        this.firePoint = firePoint;
        this.playerMomentumSO = playerMomentumSO;
        this.terrainListSO = terrainListSO;
        this.environmentalManager = new EnvironmentalManager(weaponData, terrainListSO);
        this.slowMotionEvents = slowMotionEvents; // נוסיף אותו לאובייקט הגלובלי של המנהל
    }

    public void SpawnProjectile(Vector2 position, Quaternion rotation, Vector2 shooterMomentum, WeaponSO weapon, SlowMotionEventSO slowMotionEvents)
    {
        if (weapon.projectilePrefab == null)
        {
            Debug.LogError($"Projectile Prefab is missing in {weapon.weaponName}! Please assign it in WeaponSO.");
            return;
        }

        GameObject projectile = GameObject.Instantiate(weapon.projectilePrefab, position, rotation);
        currentWeapon = weapon;
        rb = InitializeProjectileComponents(projectile, weapon);
        SetProjectileAppearance(projectile, weapon);

        if (rb == null)
        {
            Debug.LogError($"Projectile {weapon.weaponName} is missing Rigidbody2D after initialization!");
            return;
        }

        // חישוב כיוון הירייה
        Vector2 direction = rotation * Vector2.right;

        // קביעת מהירות התחלתית מהנשק הנבחר
        float initialVelocity = weapon.accelerationRate != 0 ?
                                (weapon.accelerationRate > 0 ? weapon.minSpeed : weapon.maxSpeed)
                                : weapon.maxSpeed;

        // חישוב מומנטום השחקן אם `affectedByPlayerMomentum` מופעל
        Vector2 playerMomentum = (weapon.affectedByPlayerMomentum && playerMomentumSO != null)
                                 ? playerMomentumSO.value * 100
                                 : Vector2.zero;

        // קביעת מהירות הירייה כולל מומנטום השחקן
        rb.linearVelocity = direction * initialVelocity + playerMomentum;

        // אתחול מנהלי הפיצוץ, הסביבה וכוח המשיכה
        EnvironmentalManager envManager = new EnvironmentalManager(weapon, terrainListSO);
        ExplosionManager explosionManager = new ExplosionManager(weapon, coroutineRunner, envManager, this, slowMotionEvents);
        GravityManager gravityManager = new GravityManager(weapon, coroutineRunner);

        // הוספת Projectile וטעינת ההגדרות
        Projectile projectileScript = projectile.AddComponent<Projectile>();
        projectileScript.Initialize(weapon, explosionManager, envManager, gravityManager, slowMotionEvents);

        // עדכון כוח משיכה לפי הנתונים מה-SO
        rb.gravityScale = weapon.affectedByGravity ? 1f : 0f;

        // הפעלת כוח משיכה במהלך הטיסה אם מוגדר
        if (weapon.hasGravityDuringFlight)
        {
            gravityManager.ApplyGravityDuringFlight(rb);
        }

        // הפעלת תאוצה אם נדרש
        if (weapon.accelerationRate != 0)
        {
            coroutineRunner.StartCoroutine(ApplyAcceleration(rb, direction, playerMomentum, weapon, projectileScript));
        }

        // הפעלת תנועה אקראית במהלך הטיסה אם נדרש
        if (weapon.movementInFlightRange > 0)
        {
            coroutineRunner.StartCoroutine(ApplyFlightMovement(rb, weapon));
        }

        // יצירת פרויקטילים במהלך הטיסה אם נדרש
        if (weapon.hasProjectilesDuringFlight)
        {
            coroutineRunner.StartCoroutine(ApplyProjectilesDuringFlight(rb, weapon));
        }

        // הפעלת שדה כוח משיכה לאחר הפיצוץ אם מוגדר
        if (weapon.hasGravityDuringFlight)
        {
            gravityManager.ApplyGravityDuringFlight(rb);
        }

        // ניהול חיי הפרויקטיל
        coroutineRunner.StartCoroutine(HandleProjectileLifetime(projectile, weapon));
    }



    private Rigidbody2D InitializeProjectileComponents(GameObject projectile, WeaponSO weaponData)
    {
        SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = projectile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = weaponData.shotSprite ?? GenerateDefaultCircleSprite();
        }

        Collider2D collider = projectile.GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = projectile.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = !weaponData.hasPhysicalCollider; // אם אין קוליידר פיזי, זה יהיה Trigger

        // ✅ יצירת חומר פיזיקלי מותאם אישית אם הקפיצות מופעלות
        if (weaponData.bouncesOnContact)
        {
            PhysicsMaterial2D bounceMaterial = new PhysicsMaterial2D($"Bouncy_{weaponData.weaponName}")
            {
                bounciness = weaponData.bounceBounciness,
                friction = weaponData.bounceFriction
            };
            collider.sharedMaterial = bounceMaterial;
        }

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        return rb;
    }



    private IEnumerator ApplyFlightMovement(Rigidbody2D rb, WeaponSO weaponData)
    {
        while (rb != null && weaponData.movementInFlightRange > 0)
        {
            // תנועה רנדומלית ימינה/שמאלה
            float offset = Random.Range(-weaponData.movementInFlightRange, weaponData.movementInFlightRange);

            // הוספת סטייה אקראית לנתיב הירייה
            rb.linearVelocity += new Vector2(offset, 0);

            yield return new WaitForSeconds(0.2f); // שינוי כיוון כל 0.2 שניות
        }
    }

    private Sprite GenerateDefaultCircleSprite()
    {
        int size = 64; // גודל גדול יותר לרזולוציה טובה
        Texture2D texture = new Texture2D(size, size);

        // יצירת עיגול
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                if (distance < size / 2)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        // יצירת ספרייט עם פיקסלים ליחידה כדי שגודלו יהיה 1x1 במרחב
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }



    public void StartProjectileLifetime(GameObject projectile, WeaponSO weaponData)
    {
        coroutineRunner.StartCoroutine(HandleProjectileLifetime(projectile, weaponData));
    }
    private IEnumerator HandleProjectileLifetime(GameObject projectile, WeaponSO weapon)
    {
        float lifetime = weapon.explodesAfterTime > 0 ? weapon.explodesAfterTime : 60f;
        float traveledDistance = 0f;
        Vector2 startPosition = projectile != null ? projectile.transform.position : Vector2.zero;

        while (traveledDistance < weapon.explodesAfterDistance || weapon.explodesAfterDistance == 0)
        {
            if (weapon.explodesAfterTime > 0 && lifetime <= 0) break;

            // אם הפרויקטיל נהרס, יציאה מהלולאה
            if (projectile == null) yield break;

            yield return null;
            if (projectile != null)
            {
                traveledDistance = Vector2.Distance(startPosition, projectile.transform.position);
                lifetime -= Time.deltaTime;
            }
        }

        // אם הפרויקטיל כבר נהרס, לא להמשיך
        if (projectile == null) yield break;

        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            Vector2 explosionPosition = projectileScript.transform.position; // 🔹 המיקום הנוכחי של הירייה
            Vector2 explosionDirection = projectileScript.rb.linearVelocity.normalized; // 🔹 כיוון התנועה שלה

            projectileScript.HandleImpact(explosionPosition, explosionDirection);
        }

        // הריסת הפרויקטיל אם עדיין קיים, תוך שימוש ב-coroutineRunner
        if (projectile != null)
        {
            coroutineRunner.StartCoroutine(DestroyAfterFrame(projectile));
        }
    }

    // פונקציה שתשמיד את האובייקט באמצעות MonoBehaviour
    private IEnumerator DestroyAfterFrame(GameObject projectile)
    {
        yield return null; // מחכים פריים אחד לפני ההשמדה

        if (projectile != null)
        {
            Collider2D collider = projectile.GetComponent<Collider2D>();
            if (collider != null && collider.sharedMaterial != null)
            {
                GameObject.Destroy(collider.sharedMaterial); // מחיקת החומר הפיזיקלי
            }

            GameObject.Destroy(projectile);
        }
    }






    private IEnumerator DestroyProjectile(GameObject projectile)
    {
        yield return null; // מחכה פריים אחד לפני המחיקה
        if (projectile != null)
        {
            GameObject.Destroy(projectile);
        }
    }


    private void Explode(Vector2 position)
    {
        if (currentWeapon.explosionEffect != null)
        {
            GameObject.Instantiate(currentWeapon.explosionEffect, position, Quaternion.identity);
        }
    }

    private IEnumerator ApplyAcceleration(Rigidbody2D rb, Vector2 direction, Vector2 playerMomentum, WeaponSO weaponData, Projectile projectileScript)
    {
        float elapsedTime = 0f;
        float startSpeed = weaponData.accelerationRate > 0 ? weaponData.minSpeed : weaponData.maxSpeed;
        float targetSpeed = weaponData.accelerationRate > 0 ? weaponData.maxSpeed : weaponData.minSpeed;

        while (elapsedTime < weaponData.accelerationDuration)
        {
            if (projectileScript.HasImpacted) yield break; // אם הייתה פגיעה, יוצאים מיד מהלולאה

            if (rb == null) yield break; // מונע שגיאה אם ה-Rigidbody נמחק

            float newSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / weaponData.accelerationDuration);
            rb.linearVelocity = direction * newSpeed + playerMomentum;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
        {
            rb.linearVelocity = direction * targetSpeed + playerMomentum;
        }
    }

    private void ReleaseProjectileDuringFlight(Rigidbody2D parentRb, WeaponSO weapon)
    {
        if (weapon.projectilesDuringFlightAttributes == null || weapon.projectilesDuringFlightAttributes.weaponsList.Count == 0)
            return;

        for (int i = 0; i < weapon.amountOfProjectilesOnEachRelease; i++)
        {
            WeaponSO randomWeapon = weapon.projectilesDuringFlightAttributes.weaponsList[
                Random.Range(0, weapon.projectilesDuringFlightAttributes.weaponsList.Count)
            ];

            if (randomWeapon == null)
            {
                Debug.LogError("Randomly selected WeaponSO is NULL!");
                return;
            }

            // כיוון התנועה של הירייה הראשית
            Vector2 baseDirection = parentRb.linearVelocity.normalized;
            if (baseDirection == Vector2.zero) baseDirection = Vector2.right;

            // יצירת פיזור אקראי לכל ירייה בנפרד
            float spreadAngle = Random.Range(weapon.projectilesSpreadDuringFlight.minValue, weapon.projectilesSpreadDuringFlight.maxValue);
            Quaternion spreadRotation = Quaternion.Euler(0, 0, spreadAngle);
            Vector2 spreadDirection = spreadRotation * baseDirection;

            // יצירת כל פרויקטיל בנפרד
            SpawnProjectile(parentRb.position, Quaternion.LookRotation(Vector3.forward, spreadDirection), spreadDirection * randomWeapon.minSpeed, randomWeapon, slowMotionEvents);
        }
    }




    private IEnumerator ApplyProjectilesDuringFlight(Rigidbody2D rb, WeaponSO weapon)
    {
        if (!weapon.hasProjectilesDuringFlight || weapon.projectilesDuringFlightAttributes == null || weapon.projectilesDuringFlightAttributes.weaponsList.Count == 0)
            yield break;

        float elapsedTime = 0f;
        float maxDurationForProjectiles = (weapon.explodesAfterTime > 0) ? weapon.explodesAfterTime : 20f;

        yield return new WaitForSeconds(weapon.timeBetweenProjectilesRelease);

        while (rb != null && elapsedTime < maxDurationForProjectiles)
        {
            ReleaseProjectileDuringFlight(rb, weapon);

            elapsedTime += weapon.timeBetweenProjectilesRelease;
            yield return new WaitForSeconds(weapon.timeBetweenProjectilesRelease);
        }
    }


    private void SetProjectileAppearance(GameObject projectile, WeaponSO weapon)
    {
        SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = projectile.AddComponent<SpriteRenderer>();
        }

        // צביעת הפרויקטיל לפי סוג הנשק
        switch (weapon.weaponType)
        {
            case WeaponType.Fire:
                spriteRenderer.color = new Color(1f, 0.5f, 0f); // כתום
                break;
            case WeaponType.Napalm:
                spriteRenderer.color = Color.red;
                break;
            case WeaponType.Radioactive:
                spriteRenderer.color = Color.yellow;
                break;
            case WeaponType.Poison:
                spriteRenderer.color = Color.green;
                break;
        }

        projectile.name = weapon.weaponName;
    }


}
