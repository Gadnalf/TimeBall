using UnityEngine;
using System.Collections;

public class CloneHitByBall : MonoBehaviour
{
    public int knockdownSpeed = 60;

    private GameObject ball;
    private CloneController controller;

    // state info
    private bool cloneKnockdown;
    private bool throwBall;
    private float timeSinceLastUpdate;
    private Rigidbody lockTarget;
    private PlayerThrowBall playerToNotify;
    private float chargeBall;

    private bool baseChargeOnCooldown;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CloneController>();
        throwBall = false;
        cloneKnockdown = false;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PlayerData>().playerNumber == GetComponent<PlayerData>().playerNumber)
            {
                playerToNotify = player.GetComponent<PlayerThrowBall>();
            }
        }
        if (!playerToNotify)
        {
            Debug.LogError("Player discovery failed. It's probably an issue with either the player numbers or tags.");
        }

        baseChargeOnCooldown = false;
    }

    private void FixedUpdate()
    {
        if (throwBall && ball)
        {
            playerToNotify.SetCloneWithBall(null);

            //Debug.Log("Throwing");
            
            ball.transform.parent = null;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce((transform.forward * GameConfigurations.horizontalThrowingForce * GameConfigurations.speedBoostFactor * (1 + ball.GetComponent<BallScript>().GetCharge()))
                + Vector3.up * GameConfigurations.verticalThrowingForce);
            if (lockTarget)
            {
                ball.GetComponent<BallScript>().SetHomingTarget(lockTarget);
                lockTarget = null;
            }
            ball = null;
        }
        throwBall = false;
        timeSinceLastUpdate = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (cloneKnockdown)
        {
            transform.Rotate(Vector3.forward, knockdownSpeed * Time.deltaTime);

            if (transform.localEulerAngles.z >= 90)
            {
                cloneKnockdown = false;
                controller.Kill();
            }
        }
        else
        {
            // Start by fetching controller data
            Quaternion targetRotation = controller.GetNextRotation(timeSinceLastUpdate);
            bool throwInput = controller.throwInput;

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

                if (throwInput)
                {
                    if (chargeBall > GameConfigurations.ballChargeTime)
                    {
                        ball.GetComponent<BallScript>().AddCharge(2);
                        chargeBall = 0;
                    }
                    chargeBall += Time.deltaTime;
                }
                else if (chargeBall > 0)
                {
                    throwBall = true;
                }

                if (lockTarget)
                {
                    Vector3 vectorTowardsTarget = new Vector3(lockTarget.transform.position.x, transform.position.y, lockTarget.transform.position.z) - transform.position;
                    targetRotation = Quaternion.LookRotation(vectorTowardsTarget, Vector3.up);
                }
            }
            transform.rotation = targetRotation;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!ball && collision.transform.tag == "Ball") {
            ball = collision.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is of player's color
            if (ballData.playerNumber == GetComponent<PlayerData>().playerNumber) {
                if (!ball.transform.parent) {
                    ClaimBall(collision);

                    if (baseChargeOnCooldown == false)
                    {
                        ball.GetComponent<BallScript>().AddCharge(GameConfigurations.cloneBaseCharge);
                        baseChargeOnCooldown = true;
                        StartBaseChargeCD();
                    }

                    BallScript ballScript = ball.GetComponent<BallScript>();
                    if (ballScript.IsHomingTarget(GetComponent<Rigidbody>()))
                    {
                        throwBall = false;
                    }
                    else if (ballScript.GetHomingTarget() != null && ballScript.GetHomingTarget().GetComponent<PlayerData>().playerNumber == GetComponent<PlayerData>().playerNumber)
                    {
                        throwBall = true;
                        lockTarget = ballScript.GetHomingTarget();
                    }
                }
            }

            // if ball is of no player's color
            else if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                ClaimBall(collision);
            }

            // if ball is of opponent's color
            else {
                if (ball.transform.parent == null && ball.GetComponent<BallScript>().GetCharge() > 0) {
                    cloneKnockdown = true;
                }
                else
                {
                    ClaimBall(collision);
                }
            }
        }
    }

    public void Fire()
    {
        if (ball)
            throwBall = true;
    }

    public void SetTarget(Rigidbody target)
    {
        // If the clone is not currently throwing the ball, set target
        if (!throwBall)
        {
            this.lockTarget = target;
        }
    }

    private void ClaimBall(Collision collision)
    {
        ball = collision.gameObject;
        ball.transform.parent = transform;
        ball.GetComponent<PlayerData>().playerNumber = GetComponent<PlayerData>().playerNumber;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        playerToNotify.SetCloneWithBall(this);
    }

    public void StartBaseChargeCD()
    {
        IEnumerator coroutine = coolDownCoroutine(GameConfigurations.cloneBaseChargeCDInSeconds);
        StartCoroutine(coroutine);
    }

    private IEnumerator coolDownCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        baseChargeOnCooldown = false;
    }
}
