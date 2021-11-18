using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneGuard : MonoBehaviour
{
    private CloneHitByBall cloneBallScript;

    // Start is called before the first frame update
    void Start()
    {
        cloneBallScript = GetComponentInParent<CloneHitByBall>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Ball")
        {
            GameObject ball = other.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is of no player's color
            if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer)
            {
                cloneBallScript.ClaimBall(ball);
            }

            // if ball is of opponent's color
            else if (ballData.playerNumber != GetComponentInParent<PlayerData>().playerNumber)
            {
                if (ball.transform.parent == null && ball.GetComponent<BallScript>().GetCharge() > 0)
                {
                    cloneBallScript.KnockDownClone();
                }
                else
                {
                    cloneBallScript.ClaimBall(ball);
                }
            }
        }
    }
}
