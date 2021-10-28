using UnityEngine;

public class CloneHitByBall : MonoBehaviour
{
    public int knockdownSpeed = 60;

    private PlayerData.PlayerNumber playerNumber;
    private GameObject ball;
    private Vector3 ballDirection;

    // state info
    private bool cloneKnockdown;
    private bool throwBall;
    private Rigidbody passbackTarget;
    private PlayerThrowBall playerToNotify;

    // Start is called before the first frame update
    void Start()
    {
        throwBall = false;
        playerNumber = GetComponent<PlayerData>().playerNumber;
        cloneKnockdown = false;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PlayerData>().playerNumber == playerNumber)
            {
                playerToNotify = player.GetComponent<PlayerThrowBall>();
            }
        }
        if (!playerToNotify)
        {
            Debug.LogError("Player discovery failed. It's probably an issue with either the player numbers or tags.");
        }
    }

    private void FixedUpdate()
    {
        if (throwBall && ball)
        {
            playerToNotify.SetCloneWithBall(null);

            Debug.Log("Throwing");
            
            ball.transform.parent = null;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce((transform.forward * GameConfigurations.horizontalThrowingForce * GameConfigurations.speedBoostFactor) + Vector3.up * GameConfigurations.verticalThrowingForce);
            if (passbackTarget)
            {
                ball.GetComponent<BallScript>().SetHomingTarget(passbackTarget);
            }
            ball = null;
        }
        throwBall = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if ball gone
        if (ball && ball.transform.parent != transform)
        {
            ball = null;
            playerToNotify.SetCloneWithBall(null);
            return;
        }

        if (ball)
        {
            playerToNotify.SetCloneWithBall(this);
            // TODO: Use something more gradual than transform.LookAt()
            Vector3 thingToLookAt;
            if (passbackTarget)
            {
                thingToLookAt = new Vector3(passbackTarget.transform.position.x, 0, passbackTarget.transform.position.z);
            }
            else
            {
                thingToLookAt = transform.position + ballDirection;
            }
            transform.LookAt(thingToLookAt);
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
        if (!ball && collision.transform.tag == "Ball") {
            ball = collision.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is of player's color
            if (ballData.playerNumber == playerNumber) {
                if (!ball.transform.parent) {
                    ballDirection = new Vector3(collision.relativeVelocity.x, 0, collision.relativeVelocity.z).normalized;
                    ClaimBall(collision);
                    ball.GetComponent<BallScript>().SetCharge();
                }

                BallScript ballScript = ball.GetComponent<BallScript>();
                if (ballScript.IsHomingTarget(GetComponent<Rigidbody>()))
                {
                    throwBall = false;
                }
                else if (ballScript.GetHomingTarget() != null && ballScript.GetHomingTarget().GetComponent<PlayerData>().playerNumber == playerNumber)
                {
                    throwBall = true;
                    passbackTarget = ballScript.GetHomingTarget();
                }
                else
                {
                    throwBall = true;
                }
            }

            // if ball is of no player's color
            else if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer) {
            }

            // if ball is of opponent's color
            else {
                if (ball.transform.parent == null)
                    cloneKnockdown = true;
            }
        }
    }

    public void Fire()
    {
        if (ball)
        {
            Debug.Log("In Fire()");
            throwBall = true;
            ball.GetComponent<BallScript>().SetCharge();
        }
    }

    public void SetTarget(Rigidbody target)
    {
        // If the clone is not currently throwing the ball, set target
        if (!throwBall)
        {
            passbackTarget = target;
        }
    }

    private void ClaimBall(Collision collision)
    {
        ball = collision.gameObject;
        ball.transform.parent = transform;
        ball.GetComponent<PlayerData>().playerNumber = playerNumber;
        ball.transform.localPosition = new Vector3(0, 0, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        playerToNotify.SetCloneWithBall(this);
    }
}
