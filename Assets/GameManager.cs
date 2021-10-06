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

    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject pauseMenuPanel;

    [SerializeField]
    private GameObject endMenuPanel;

    private float timeRemaining = 10;
    private int roundNumber = 1;

    private bool timerIsRunning = true;

    public static bool gameStarted = false;

    public static bool gamePaused = true;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
        CloneManager.Configure(clonePrefab, playerControllers);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!gameStarted)
            {
                StartGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gamePaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }


        if (!gamePaused && gameStarted)
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
    }

    public void StartGame()
    {
        Debug.Log("button work");
        gameStarted = true;
        gamePaused = false;
        Time.timeScale = 1f;
        mainMenuPanel.SetActive(false);
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
        if (roundNumber == 5)
        {
            EndGame();
            return;
        }
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

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
    }

    public void EndGame()
    {
        gameStarted = false;
        gamePaused = true;
        endMenuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

}
