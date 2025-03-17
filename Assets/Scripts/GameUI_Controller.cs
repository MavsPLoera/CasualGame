using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameUI_Controller : MonoBehaviour
{
    [Header("Game UI")]
    public GameObject gameUI_Panel;
    public TextMeshProUGUI playerLivesText;
    public TextMeshProUGUI scoreText;
    public int score = 0;
    public TextMeshProUGUI timerText;
    public float time = 300.0f;
    public TextMeshProUGUI ammoStatusText;
    public GameObject[] bulletImages; 

    [Header("GameOver UI")]
    public GameObject gameOver_Panel;

    [Header("GameWin UI")]
    public GameObject gameWin_Panel;


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
        scoreText.text = score.ToString();
        playerController = player.GetComponent<Player_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        timerText.text = time.ToString(".");

        if(time <= 0f)
        {
            //Call gameover on player
        }
    }

    public void updateScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score.ToString();
    }

    public void updatePlayerLives()
    {
        playerLivesText.text = "Lives: " + playerController.playerLives.ToString();
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
}
