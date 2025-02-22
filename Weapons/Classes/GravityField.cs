using System.Collections;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    private float gravityForce;
    private float gravityRadius;
    private float duration;

    public void Initialize(float force, float radius, float time)
    {
        gravityForce = force;
        gravityRadius = radius;
        duration = time;
        StartCoroutine(ApplyGravity());
    }

    private IEnumerator ApplyGravity()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Collider2D[] affectedObjects = Physics2D.OverlapCircleAll(transform.position, gravityRadius);
            foreach (Collider2D obj in affectedObjects)
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 forceDirection = ((Vector2)transform.position - rb.position).normalized;
                    float distanceFactor = Mathf.Clamp01(1 - (Vector2.Distance(transform.position, rb.position) / gravityRadius));

                    rb.AddForce(forceDirection * gravityForce * distanceFactor * Time.deltaTime, ForceMode2D.Force);
                }
            }

            yield return new WaitForSeconds(0.05f);
            elapsedTime += 0.05f;
        }
    }
}
