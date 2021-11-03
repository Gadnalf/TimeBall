using System;
using UnityEngine;

//[RequireComponent(typeof(PlayerData))]
public class PlayerCameraControl : MonoBehaviour
{
    // Config
    public Vector3 targetLocation;
    public float offset = 2.5f;
    public float spacing = 7.5f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        Vector3 backwards = transform.forward * -1;
        Ray ray = new Ray(transform.parent.position + new Vector3(targetLocation.x, targetLocation.y, 0), backwards);
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo, offset + spacing))
        {
            float cameraDistance;
            if (hitInfo.distance > offset)
            {
                Debug.Log((1 - (hitInfo.distance - offset) / spacing));
                cameraDistance = -targetLocation.z - (1-(hitInfo.distance - offset) / spacing) * -targetLocation.z;
            }
            else
            {
                cameraDistance = hitInfo.distance - offset;
            }
            transform.localPosition = new Vector3(targetLocation.x, targetLocation.y, -cameraDistance);
        }
        else
        {
            transform.localPosition = targetLocation;
        }
    }
}
