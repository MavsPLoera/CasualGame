using TMPro;
using UnityEngine;

public class GameUI_Controller : MonoBehaviour
{
    [Header("Game UI")]
    public GameObject gameUI_Panel;
    public TextMeshProUGUI playerLivesText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public float time = 300.0f;
    public TextMeshProUGUI ammoStatusText;

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

        playerController = player.GetComponent<Player_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;

        timerText.text = time.ToString(".");
    }
}
