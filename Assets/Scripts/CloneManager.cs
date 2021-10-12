using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject clonePrefab;
    private static PlayerMovement[] players;
    private static int cloneCap = 10;

    private static Queue<CloneData> clones = new Queue<CloneData>();

    public static void Configure(GameObject prefab, PlayerMovement[] playerList, int cap = 10)
    {
        clonePrefab = prefab;
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
            GameObject newClone = Object.Instantiate(clonePrefab, clone.Positions[0], Quaternion.identity);
            CloneController controller = newClone.GetComponent<CloneController>();
            controller.directions = clone.Positions;
            controller.skipFrames = clone.SkipFrames;
            controller.GetComponent<PlayerData>().playerNumber = clone.Number;
        }
    }

    private class CloneData
    {
        public PlayerData.PlayerNumber Number { get; set; }
        public int SkipFrames { get; set; }
        public Vector3[] Positions { get; set; }
    }
}
