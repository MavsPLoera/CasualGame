using UnityEngine;

public class ScorePickUp_Controller : MonoBehaviour
{
    public AudioClip audioClip;
    public AudioSource audioSource;
    public float addToScore;

    //Not sure why I cant make this prefab with audio source.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player_Controller player = collision.gameObject.GetComponent<Player_Controller>();
            player.score += addToScore;
            player.controller.updateScore();
            audioSource.PlayOneShot(audioClip);
            Destroy(gameObject);
        }
    }
}
