using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Vector3 spawnLocation;

    private Rigidbody rb;
    private PlayerData playerData;
    private Rigidbody target;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerData = GetComponent<PlayerData>();
    }

    private void FixedUpdate()
    {
        if (target)
        {
            float magnitude = Mathf.Pow(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2), 0.5f);
            Vector3 direction = 0f*(rb.velocity) + 1f*(target.transform.position - transform.position);
            Vector2 scaledDirection = new Vector2(direction.x, direction.z).normalized * magnitude;
            rb.velocity = new Vector3(scaledDirection.x, rb.velocity.y, scaledDirection.y);
        }
    }

    public void SetHomingTarget(Rigidbody target = null)
    {
        this.target = target;
    }

    public void Reset()
    {
        rb.isKinematic = false;
        rb.transform.position = spawnLocation;
        rb.transform.parent = null;
        rb.velocity = Vector3.zero;
        playerData.playerNumber = PlayerData.PlayerNumber.NoPlayer;
        target = null;
        GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.gray);
    }
}
