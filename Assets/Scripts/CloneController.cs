using UnityEngine;

public class CloneController : MonoBehaviour
{
    public Vector3[] directions;
    public int skipFrames;

    // State data
    private Rigidbody rb;
    private int frame;
    private Vector3 nextPos;
    private PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
        frame = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (frame/(skipFrames + 1) < directions.Length)
        {
            if (frame % skipFrames == 0)
            {
                nextPos = directions[frame/(skipFrames + 1)];
            }
            // move whatever fraction of the way to the target is necessary
            Vector3 partialMove = transform.position + (nextPos - transform.position)/(skipFrames + 1);
            //Debug.Log(partialMove);
            rb.MovePosition(partialMove);
            frame++;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
}
