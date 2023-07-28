using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenHandler : MonoBehaviour
{
    public void OnPlayButton(int playerChoice)
    {
        //Player Choice Int Controls Both Difficulty, Gamemode and Color
        SceneManager.LoadScene("Chess");
        PlayerPrefs.SetInt("StartValue", playerChoice);
    }
    public void onQuitButton()
    {
        Application.Quit();
    }
    public void onHomeScreenButton()
    {
        SceneManager.LoadScene("StartScreen");
    }
}
