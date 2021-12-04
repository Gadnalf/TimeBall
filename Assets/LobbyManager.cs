using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class LobbyManager : MonoBehaviour
{

    [SerializeField]
    private GameObject videoScreen;

    [SerializeField]
    private GameObject videoPlayer;

    [SerializeField]
    private GameObject instructions;

    [SerializeField]
    private AudioManager audioManager;

    [SerializeField]
    private GameObject playerManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager.GetAudio("Lobby").Play();
        videoPlayer.GetComponent<VideoPlayer>().loopPointReached += CheckOver;
    }

    void CheckOver(VideoPlayer vp)
    {
        playerManager.SetActive(true);
        instructions.SetActive(true);
    }

}
