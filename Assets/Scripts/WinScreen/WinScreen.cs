using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    IEnumerator ReturnToMainMenuRoutine()
    {
        yield return new WaitForSeconds(32);
        SceneManager.LoadScene("Main_Menu");
    }
}
