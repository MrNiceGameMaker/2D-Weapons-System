using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    [Header("Main Attributes")]
    public string weaponName;
    public GameObject projectilePrefab;
    public Sprite shotSprite;
    public Sprite weaponSprite;
    public bool hasPhysicalCollider;

    [Header("Bullets and Magazine")]
    public float magazineReloadTime;
    public int magazineSize;
    public float fireRate;
    public bool isBurstFire;
    [Range(2, 20)] public int burstAmount;
    [Range(0, 1f)] public float ySpread;
    [Range(0f, 0.3f)] public float burstTotalTime;

    [Header("During Flight")]
    public float minSpeed; // המהירות ההתחלתית
    public float maxSpeed; // המהירות המרבית
    [Range(-1, 1)] public int accelerationRate; // קצב התאוצה (חיובי -> האצה, שלילי -> האטה)
    public float accelerationDuration; // תוך כמה זמן מגיעים למהירות הסופית
    public bool affectedByPlayerMomentum;
    public bool affectedByGravity;
    public float movementInFlightRange;
    public bool stopsOnImpact; // האם נעצר במגע

    [Header("Projectiles During Flight")]
    public MinMaxRange projectilesSpreadDuringFlight;
    public bool hasProjectilesDuringFlight;
    public WeaponsList projectilesDuringFlightAttributes;
    [Range(1, 5)] public int amountOfProjectilesOnEachRelease;
    [Range(0.1f, 10f)] public float timeBetweenProjectilesRelease;
   
    [Header("Explosion")]
    public bool isExploding;
    public float explosionRadius;
    public float explodesAfterTime;
    public float explodesAfterDistance;
    public bool hasShockwave;
    public float shockwaveSize;
    public float shockwaveStrength;
    public GameObject shockwaveParticleSystem;

    [Header("Effects")]
    public GameObject explosionEffect; // אפקט גרפי לפיצוץ
    public GameObject projectileTrail; // שביל קליע בזמן טיסה

    [Header("Type")]
    public WeaponType weaponType;

    [Header("Sand")]
    public LayerMask sandLayer;
    public bool isDigging;
    public float diggingSize;
    public bool isExplosionDestroyingSand;
    public float sandDestructionRadius;
    public bool isExplosionMakingNewSand;
    public float newSandRadius;

    [Header("Objects Reaction")]
    public LayerMask explodesOnContactWithLayer;
    [TagMask] public string explodesOnContactWithTag;

    [Header("Bounce Control")]
    public bool bouncesOnContact;
    [Range(0f, 1f)] public float bounceBounciness; 
    [Range(0f, 1f)] public float bounceFriction;

    [Header("Damage")]
    public int explosionDamage;
    public int duringFlightDamage;
    public int hitDamage;
    public bool hasDamageOverTime;
    public int damageOverTime;
    public float totalDuration; 
    public float tickRate;

    [Header("After Explosion")]
    public MinMaxRange projectilesSpread;
    public bool hasProjectiles;
    public WeaponsList projectilesAttributes;
    public int amountOfProjectiles;
    public bool hasRandomAmountOfProjectiles;
    public int minimumAmount;
    public int maximumAmount;

    [Header("Physics")]
    public bool hasGravityAfterExplosion;
    public bool hasGravityDuringFlight;
    public float gravityForce; // can be negative
    public float gravityForceRadius;
    public float gravityDurationAfterExplosion;
    public GameObject gravityFieldPrefab;

    private void OnValidate()
    {
        if (maxSpeed < minSpeed)
        {
            Debug.LogWarning("WeaponSO: maxSpeed קטן מ-minSpeed! מתקן בהתאם לתאוצה.");
            if (accelerationRate > 0)
            {
                float temp = minSpeed;
                minSpeed = maxSpeed;
                maxSpeed = temp;
            }
        }

        accelerationDuration = Mathf.Max(accelerationDuration, 0.1f); // לא יכול להיות קטן מ-0.1
        if (maxSpeed < minSpeed)
        {
            Debug.LogWarning("WeaponSO: maxSpeed קטן מ-minSpeed! מתקן בהתאם לתאוצה.");
            if (accelerationRate > 0)
            {
                float temp = minSpeed;
                minSpeed = maxSpeed;
                maxSpeed = temp;
            }
        }

        // תיקון טווחים בפרויקטילים
        projectilesSpreadDuringFlight?.Validate();
        projectilesSpread?.Validate();
    }
}
[System.Serializable]
public class MinMaxRange
{
    [Range(0f, 360f)] public float minValue;
    [Range(0f, 360f)] public float maxValue;
    public void Validate()
    {
        if (minValue > maxValue)
        {
            float temp = minValue;
            minValue = maxValue;
            maxValue = temp;
        }
    }
}