using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }
}
