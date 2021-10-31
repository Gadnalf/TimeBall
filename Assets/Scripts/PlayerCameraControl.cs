using System;
using UnityEngine;

//[RequireComponent(typeof(PlayerData))]
public class PlayerCameraControl : MonoBehaviour
{
    // Config
    public Vector3 targetLocation;
    public float spacing = 0.05f;

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
        if (Physics.Raycast(ray, out hitInfo, Math.Abs(targetLocation.z) + spacing))
        {
            float adjustedZ = -(hitInfo.distance - spacing);
            transform.localPosition = new Vector3(targetLocation.x, targetLocation.y, adjustedZ);
        }
        else
        {
            transform.localPosition = targetLocation;
        }
    }
}
