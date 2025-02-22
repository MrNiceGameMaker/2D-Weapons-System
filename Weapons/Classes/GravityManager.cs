using System.Collections;
using UnityEngine;

public class GravityManager : IGravityManager
{
    private WeaponSO weaponData;
    private MonoBehaviour coroutineRunner;

    public GravityManager(WeaponSO weaponData, MonoBehaviour runner)
    {
        this.weaponData = weaponData;
        this.coroutineRunner = runner;
    }

    public void ApplyGravityDuringFlight(Rigidbody2D rb)
    {
        if (!weaponData.hasGravityDuringFlight) return;

        coroutineRunner.StartCoroutine(ApplyGravityFieldDuringFlight(rb));
    }

    public void TriggerGravityField(Vector2 position)
    {
        if (!weaponData.hasGravityAfterExplosion) return;

        // יצירת אובייקט כוח משיכה
        GameObject gravityFieldObject;

        if (weaponData.gravityFieldPrefab != null)
        {
            // אם יש אובייקט מוגדר, יוצרים אותו
            gravityFieldObject = GameObject.Instantiate(weaponData.gravityFieldPrefab, position, Quaternion.identity);
        }
        else
        {
            // אם אין אובייקט מוגדר, יוצרים אובייקט ריק
            gravityFieldObject = new GameObject("GravityField");
            gravityFieldObject.transform.position = position;
        }

        // הוספת Collider למעקב אחר אובייקטים
        CircleCollider2D gravityCollider = gravityFieldObject.AddComponent<CircleCollider2D>();
        gravityCollider.radius = weaponData.gravityForceRadius;
        gravityCollider.isTrigger = true;

        // הוספת סקריפט להפעלת כוח משיכה
        GravityField gravityField = gravityFieldObject.AddComponent<GravityField>();
        gravityField.Initialize(weaponData.gravityForce, weaponData.gravityForceRadius, weaponData.gravityDurationAfterExplosion);

        // השמדת האובייקט אחרי משך הזמן
        GameObject.Destroy(gravityFieldObject, weaponData.gravityDurationAfterExplosion);
    }


    private IEnumerator ApplyGravityFieldDuringFlight(Rigidbody2D rb)
    {
        while (rb != null)
        {
            Collider2D[] affectedObjects = Physics2D.OverlapCircleAll(rb.position, weaponData.gravityForceRadius);
            foreach (Collider2D obj in affectedObjects)
            {
                Rigidbody2D objRb = obj.GetComponent<Rigidbody2D>();
                if (objRb != null && objRb != rb) // לא משפיע על עצמו
                {
                    Vector2 forceDirection = (rb.position - objRb.position).normalized;
                    float distance = Vector2.Distance(rb.position, objRb.position);

                    // חישוב כוח המשיכה בהתאם למרחק (הפחתת הכוח לפי המרחק)
                    float gravityEffect = Mathf.Clamp01(1 - (distance / weaponData.gravityForceRadius));

                    // הכוח מוגבר עם מקדם נוסף כדי לוודא השפעה משמעותית
                    float forceToApply = weaponData.gravityForce * gravityEffect * 100f;

                    objRb.AddForce(forceDirection * forceToApply * Time.deltaTime, ForceMode2D.Force);
                }
            }
            yield return new WaitForSeconds(0.05f); // עדכון כוח המשיכה כל 0.05 שניות
        }
    }

}
