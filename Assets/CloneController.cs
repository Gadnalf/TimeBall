using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    private float delay;
    private Rigidbody rb;
    public PlayerController player;
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
        if (player.recordLength <= 0)
        {
            if (frame == 0)
            {
                nextPos = player.lastPositions.Dequeue();
            }
            // move whatever fraction of the way to the target is necessary
            Vector3 partialMove = transform.position + (nextPos - transform.position)/(player.framesToSkip + 1);
            Debug.Log(partialMove);
            rb.MovePosition(partialMove);
            frame = (frame + 1) % (player.framesToSkip + 1);
        }
    }
}
