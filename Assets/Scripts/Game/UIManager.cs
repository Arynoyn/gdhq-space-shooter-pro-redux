using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Player Score")]
    [SerializeField] private Text _scoreText;

    [Header("Player Lives")]
    [SerializeField] private Image _livesDisplay;
    [SerializeField] private Sprite[] _livesImages = new Sprite[4];
    
    [Header("Player Shields")]
    [SerializeField] private Image _shieldStrengthImage;
    [SerializeField] private Sprite[] _shieldStrengthImages = new Sprite[4];

    [Header("Player Ammo")]
    [SerializeField] private Text _ammoText;
    
    [Header("Player Thruster Charge")]
    [SerializeField] private ThrusterDisplay _thrusterDisplay;
    
    [Header("Game Over Text")]
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Text _restartText;
    [SerializeField] private float _gameOverFlashRate = 0.5f;
    
    private bool _isGameOverTextNull;
    private bool _isScoreTextNull;
    private bool _isLivesDisplayNull;
    private bool _isLivesImagesNull;
    private bool _isRestartTextNull;

    private Camera _camera;
    private CameraShake _cameraShaker;

    private void Awake()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("UIManager::Start(44): Main Camera is Null on UI Manager!");
        }
        else
        {
            _cameraShaker = _camera.GetComponent<CameraShake>();
            if (_cameraShaker == null)
            {
                Debug.LogError("UIManager::Start(51): Camera Shaker is Null on UI Manager!");
            }
        }
    }

    private void Start()
    {
        _isScoreTextNull = _scoreText == null;
        if (_isScoreTextNull) { Debug.Log("UIManager::Start(56): Score Text object on UI Manager is NULL"); }
        
        _isLivesDisplayNull = _livesDisplay == null;
        if (_isLivesDisplayNull) { Debug.Log("UIManager::Start(59): Lives Display Image on UI Manager is NULL"); }
        
        _isLivesImagesNull = _livesImages == null;
        if (_isLivesImagesNull) { Debug.Log("UIManager::Start(62): Lives Images Array on UI Manager is NULL"); }
        
        _isGameOverTextNull = _gameOverText == null;
        if (_isGameOverTextNull) { Debug.Log("UIManager::Start(65): Game Over Text object on UI Manager is NULL"); }
        
        _isRestartTextNull = _restartText == null;
        if (_isRestartTextNull) { Debug.Log("UIManager::Start(68): Restart Text object on UI Manager is NULL"); }
    }

    public void SetScore(int score)
    {
        if (!_isScoreTextNull) { _scoreText.text = score.ToString(); }
        if (!_isGameOverTextNull) { _gameOverText.gameObject.SetActive(false); }
        if (!_isRestartTextNull) { _restartText.gameObject.SetActive(false); }
    }

    public void SetLives(int lives)
    {
        if (_isLivesDisplayNull || _isLivesImagesNull) { return; }
        if (_livesImages.Length >= lives)
        {
            _livesDisplay.sprite = _livesImages[lives];
        }
        else
        {
            Debug.LogError($"UIManager::SetLives(87): Index {lives} is out of bounds of the array of lives images");
        }
    }
    
    public void UpdateShieldStrength(int shieldStrength)
    {
        GameObject imageGameObject = _shieldStrengthImage.gameObject;
        GameObject parentGameObject = imageGameObject.transform.parent.gameObject;
        
        _shieldStrengthImage.sprite = _shieldStrengthImages[shieldStrength];
        imageGameObject.SetActive(shieldStrength > 0);
        parentGameObject.SetActive(shieldStrength > 0);
    }
    
    public void UpdateAmmoCount(int ammo, int maxAmmo)
    {
        _ammoText.text = $"{ammo}/{maxAmmo}";
    }
    
    public void UpdateThrusterCharge(int thrusterCharge)
    {
        _thrusterDisplay.SetCharge(thrusterCharge);
    }
    
    public void UpdateMaxThrusterCharge(int thrusterCharge)
    {
        _thrusterDisplay.SetMaxCharge(thrusterCharge);
    }
    
    public void ShakeCamera()
    {
        if (_cameraShaker == null)
        {
            Debug.LogError("UIManager::ShakeCamera(120): Camera Shaker is missing from UI Manager!");
        }
        else
        {
            StartCoroutine(_cameraShaker.ShakeCamera(0.2f, 0.3f));
        }
    }

    public void DisplayGameOver()
    {
        if (!_isGameOverTextNull) {_gameOverText.gameObject.SetActive(true);}
        if (!_isRestartTextNull) {_restartText.gameObject.SetActive(true);}
        StartCoroutine(GameOverFlashRoutine());
    }

    IEnumerator GameOverFlashRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_gameOverFlashRate);
            if (!_isGameOverTextNull) {_gameOverText.gameObject.SetActive(!_gameOverText.gameObject.activeSelf);}
        }
    }

    public ViewportBounds GetViewportBounds()
    {
        if (_camera != null)
        {
            var cameraPosition = _camera.transform.position;
            var viewportBounds = new ViewportBounds
            {
                Top = _camera.ViewportToWorldPoint(new Vector3(0, 1, Mathf.Abs(cameraPosition.z))).y,
                Bottom = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cameraPosition.z))).y,
                Left = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cameraPosition.z))).x,
                Right = _camera.ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(cameraPosition.z))).x
            };
            return viewportBounds;
        }

        Debug.LogError("UIManager::GetViewportBounds (159): Main Camera is NULL on UI Manager");
        return new ViewportBounds();
    }
}