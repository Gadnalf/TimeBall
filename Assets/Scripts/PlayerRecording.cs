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

    public Queue<(int, int?)> passTargetChangeFrames = new Queue<(int, int?)>();
    private int? lastPassTarget;

    public Queue<int> guardInputChangeFrames = new Queue<int>();
    private bool lastGuardInput;

    private float timeLeftToRecord = GameConfigurations.roundDuration;

    public void RecordLocation(int frame)
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

    public void RecordThrowInput(bool throwInput, int frame)
    {
        if (throwInput != lastThrowInput)
        {
            throwInputChangeFrames.Enqueue(frame);
            lastThrowInput = throwInput;
        }
    }

    public void RecordPassInput(int? passTargetId, int frame)
    {
        if (passTargetId != lastPassTarget)
        {
            passTargetChangeFrames.Enqueue((frame, passTargetId));
            lastPassTarget = passTargetId;
        }
    }

    public void RecordGuardInput(bool guardInput, int frame)
    {
        if (guardInput != lastGuardInput)
        {
            guardInputChangeFrames.Enqueue(frame);
            lastGuardInput = guardInput;
        }
    }

    public CloneData GetPlayerData()
    {
        return new CloneData() {
            PlayerNumber = GetComponent<PlayerData>().playerNumber,
            RoundNumber = FindObjectOfType<GameManager>().GetRoundNumber(),
            PositionSkipFrames = postionFramesToSkip, 
            RotationSkipFrames = rotationFramesToSkip, 
            Positions = lastPositions.ToArray(), 
            Rotations = lastRotations.ToArray(), 
            ThrowInputs = throwInputChangeFrames.ToArray(),
            GuardInputs = guardInputChangeFrames.ToArray()
        };
    }

    public void Reset()
    {
        timeLeftToRecord = FindObjectOfType<GameManager>().GetTimeLeft();
        lastPositions.Clear();
        lastRotations.Clear();
        throwInputChangeFrames.Clear();
        lastThrowInput = false;
        lastPassTarget = null;
        lastGuardInput = false;
    }
}