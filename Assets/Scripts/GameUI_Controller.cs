using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameUI_Controller : MonoBehaviour
{
    [Header("Game UI")]
    public GameObject gameUI_Panel;
    public GameObject gameWin_Panel;
    public TextMeshProUGUI gameWinScore;
    public GameObject gameOver_Panel;
    public TextMeshProUGUI playerLivesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public float time = 300.0f;
    public TextMeshProUGUI ammoStatusText;
    public GameObject[] bulletImages;

    [Header("Player")]
    public GameObject player;
    public Player_Controller playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set status of other panels just in case.
        gameOver_Panel.SetActive(false);
        gameWin_Panel.SetActive(false);
        gameUI_Panel.SetActive(true);
        playerController = player.GetComponent<Player_Controller>();
        updateScore();
        updatePlayerLives();
    }

    // Update is called once per frame
    void Update()
    {
        if (time <= 0f)
        {
            playerController.playerCanInput = false;
            triggerGameLose();
        }

        time -= Time.deltaTime;
        timerText.text = time.ToString(".");
    }

    public void updateScore()
    {
        scoreText.text = "Score: " + playerController.score;
    }

    public void updatePlayerLives()
    {
        playerLivesText.text = "Lives: " + playerController.playerLives;
    }

    public void updateAmmoStatus(string message)
    {
        ammoStatusText.text = message;
    }

    public void updateAmmoImageUI()
    {
        for(int i = 0; i < playerController.maxAmmo; i++)
        {
            if (i < playerController.ammo)
            {
                bulletImages[i].SetActive(true);
            }
            else
            {
                bulletImages[i].SetActive(false);
            }
        }

    }

    //Change color of the bullet images when a sort of event happens.
    public void changeAmmoImageColor(Color color)
    {
        for (int i = 0; i < playerController.maxAmmo; i++)
        {
            bulletImages[i].GetComponent<SpriteRenderer>().color = color;
        }
    }

    public void triggerGameWin()
    {
        gameUI_Panel.SetActive(false);
        gameWin_Panel.SetActive(true);
        gameWinScore.text = "SCORE: " + playerController.score.ToString();
    }

    public void triggerGameLose()
    {
        gameUI_Panel.SetActive(false);
        gameOver_Panel.SetActive(true);
    }
}
