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
    
    [Header("Game Over Text")] 
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Text _restartText;
    [SerializeField] private float _gameOverFlashRate = 0.5f;

    private bool _isGameOverTextNull;
    private bool _isScoreTextNull;
    private bool _isLivesDisplayNull;
    private bool _isLivesImagesNull;
    private bool _isRestartTextNull;

    private void Start()
    {
        _isScoreTextNull = _scoreText == null;
        _isLivesDisplayNull = _livesDisplay == null;
        _isLivesImagesNull = _livesImages == null;
        _isGameOverTextNull = _gameOverText == null;
        _isRestartTextNull = _restartText == null;
        if (_isScoreTextNull)
        {
            Debug.LogError("Score Text object on UI Manager is NULL");
        }

        if (_isLivesDisplayNull)
        {
            Debug.LogError("Lives Display Image on UI Manager is NULL");
        }

        if (_isLivesImagesNull)
        {
            Debug.LogError("Lives Images Array on UI Manager is NULL");
        }

        if (_isGameOverTextNull)
        {
            Debug.LogError("Game Over Text object on UI Manager is NULL");
        }

        if (_isRestartTextNull)
        {
            Debug.LogError("Restart Text object on UI Manager is NULL");
        }
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
}