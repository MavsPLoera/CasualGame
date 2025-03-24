using Unity.VisualScripting;
using UnityEngine;

public class Enemy_Controller : MonoBehaviour
{
    public float scoreAdded = 100f;
    public float speed;
    public bool dontMove = false;
    public GameObject point1;
    public GameObject point2;
    public AudioClip deathSound;
    public AudioSource soundPlayer;
    private Rigidbody2D rb;
    private Transform goalPosition;
    private BoxCollider2D boxCollider;
    
    public ParticleSystem deathParticles; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        soundPlayer = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        goalPosition = point2.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(dontMove)
        {
            return;
        }


        //Move between teo transform positions. If the goal position is met rotate the enemy and set the other point for the goal.
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
        dontMove = true;
        boxCollider.enabled = false;
        Player_Controller.instance.increaseScore(scoreAdded);
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        float temp = Random.Range(.90f, 1.1f);
        soundPlayer.pitch = temp;
        soundPlayer.PlayOneShot(deathSound);

        deathParticles.Play();
        Destroy(gameObject, deathSound.length);
    }
}
