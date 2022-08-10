using UnityEngine;

public interface IFireLaserBehavior
{
    void DisableFiring();
    void Fire(Transform target);
    void FireAtClosestTarget();
}