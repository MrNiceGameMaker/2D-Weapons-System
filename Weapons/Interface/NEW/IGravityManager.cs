using UnityEngine;

public interface IGravityManager
{
    void ApplyGravityDuringFlight(Rigidbody2D rb); // כוח משיכה בזמן טיסה
    void TriggerGravityField(Vector2 position); // הפעלת כוח משיכה אחרי פיצוץ
}
