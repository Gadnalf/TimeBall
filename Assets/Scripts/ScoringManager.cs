using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoringManager : MonoBehaviour
{
    private int[] playerScores;

    private PlayerData.PlayerNumber currentPlayer;

    [SerializeField]
    private Text p1Score;

    [SerializeField]
    private Text p2Score;

    // Start is called before the first frame update
    void Start()
    {
        playerScores = new int[] { 0, 0 };
    }

    void Update()
    {
        p1Score.text = playerScores[0].ToString();
        p2Score.text = playerScores[1].ToString();
    }

    public void setCurrentPlayer(PlayerData.PlayerNumber player)
    {
        currentPlayer = player;
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

        for (int i = 0; i < playerScores.Length; i++)
        {
            print(playerScores[i]);
        }
    }

    public void PlayerGoal() {
        AddPlayerScore(currentPlayer);
    }
}
