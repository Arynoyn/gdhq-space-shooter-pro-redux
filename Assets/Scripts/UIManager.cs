using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Text _restartText;
    [SerializeField] private Sprite[] _livesImages;
    [SerializeField] private Image _livesDisplay;
    [SerializeField] private float _gameOverFlashRate = 0.5f;

    public void SetScore(int score)
    {
        _scoreText.text = score.ToString();
    }
    
    public void SetLives(int lives)
    {
        _livesDisplay.sprite = _livesImages[lives];
    }
    
    public void DisplayGameOver()
    {
        _gameOverText.gameObject.SetActive(true);
        _restartText.gameObject.SetActive(true);
        StartCoroutine(GameOverFlashRoutine());
    }
    
    IEnumerator GameOverFlashRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_gameOverFlashRate);
            _gameOverText.gameObject.SetActive(!_gameOverText.gameObject.activeSelf);
        }
    }
}