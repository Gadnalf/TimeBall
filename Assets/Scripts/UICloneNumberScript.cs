using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UICloneNumberScript : MonoBehaviour
{
    [SerializeField]
    private BallScript ballScript;
    [SerializeField]
    private PlayerData.PlayerNumber playerNumber;
    [SerializeField]
    private GameObject indicatorPrefab;
    [SerializeField]
    private float indicatorVerticalOffset = 3f;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color activeColor;
    [SerializeField]
    [ColorUsage(true, true)]
    private Color inactiveColor;

    private float padding = 10f;

    private float verticalBound;
    private float horizontalBound;

    private List<CloneController> cloneControllers;
    private Camera playerCamera;
    private Canvas canvas;

    // Stuff to move around
    private List<GameObject> cloneNumberIndicators;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        Camera[] cameraList = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameraList)
        {
            if (camera.name.StartsWith("P1") && playerNumber == PlayerData.PlayerNumber.PlayerOne)
            {
                playerCamera = camera;
            }
            else if (camera.name.StartsWith("P2") && playerNumber == PlayerData.PlayerNumber.PlayerTwo)
            {
                playerCamera = camera;
            }
        }
        if (!playerCamera)
        {
            Debug.LogError("Camera discovery failed");
        }

        Rect canvasRect = canvas.GetComponent<RectTransform>().rect;
        verticalBound = canvasRect.height/2;
        horizontalBound = canvasRect.width/2;

        cloneNumberIndicators = new List<GameObject>();
        cloneControllers = new List<CloneController>();

        Reset();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < cloneControllers.Count; i++)
        {
            CloneController cloneController = cloneControllers[i];
            GameObject cloneNumberIndicator = cloneNumberIndicators[i];
            if (cloneController != null)
            {
                Vector3 targetPosition = GetTargetPosition(cloneController.transform.position);
                Vector3 boundedPosition = GetBoundedPosition(targetPosition);
                cloneNumberIndicator.transform.localPosition = boundedPosition;
                TextMeshProUGUI textMesh = cloneNumberIndicator.GetComponent<TextMeshProUGUI>();
                if (ballScript.GetPlayerNumber() != playerNumber || 
                    (ballScript.GetPlayerNumber() == playerNumber && !ballScript.IsChargedClone(cloneController.cloneData.RoundNumber)))
                {
                    textMesh.color = activeColor;
                }
                else
                {
                    textMesh.color = inactiveColor;
                }
            }
            else
            {
                cloneNumberIndicator.SetActive(false);
            }
        }
    }

    public void Reset()
    {
        foreach (GameObject indicator in cloneNumberIndicators)
        {
            Destroy(indicator);
        }
        cloneNumberIndicators.Clear();

        GameObject[] cloneObjects = GameObject.FindGameObjectsWithTag("Clone");
        cloneControllers.Clear();

        cloneControllers.AddRange(from GameObject clone in cloneObjects where clone.GetComponent<PlayerData>().playerNumber == playerNumber select clone.GetComponent<CloneController>());
        foreach (CloneController cloneController in cloneControllers)
        {
            int roundNumber = cloneController.cloneData.RoundNumber;
            GameObject newIndicator = Instantiate(indicatorPrefab, canvas.transform);
            Vector3 targetPosition = GetTargetPosition(cloneController.transform.position);
            Vector3 boundedPosition = GetBoundedPosition(targetPosition);
            newIndicator.transform.localPosition = boundedPosition;
            TextMeshProUGUI textMesh = newIndicator.GetComponent<TextMeshProUGUI>();
            textMesh.text = cloneController.cloneData.RoundNumber.ToString();
            textMesh.color = activeColor;
            cloneNumberIndicators.Add(newIndicator);
        }
    }

    private Vector3 GetTargetPosition(Vector3 clonePosition)
    {
        Vector3 screenPoint = playerCamera.WorldToScreenPoint(clonePosition += Vector3.up * indicatorVerticalOffset);
        if (screenPoint.z < 0)
        {
            screenPoint.x = -screenPoint.x;
            screenPoint.y = -verticalBound;
        }
        Vector3 shiftedScreenPoint = new Vector3(screenPoint.x - horizontalBound, screenPoint.y - verticalBound, screenPoint.z);
        return shiftedScreenPoint;
    }

    private Vector3 GetBoundedPosition(Vector3 targetPosition)
    {
        float paddedHorizontalBound = horizontalBound - padding;
        float paddedVerticalBound = verticalBound - padding;
        float verticalBoundOffset = 0;
        if (playerNumber == PlayerData.PlayerNumber.PlayerTwo)
        {
            // One screen width
            verticalBoundOffset = verticalBound;
        }
        return new Vector3(Mathf.Clamp(targetPosition.x, -paddedHorizontalBound, paddedHorizontalBound), Mathf.Clamp(targetPosition.y, -paddedVerticalBound + verticalBoundOffset, paddedVerticalBound + verticalBoundOffset), targetPosition.z);
    }
}
