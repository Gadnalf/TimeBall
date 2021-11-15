using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CloneManager;

public class PlayerRecording : MonoBehaviour
{
    public Queue<Vector3> lastPositions = new Queue<Vector3>();
    public Queue<Quaternion> lastRotations = new Queue<Quaternion>();

    public int postionFramesToSkip = 3;
    public int rotationFramesToSkip = 3;
    private int frame = 0;

    public Queue<int> throwInputChangeFrames;
    private bool lastThrowInput;

    private float timeLeftToRecord = GameConfigurations.roundDuration;

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

            frame++;
        }
    }

    public void RecordInput(bool throwInput)
    {
        if (throwInput != lastThrowInput)
        {
            throwInputChangeFrames.Enqueue(frame);
            lastThrowInput = throwInput;
        }
        frame++;
    }

    public CloneData GetPlayerData()
    {
        return new CloneData() { Number = GetComponent<PlayerData>().playerNumber, PositionSkipFrames = postionFramesToSkip, RotationSkipFrames = rotationFramesToSkip, Positions = lastPositions.ToArray(), Rotations = lastRotations.ToArray(), ThrowInputs = throwInputChangeFrames.ToArray()};
    }

    public void Reset()
    {
        timeLeftToRecord = FindObjectOfType<GameManager>().GetTimeLeft();
        lastPositions.Clear();
        lastRotations.Clear();
    }
}