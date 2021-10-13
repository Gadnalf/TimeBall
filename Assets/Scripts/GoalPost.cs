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

    private int explosionFrame;

    private void Start() {
        explosionFrame = 0;
    }

    private void FixedUpdate() {

        if (explosionFrame > 0) {
            float explosionFactor = GameConfigurations.explosionSpeed / GameConfigurations.explosionFrame;
            float explosionBonus = GameConfigurations.explosionSpeed - explosionFactor * (GameConfigurations.explosionFrame - explosionFrame);

            explosionFrame--;

            foreach (PlayerMovement playerMovement in playerMovements) {
                var direction = playerMovement.transform.position - transform.position;
                direction.y = 0;

                playerMovement.getRb().AddForce(direction.normalized * explosionBonus, ForceMode.Impulse);
            }
        }
    }

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
                explosionFrame = GameConfigurations.explosionFrame;
            }
        }
    }

    private void ResetBall() {
        ball.SetActive(true);
        ball.GetComponent<BallScript>().Reset();
    }
}