using UnityEngine;

public class CloneHitByBall : MonoBehaviour
{
    public float speedBoostFactor = 2f;
    public int knockdownSpeed = 60;

    private PlayerData.PlayerNumber playerNumber;
    private GameObject ball;
    private Vector3 ballDirection;

    // state info
    private bool cloneKnockdown;
    private bool throwInput;
    private Rigidbody passBackTarget;

    // Start is called before the first frame update
    void Start()
    {
        playerNumber = GetComponent<PlayerData>().playerNumber;
        cloneKnockdown = false;
    }

    private void FixedUpdate()
    {
        if (passBackTarget && throwInput)
        {
            ballDirection = transform.forward;
            ball.GetComponent<BallScript>().SetHomingTarget(passBackTarget);
            passBackTarget = null;
            ThrowBall();
        }
        else if (throwInput)
        {
            ThrowBall();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (passBackTarget)
        {
            transform.LookAt(passBackTarget.transform);
        }

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
            ball = collision.gameObject;
            ball.GetComponent<BallScript>().SetHomingTarget(null);
            PlayerData ballData = ball.GetComponent<PlayerData>();
            
            // if ball is of player's color
            if (ballData.playerNumber == playerNumber) {
                if (ball.transform.parent == null) {
                    Debug.Log("ball passed to friendly clone...");
                    ballDirection = new Vector3(collision.relativeVelocity.x, 0, collision.relativeVelocity.z).normalized;
                }
                
                ClaimBall();
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

    public void Boost()
    {
        throwInput = true;
    }

    public void TossBack(Rigidbody target)
    {
        passBackTarget = target;
    }

    private void ClaimBall()
    {
        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, 0, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void ThrowBall()
    {
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().AddForce(ballDirection * GameConfigurations.throwingForce * speedBoostFactor);
        ball.transform.parent = transform;
        ball.transform.localPosition = ballDirection * GameConfigurations.ballDistance;
        ball.transform.parent = null;
    }
}
