using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    private float delay;
    private Rigidbody rb;
    public PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.recordLength <= 0)
        {
            Vector3 pos = player.lastPositions.Dequeue();
            rb.MovePosition(pos);
        }
    }
}
