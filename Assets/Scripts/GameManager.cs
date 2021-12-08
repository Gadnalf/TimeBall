using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private GameObject[] clonePrefabs;

    [SerializeField]
    private PlayerMovement[] playerControllers;

    [SerializeField]
    private PlayerRecording[] playerRecordings;

    [SerializeField]
    private TextMeshProUGUI timer;
    private bool increaseSize;

    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private BallScript ball;

    [SerializeField]
    private GameObject preparePanel;

    [SerializeField]
    private GameObject pauseMenuPanel;

    [SerializeField]
    private GameObject endMenuPanel;

    [SerializeField]
    private Image winnerImage;

    [SerializeField]
    private TextMeshProUGUI roundText;

    [SerializeField]
    private ScoringManager scoringManager;

    [SerializeField]
    private GameObject controlsPanel;

    [SerializeField]
    private GameObject[] playerPrefabs;

    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private Sprite[] winnerSprites;

    private float timeRemaining = GameConfigurations.roundDuration;
    private int roundNumber = 1;

    private bool roundEndTimeSlowdown = false;

    private bool timerIsRunning = false;

    private bool gamePrepare = false;
    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool gameEnded = false;

    PlayerControls controls;

    private AudioManager audioManager;
    private AudioSource runningWithouBall;
    private AudioSource stadiumCrowd;
    private AudioSource gameTheme;

    private void Awake()
    {
        GameConfigurations.roundDuration = 30f;
        var playerConfigs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray();
        var goals = GameObject.FindGameObjectsWithTag("Goals");

        for (int i = 0; i < playerConfigs.Length; i++) {
            var player = Instantiate(playerPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation, gameObject.transform);
            player.GetComponent<PlayerMovement>().InitializePlayer(playerConfigs[i]);
            playerRecordings[i] = player.GetComponent<PlayerRecording>();
            playerControllers[i] = player.GetComponent<PlayerMovement>();
            playerControllers[i].SetTutorial(false);

            foreach (GameObject goal in goals) {
                goal.GetComponent<GoalPost>().playerMovements[i] = player.GetComponent<PlayerMovement>();
            }
        }

        controls = new PlayerControls();
        controls.MainMenu.StartGame.performed += ctx =>
        {
            if (!gameStarted && !gameEnded && !gamePrepare) {
                PrepareStartGame();
            }

            if (!gameStarted && gameEnded)
            {
                Debug.Log("game ended");
                CloneManager.DeleteClones();
                SceneManager.LoadScene("MainScene");
            }
        };

        controls.MainMenu.ShowControls.performed += ctx =>
        {
            if (!gameStarted && !gameEnded)
            {
                if (controlsPanel.activeInHierarchy)
                {
                    ShowControlsPanel(false);
                }
                else
                {
                    ShowControlsPanel(true);
                }
            }
        };

        controls.MainMenu.PauseGame.started += ctx =>
        {
            if (gameStarted && !gameEnded && !gamePrepare)
            {
                if (!gamePaused) {
                    PauseGame();
                }
                else {
                    ResumeGame();
                }
            }
        };

    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
        ball.gameObject.SetActive(false);

        CloneManager.Configure(clonePrefabs, playerRecordings);
        foreach (PlayerMovement player in playerControllers)
        {
            player.GetComponent<PlayerMovement>().enabled = false;
        }

        audioManager = FindObjectOfType<AudioManager>();
        runningWithouBall = audioManager.GetAudio("Running");
        stadiumCrowd = audioManager.GetAudio("Crowd");
        gameTheme = audioManager.GetAudio("Game");
        increaseSize = true;
    }

    void Update()
    {
        if (gamePrepare) {
            if (timeRemaining > 0) {
                timeRemaining -= Time.unscaledDeltaTime;
                DisplaySecondsOnly(timeRemaining);
            }
            
            else {
                timeRemaining = 0;
                DisplaySecondsOnly(timeRemaining);
                gamePrepare = false;
                timer.transform.localScale /= 2;
                StartGame();
            }
        }

        else {
            roundText.text = roundNumber.ToString();

            if (!gamePaused && gameStarted) {
                if (timerIsRunning) {
                    if (timeRemaining > 0) {
                        if (timeRemaining <= GameConfigurations.nearEndingTime) {
                            roundEndTimeSlowdown = true;
                            Time.timeScale = GameConfigurations.slowTimeScale;
                        }

                        timeRemaining -= Time.deltaTime;
                        timeRemaining = Math.Max(timeRemaining, 0f);
                        DisplayTime(timeRemaining);
                    }
                    else {
                        timeRemaining = 0;
                        DisplayTime(timeRemaining);
                        timerIsRunning = false;
                        CloneManager.AddClones();
                        doNextRoundStuff();

                        Time.timeScale = GameConfigurations.normalTimeScale;
                        roundEndTimeSlowdown = false;
                    }
                }
            }

            if (!ball.enabled)
                ball.enabled = true;

            if (playerControllers[0].ShouldStopRunningSound() && playerControllers[1].ShouldStopRunningSound()) {
                if (runningWithouBall.isPlaying)
                    runningWithouBall.Stop();
            }
        }

        if (gameEnded) {
            Time.timeScale = 0;
            foreach (PlayerMovement player in playerControllers) {
                player.GetComponent<PlayerMovement>().enabled = false;
            }
        }
    }

    public void PrepareStartGame() {
        timeRemaining = GameConfigurations.nearEndingTime;
        timer.transform.localScale *= 2;
        gamePrepare = true;
        mainPanel.SetActive(true);
        preparePanel.SetActive(false);
    }

    private void StartGame() {
        Time.timeScale = GameConfigurations.normalTimeScale;
        gameStarted = true;
        gamePaused = false;
        roundEndTimeSlowdown = false;
        timerIsRunning = true;
        timeRemaining = GameConfigurations.roundDuration;
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = true;
        }
        ball.gameObject.SetActive(true);
        stadiumCrowd.Play();
        gameTheme.Play();
    }

    public void ShowControlsPanel(bool value)
    {
        controlsPanel.SetActive(value);
    }

    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timeRemaining < GameConfigurations.nearEndingTime && timeRemaining > 0)
        {
            timer.color = Color.red;
            float factor = Time.deltaTime;
            if (increaseSize)
                timer.transform.localScale *= (1 + factor * 2);
            else
                timer.transform.localScale /= (1 + factor * 2);
            if (timer.transform.localScale.x >= 3 || timer.transform.localScale.x <= 1)
                increaseSize = !increaseSize;
        }
        else
            timer.color = Color.white;
    }

    private void DisplaySecondsOnly(float timeToDisplay) {
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timer.color = Color.red;
        timer.text = string.Format("{0}", seconds);
    }

    private void doNextRoundStuff()
    {
        timer.transform.localScale = Vector3.one;

        CloneManager.KillClones();
        //Debug.Log("started round " + roundNumber.ToString());

        if (roundNumber == GameConfigurations.numberOfRounds)
        {
            EndGame();
            return;
        }
        roundNumber++;
        timerIsRunning = true;
        
        timeRemaining = GameConfigurations.roundDuration + Math.Min((roundNumber - 1) * GameConfigurations.roundLengthIncrease, GameConfigurations.maxRoundLength);
        CloneManager.SpawnClones();
        foreach (PlayerMovement player in playerControllers)
        {
            player.Reset();
        }

        foreach (UICloneNumberScript playerOverlayScript in FindObjectsOfType<UICloneNumberScript>())
        {
            playerOverlayScript.Reset();
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

        stadiumCrowd.Stop();
        gameTheme.Stop();
    }

    public void ResumeGame()
    {
        gamePaused = false;
        if (!roundEndTimeSlowdown)
            Time.timeScale = GameConfigurations.normalTimeScale;
        else
            Time.timeScale = GameConfigurations.slowTimeScale;
        pauseMenuPanel.SetActive(false);
        foreach (PlayerMovement player in playerControllers)
        {
            player.GetComponent<PlayerMovement>().enabled = true;
        }

        stadiumCrowd.Play();
        gameTheme.Play();
    }

    public void EndGame()
    {
        stadiumCrowd.Stop();
        gameTheme.Stop();

        gameStarted = false;
        gameEnded = true;
        gamePaused = true;
        endMenuPanel.SetActive(true);
        int winner = scoringManager.GetWinner();
        if (winner == 0)
        {
            winnerImage.sprite = winnerSprites[0];
        }
        else
        {
            winnerImage.sprite = winnerSprites[winner];
        }

        Time.timeScale = 0f;
        roundEndTimeSlowdown = false;
        foreach (PlayerMovement player in playerControllers)
        {
            player.GetComponent<PlayerMovement>().SetIdle();
            player.GetComponent<PlayerMovement>().enabled = false;
        }
    }

    public float GetTimeLeft()
    {
        return timeRemaining;
    }

    public int GetRoundNumber()
    {
        return roundNumber;
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
