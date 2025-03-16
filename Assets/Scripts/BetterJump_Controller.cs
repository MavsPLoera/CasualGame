using UnityEngine;

public class BetterJump_Controller : MonoBehaviour
{
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
