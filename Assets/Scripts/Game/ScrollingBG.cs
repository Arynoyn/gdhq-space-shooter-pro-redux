using UnityEngine;

public class ScrollingBG : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private float _speed = 0.1f;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        _renderer.material.mainTextureOffset = new Vector2(0, Time.time * _speed);
    }
}
