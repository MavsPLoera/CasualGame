using UnityEngine;

public class CameraFollow_Controller : MonoBehaviour
{
    /*
     * Followed https://www.youtube.com/watch?v=GTxiCzvYNOc&t=306s
     * on how to be able to make a camera that has threshholds.
     */

    public GameObject player;
    private Rigidbody2D rb;
    public float speed = 3f;
    public Vector2 followOffset;
    private Vector2 threshold;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = player.GetComponent<Rigidbody2D>();
        threshold = calculateThreshold();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 follow = player.transform.position;

        //Get the difference between the player X position and the camera X positon
        //Same for the Y
        float xDifference = Vector2.Distance(Vector2.right * transform.position.x, Vector2.right * follow.x);
        float yDifference = Vector2.Distance(Vector2.up * transform.position.y, Vector2.up * follow.y);

        //Make the current positon the same and update the values if the player exceeds the threshold values
        Vector3 newPosition = transform.position;
        if(Mathf.Abs(xDifference) >= threshold.x)
        {
            newPosition.x = follow.x;
        }

        if (Mathf.Abs(yDifference) >= threshold.y)
        {
            newPosition.y = follow.y;
        }

        //If statement to change speed depending on if the player or the camera is faster.
        float moveSpeed = rb.linearVelocity.magnitude > speed ? rb.linearVelocity.magnitude : speed;
        transform.position = Vector3.MoveTowards(transform.position, newPosition, moveSpeed * Time.deltaTime);
    }

    //Get the screen dimensions using some math that the video mentions, No idea I could even do this. What is a rect??!?!?
    private Vector3 calculateThreshold()
    {
        Rect aspect = Camera.main.pixelRect;
        Vector2 dimensions = new Vector2(Camera.main.orthographicSize * aspect.width / aspect.height, Camera.main.orthographicSize);
        dimensions.x -= followOffset.x;
        dimensions.y -= followOffset.y;

        return dimensions;
    }

    //On Shows example box, gizoms are so useful.
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Vector2 border = calculateThreshold();

        Gizmos.DrawWireCube(transform.position, new Vector3(border.x * 2, border.y * 2, 1));
    }
}
