using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{

    private int PlayerIndex;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private Button readyBtn;

    [SerializeField]
    private Sprite[] deviceImages;

    [SerializeField]
    private Image deviceImage;

    public void SetPlayerIndex(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;

        if (PlayerIndex == 0)
        {
            titleText.SetText("Player 1");
            titleText.color = new Color(0, 78, 251);
        }
        else
        {
            titleText.SetText("Player 2");
            titleText.color = new Color(219, 8, 0);
        }

        if (pi.currentControlScheme == "Gamepad")
        {
            deviceImage.sprite = deviceImages[0];
        }
        else
        {
            deviceImage.sprite = deviceImages[1];
        }

        //readyBtn.Select();

    }

    //public void SetPrefab(GameObject prefab)
    //{

    //    PlayerConfigManager.Instance.SetPlayerPrefab(PlayerIndex, prefab);
    //}

    public void ReadyPlayerUp()
    {
        PlayerConfigManager.Instance.ReadyPlayer(PlayerIndex);
        readyBtn.gameObject.SetActive(false);
    }
}
