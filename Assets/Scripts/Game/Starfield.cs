using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Starfield : MonoBehaviour
{
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private StarfieldDirection _direction = StarfieldDirection.Down;
    [SerializeField] private int _maxStars = 100;
    [SerializeField] private float _starSize = 0.1f;
    [SerializeField] private float _starSizeRange = 0.5f;
    [SerializeField] private float _fieldWidth = 20f;
    [SerializeField] private float _fieldHeight = 25f;
    [SerializeField] private bool _colorize = false;
    [SerializeField] private StarfieldColorization _colorization = StarfieldColorization.Green; 
    
    private float _starFieldCenterXPos;
    private float _starFieldCenterYPos;

    private Camera _bgCamera;
    
    private ParticleSystem _particles;
    private ParticleSystem.Particle[] _stars;

    private void Awake()
    {
        _bgCamera = FindObjectsOfType<Camera>().FirstOrDefault(c => c.CompareTag("BG_Camera"));
        if (_bgCamera == null)
        {
            Debug.LogError($"Background Camera is NULL On Starfield::{gameObject.name}");
        }
        
        _stars = new ParticleSystem.Particle[_maxStars];
        _particles = GetComponent<ParticleSystem>();

        if (_particles == null)
        {
            Debug.LogError($"Particle System is NULL On Starfield::{gameObject.name}");
        }

        CalculateFieldCenter();
        GenerateStarfield();
        _particles.SetParticles(_stars, _stars.Length);
    }

    private void Update()
    {
        CalculateMovement();
    }

    private void CalculateFieldCenter()
    {
        _starFieldCenterXPos = _fieldWidth / 2.0f;
        _starFieldCenterYPos = _fieldHeight / 2.0f;
    }

    private void GenerateStarfield()
    {
        for (int starIndex = 0; starIndex < _maxStars; starIndex++)
        {
            float randomSize = Random.Range(_starSizeRange, _starSizeRange + 1.0f);
            float scaledColor = _colorize ? randomSize - _starSizeRange : 1.0f;

            var star = _stars[starIndex];
            star.position = GetRandomInRectangle(_fieldWidth, _fieldHeight) + transform.position;
            star.startSize = _starSize * randomSize;
            star.startColor = _colorization switch
            {
                StarfieldColorization.Red => new Color(1.0f, scaledColor, scaledColor, 1.0f),
                StarfieldColorization.Green => new Color(scaledColor, 1.0f, scaledColor, 1.0f),
                StarfieldColorization.Blue => new Color(scaledColor, scaledColor, 1.0f, 1.0f),
                _ => throw new ArgumentOutOfRangeException()
            };

            _stars[starIndex] = star;
        }
    }

    private Vector3 GetRandomInRectangle(float width, float height)
    {
        float x = Random.Range(0, width);
        float y = Random.Range(0, height);

        return new Vector3(x - _starFieldCenterXPos, y - _starFieldCenterYPos, 0);
    }

    private void CalculateMovement()
    {
        Vector3 movementDirection = _direction switch
        {
            StarfieldDirection.Down => Vector3.down,
            StarfieldDirection.Left => Vector3.left,
            StarfieldDirection.Up => Vector3.up,
            StarfieldDirection.Right => Vector3.right,
            _ => throw new ArgumentOutOfRangeException()
        };
        transform.Translate(movementDirection * (_speed * Time.deltaTime));
        
        for (int starIndex = 0; starIndex < _maxStars; starIndex++)
        {
            var fieldCenter = transform.position;
            Vector3 newStarPosition = _stars[starIndex].position + fieldCenter;
            newStarPosition = GetNewStarPositionWithWrap(newStarPosition);
            _stars[starIndex].position = newStarPosition - fieldCenter;
        }
        _particles.SetParticles(_stars, _stars.Length);
    }

    private Vector3 GetNewStarPositionWithWrap(Vector3 newStarPosition)
    {
        switch (_direction)
        {
            case StarfieldDirection.Down:
                if (newStarPosition.y < _bgCamera.transform.position.y - _starFieldCenterYPos)
                {
                    newStarPosition.y += _fieldHeight;
                }
                break;
            case StarfieldDirection.Left:
                if (newStarPosition.x < _bgCamera.transform.position.x - _starFieldCenterXPos)
                {
                    newStarPosition.x += _fieldWidth;
                }
                break;
            case StarfieldDirection.Up:
                if (newStarPosition.y > _bgCamera.transform.position.y + _starFieldCenterYPos)
                {
                    newStarPosition.y -= _fieldHeight;
                }
                break;
            case StarfieldDirection.Right:
                if (newStarPosition.x > _bgCamera.transform.position.x + _starFieldCenterXPos)
                {
                    newStarPosition.x -= _fieldWidth;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return newStarPosition;
    }
}