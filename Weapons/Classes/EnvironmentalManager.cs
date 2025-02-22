using System.Collections;
using TerraformingTerrain2d;
using UnityEngine;

public class EnvironmentalManager : IEnvironmentalManager
{
    private WeaponSO weaponData;
    private TerrainListSO terrainList; // רשימה של כל השטחים שניתן לחפור בהם

    public EnvironmentalManager(WeaponSO weaponData, TerrainListSO terrainList)
    {
        this.weaponData = weaponData;
        this.terrainList = terrainList;
    }

    public void DigInTerrain(Vector2 position, bool isExplosion)
    {
        if (!weaponData.isDigging && !weaponData.isExplosionDestroyingSand) return; // רק אם שני התנאים לא מתקיימים לא חופרים

        float radius = isExplosion ? weaponData.sandDestructionRadius : weaponData.diggingSize; // בוחר את הרדיוס המתאים

        foreach (var terrain in terrainList.terrains)
        {
            terrain.TerraformCircle(position, radius, TerraformingMode.Carve);
        }
    }


    public void CreateNewSand(Vector2 position)
    {
        if (!weaponData.isExplosionMakingNewSand) return;

        foreach (var terrain in terrainList.terrains)
        {
            terrain.TerraformCircle(position, weaponData.newSandRadius, TerraformingMode.Fill);
        }
    }
    public void HandleExplosionTerrainEffects(Vector2 position)
    {
        if (weaponData.isExplosionDestroyingSand)
        {
            foreach (var terrain in terrainList.terrains)
            {
                terrain.TerraformCircle(position, weaponData.sandDestructionRadius, TerraformingMode.Carve);
            }
        }

        if (weaponData.isExplosionMakingNewSand)
        {
            foreach (var terrain in terrainList.terrains)
            {
                terrain.TerraformCircle(position, weaponData.newSandRadius, TerraformingMode.Fill);
            }
        }
    }
}
