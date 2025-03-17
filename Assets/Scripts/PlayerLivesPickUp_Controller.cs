using UnityEngine;

public class PlayerLivesPickUp_Controller : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("coliision");
        if (collision.gameObject.CompareTag("Player"))
        {
            Player_Controller player = collision.gameObject.GetComponent<Player_Controller>();
            player.playerLives++;
            player.controller.updatePlayerLives();
            Destroy(gameObject);
        }
    }
}
