using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoringManager : MonoBehaviour
{
    private int[] playerScores;

    private PlayerData.PlayerNumber currentPlayer;

    [SerializeField]
    private TextMeshProUGUI playerOneScore;

    [SerializeField]
    private TextMeshProUGUI playerTwoScore;

    // Start is called before the first frame update
    void Start()
    {
        playerScores = new int[] { 0, 0 };
    }

    void Update()
    {
        playerOneScore.text = GetPlayerScore(PlayerData.PlayerNumber.PlayerOne).ToString();
        playerTwoScore.text = GetPlayerScore(PlayerData.PlayerNumber.PlayerTwo).ToString();
    }

    public void SetCurrentPlayer(PlayerData.PlayerNumber player)
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

    public void AddPlayerScore(PlayerData.PlayerNumber player, int playerNum, int score) {
        //int playerNum = GetPlayerNumber(player);

        playerScores[playerNum] += score;
    }

    public void PlayerGoal(int player, int score) {
        AddPlayerScore(currentPlayer, player - 1, score);
    }

    public int GetWinner()
    {
        if (playerScores[0] > playerScores[1])
            return 1;
        else if (playerScores[0] < playerScores[1])
            return 2;
        else
            return 0;
    }

}
