using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public UnityEvent<bool> OnImpact = new UnityEvent<bool>(); // אירוע שמשדר כשיש פגיעה
    public bool HasImpacted { get; private set; } = false; // האם הירייה כבר פגעה במשהו?

    private WeaponSO weapon;
    public Rigidbody2D rb;
    private ExplosionManager explosionManager;
    private EnvironmentalManager environmentalManager;
    private int currentBounces = 0; // מספר קפיצות שבוצעו
    private GravityManager gravityManager; // שמירת ניהול כוח המשיכה
    private SlowMotionEventSO slowMotionEvents; // נוסיף את ה-SO

    public void Initialize(WeaponSO weaponSO, ExplosionManager explosionMgr, EnvironmentalManager envManager, GravityManager gravManager, SlowMotionEventSO slowMotionEvents)
    {
        weapon = weaponSO;
        rb = GetComponent<Rigidbody2D>();
        explosionManager = explosionMgr;
        environmentalManager = envManager;
        gravityManager = gravManager;
        this.slowMotionEvents = slowMotionEvents; // קבלת ה-SO

        StartCoroutine(IgnoreInitialCollisions(GetComponent<Collider2D>()));

        if (weapon.isDigging)
        {
            StartCoroutine(ApplyDigging());
        }

        // הודעה שהפרויקטיל נוצר (אם ה-SO מחובר)
        slowMotionEvents?.OnProjectileCreated.Invoke();
    }

    private void OnDisable()
    {
        // הודעה שהפרויקטיל הושמד (אם ה-SO מחובר)
        slowMotionEvents?.OnProjectileDestroyed.Invoke();
    }


    private IEnumerator ApplyDigging()
    {
        while (gameObject != null && rb != null)
        {
            environmentalManager.DigInTerrain(rb.position, false);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ShouldExplode(collision.gameObject))
        {
            ContactPoint2D contact = collision.GetContact(0); // 🔹 מקבל את נקודת המגע הראשונה
            Vector2 explosionPosition = contact.point; // 🔹 נקודת הפיצוץ המדויקת
            Vector2 explosionDirection = Vector2.Reflect(rb.linearVelocity.normalized, contact.normal); // 🔹 חישוב כיוון הפיצוץ

            HandleImpact(explosionPosition, explosionDirection);
        }
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && weapon.hitDamage > 0)
        {
            damageable.TakeDamage(weapon.hitDamage);
        }

    }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (ShouldExplode(collider.gameObject))
        {
            Vector2 explosionPosition = transform.position; // 🔹 אם זה טריגר, נשתמש במיקום הנוכחי של הפרויקטיל
            Vector2 explosionDirection = rb.linearVelocity.normalized; // 🔹 נשתמש בכיוון התנועה שלו

            HandleImpact(explosionPosition, explosionDirection);
        }
        IDamageable damageable = collider.GetComponent<IDamageable>();
        if (damageable != null && weapon.duringFlightDamage > 0)
        {
            StartCoroutine(ApplyContinuousDamage(damageable));
        }
    }


    private bool ShouldExplode(GameObject obj)
    {
        if ((weapon.explodesOnContactWithLayer.value & (1 << obj.layer)) != 0)
        {
            return true;
        }

        string[] selectedTags = weapon.explodesOnContactWithTag.Split(',');
        if (selectedTags.Contains("Everything") || selectedTags.Contains(obj.tag))
        {
            return true;
        }

        return false;
    }

    public void HandleImpact(Vector2 explosionPosition, Vector2 explosionDirection)
    {
        if (HasImpacted) return;
        HasImpacted = true;

        OnImpact.Invoke(true);

        if (weapon.isExploding && explosionManager != null)
        {
            explosionManager.TriggerExplosion(explosionPosition, explosionDirection);
        }

        if (environmentalManager != null)
        {
            if (weapon.isExplosionDestroyingSand)
            {
                environmentalManager.DigInTerrain(explosionPosition, true);
            }

            if (weapon.isExplosionMakingNewSand)
            {
                environmentalManager.CreateNewSand(explosionPosition);
            }
        }

        if (weapon.stopsOnImpact)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (weapon.hasGravityAfterExplosion && gravityManager != null)
        {
            gravityManager.TriggerGravityField(explosionPosition);
        }

        Destroy(gameObject);
    }



    private IEnumerator IgnoreInitialCollisions(Collider2D projectileCollider)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, weapon.explodesOnContactWithLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider == projectileCollider) continue; // לא בודקים את עצמנו

            Vector2 directionToObstacle = (collider.transform.position - transform.position).normalized;
            float dotProduct = Vector2.Dot(rb.linearVelocity.normalized, directionToObstacle);

            if (dotProduct > 0) // אם הכיוון של הפרויקטיל מוביל ישירות לאובייקט – נפוצץ מיד
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.linearVelocity.normalized, 0.2f, weapon.explodesOnContactWithLayer);
                if (hit.collider != null)
                {
                    Vector2 explosionDirection = Vector2.Reflect(rb.linearVelocity.normalized, hit.normal); // 🔹 חישוב כיוון הפיצוץ לפי נורמל הפגיעה
                    HandleImpact(hit.point, explosionDirection); // 🔹 שולח את נקודת הפגיעה ואת הכיוון של הפיצוץ
                    yield break;
                }
            }
        }

        int layerToIgnore = GetFirstLayerFromMask(weapon.explodesOnContactWithLayer);
        if (layerToIgnore >= 0 && layerToIgnore <= 31)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, true);
            yield return new WaitForSeconds(0.05f);
            Physics2D.IgnoreLayerCollision(gameObject.layer, layerToIgnore, false);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.linearVelocity.normalized, 0.2f, weapon.explodesOnContactWithLayer);
            if (hit.collider != null)
            {
                Vector2 explosionDirection = Vector2.Reflect(rb.linearVelocity.normalized, hit.normal);
                HandleImpact(hit.point, explosionDirection);
            }
        }
    }

    private int GetFirstLayerFromMask(LayerMask mask)
    {
        int layer = 0;
        int maskValue = mask.value;
        while (maskValue > 0)
        {
            if ((maskValue & 1) == 1)
                return layer;
            maskValue >>= 1;
            layer++;
        }
        return -1; // במקרה שלא נמצאה שכבה
    }
    private IEnumerator ApplyContinuousDamage(IDamageable target)
    {
        while (target != null)
        {
            target.TakeDamage(weapon.duringFlightDamage);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
