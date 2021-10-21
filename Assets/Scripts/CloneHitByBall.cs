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
    private bool throwBall;
    private Rigidbody passbackTarget;
    private PlayerThrowBall playerToNotify;

    // Start is called before the first frame update
    void Start()
    {
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
        if (throwBall)
        {
            playerToNotify.SetCloneWithBall(null);

            Debug.Log("Throwing");
            throwBall = false;
            ball.transform.parent = null;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce(transform.forward * GameConfigurations.throwingForce * speedBoostFactor + Vector3.up * GameConfigurations.verticalThrowingForce);
            if (passbackTarget)
            {
                ball.GetComponent<BallScript>().SetHomingTarget(passbackTarget);
            }
            ball = null;
        }
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
                thingToLookAt = passbackTarget.transform.position;
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
            Debug.Log("touched a ball: " + ballData.playerNumber);

            // if ball is of player's color
            if (ballData.playerNumber == playerNumber) {
                Debug.Log("ball passed to friendly clone...");
                if (!ball.transform.parent) {
                    ballDirection = new Vector3(collision.relativeVelocity.x, transform.position.y, collision.relativeVelocity.z).normalized;
                    ClaimBall(collision);
                }
                else
                {
                    Debug.Log(ball.transform.parent.name);
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

    public void Fire()
    {
        if (ball)
        {
            throwBall = true;
        }
    }

    public void SetTarget(Rigidbody target)
    {
        passbackTarget = target;
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
