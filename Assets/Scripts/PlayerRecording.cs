using System.Collections.Generic;
using UnityEngine;
using static CloneManager;

public class PlayerRecording : MonoBehaviour
{
    public Queue<Vector3> lastPositions = new Queue<Vector3>();
    public Queue<Quaternion> lastRotations = new Queue<Quaternion>();

    public int postionFramesToSkip = 3;
    public int rotationFramesToSkip = 3;

    public Queue<int> throwInputChangeFrames = new Queue<int>();
    private bool lastThrowInput;

    public Queue<int> passInputChangeFrames = new Queue<int>();
    private bool lastPassInput;

    public Queue<int> guardInputChangeFrames = new Queue<int>();
    private bool lastGuardInput;

    private float timeLeftToRecord = GameConfigurations.roundDuration;
    private float roundStartTime = 0;

    private int frame;

    private void Start()
    {
        roundStartTime = Time.time;
        frame = 0;
    }

    private void FixedUpdate() {
        frame = Mathf.FloorToInt((Time.time - roundStartTime) / Time.fixedDeltaTime);
    }

    public void RecordLocation()
    {
        if (timeLeftToRecord > 0)
        {
            timeLeftToRecord -= Time.deltaTime;
            if (frame % (postionFramesToSkip + 1) == 0)
            {
                lastPositions.Enqueue(transform.position);
            }

            if (frame % (rotationFramesToSkip + 1) == 0)
            {
                lastRotations.Enqueue(transform.rotation);
            }
        }
    }

    public void RecordThrowInput(bool throwInput)
    {
        if (throwInput != lastThrowInput)
        {
            if (throwInputChangeFrames.Count == 0 || throwInputChangeFrames.Peek() != frame)
            {
                throwInputChangeFrames.Enqueue(frame);
                lastThrowInput = throwInput;
            }
        }
    }

    public void RecordPassInput(bool passInput)
    {
        if (passInput != lastPassInput)
        {
            if (passInputChangeFrames.Count == 0 || passInputChangeFrames.Peek() != frame)
            {
                passInputChangeFrames.Enqueue(frame);
                lastPassInput = passInput;
            }
        }
    }

    public void RecordGuardInput(bool guardInput)
    {
        if (guardInput != lastGuardInput)
        {
            if (guardInputChangeFrames.Count == 0 || guardInputChangeFrames.Peek() != frame)
            {
                guardInputChangeFrames.Enqueue(frame);
                lastGuardInput = guardInput;
            }
        }
    }

    public CloneData GetPlayerData()
    {
        bool inTutorial = FindObjectOfType<GameManager>() == null;

        return new CloneData() {
            PlayerNumber = GetComponent<PlayerData>().playerNumber,
            RoundNumber = inTutorial ? FindObjectOfType<TutorialManager>().GetRoundNumber() : FindObjectOfType<GameManager>().GetRoundNumber(),
            PositionSkipFrames = postionFramesToSkip,
            RotationSkipFrames = rotationFramesToSkip,
            Positions = lastPositions.ToArray(),
            Rotations = lastRotations.ToArray(),
            ThrowInputs = throwInputChangeFrames.ToArray(),
            PassInputs = passInputChangeFrames.ToArray(),
            GuardInputs = guardInputChangeFrames.ToArray()
        };
    }

    public void Reset()
    {
        bool inTutorial = FindObjectOfType<GameManager>() == null;

        roundStartTime = Time.time;
        frame = 0;
        timeLeftToRecord = inTutorial ? FindObjectOfType<TutorialManager>().GetTimeLeft() : FindObjectOfType<GameManager>().GetTimeLeft();
        lastPositions.Clear();
        lastRotations.Clear();
        throwInputChangeFrames.Clear();
        lastThrowInput = false;
        lastPassInput = false;
        lastGuardInput = false;
    }
}