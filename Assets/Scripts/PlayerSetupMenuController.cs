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
    private Sprite[] titleImages;

    [SerializeField]
    private Image titleImage;

    [SerializeField]
    private Sprite[] deviceImages;

    [SerializeField]
    private Image deviceImage;

    [SerializeField]
    private Sprite[] buttonImages;

    [SerializeField]
    private Image readyBtn;

    [SerializeField]
    private TextMeshProUGUI readyText;

    public void SetPlayerIndex(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;

        titleImage.sprite = titleImages[PlayerIndex];

        readyBtn.sprite = buttonImages[PlayerIndex];

        if (PlayerIndex == 0)
        {
            readyText.color = new Color(0, 224, 255, 255);
        } else
        {
            readyText.color = new Color(255, 0, 110, 255);
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

    public void ReadyPlayerUp()
    {
        PlayerConfigManager.Instance.ReadyPlayer(PlayerIndex);
        readyBtn.gameObject.SetActive(false);
    }
}
