using System;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    public Vector3[] directions;
    public Quaternion[] rotations;
    public int positionSkipFrames;
    public int rotationSkipFrames;

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
        if (frame/(positionSkipFrames + 1) < directions.Length)
        {
            if (frame % positionSkipFrames == 0)
            {
                int nextIndex = frame / (positionSkipFrames + 1);
                nextPos = directions[nextIndex];
            }
            // move whatever fraction of the way to the target is necessary
            Vector3 partialMove = transform.position + (nextPos - transform.position)/(positionSkipFrames + 1);
            //Debug.Log(partialMove);
            rb.MovePosition(partialMove);
            frame++;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Quaternion GetNextRotation(float timeSinceLastUpdate)
    {
        float howFarToSlerp = (timeSinceLastUpdate / Time.fixedDeltaTime) + (frame % (rotationSkipFrames + 1)) / (rotationSkipFrames + 1);
        int currentRot = Math.Min(frame / (rotationSkipFrames + 1), rotations.Length - 1);
        int nextRot = Math.Min(currentRot + 1, rotations.Length - 1);
        return Quaternion.Slerp(rotations[currentRot], rotations[nextRot], howFarToSlerp);
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
}
