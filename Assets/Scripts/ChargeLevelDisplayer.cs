using UnityEngine;
using TMPro;

public class ChargeLevelDisplayer : MonoBehaviour
{
    private PlayerData.PlayerNumber playerNumber;

    private TextMeshProUGUI chargeLevel;
    private Camera playerCamera;

    [SerializeField]
    private BallScript ballScript;

    private void Start() {
        chargeLevel = GetComponent<TextMeshProUGUI>();

        // var canvasHeight = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;

        Camera[] playerCameras = FindObjectsOfType<Camera>();

        foreach (Camera camera in playerCameras) {
            if (camera.name.StartsWith("P1") && gameObject.name == "P1ChargeLevel") {
                playerCamera = camera;
                var player = camera.transform.parent;
                var playerPos = player.GetComponent<Rigidbody>().position;
                var height = player.GetComponent<Collider>().bounds.size.y;
                var y = playerPos.y + height / 2 + 1;
                transform.position = playerCamera.WorldToScreenPoint(new Vector3(playerPos.x, y, playerPos.z));
                playerNumber = (PlayerData.PlayerNumber)1;
            }
            else if (camera.name.StartsWith("P2") && gameObject.name == "P2ChargeLevel") {
                playerCamera = camera;
                var player = camera.transform.parent;
                var playerPos = player.GetComponent<Rigidbody>().position;
                var height = player.GetComponent<Collider>().bounds.size.y;
                var y = playerPos.y + height / 2 + 1.1f;
                transform.position = playerCamera.WorldToScreenPoint(new Vector3(playerPos.x, y, playerPos.z));
                playerNumber = (PlayerData.PlayerNumber)2;
            }
        }
    }

    void Update() {
        if (ballScript.GetPlayerNumber() == playerNumber) {
            int chargeNum = ballScript.GetCharge();
            chargeLevel.color = GameConfigurations.FromChargeToColor(chargeNum);
            chargeLevel.text = chargeNum.ToString();
        }
        else {
            chargeLevel.color = new Color(0, 0, 0, 0f);
        }
    }
}
