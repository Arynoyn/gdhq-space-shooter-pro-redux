using UnityEngine;

public class EnemyWithAttachedExplosionAnimation : Enemy
{
    [SerializeField] private GameObject _explosionPrefab;
    
    protected override void Start()
    {
        base.Start();
        PlayExplosion = PlayExplosionMethod;
        DestroySelf = DestroySelfMethod;
        
        if (_explosionPrefab == null)
        {
            Debug.LogError("EnemyWithAttachedExplosionAnimation::Start(187): Explosion Prefab is NULL in EnemyWithAttachedExplosionAnimation");
        }
    }

    private void PlayExplosionMethod()
    {
        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    private void DestroySelfMethod()
    {
        Destroy(gameObject);
    }
}