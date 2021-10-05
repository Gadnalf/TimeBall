using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject clonePrefab;
    private static PlayerController[] players;

    private static List<CloneData> clones = new List<CloneData>();

    public static void Configure(GameObject prefab, PlayerController[] playerList)
    {
        clonePrefab = prefab;
        players = playerList;
    }

    public static void AddClones()
    {
        
        foreach (PlayerController player in players)
        {
            Debug.Log(player);
            CloneData c = new CloneData() { SkipFrames = player.framesToSkip, Positions = player.lastPositions.ToArray() };
            clones.Add(c);
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
        }
    }

    private class CloneData
    {
        public int SkipFrames { get; set; }
        public Vector3[] Positions { get; set; }
    }
}
