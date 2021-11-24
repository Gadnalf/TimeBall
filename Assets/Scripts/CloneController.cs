using System;
using UnityEngine;
using static CloneManager;

public class CloneController : MonoBehaviour
{

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject trailPrefab;
    public CloneData cloneData;

    // State data
    private Rigidbody rb;
    private int frame;
    private Vector3 nextPos;
    private int paused;
    public bool throwInput { get; private set; }
    private int nextThrowInputChange;

    private GameObject lr;
    private GameObject tr;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        frame = 0;
        throwInput = false;
        nextThrowInputChange = 0;
    }

    void FixedUpdate()
    {
        if (frame/(cloneData.PositionSkipFrames + 1) < cloneData.Positions.Length)
        {
            if (frame % cloneData.PositionSkipFrames == 0)
            {
                int nextIndex = frame / (cloneData.PositionSkipFrames + 1);
                nextPos = cloneData.Positions[nextIndex];
            }

            if (cloneData.ThrowInputs.Length > nextThrowInputChange)
            {
                if (frame == cloneData.ThrowInputs[nextThrowInputChange])
                {
                    throwInput = !throwInput;
                    nextThrowInputChange++;
                }
            }

            // move whatever fraction of the way to the target is necessary
            Vector3 partialMove = transform.position + (nextPos - transform.position)/(cloneData.PositionSkipFrames + 1);
            //Debug.Log(partialMove);
            rb.MovePosition(partialMove);
            if (paused <= 0)
            {
                frame++;
            }
            else
            {
                paused -= 1;
            }
        }
    }

    public void SetupLines()
    {
        Vector3[] positions = cloneData.Positions;
        if (tr != null)
        {
            Destroy(tr);
        }
        tr = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity);
        tr.transform.parent = null;
        if (cloneData.Number == PlayerData.PlayerNumber.PlayerOne)
        {
            tr.GetComponent<TrailRenderer>().startColor = Color.blue;
            tr.GetComponent<TrailRenderer>().endColor = Color.blue;
        }
        else
        {
            tr.GetComponent<TrailRenderer>().startColor = Color.red;
            tr.GetComponent<TrailRenderer>().endColor = Color.red;
        }
        tr.GetComponent<TrailRenderer>().time = 5;
        tr.GetComponent<TrailRenderer>().AddPositions(positions);
        //if (lr != null)
        //{
        //    Destroy(lr);
        //}
        //lr = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        //lr.transform.parent = null;
        //if (cloneData.Number == PlayerData.PlayerNumber.PlayerOne)
        //{
        //    lr.GetComponent<LineRenderer>().material.color = Color.blue;
        //}
        //else
        //{
        //    lr.GetComponent<LineRenderer>().material.color = Color.red;
        //}
        //lr.GetComponent<LineRenderer>().useWorldSpace = true;
        //lr.GetComponent<LineRenderer>().positionCount = positions.Length;
        //lr.GetComponent<LineRenderer>().startWidth = 0.3f;
        //lr.GetComponent<LineRenderer>().endWidth = 0.3f;
        //lr.GetComponent<LineRenderer>().SetPositions(positions);

    }

    public Quaternion GetNextRotation(float timeSinceLastUpdate)
    {
        float howFarToSlerp = (timeSinceLastUpdate / Time.fixedDeltaTime) + (frame % (cloneData.RotationSkipFrames + 1)) / (cloneData.RotationSkipFrames + 1);
        int currentRot = Math.Min(frame / (cloneData.RotationSkipFrames + 1), cloneData.Rotations.Length - 1);
        int nextRot = Math.Min(currentRot + 1, cloneData.Rotations.Length - 1);
        return Quaternion.Slerp(cloneData.Rotations[currentRot], cloneData.Rotations[nextRot], howFarToSlerp);
    }

    public void SetData(CloneData cloneData)
    {
        this.cloneData = cloneData;
    }

    public void Pause()
    {
        paused = GameConfigurations.cloneMaxPauseFrames;
    }

    public void Unpause()
    {
        paused = 0;
    }

    public void Kill()
    {
        if (lr != null)
            Destroy(lr.gameObject);
        if (tr != null)
            Destroy(tr.gameObject);
        Destroy(gameObject);
    }
}
