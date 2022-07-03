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

    private void Start()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("Main Camera is Null on UI Manager!");
        }
        else
        {
            _cameraShaker = _camera.GetComponent<CameraShake>();
            if (_cameraShaker == null)
            {
                Debug.LogError("Camera Shaker is Null on UI Manager!");
            }
        }
        _isScoreTextNull = _scoreText == null;
        if (_isScoreTextNull) { Debug.Log("Score Text object on UI Manager is NULL"); }
        
        _isLivesDisplayNull = _livesDisplay == null;
        if (_isLivesDisplayNull) { Debug.Log("Lives Display Image on UI Manager is NULL"); }
        
        _isLivesImagesNull = _livesImages == null;
        if (_isLivesImagesNull) { Debug.Log("Lives Images Array on UI Manager is NULL"); }
        
        _isGameOverTextNull = _gameOverText == null;
        if (_isGameOverTextNull) { Debug.Log("Game Over Text object on UI Manager is NULL"); }
        
        _isRestartTextNull = _restartText == null;
        if (_isRestartTextNull) { Debug.Log("Restart Text object on UI Manager is NULL"); }
    }

    public void SetScore(int score)
    {
        if (!_isScoreTextNull)
        {
            _scoreText.text = score.ToString();
        }
        else
        {
            Debug.LogError("Score Text object on UI Manager is NULL");
        }

        if (!_isGameOverTextNull)
        {
            _gameOverText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Game Over Text object on UI Manager is NULL");
        }

        if (!_isRestartTextNull)
        {
            _restartText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Restart Text object on UI Manager is NULL");
        }
    }

    public void SetLives(int lives)
    {
        if (_isLivesDisplayNull || _isLivesImagesNull)
        {
            if (_isLivesDisplayNull)
            {
                Debug.LogError("Lives Display Image on UI Manager is NULL");
            }

            if (_isLivesImagesNull)
            {
                Debug.LogError("Lives Images Array on UI Manager is NULL");
            }

            return;
        }

        if (_livesImages.Length >= lives)
        {
            _livesDisplay.sprite = _livesImages[lives];
        }
        else
        {
            Debug.LogError($"Index {lives} is out of bounds of the array of lives images");
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
            Debug.LogError("Camera Shaker is missing from UI Manager!");
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
            Debug.LogError("Game Over Text object on UI Manager is NULL");
        }

        if (!_isRestartTextNull)
        {
            _restartText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Restart Text object on UI Manager is NULL");
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
                Debug.LogError("Game Over Text object on UI Manager is NULL");
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

        Debug.LogError("Main Camera is NULL on UI Manager");
        return new ViewportBounds();
    }
}