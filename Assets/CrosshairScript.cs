using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    public Camera playerCamera;
    private Rigidbody target;
    private Image crosshair;

    private void Start()
    {
        crosshair = GetComponent<Image>();
    }

    void Update()
    {
        if (target)
        {
            transform.position = playerCamera.WorldToScreenPoint(target.position);
            transform.localScale = Mathf.Clamp((1f/Mathf.Max(1,Vector3.Distance(playerCamera.transform.position, target.position)))*5, 1, 3) * Vector2.one;
        }
    }

    public void SetTarget(Rigidbody target)
    {
        this.target = target;
        crosshair.enabled = target;
    }
}
