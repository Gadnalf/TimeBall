using System.Collections.Generic;
using UnityEngine;

public static class CloneManager
{
    private static GameObject clonePrefab;
    private static PlayerController[] players;

    private static List<CloneData> clones;

    static void Configure(GameObject prefab, PlayerController[] playerList)
    {
        clonePrefab = prefab;
        players = playerList;
    }

    static void AddClones()
    {
        foreach (PlayerController player in players)
        {
            clones.Add(new CloneData() { SkipFrames = player.framesToSkip, Positions = player.lastPositions.ToArray() });
        }
    }

    static void SpawnClones()
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
