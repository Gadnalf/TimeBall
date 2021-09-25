using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBall : MonoBehaviour
{

    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private float ballDistance = 2f;

    [SerializeField]
    private float throwingForce = 5f;

    private bool holdingBall = true;

    // Start is called before the first frame update
    void Start()
    {
        ball.GetComponent<Rigidbody>().useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (holdingBall)
        {
            ball.transform.position = this.transform.position + this.transform.forward * ballDistance;

            if (Input.GetMouseButtonDown(0))
            {
                holdingBall = false;
                ball.GetComponent<Rigidbody>().useGravity = true;
                ball.GetComponent<Rigidbody>().AddForce(this.transform.forward * throwingForce);
            }


        }
    }
}
