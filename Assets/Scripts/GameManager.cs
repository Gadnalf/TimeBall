using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int[] playerScores;

    [SerializeField] private PlayerData.PlayerNumber currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerScores = new int[]{0, 0};
    }

    public int GetPlayerOneScore() {
        return playerScores[0];
    }

    public int GetPlayerTwoScore() {
        return playerScores[1];
    }

    public void AddPlayerOneScore() {
        playerScores[0]++;
    }

    public void AddPlayerTwoScore() {
        playerScores[1]++;
    }

    public void PlayerGoal() {
        int playerNum;

        switch (currentPlayer) {
            case (PlayerData.PlayerNumber.PlayerOne):
                playerNum = 0;
                break;
            case (PlayerData.PlayerNumber.PlayerTwo):
                playerNum = 1;
                break;
            default:
                break;
        }
    }
}
