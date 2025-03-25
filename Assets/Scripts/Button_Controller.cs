using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Controller : MonoBehaviour
{
    public GameObject instructions;
    public GameObject controlInformation;

    public void loadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void loadMainMenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
