using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class LaserBeam : Laser
{
    [Header("Shot Properties")] 
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _range;
    [SerializeField] private LineRenderer _line;
    
    
    protected override void Start()
    {
        base.Start();
        SetOffsetPosition();

        RaycastHit2D[] hits = new RaycastHit2D[] { };
        Vector3 bottomOfScreenFromLaser = default;
        if (_viewportBounds is {})
        {
            bottomOfScreenFromLaser = new Vector3(transform.position.x, _viewportBounds.Bottom);
            _range = (transform.position - bottomOfScreenFromLaser).magnitude;
            hits = Physics2D.RaycastAll(transform.position, Vector2.down, _range);
        }
        else
        {
            Debug.LogError("LaserBeam::Start(23): View Bounds Object is Null");
        }
        
        _line = GetComponent<LineRenderer>();
        if (_line is {} && hits.Any())
        {
            _line.SetPosition(0, transform.position);
            _line.SetPosition(1, bottomOfScreenFromLaser);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    DamagePlayer(hit.collider);
                }
            }
            
            Destroy(gameObject, _duration);
        }
        else
        {
            Debug.LogError("LaserBeam::Start(33): Line Renderer is Null");
        }
    }

    protected override void Update()
    {
        base.Update();
        if (_line is { })
        {
            _line.SetPosition(0, transform.position);
        }
    }
}
