using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private GameObject clonePrefab;

    [SerializeField]
    private PlayerController[] playerControllers;

    [SerializeField]
    private TextMeshProUGUI timer;

    [SerializeField]
    private BallScript ball;

    private float timeRemaining = 10;
    private int roundNumber = 1;

    private bool timerIsRunning = true;


    // Start is called before the first frame update
    void Start()
    {
        CloneManager.Configure(clonePrefab, playerControllers);
    }

    void Update()
    {

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }

            else
            {
                timeRemaining = 0;
                DisplayTime(timeRemaining);
                Debug.Log("end of round");
                timerIsRunning = false;
                CloneManager.AddClones();
                doNextRoundStuff();
            }
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void doNextRoundStuff()
    {
        roundNumber++;
        Debug.Log("starting new round " + roundNumber);
        timerIsRunning = true;
        timeRemaining = 10;
        CloneManager.SpawnClones();
        foreach (PlayerController player in playerControllers)
        {
            player.Reset();
        }
        ball.Reset();
    }

}
