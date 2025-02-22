using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public WeaponSO weaponData; // החזקת הנתונים של הנשק
    public TerrainListSO terrainList;
    [SerializeField] private SOVector2 playerMomentumSO;
    // מחלקות ניהול
    private AmmoManager ammoManager;
    private ProjectileManager projectileManager;
    private ExplosionManager explosionManager;
    private StatusEffectManager statusEffectManager;
    private GravityManager gravityManager;
    private EnvironmentalManager environmentalManager;
    [SerializeField] private Transform firePoint;
    [SerializeField] private TerrainListSO terrainListSO;
    [SerializeField] private SlowMotionEventSO slowMotionEvents;
    private void Awake()
    {
        InitializeManagers();
    }

    // אתחול כל המנהלים הרלוונטיים
    private void InitializeManagers()
    {
        projectileManager = new ProjectileManager(weaponData, this, firePoint, playerMomentumSO, terrainListSO, slowMotionEvents);
        ammoManager = new AmmoManager(weaponData, this, projectileManager, firePoint);
        explosionManager = new ExplosionManager(weaponData, this, environmentalManager, projectileManager, slowMotionEvents);
        statusEffectManager = new StatusEffectManager(weaponData, this);
        gravityManager = new GravityManager(weaponData, this);
        environmentalManager = new EnvironmentalManager(weaponData, terrainList);
    }




    // פונקציה לשליפת מנהלים (נשתמש בה בעתיד כדי לאפשר גישה קלה)
    public T GetManager<T>() where T : class
    {
        if (typeof(T) == typeof(AmmoManager)) return ammoManager as T;
        if (typeof(T) == typeof(ProjectileManager)) return projectileManager as T;
        if (typeof(T) == typeof(ExplosionManager)) return explosionManager as T;
        if (typeof(T) == typeof(StatusEffectManager)) return statusEffectManager as T;
        if (typeof(T) == typeof(GravityManager)) return gravityManager as T;
        if (typeof(T) == typeof(EnvironmentalManager)) return environmentalManager as T;

        Debug.LogWarning($"Manager of type {typeof(T)} not found!");
        return null;
    }


}
