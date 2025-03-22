using UnityEngine;

public class Enemy_Controller : MonoBehaviour
{
    public GameObject player;
    private Player_Controller playerController;
    public float scoreAdded = 100f;
    public GameObject point1;
    public GameObject point2;
    public Rigidbody2D rb;
    public Transform goalPosition;
    public float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = player.GetComponent<Player_Controller>();

        goalPosition = point2.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 temp = goalPosition.position - transform.position;
        if(goalPosition == point2.transform)
        {
            rb.linearVelocity = new Vector2(speed, 0f);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, 0f);
        }

        if(Vector2.Distance(transform.position, goalPosition.position) < .05f  && goalPosition == point2.transform)
        {
            goalPosition = point1.transform;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (Vector2.Distance(transform.position, goalPosition.position) < .05f && goalPosition == point1.transform)
        {
            goalPosition = point2.transform;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public void death()
    {
        playerController.increaseScore(scoreAdded);
        Destroy(gameObject);
    }
}
