using UnityEngine;

public class CloneController : MonoBehaviour
{
    public Vector3[] directions;
    public int skipFrames;

    // State data
    private Rigidbody rb;
    private int frame;
    private Vector3 nextPos;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        frame = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (frame < directions.Length)
        {
            if (frame % skipFrames == 0)
            {
                nextPos = directions[frame/skipFrames];
            }
            // move whatever fraction of the way to the target is necessary
            Vector3 partialMove = transform.position + (nextPos - transform.position)/(skipFrames + 1);
            Debug.Log(partialMove);
            rb.MovePosition(partialMove);
            frame++;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
