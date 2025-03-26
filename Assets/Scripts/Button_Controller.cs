using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_Controller : MonoBehaviour
{
    public GameObject instructions;
    public GameObject controlInformation;
    public AudioClip buttonHoveredSound;
    public AudioSource audioSource;

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

    public void playButtonSound()
    {
        audioSource.PlayOneShot(buttonHoveredSound);
    }
}
