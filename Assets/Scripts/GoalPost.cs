using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPost : MonoBehaviour
{
    [SerializeField]
    private ScoringManager scoringManager;

    [SerializeField]
    private int playerGoal;

    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private PlayerMovement[] playerMovements;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball") {
            if (playerGoal == 1)
            {
                scoringManager.PlayerGoal(2);
            }
            else
            {
                scoringManager.PlayerGoal(1);
            }

            ball.SetActive(false);
            Invoke("ResetBall", 1);
            
            foreach (PlayerMovement playerMovement in playerMovements) {
                playerMovement.StartExplosion(GameConfigurations.goalExplosionSpeed, GameConfigurations.goalExplosionFrame, transform.position);
            }
        }
    }

    private void ResetBall() {
        ball.SetActive(true);
        ball.GetComponent<BallScript>().Reset();
    }
}