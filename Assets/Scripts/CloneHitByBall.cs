using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneHitByBall : MonoBehaviour
{
    private PlayerData.PlayerNumber playerNumber;
    public int knockdownSpeed = 90;

    // state info
    private bool cloneKnockdown;

    // Start is called before the first frame update
    void Start()
    {
        playerNumber = GetComponent<PlayerData>().playerNumber;
        cloneKnockdown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (cloneKnockdown) {
            int knockdownSpeed = 90;
            transform.Rotate(Vector3.forward, knockdownSpeed * Time.deltaTime);

            if (transform.localEulerAngles.z >= 90) {
                cloneKnockdown = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {

        if (collision.transform.tag == "Ball") {
            PlayerData ballData = collision.gameObject.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber) {
                Debug.Log("ball passed to enemy clone");
                cloneKnockdown = true;
            }

            // if ball is of player's color
            else {
                Debug.Log("ball passed to friendly clone");
                // add acceleration to the ball
                // collision.gameObject.GetComponent<Rigidbody>().AddRelativeForce(...); 
            }
        }
    }
}
