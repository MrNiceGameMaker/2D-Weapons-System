public interface IAmmoManager
{
    bool CanShoot(); // האם אפשר לירות כרגע
    void Shoot(); // ביצוע ירי
    void Reload(); // טעינת מחסנית מחדש
}
