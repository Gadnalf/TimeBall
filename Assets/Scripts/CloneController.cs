using System;
using UnityEngine;
using static CloneManager;

public class CloneController : MonoBehaviour
{
    public CloneData cloneData;

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject trailPrefab;

    // State data
    private Rigidbody rb;
    private float roundStartTime;
    private float lastFrameTime;
    private float pauseOffset;
    private int frame;
    private Vector3 nextPos;
    private float paused;

    private CloneHitByBall ballScript;
    public bool throwInput { get; private set; }
    private int nextThrowInputChangeIndex;

    public bool passInput { get; private set; }
    private int nextPassInputChangeIndex;

    private CloneGuard guardScript;
    public bool guardInput { get; private set; }
    private int nextGuardInputChangeIndex;

    private GameObject lr;
    private GameObject tr;

    private Animator animator;
    private float VelocityX, VelocityZ;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        roundStartTime = Time.time;
        rb = GetComponent<Rigidbody>();
        guardScript = GetComponentInChildren<CloneGuard>();
        ballScript = GetComponent<CloneHitByBall>();
        frame = 0;
        throwInput = false;
        passInput = false;
        guardInput = false;
        nextThrowInputChangeIndex = 0;
    }

    void FixedUpdate()
    {
        if (frame / (cloneData.PositionSkipFrames + 1) < cloneData.Positions.Length)
        {
            if (frame % cloneData.PositionSkipFrames == 0)
            {
                int nextIndex = frame / (cloneData.PositionSkipFrames + 1);
                SetupLines(nextIndex);
                nextPos = cloneData.Positions[nextIndex];
            }

            if (cloneData.ThrowInputs.Length > nextThrowInputChangeIndex)
            {
                if (frame == cloneData.ThrowInputs[nextThrowInputChangeIndex])
                {
                    throwInput = !throwInput;
                    nextThrowInputChangeIndex++;
                }
            }

            if (cloneData.PassInputs.Length > nextPassInputChangeIndex)
            {
                if (frame == cloneData.PassInputs[nextPassInputChangeIndex])
                {
                    passInput = !passInput;
                    nextPassInputChangeIndex++;
                }
            }

            if (cloneData.GuardInputs.Length > nextGuardInputChangeIndex)
            {
                if (frame == cloneData.GuardInputs[nextGuardInputChangeIndex])
                {
                    guardInput = !guardInput;
                    nextGuardInputChangeIndex++;
                }
            }
            guardScript.UpdateGuard(guardInput && !ballScript.HasBall());

            // move whatever fraction of the way to the target is necessary
            Vector3 move = (nextPos - transform.position) / (cloneData.PositionSkipFrames + 1);
            Vector3 partialMove = transform.position + move;

            HandleAnimation(move, true, false);
            rb.MovePosition(partialMove);

            lastFrameTime = Time.time;
            if (paused <= 0)
            {
                frame = Mathf.FloorToInt((Time.time - roundStartTime - pauseOffset) / Time.fixedDeltaTime);
            }
            else
            {
                float timeDelta = Time.time - lastFrameTime;
                pauseOffset += timeDelta;
                paused -= timeDelta;
            }
        }
    }

    void HandleAnimation(Vector3 move, bool moving, bool dashing)
    {
        if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo)
        {
            Debug.Log(move.magnitude);
        }
        if ((dashing || moving) && move.magnitude > 0.01)
        {
            VelocityZ = Vector3.Dot(move.normalized, transform.forward);
            VelocityX = Vector3.Dot(move.normalized, transform.right);
        }
        else
        {
            VelocityX = 0;
            VelocityZ = 0;
        }

        animator.SetFloat("VelocityZ", VelocityZ, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityX", VelocityX, 0.1f, Time.deltaTime);
    }

    public void SetupLines(int index)
    {

        Vector3[] positions = new Vector3[3];
        if (index + 2 < cloneData.Positions.Length)
        {
            positions[0] = cloneData.Positions[index];
            positions[1] = cloneData.Positions[index + 1];
            positions[2] = cloneData.Positions[index + 2];
        }
        else if (index + 1 < cloneData.Positions.Length)
        {
            positions[0] = cloneData.Positions[index];
            positions[1] = cloneData.Positions[index + 1];
        }
        else
        {
            positions[0] = cloneData.Positions[index];
        }

        if (lr != null)
        {
            Destroy(lr);
        }
        lr = Instantiate(linePrefab, positions[0], Quaternion.identity);
        lr.transform.parent = null;
        if (cloneData.PlayerNumber == PlayerData.PlayerNumber.PlayerOne)
        {
            lr.GetComponent<LineRenderer>().material.color = Color.blue;
        }
        else
        {
            lr.GetComponent<LineRenderer>().material.color = Color.red;
        }
        lr.GetComponent<LineRenderer>().useWorldSpace = true;
        lr.GetComponent<LineRenderer>().positionCount = positions.Length;
        lr.GetComponent<LineRenderer>().startWidth = 1.0f;
        lr.GetComponent<LineRenderer>().endWidth = 1.0f;
        lr.GetComponent<LineRenderer>().SetPositions(positions);
    }

    public Quaternion GetNextRotation(float timeSinceLastUpdate)
    {
        float howFarToSlerp = (timeSinceLastUpdate / Time.fixedDeltaTime) + (frame % (cloneData.RotationSkipFrames + 1)) / (cloneData.RotationSkipFrames + 1);
        int currentRot = Math.Min(frame / (cloneData.RotationSkipFrames + 1), cloneData.Rotations.Length - 1);
        int nextRot = Math.Min(currentRot + 1, cloneData.Rotations.Length - 1);
        return Quaternion.Slerp(cloneData.Rotations[currentRot], cloneData.Rotations[nextRot], howFarToSlerp);
    }

    public int FramesToNextThrow()
    {
        int nextInputDownIndex = !throwInput ? nextThrowInputChangeIndex : nextThrowInputChangeIndex + 1;
        if (cloneData.ThrowInputs.Length > nextInputDownIndex)
        {
            return cloneData.ThrowInputs[nextInputDownIndex] - frame;
        }
        else
        {
            return -1;
        }
    }

    public int FramesToNextPass()
    {
        int nextInputDownIndex = !passInput ? nextPassInputChangeIndex : nextPassInputChangeIndex + 1;
        if (cloneData.PassInputs.Length > nextInputDownIndex)
        {
            return cloneData.PassInputs[nextInputDownIndex] - frame;
        }
        else
        {
            return -1;
        }
    }

    public void SetData(CloneData cloneData)
    {
        this.cloneData = cloneData;
    }

    public void Pause()
    {
        paused = GameConfigurations.cloneMaxPauseSeconds;
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

