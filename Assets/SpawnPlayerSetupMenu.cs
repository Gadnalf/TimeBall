using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour
{

    [SerializeField] 
    private GameObject playerSetupMenuPrefab;

    public PlayerInput playerInput;

    private void Awake()
    {
        var rootMenu = GameObject.Find("MainLayout");

        if (rootMenu != null)
        {
            var menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            playerInput.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(playerInput.playerIndex);
        }

    }


}
