using UnityEngine;

public class PlayerLivesPickUp_Controller : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("coliision");
        if (collision.gameObject.CompareTag("Player"))
        {
            Player_Controller player = collision.gameObject.GetComponent<Player_Controller>();
            player.playerLives++;
            player.controller.updatePlayerLives();
            audioSource.PlayOneShot(audioClip);
            Destroy(gameObject);
        }
    }
}
