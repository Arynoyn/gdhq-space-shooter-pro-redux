using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _gameOverText;
    [SerializeField] private Sprite[] _livesImages;
    [SerializeField] private Image _livesDisplay;

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
    }
}