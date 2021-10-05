using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPost : MonoBehaviour
{
    [SerializeField]
    private ScoringManager scoringManager;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball") {
            scoringManager.PlayerGoal();
        }
    }
}