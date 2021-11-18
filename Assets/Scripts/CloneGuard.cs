using UnityEngine;

public class CloneGuard : MonoBehaviour
{
    private CloneHitByBall cloneBallScript;
    private Collider col;

    // Start is called before the first frame update
    void Start()
    {
        cloneBallScript = GetComponentInParent<CloneHitByBall>();
        col = GetComponent<Collider>();
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
                ball.GetComponent<BallScript>().ClearCharge();
            }

            // if ball is of opponent's color
            else if (ballData.playerNumber != GetComponentInParent<PlayerData>().playerNumber)
            {
                if (ball.transform.parent == null && ball.GetComponent<BallScript>().GetCharge() > 0)
                {
                    cloneBallScript.KnockDownClone();
                    ball.GetComponent<BallScript>().ClearCharge();
                }
                else
                {
                    cloneBallScript.ClaimBall(ball);
                    ball.GetComponent<BallScript>().ClearCharge();
                }
            }
        }
    }

    public void UpdateGuard(bool guardInput)
    {
        Debug.Log("guarding");
        if (guardInput)
        {
            col.enabled = true;
            float currentScale = transform.localScale.x;
            float scale = Mathf.Min(currentScale + GameConfigurations.guardExpandRate, GameConfigurations.guardMaxSize);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            col.enabled = false;
            transform.localScale = Vector3.zero;
        }
    }
}
