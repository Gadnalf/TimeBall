using UnityEngine;

[RequireComponent(typeof(PlayerData))]
public class PlayerCameraControl : MonoBehaviour
{
    // Config
    private PlayerData.PlayerNumber playerNumber;

    // State info
    private float rotationOnX = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerNumber = GetComponent<PlayerData>().playerNumber;
    }

    // Update is called once per frame
    void Update()
    {
        float rotationInput = 0;
        switch (playerNumber)
        {
            case PlayerData.PlayerNumber.PlayerOne:
                rotationInput = Input.GetAxis("P1Camera");
                break;
            case PlayerData.PlayerNumber.PlayerTwo:
                rotationInput = Input.GetAxis("P2Camera");
                break;
            default:
                Debug.LogError("Player object not assigned type.");
                break;
        }

        rotationOnX -= rotationInput * Time.deltaTime;

        transform.localEulerAngles = new Vector3(0, rotationOnX, 0);
    }
}
