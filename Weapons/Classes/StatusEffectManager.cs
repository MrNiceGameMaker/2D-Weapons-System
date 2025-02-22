using System.Collections;
using UnityEngine;

public class StatusEffectManager : IStatusEffectManager
{
    private WeaponSO weaponData;
    private MonoBehaviour coroutineRunner; // רפרנס להפעלת קורוטינות

    public StatusEffectManager(WeaponSO weaponData, MonoBehaviour runner)
    {
        this.weaponData = weaponData;
        this.coroutineRunner = runner;
    }

    public void ApplyDamageOverTime(IDamageable target)
    {
        if (!weaponData.hasDamageOverTime) return;
        coroutineRunner.StartCoroutine(DamageOverTimeCoroutine(target));
    }

    private IEnumerator DamageOverTimeCoroutine(IDamageable target)
    {
        float elapsedTime = 0f;
        while (elapsedTime < weaponData.totalDuration)
        {
            target.TakeDamage(weaponData.damageOverTime);
            yield return new WaitForSeconds(weaponData.tickRate);
            elapsedTime += weaponData.tickRate;  
        }
    }
}
