using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneHitByBall : MonoBehaviour
{
    private PlayerData.PlayerNumber playerNumber;
    public int knockdownSpeed = 90;
    [SerializeField]
    private float throwingForce = 1000f;

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
            int knockdownSpeed = 60;
            transform.Rotate(Vector3.forward, knockdownSpeed * Time.deltaTime);

            if (transform.localEulerAngles.z >= 90) {
                cloneKnockdown = false;
                GetComponent<CloneController>().Kill();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "Ball") {
            PlayerData ballData = collision.gameObject.GetComponent<PlayerData>();
            
            // if ball is of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber == playerNumber) {
                Debug.Log("ball passed to friendly clone");
                var ballDirection = collision.gameObject.transform.forward;

                collision.gameObject.GetComponent<Rigidbody>().AddForce(collision.gameObject.transform.forward * throwingForce * 2);
            }

            // if ball is of no player's color
            else if (collision.gameObject.GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                Debug.Log("clone collided with unclaimed ball");
            }

            // if ball is of opponent's color
            else {
                Debug.Log("ball passed to enemy clone");
                cloneKnockdown = true;
            }
        }
    }
}
