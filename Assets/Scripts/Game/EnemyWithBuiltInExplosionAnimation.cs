using System.Linq;
using UnityEngine;

public class EnemyWithBuiltInExplosionAnimation : Enemy
{
    // Animation Properties
    [Header("Animation")]
    [Space]
    [SerializeField] private string _enemyDestroyedAnimationName = "Enemy_Destroyed_anim";
    [SerializeField] private string _enemyDeathTriggerName = "OnEnemyDeath";
    private Animator _animator;
    private float _deathAnimationLength;

    protected override void Start()
    {
        base.Start();
        PlayExplosion = PlayExplosionMethod;
        DestroySelf = DestroySelfMethod;
        _animator = GetComponent<Animator>();
        if (_animator == null) { Debug.LogError("EnemyWithBuiltInExplosionAnimation::Start(127): Animator is NULL on Enemy"); }
        else
        {
            _deathAnimationLength = GetAnimationLength(_enemyDestroyedAnimationName);
        }
    }
    
    private float GetAnimationLength(string animationName)
    {
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        var deathAnimationClip = clips.FirstOrDefault(clip => clip.name == animationName);
        if (deathAnimationClip == null)
        {
            Debug.LogError("EnemyWithBuiltInExplosionAnimation::GetAnimationLength(169): Death Animation clip is NULL in animator on Enemy");
        }
        else
        {
            return deathAnimationClip.length;
        }

        return 0f;
    }
    
    private void PlayExplosionMethod()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_enemyDeathTriggerName);
        }
    }

    private void DestroySelfMethod()
    {
        Destroy(gameObject, _deathAnimationLength);
    }
}