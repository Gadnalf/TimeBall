using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    public Camera playerCamera;
    public float crosshairScale = 25f;

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
            UpdatePosition();
        }
    }

    public void SetTarget(Rigidbody target)
    {
        this.target = target;
        crosshair.enabled = target;
        if (target)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        transform.position = playerCamera.WorldToScreenPoint(target.position);
        transform.localScale = crosshairScale / Mathf.Max(1, transform.position.z) * Vector2.one;
    }

    public void Reset()
    {
        SetTarget(null);
    }
}
