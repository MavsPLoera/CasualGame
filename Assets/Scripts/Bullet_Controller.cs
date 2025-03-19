using UnityEngine;

public class Bullet_Controller : MonoBehaviour
{
    public Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    public float bulletSpeed = 5.0f;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
        rb.AddForce(transform.right * bulletSpeed);
        Destroy(gameObject, 2f);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("enemy"))
        {
            //Do enemy damage stuff
        }
        else if(!collision.CompareTag("Player"))
        {
            rb.linearVelocity = Vector3.zero;;
            animator.SetBool("destroyed", true);
            circleCollider.enabled = false;
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
