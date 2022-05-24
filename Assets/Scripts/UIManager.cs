using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Text _scoreText;

    public void SetScoreText(int score)
    {
        _scoreText.text = score.ToString();
    }
}