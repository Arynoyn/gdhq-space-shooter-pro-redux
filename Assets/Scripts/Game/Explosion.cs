using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator is NULL on Explosion");
        }

        var animationTime = GetAnimationTime(_animator);
        Destroy(gameObject, animationTime);
    }

    private float GetAnimationTime(Animator animator)
    {
        var currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        var animationTime = currentAnimatorStateInfo.length;
        return animationTime;
    }
}
