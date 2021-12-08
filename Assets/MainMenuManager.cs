using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    private Canvas canvas;
    private GameObject menuPanel;
    private GameObject videoPanel;
    private GameObject controlPanel;
    private VideoPlayer videoPlayer;

    public PlayerInput input;

    private bool videoActive;
    private bool controlsActive;

    private void Awake()
    {
        videoActive = true;
        controlsActive = false;

        videoPlayer = FindObjectOfType<VideoPlayer>();
        canvas = FindObjectOfType<Canvas>(true);
        menuPanel = canvas.transform.Find("MainPanel").gameObject;
        videoPanel = canvas.transform.Find("Video").gameObject;
        controlPanel = canvas.transform.Find("ControlPanel").gameObject;

        videoPlayer.loopPointReached += OnVideoOver;
    }

    public void OnStartInput()
    {
        Debug.Log("Start registered");
        if (videoActive)
        {
            CloseVideo();
        }
        if (controlsActive)
        {
            CloseControls();
        }
    }

    private void StartGame()
    {
        LobbySceneVariables.SetNextScene("MainScene");
        SceneManager.LoadScene("LobbyScene");
    }

    private void StartTutorial()
    {
        LobbySceneVariables.SetNextScene("TutorialScene");
        SceneManager.LoadScene("LobbyScene");
    }

    private void OpenControls()
    {
        controlsActive = true;
        controlPanel.SetActive(true);
    }

    private void CloseControls()
    {
        controlsActive = false;
        controlPanel.SetActive(false);
        menuPanel.SetActive(true);
        ActivateButtons();
    }

    private void Exit()
    {
        Application.Quit();
    }

    private void OnVideoOver(VideoPlayer vp)
    {
        CloseVideo();
    }

    private void CloseVideo()
    {
        videoActive = false;
        videoPlayer.Stop();
        videoPanel.SetActive(false);
        menuPanel.SetActive(true);
        ActivateButtons();
    }

    private void ActivateButtons()
    {
        Button[] buttons = canvas.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "StartGame":
                    button.onClick.AddListener(StartGame);
                    break;
                case "Tutorial":
                    button.onClick.AddListener(StartTutorial);
                    break;
                case "Options":
                    button.onClick.AddListener(OpenControls);
                    break;
                case "Exit":
                    button.onClick.AddListener(Exit);
                    break;
            }
        }
    }
}
