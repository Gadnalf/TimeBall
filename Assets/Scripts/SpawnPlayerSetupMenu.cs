using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour
{

    [SerializeField]
    private GameObject playerSetupMenuPrefab;

    [SerializeField]
    private PlayerInput playerInput;

    private void Awake()
    {
        var rootMenu = GameObject.Find("MainPanel");

        if (rootMenu != null)
        {
            GameObject menu;
            if (playerInput.playerIndex == 0)
            {
                menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
                menu.GetComponent<RectTransform>().anchoredPosition = new Vector3(-319, 75);
            }
            else
            {
                menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
                menu.GetComponent<RectTransform>().anchoredPosition = new Vector3(319, 75);
            }
            
            playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(playerInput);
        }

    }


}
