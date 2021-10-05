using UnityEngine;

[RequireComponent(typeof(PlayerData))]
public class PlayerCameraControl : MonoBehaviour
{
    // Config
    private PlayerData.PlayerNumber playerNumber;
    private Camera playerCamera;

    // State info
    private Vector3 lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerNumber = GetComponent<PlayerData>().playerNumber;
        playerCamera = GetComponentInChildren<Camera>();
        lastRotation = transform.eulerAngles;
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
                // TODO: Getting button input rather than mouse, should be GetAxis
                rotationInput = Input.GetAxisRaw("P2Camera") * 500;
                break;
            default:
                Debug.LogError("Player object not assigned type.");
                break;
        }
        
        playerCamera.transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput * Time.deltaTime, lastRotation.z);
        lastRotation = playerCamera.transform.eulerAngles;
    }
}
