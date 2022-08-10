using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
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
            LogError("Main Camera is Null on UI Manager!");
            LogError("UIManager::Start(43): Main Camera is Null on UI Manager!");
        }
        else
        {
            _cameraShaker = _camera.GetComponent<CameraShake>();
            if (_cameraShaker == null)
            {
                LogError("Camera Shaker is Null on UI Manager!");
            }
        }
    }

    private void Start()
    {
        _isScoreTextNull = _scoreText == null;
        if (_isScoreTextNull) { LogMessage("Score Text object on UI Manager is NULL"); }
        
        _isLivesDisplayNull = _livesDisplay == null;
        if (_isLivesDisplayNull) { LogMessage("Lives Display Image on UI Manager is NULL"); }
        
        _isLivesImagesNull = _livesImages == null;
        if (_isLivesImagesNull) { LogMessage("Lives Images Array on UI Manager is NULL"); }
        
        _isGameOverTextNull = _gameOverText == null;
        if (_isGameOverTextNull) { LogMessage("Game Over Text object on UI Manager is NULL"); }
        
        _isRestartTextNull = _restartText == null;
        if (_isRestartTextNull) { LogMessage("Restart Text object on UI Manager is NULL"); }
    }

    public void SetScore(int score)
    {
        if (!_isScoreTextNull)
        {
            _scoreText.text = score.ToString();
        }
        else
        {
            LogError("Score Text object on UI Manager is NULL");
        }

        if (!_isGameOverTextNull)
        {
            _gameOverText.gameObject.SetActive(false);
        }
        else
        {
            LogError("Game Over Text object on UI Manager is NULL");
        }

        if (!_isRestartTextNull)
        {
            _restartText.gameObject.SetActive(false);
        }
        else
        {
            LogError("Restart Text object on UI Manager is NULL");
        }
    }

    public void SetLives(int lives)
    {
        if (_isLivesDisplayNull || _isLivesImagesNull)
        {
            if (_isLivesDisplayNull)
            {
                LogError("Lives Display Image on UI Manager is NULL");
            }

            if (_isLivesImagesNull)
            {
                LogError("Lives Images Array on UI Manager is NULL");
            }

            return;
        }

        if (_livesImages.Length >= lives)
        {
            _livesDisplay.sprite = _livesImages[lives];
        }
        else
        {
            LogError($"Index {lives} is out of bounds of the array of lives images");
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
            LogError("Camera Shaker is missing from UI Manager!");
        }
        else
        {
            StartCoroutine(_cameraShaker.ShakeCamera(0.2f, 0.3f));
        }
    }

    public void DisplayGameOver()
    {
        if (!_isGameOverTextNull)
        {
            _gameOverText.gameObject.SetActive(true);
        }
        else
        {
            LogError("Game Over Text object on UI Manager is NULL");
        }

        if (!_isRestartTextNull)
        {
            _restartText.gameObject.SetActive(true);
        }
        else
        {
            LogError("Restart Text object on UI Manager is NULL");
        }

        StartCoroutine(GameOverFlashRoutine());
    }

    private IEnumerator GameOverFlashRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_gameOverFlashRate);
            if (!_isGameOverTextNull)
            {
                _gameOverText.gameObject.SetActive(!_gameOverText.gameObject.activeSelf);
            }
            else
            {
                LogError("Game Over Text object on UI Manager is NULL");
            }
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

        LogError("Main Camera is NULL on UI Manager");
        return new ViewportBounds();
    }
    
    public void LogError(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.LogError($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
    
    public void LogMessage(string message,
        [CallerMemberName] string callingMethod = "",
        [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0)
    {
        var className = Path.GetFileNameWithoutExtension(callingFilePath);
        Debug.Log($"{className}::{callingMethod}({callingFileLineNumber}): {message}!");
    }
}