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
    private PlayerMovement[] playerControllers;

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

    private float timeRemaining = GameConfigurations.roundDuration;
    private int roundNumber = 1;

    private bool timerIsRunning = false;

    private bool gameStarted = false;

    private bool gamePaused = false;

    PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.MainMenu.StartGame.performed += ctx =>
        {
            if (!gameStarted)
            {
                StartGame();
            }

        };

        //controls.MainMenu.StartGame.canceled += ctx =>
        //{
        //    gamePaused = true;
        //};

        controls.MainMenu.PauseGame.started += ctx =>
        {

            if (gameStarted)
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

        };

    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
        CloneManager.Configure(clonePrefab, playerControllers);
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = false;
        }
    }

    void Update()
    {

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
        //Debug.Log("button work");
        gameStarted = true;
        gamePaused = false;
        Time.timeScale = 1f;
        timerIsRunning = true;
        mainMenuPanel.SetActive(false);
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = true;
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
        if (roundNumber == 5)
        {
            EndGame();
            return;
        }
        Debug.Log("starting new round " + roundNumber);
        timerIsRunning = true;
        timeRemaining = GameConfigurations.roundDuration;
        CloneManager.SpawnClones();
        foreach (PlayerMovement player in playerControllers)
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
        foreach (PlayerMovement player in playerControllers)
        {
            player.GetComponent<PlayerMovement>().enabled = false;
        }
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        foreach (PlayerMovement player in playerControllers)
        {
            player.GetComponent<PlayerMovement>().enabled = true;
        }
    }

    public void EndGame()
    {
        gameStarted = false;
        gamePaused = true;
        endMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = false;
        }
    }

    private void OnEnable()
    {
        controls.MainMenu.Enable();
    }

    private void OnDisable()
    {
        controls.MainMenu.Disable();
    }

}
