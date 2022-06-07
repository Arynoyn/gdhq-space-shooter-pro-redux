using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, Controls.IUIActions
{
    private SpawnManager _spawnManager;
    private UIManager _uiManager;
    
    private bool _isGameOver;
    
    private void Awake()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager in Game Manager class is NULL");
        }
        
        _uiManager = FindObjectOfType<UIManager>();
        if (_uiManager == null)
        {
            Debug.LogError("UI Manager in Game Manager class is NULL");
        }
    }
    
    public void OnRestartLevel(InputAction.CallbackContext context)
    {
        if (_isGameOver)
        {
            SceneManager.LoadScene("Main_Menu");
        }
    }

    public void OnExitGame(InputAction.CallbackContext context) { 
        Debug.Log("GameManager::OnExitGame - OnExitGame() Called");
        Application.Quit(); 
    }

    private void GameOver()
    {
        _isGameOver = true;
        _uiManager.DisplayGameOver();
        _spawnManager.StopSpawningEnemies();
        _spawnManager.StopSpawningPowerups();
    }
    
    public void StartGame()
    {
        _spawnManager.StartSpawningEnemies();
        _spawnManager.StartSpawningPowerups();
    }
    
    public void SetScore(int score)
    {
        if (_uiManager != null)
        {
            _uiManager.SetScore(score);
        }
    }

    public void SetLives(int lives)
    {
        if (_uiManager != null)
        {
            _uiManager.SetLives(lives);
            if (lives < 1)
            {
                GameOver();
            }
        }
    }
}