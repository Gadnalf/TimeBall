using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringManager : MonoBehaviour
{
    private int[] playerScores;

    [SerializeField] private PlayerData.PlayerNumber currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        playerScores = new int[] { 0, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        // should detect which player last thrown ball is from
        // currently always set to player one
        currentPlayer = PlayerData.PlayerNumber.PlayerOne;
    }

    private int GetPlayerNumber(PlayerData.PlayerNumber player) {
        int playerNum;

        switch (player) {
            case (PlayerData.PlayerNumber.PlayerOne):
                playerNum = 0;
                break;
            case (PlayerData.PlayerNumber.PlayerTwo):
                playerNum = 1;
                break;
            default:
                playerNum = -1;
                Debug.LogError("Invalid player number");
                break;
        }

        return playerNum;
    }

    public int GetPlayerScore(PlayerData.PlayerNumber player) {
        int playerNum = GetPlayerNumber(player);

        return playerScores[playerNum];
    }

    public void AddPlayerScore(PlayerData.PlayerNumber player) {
        int playerNum = GetPlayerNumber(player);

        playerScores[playerNum]++;
    }

    public void PlayerGoal() {
        AddPlayerScore(currentPlayer);
    }
}
