using UnityEngine;

public class BulletTimePickUp_Controller : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player_Controller>().bulletPickUp();
            audioSource.PlayOneShot(audioClip);
            Destroy(gameObject);
        }
    }
}
