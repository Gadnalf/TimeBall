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
    }

    private void LateUpdate()
    {
        playerCamera.transform.LookAt(transform.parent.transform);
    }
}
