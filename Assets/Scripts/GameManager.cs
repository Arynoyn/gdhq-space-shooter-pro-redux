using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, Controls.IUIActions
{
    private bool _isGameOver;


    public void OnRestartLevel(InputAction.CallbackContext context)
    {
        if (_isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }
}