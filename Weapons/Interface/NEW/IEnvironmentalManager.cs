using UnityEngine;
public interface IEnvironmentalManager
{
    void DigInTerrain(Vector2 position, bool isExplosion); // חפירה בחול
    void CreateNewSand(Vector2 position); // יצירת חול חדש
}

