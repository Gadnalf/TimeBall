using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChargeLevelDisplayer : MonoBehaviour
{
    private PlayerData.PlayerNumber playerNumber;

    private Image chargeLevel;
    private Camera playerCamera;

    [SerializeField]
    private BallScript ballScript;

    [SerializeField]
    private Sprite[] batteries;

    private void Start()
    {
        chargeLevel = GetComponent<Image>();

        // var canvasHeight = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;

        Camera[] playerCameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in playerCameras)
        {
            if (camera.name.StartsWith("P1") && gameObject.name == "P1ChargeLevel")
            {
                playerCamera = camera;
                var player = camera.transform.parent;
                var playerPos = player.GetComponent<Rigidbody>().position;
                var height = player.GetComponent<Collider>().bounds.size.y;
                var y = playerPos.y + height / 2 + 1.15f;
                transform.position = playerCamera.WorldToScreenPoint(new Vector3(playerPos.x, y, playerPos.z));
                playerNumber = (PlayerData.PlayerNumber)1;
            }
            else if (camera.name.StartsWith("P2") && gameObject.name == "P2ChargeLevel")
            {
                playerCamera = camera;
                var player = camera.transform.parent;
                var playerPos = player.GetComponent<Rigidbody>().position;
                var height = player.GetComponent<Collider>().bounds.size.y;
                var y = playerPos.y + height / 2 + 1.25f;
                transform.position = playerCamera.WorldToScreenPoint(new Vector3(playerPos.x, y, playerPos.z));
                playerNumber = (PlayerData.PlayerNumber)2;
            }
        }
    }

    void Update()
    {
        if (ballScript.GetPlayerNumber() == playerNumber)
        {
            Color c = chargeLevel.color;
            c.a = 1f;
            chargeLevel.color = c;
            int chargeNum = ballScript.GetCharge();
            chargeLevel.sprite = batteries[chargeNum];
        }
        else
        {
            Color c = chargeLevel.color;
            c.a = 0;
            chargeLevel.color = c;
        }
    }
}
