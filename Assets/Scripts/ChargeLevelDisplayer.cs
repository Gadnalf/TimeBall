using System.Collections;
using UnityEngine;
using TMPro;

public class ChargeLevelDisplayer : MonoBehaviour
{
    private PlayerData.PlayerNumber playerNumber;

    private TextMeshProUGUI chargeLevel;

    [SerializeField]
    private BallScript ballScript;

    private void Start() {
        chargeLevel = GetComponent<TextMeshProUGUI>();

        var height = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.height;

        if (gameObject.name == "P1ChargeLevel") {
            transform.position = new Vector3(90, 30, 0);
            playerNumber = PlayerData.PlayerNumber.PlayerOne;
        }
        else if (gameObject.name == "P2ChargeLevel") {

            transform.position = new Vector3(90, height / 2 + 30, 0);
            playerNumber = PlayerData.PlayerNumber.PlayerTwo;
        }
    }

    private void Update() {
        if (ballScript.GetPlayerNumber() == playerNumber) {
            chargeLevel.text = ballScript.GetCharge().ToString();
        }
        else {
            chargeLevel.text = "0";
        }
    }
}
