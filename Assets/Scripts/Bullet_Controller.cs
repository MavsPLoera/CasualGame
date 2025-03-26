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
        Destroy(gameObject, .5f);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            rb.linearVelocity = Vector3.zero;
            collision.gameObject.GetComponent<Enemy_Controller>().death();
            animator.SetBool("destroyed", true);
            circleCollider.enabled = false;
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
            rb.linearVelocity = Vector3.zero;
            animator.SetBool("destroyed", true);
            circleCollider.enabled = false;
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
