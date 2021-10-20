using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneHitByBall : MonoBehaviour
{
    public float speedBoostFactor = 2f;
    public int knockdownSpeed = 60;

    private PlayerData.PlayerNumber playerNumber;

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
            transform.Rotate(Vector3.forward, knockdownSpeed * Time.deltaTime);

            if (transform.localEulerAngles.z >= 90) {
                cloneKnockdown = false;
                GetComponent<CloneController>().Kill();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.tag == "Ball") {
            GameObject ball = collision.gameObject;
            ball.GetComponent<BallScript>().SetHomingTarget(null);
            PlayerData ballData = ball.GetComponent<PlayerData>();
            
            // if ball is of player's color
            if (ballData.playerNumber == playerNumber) {
                if (ball.transform.parent == null) {
                    Debug.Log("ball passed to friendly clone...");
                    var ballDirection = collision.relativeVelocity.normalized;
                    ballSpeedBoost(ball, ballDirection);
                }
            }

            // if ball is of no player's color
            else if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                Debug.Log("clone collided with unclaimed ball");
            }

            // if ball is of opponent's color
            else {
                if (ball.transform.parent == null)
                    cloneKnockdown = true;
            }
        }
    }

    private void ballSpeedBoost(GameObject ball, Vector3 direction) {
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        ball.transform.parent = transform;
        ball.transform.localPosition = direction * GameConfigurations.ballDistance;
        ball.transform.parent = null;
        ball.GetComponent<Rigidbody>().AddForce(direction * GameConfigurations.throwingForce * speedBoostFactor);
    }
}
