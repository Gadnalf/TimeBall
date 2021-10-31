using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{

    private int PlayerIndex;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private GameObject readyPanel;

    [SerializeField]
    private GameObject menuPanel;

    [SerializeField]
    private Button readyBtn;

    public void SetPlayerIndex(int pi)
    {
        PlayerIndex = pi;
        titleText.SetText("Player " + (pi + 1).ToString());
    }

    public void SetPrefab(GameObject prefab)
    {

        PlayerConfigManager.Instance.SetPlayerPrefab(PlayerIndex, prefab);
        Debug.Log("this stuff is happening");
        readyPanel.SetActive(true);
        readyBtn.Select();
        menuPanel.SetActive(false);

    }

    public void ReadyPlayerUp()
    {
        PlayerConfigManager.Instance.ReadyPlayer(PlayerIndex);
        readyBtn.gameObject.SetActive(false);
    }



}
