using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private Animator _animator;
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator is NULL on Explosion");
        }
        
        _audioSource = GetComponent<AudioSource>();
        if (_animator == null)
        {
            Debug.LogError("AudioSource is NULL on Explosion");
        }

        var animationTime = GetAnimationTime(_animator);
        _audioSource.Play();
        Destroy(gameObject, animationTime);
    }

    private float GetAnimationTime(Animator animator)
    {
        var currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        var animationTime = currentAnimatorStateInfo.length;
        return animationTime;
    }
}
