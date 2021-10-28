using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject[] clonePrefabs;
    private static PlayerMovement[] players;
    private static int cloneCap = 10;

    private static Queue<CloneData> clones = new Queue<CloneData>();

    public static void Configure(GameObject[] prefabs, PlayerMovement[] playerList, int cap = 10)
    {
        clonePrefabs = prefabs;
        players = playerList;
        cloneCap = cap;
    }

    public static void AddClones()
    {
        foreach (PlayerMovement player in players)
        {
            CloneData c = new CloneData() { Number = player.playerNumber, SkipFrames = player.framesToSkip, Positions = player.lastPositions.ToArray() };
            clones.Enqueue(c);
            if(clones.Count > cloneCap)
            {
                clones.Dequeue();
            }
        }
    }

    public static void SpawnClones()
    {
        foreach (CloneData clone in clones)
        {
            GameObject newClone;
            if (clone.Number == PlayerData.PlayerNumber.PlayerOne) {
                newClone = Object.Instantiate(clonePrefabs[0], clone.Positions[0], Quaternion.identity);
            }
            else {
                newClone = Object.Instantiate(clonePrefabs[1], clone.Positions[0], Quaternion.identity);
            }
            
            CloneController controller = newClone.GetComponent<CloneController>();
            controller.directions = clone.Positions;
            controller.skipFrames = clone.SkipFrames;
            controller.GetComponent<PlayerData>().playerNumber = clone.Number;
        }
    }

    public static void DeleteClones()
    {
        clones.Clear();
    }

    private class CloneData
    {
        public PlayerData.PlayerNumber Number { get; set; }
        public int SkipFrames { get; set; }
        public Vector3[] Positions { get; set; }
    }
}
