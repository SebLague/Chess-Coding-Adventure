using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenHandler : MonoBehaviour
{
    public void OnPlayButton(int playerChoice)
    {
        SceneManager.LoadScene("Chess");
        PlayerPrefs.SetInt("StartValue", playerChoice);
    }
    public void onQuitButton()
    {
        Application.Quit();
    }
}
