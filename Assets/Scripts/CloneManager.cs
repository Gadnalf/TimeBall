using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject[] clonePrefabs;
    private static PlayerRecording[] players;
    private static int cloneCap = 3;

    private static Queue<CloneData> clones = new Queue<CloneData>();

    public static void Configure(GameObject[] prefabs, PlayerRecording[] playerList, int cap = 3)
    {
        clonePrefabs = prefabs;
        players = playerList;
        cloneCap = cap;
    }

    public static void AddClones()
    {
        foreach (PlayerRecording player in players)
        {
            clones.Enqueue(player.GetPlayerData());
            if (clones.Count > cloneCap * 2)
            {
                clones.Dequeue();
            }
        }
    }

    public static void KillClones()
    {
        foreach (CloneController clone in Object.FindObjectsOfType<CloneController>())
        {
            clone.Kill();
        }
    }

    public static void SpawnClones()
    {
        foreach (CloneData clone in clones)
        {
            GameObject newClone;
            if (clone.PlayerNumber == PlayerData.PlayerNumber.PlayerOne)
            {
                newClone = Object.Instantiate(clonePrefabs[0], clone.Positions[0], clone.Rotations[0]);
            }
            else
            {
                newClone = Object.Instantiate(clonePrefabs[1], clone.Positions[0], clone.Rotations[0]);
            }

            CloneController controller = newClone.GetComponent<CloneController>();
            controller.SetData(clone);
        }
    }

    public static void DeleteClones()
    {
        clones.Clear();
    }

    public class CloneData
    {
        public PlayerData.PlayerNumber PlayerNumber { get; set; }
        public int RoundNumber { get; set; }
        public int PositionSkipFrames { get; set; }
        public int RotationSkipFrames { get; set; }
        public Vector3[] Positions { get; set; }
        public Quaternion[] Rotations { get; set; }
        public int[] ThrowInputs { get; set; }
        public int[] PassInputs { get; set; }
        public int[] GuardInputs { get; set; }
    }
}
