using UnityEngine;

public class BetterJump_Controller : MonoBehaviour
{
    //Followed this tutorial for jumping https://www.youtube.com/watch?v=7KiK0Aqtmzc&t=0s
    //This video also gives a good explanation on movement https://www.youtube.com/watch?v=STyY26a_dPY

    public float fallForce = 2.5f;
    public float earlyFallForce = 2f;
    public float maxFallSpeed;
    public Rigidbody2D rb;


    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        if (rb.linearVelocityY < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallForce - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocityY > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (earlyFallForce - 1) * Time.deltaTime;
        }
    }
}
