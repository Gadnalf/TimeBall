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

    public void RecordInput(bool throwInput, int frame)
    {
        if (throwInput != lastThrowInput)
        {
            throwInputChangeFrames.Enqueue(frame);
            lastThrowInput = throwInput;
        }
    }

    public CloneData GetPlayerData()
    {
        return new CloneData() { 
            Number = GetComponent<PlayerData>().playerNumber, 
            PositionSkipFrames = postionFramesToSkip, 
            RotationSkipFrames = rotationFramesToSkip, 
            Positions = lastPositions.ToArray(), 
            Rotations = lastRotations.ToArray(), 
            ThrowInputs = throwInputChangeFrames.ToArray()
        };
    }

    public void Reset()
    {
        timeLeftToRecord = FindObjectOfType<GameManager>().GetTimeLeft();
        lastPositions.Clear();
        lastRotations.Clear();
        throwInputChangeFrames.Clear();
        lastThrowInput = false;
    }
}