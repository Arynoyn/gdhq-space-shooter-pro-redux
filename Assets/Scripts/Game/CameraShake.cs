using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, transform.position.z);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = startPosition;
    }
}
