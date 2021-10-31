using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject[] clonePrefabs;
    private static PlayerMovement[] players;
    private static int cloneCap = 3;

    private static Queue<CloneData> clones = new Queue<CloneData>();

    public static void Configure(GameObject[] prefabs, PlayerMovement[] playerList, int cap = 3)
    {
        clonePrefabs = prefabs;
        players = playerList;
        cloneCap = cap;
    }

    public static void AddClones()
    {
        foreach (PlayerMovement player in players)
        {
            CloneData c = new CloneData() { Number = player.playerNumber, PositionSkipFrames = player.postionFramesToSkip, RotationSkipFrames = player.rotationFramesToSkip, Positions = player.lastPositions.ToArray(), Rotations = player.lastRotations.ToArray() };
            clones.Enqueue(c);
            if(clones.Count > cloneCap * 2)
            {
                clones.Dequeue();
            }
        }
    }

    public static void KillClones()
    {
        foreach (CloneController clone in GameObject.FindObjectsOfType<CloneController>())
        {
            clone.Kill();
        }
    }

    public static void SpawnClones()
    {
        foreach (CloneData clone in clones)
        {
            GameObject newClone;
            if (clone.Number == PlayerData.PlayerNumber.PlayerOne) {
                newClone = Object.Instantiate(clonePrefabs[0], clone.Positions[0], clone.Rotations[0]);
            }
            else {
                newClone = Object.Instantiate(clonePrefabs[1], clone.Positions[0], clone.Rotations[0]);
            }
            
            CloneController controller = newClone.GetComponent<CloneController>();
            controller.directions = clone.Positions;
            controller.rotations = clone.Rotations;
            controller.rotationSkipFrames = clone.RotationSkipFrames;
            controller.positionSkipFrames = clone.PositionSkipFrames;
        }
    }

    public static void DeleteClones()
    {
        clones.Clear();
    }

    private class CloneData
    {
        public PlayerData.PlayerNumber Number { get; set; }
        public int PositionSkipFrames { get; set; }
        public int RotationSkipFrames { get; set; }
        public Vector3[] Positions { get; set; }
        public Quaternion[] Rotations { get; set; }
    }
}
