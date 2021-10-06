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
            
        }
    }
}