using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayerConfigManager : MonoBehaviour
{

    private List<PlayerConfig> playerConfigs;

    private int MaxPlayers = 2;

    public static PlayerConfigManager Instance { get; private set; }

    [SerializeField]
    private AudioManager audioManager;

    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private GameObject instructions;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("trying to create another playerconfigmanager.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfig>();
        }
    }

    //public void SetPlayerPrefab(int index, GameObject prefab)
    //{
    //    playerConfigs[index].PlayerPrefab = prefab;
    //}

    public void ReadyPlayer(int index)
    {
        playerConfigs[index].IsReady = true;

        if (playerConfigs.Count == MaxPlayers) {
            bool b = true;
            foreach (PlayerConfig pc in playerConfigs) {
                if (!pc.IsReady)
                    b = false;
            }

            if (b) {
                GetComponent<PlayerInputManager>().DisableJoining();
                audioManager.GetAudio("Lobby").Stop();

                // SceneManager.LoadScene("TutorialScene");
                SceneManager.LoadScene("MainScene");
            }
        }
    }

    public List<PlayerConfig> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public void HandlePlayerJoin(PlayerInput pi)
    {
        instructions.SetActive(false);
        mainPanel.SetActive(true);
        if (playerConfigs.Count >= MaxPlayers) return;

        Debug.Log("Player " + pi.playerIndex.ToString() + " joined.");

        foreach (PlayerConfig pc in playerConfigs)
        {
            if (pc.PlayerIndex == pi.playerIndex)
                return;
        }

        pi.transform.SetParent(transform);
        playerConfigs.Add(new PlayerConfig(pi));

    }

}

public class PlayerConfig
{

    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    //public GameObject PlayerPrefab { get; set; }

    public PlayerConfig(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }

}
