using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] clonePrefabs;

    [SerializeField]
    private PlayerMovement[] playerControllers;

    [SerializeField]
    private PlayerRecording[] playerRecordings;

    [SerializeField]
    private TextMeshProUGUI timer;

    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private BallScript[] balls;

    [SerializeField]
    private GoalPost[] opponentGoalPosts;

    [SerializeField]
    private GameObject helpPanel;
    [SerializeField]
    private TextMeshProUGUI[] helpTexts;
    private int[] playerCurrentHelp = new int[2];
    public string[] roundOneHelps;
    public string[] roundTwoHelps;
    public string[] roundThreeHelps;
    public string[] roundFourHelps;
    public string[] roundFiveHelps;
    [SerializeField]
    private GameObject preparePanel;
    private TextMeshProUGUI prepareText;

    [SerializeField]
    private GameObject pauseMenuPanel;

    [SerializeField]
    private GameObject endMenuPanel;

    [SerializeField]
    private GameObject controlsPanel;

    [SerializeField]
    private GameObject[] playerPrefabs;

    [SerializeField]
    private GameObject mainPanel;

    private float timeRemaining = GameConfigurations.roundDuration;
    private int roundNumber = 1;

    private bool timerIsRunning = false;

    private bool gamePrepare = false;
    private bool gameStarted = false;
    private bool gamePaused = false;
    private bool gameEnded = false;

    private bool roundEnd = true;

    PlayerControls controls;

    private AudioManager audioManager;
    private AudioSource runningWithouBall;
    private AudioSource stadiumCrowd;
    private AudioSource gameTheme;

    private HashSet<int> playerTutorialFinished = new HashSet<int>();
    [SerializeField]
    private GameObject[] playerNextRoundReady;

    private void Awake()
    {
        GameConfigurations.roundDuration = 3600f;
        var playerConfigs = PlayerConfigManager.Instance.GetPlayerConfigs().ToArray();
        var goals = GameObject.FindGameObjectsWithTag("Goals");

        for (int i = 0; i < playerConfigs.Length; i++)
        {
            var player = Instantiate(playerPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation, gameObject.transform);
            player.GetComponent<PlayerMovement>().InitializePlayer(playerConfigs[i]);
            playerRecordings[i] = player.GetComponent<PlayerRecording>();
            playerControllers[i] = player.GetComponent<PlayerMovement>();
            playerControllers[i].SetTutorial(true);

            foreach (GameObject goal in goals) {
                goal.GetComponent<GoalPost>().playerMovements[i] = player.GetComponent<PlayerMovement>();
            }

        }

        controls = new PlayerControls();
        controls.MainMenu.StartGame.performed += ctx =>
        {
            if (!gameStarted && !gameEnded && !gamePrepare && roundEnd) {
                PrepareStartGame();
            }

            else if (gameStarted && !gameEnded && !gamePrepare && roundEnd) {
                PrepareNextRound();
            }

            else if (!gameStarted && gameEnded)
            {
                Debug.Log("game ended");
                CloneManager.DeleteClones();
                SceneManager.LoadScene("LobbyScene");
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
            if (gameStarted && !gameEnded && !roundEnd && !gamePrepare)
            {
                if (!gamePaused)
                    PauseGame();
                else
                    ResumeGame();       
            }
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;

        CloneManager.Configure(clonePrefabs, playerRecordings);
        foreach (PlayerMovement player in playerControllers)
        {
            if (player.playerNumber == PlayerData.PlayerNumber.PlayerOne) {
                player.GetComponent<PlayerMovement>().spawnLocation = spawnPoints[0].position;
                player.GetComponent<PlayerMovement>().spawnRotation = spawnPoints[0].rotation.eulerAngles;
            }
            else {
                player.GetComponent<PlayerMovement>().spawnLocation = spawnPoints[1].position;
                player.GetComponent<PlayerMovement>().spawnRotation = spawnPoints[1].rotation.eulerAngles;
            }
            player.GetComponent<PlayerMovement>().enabled = false;
        }

        prepareText = preparePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        audioManager = FindObjectOfType<AudioManager>();
        runningWithouBall = audioManager.GetAudio("Running");
        stadiumCrowd = audioManager.GetAudio("Crowd");
        gameTheme = audioManager.GetAudio("Game");
    }

    void Update()
    {
        // Debug.Log("Player distance to opponent goal post: " + (playerControllers[0].transform.position - opponentGoalPosts[0].transform.position).magnitude.ToString());

        if (gamePrepare) {
            if (timeRemaining > 0) {
                timeRemaining -= Time.unscaledDeltaTime;
                timeRemaining = Math.Max(timeRemaining, 0);
                DisplaySecondsOnly(timeRemaining);
            }
            
            else {
                timeRemaining = 0;
                DisplaySecondsOnly(timeRemaining);
                gamePrepare = false;

                if (roundNumber == 1)
                    StartGame();
                else
                    StartRound();
            }
        }

        else {
            if (!gamePaused && gameStarted) {
                if (timerIsRunning) {
                    if (playerTutorialFinished.Count < 2) {
                        runTutorial();
                    }

                    else {
                        timeRemaining = 0;
                        timerIsRunning = false;
                        CloneManager.AddClones();
                        FinishRound();
                    }
                }
            }

            foreach (var ball in balls) {
                if (!ball.enabled)
                    ball.enabled = true;
            }

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

    public void PrepareNextRound() {
        foreach (GameObject x in playerNextRoundReady)
            x.SetActive(false);

        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = true;
        }

        if (roundNumber == 2) {
            CloneManager.KillClones();
            CloneManager.DeleteClones();
        }

        doNextRoundStuff();
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = false;
        }
        foreach (UICloneNumberScript playerOverlayScript in FindObjectsOfType<UICloneNumberScript>()) {
            playerOverlayScript.HideCloneNumbers(true);
        }

        if (roundNumber == 3) {
            foreach (BallScript ball in balls)
                ball.gameObject.SetActive(false);
        }
        else if (roundNumber == 4) {
            for (int i = 0; i <= 1; i++)
                waitAndSetBall(i, 1.5f);
        }
        else if (roundNumber == 3) {
            foreach (BallScript ball in balls)
                ball.gameObject.SetActive(false);
        }

        preparePanel.SetActive(false);
        if (!gameEnded) {
            helpPanel.SetActive(true);
            showStartHelp();

            timer.gameObject.SetActive(true);
            timeRemaining = GameConfigurations.nearEndingTime;
            timer.transform.localScale *= 2;

            roundEnd = false;
            gamePrepare = true;
        }

        playerTutorialFinished.Clear();
    }

    private void waitAndSetBall(int player, float seconds) {
        IEnumerator coroutine = setBallCoroutine(player, seconds);
        StartCoroutine(coroutine);
    }

    private IEnumerator setBallCoroutine(int player, float seconds) {
        balls[player].gameObject.SetActive(false);
        yield return new WaitForSeconds(seconds);
        balls[player].gameObject.SetActive(true);
        playerControllers[player].GetComponent<PlayerThrowBall>().ClaimBall(balls[player].gameObject);
    }

    public void StartRound() {
        timer.gameObject.SetActive(false);
        Time.timeScale = GameConfigurations.normalTimeScale;

        timerIsRunning = true;
        timeRemaining = GameConfigurations.roundDuration;
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = true;
        }
        foreach (UICloneNumberScript playerOverlayScript in FindObjectsOfType<UICloneNumberScript>()) {
            playerOverlayScript.HideCloneNumbers(false);
        }

        gamePrepare = false;

        if (roundNumber == 2) {
            foreach (PlayerMovement player in playerControllers) {
                setMovement(player, 10f);
            }
        }
        else if (roundNumber == 3) {
            foreach (BallScript ball in balls)
                ball.gameObject.SetActive(false);
        }
        else if (roundNumber == 4) {
            foreach (PlayerMovement player in playerControllers) {
                setMovement(player, 1.5f);
            }
        }
        else if (roundNumber == 5) {
            prepareNext = true;
            foreach (BallScript ball in balls)
                ball.gameObject.SetActive(false);
        }
    }

    private void setMovement (PlayerMovement player, float seconds) {
        IEnumerator coroutine = allowMovementCoroutine(player, seconds);
        StartCoroutine(coroutine);
    }

    private IEnumerator allowMovementCoroutine (PlayerMovement player, float seconds) {
        player.canMove = false;
        yield return new WaitForSeconds(seconds);
        player.canMove = true;
    }

    public void FinishRound() {
        Time.timeScale = 0f;
        timerIsRunning = false;

        timeRemaining = GameConfigurations.roundDuration;
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = false;
        }

        preparePanel.SetActive(true);
        helpPanel.SetActive(false);

        prepareText.text = "Well Done!";

        roundEnd = true;
    }

    private void PrepareStartGame() {
        mainPanel.SetActive(true);
        preparePanel.SetActive(false);
        helpPanel.SetActive(true);
        showStartHelp();

        timer.gameObject.SetActive(true);
        timeRemaining = GameConfigurations.nearEndingTime;
        timer.transform.localScale *= 2;

        gamePrepare = true;
    }

    private void StartGame() {
        gameStarted = true;
        gamePaused = false;
        roundEnd = false;

        Time.timeScale = GameConfigurations.normalTimeScale;
        timerIsRunning = true;
        timeRemaining = GameConfigurations.roundDuration;
        timer.gameObject.SetActive(false);
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = true;
        }

        foreach (var ball in balls) {
            ball.gameObject.SetActive(true);
        }

        stadiumCrowd.Play();
        gameTheme.Play();
    }

    public void ShowControlsPanel(bool value)
    {
        controlsPanel.SetActive(value);
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

        if (roundNumber == GameConfigurations.numberOfTutorials) {
            EndGame();
            return;
        }
        roundNumber++;
        timerIsRunning = true;
        
        timeRemaining = GameConfigurations.roundDuration + Math.Min((roundNumber - 1) * GameConfigurations.roundLengthIncrease, GameConfigurations.maxRoundLength);
        CloneManager.SpawnClones();
        foreach (PlayerMovement player in playerControllers) {
            player.Reset();
        }

        foreach (UICloneNumberScript playerOverlayScript in FindObjectsOfType<UICloneNumberScript>()) {
            playerOverlayScript.Reset();
        }

        foreach (var ball in balls) {
            ball.gameObject.SetActive(true);
            ball.Reset();
        }
    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        foreach (PlayerMovement player in playerControllers) {
            player.GetComponent<PlayerMovement>().enabled = false;
        }

        stadiumCrowd.Stop();
        gameTheme.Stop();
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = GameConfigurations.normalTimeScale;
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

        timeRemaining = 0f;
        timerIsRunning = false;
        Time.timeScale = 0f;
        foreach (PlayerMovement player in playerControllers) {
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

    public void ReadyPlayer(int index) {
        playerTutorialFinished.Add(index);
        playerNextRoundReady[index].SetActive(true);
        Debug.Log("Player ready in tutorial");
    }

    private string[] getHelpText() {
        switch (roundNumber) {
            case 1:
                return roundOneHelps;
            case 2:
                return roundTwoHelps;
            case 3:
                return roundThreeHelps;
            case 4:
                return roundFourHelps;
            case 5:
                return roundFiveHelps;
            default:
                return new string[] {""};
        }  
    }

    private void showStartHelp() {
        foreach (var ht in helpTexts) {
            ht.text = getHelpText()[0];
        }
        playerCurrentHelp[0] = 0;
        playerCurrentHelp[1] = 0;
    }

    private void showNextHelp(int player) {
        playerCurrentHelp[player]++;
        helpTexts[player].text = getHelpText()[playerCurrentHelp[player]];
    }

    private void runTutorial() {
        switch (roundNumber) {
            case 1:
                tutorialRoundOne();
                return;
            case 2:
                tutorialRoundTwo();
                return;
            case 3:
                tutorialRoundThree();
                return;
            case 4:
                tutorialRoundFour();
                return;
            case 5:
                tutorialRoundFive();
                return;
            default:
                return;
        }
    }

    private void tutorialRoundOne() {
        for (int i = 0; i <= 1; i++) {
            if (playerCurrentHelp[i] == 0 && playerControllers[i].hasBall()) {
                showNextHelp(i);
            }
            else if (playerCurrentHelp[i] == 1 && !balls[i].gameObject.activeInHierarchy) {
                showNextHelp(i);
            }
            else if (playerCurrentHelp[i] == 2 && balls[i].gameObject.activeInHierarchy) {
                balls[i].gameObject.SetActive(false);
            }
        }
    }

    private void tutorialRoundTwo() {
        for (int i = 0; i <= 1; i++) {
            if (playerCurrentHelp[i] == 0 && (!balls[i].gameObject.activeInHierarchy || playerControllers[i].hasBall())) {
                showNextHelp(i);
                playerControllers[i].canMove = true;
            }
            else if (playerCurrentHelp[i] == 1 && balls[i].gameObject.activeInHierarchy) {
                showNextHelp(i);
            }
            else if (playerCurrentHelp[i] == 2 && playerControllers[i].hasBall()) {
                showNextHelp(i);
            }
            else if (playerCurrentHelp[i] == 3 && !playerControllers[i].hasBall()) {
                showNextHelp(i);
                waitAndShowNextHelp(i, 5f);
            }
        }
    }

    private void tutorialRoundThree() {
        for (int i = 0; i <= 1; i++) {
            if (playerCurrentHelp[i] == 0 && (playerControllers[i].transform.position - opponentGoalPosts[i].transform.position).magnitude <= 15f) {
                showNextHelp(i);
                waitAndShowNextHelp(i);
            }
        }
    }

    private void tutorialRoundFour() {
        for (int i = 0; i <= 1; i++) {
            if (playerCurrentHelp[i] == 0 && playerControllers[i].hasBall()) {
                showNextHelp(i);
            }
            else if (playerCurrentHelp[i] == 1 && !balls[i].gameObject.activeInHierarchy) {
                showNextHelp(i);
            }
        }
    }

    private bool prepareNext;

    private void tutorialRoundFive() {
        for (int i = 0; i <= 1; i++) {
            if (playerCurrentHelp[i] == 0) {
                if (prepareNext) {
                    prepareNext = false;
                    waitAndShowNextHelp(i);
                }
            }
            else if (playerCurrentHelp[i] == 1) {
                if (prepareNext) {
                    prepareNext = false;
                    waitAndShowNextHelp(i, 5f);
                }
            }
            else if (playerCurrentHelp[i] == 2) {
                if (prepareNext) {
                    prepareNext = false;
                    waitAndShowNextHelp(i);
                }
            }
            else if (playerCurrentHelp[i] == 3) {
                if (prepareNext) {
                    prepareNext = false;
                    balls[i].gameObject.SetActive(true);
                    waitAndShowNextHelp(i, 5f);
                }
            }
        }
    }

    private void waitAndShowNextHelp(int player, float seconds=3f) {
        IEnumerator coroutine = nextStepCoroutine(player, seconds);
        StartCoroutine(coroutine);
    }

    private IEnumerator nextStepCoroutine(int player, float seconds) {
        yield return new WaitForSeconds(seconds);
        showNextHelp(player);
        prepareNext = true;
    }
}
