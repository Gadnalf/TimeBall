using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CloneHitByBall : MonoBehaviour
{
    public int knockdownSpeed = 60;

    private GameObject ball;
    private CloneController controller;

    // state info
    private bool cloneKnockdown;

    private bool throwBall;
    private bool throwHeldDown;
    private bool passBall;
    private bool passHeldDown;

    private float timeSinceLastUpdate;
    private Rigidbody lockedTarget;
    private Rigidbody priorityLockedTarget;
    private PlayerThrowBall playerToNotify;
    private float chargeTime;

    private Animator animator;
    private int throwAnimation;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CloneController>();
        animator = GetComponent<Animator>();
        throwAnimation = Animator.StringToHash("Throw");

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

        chargeTime = 0;
    }

    private void FixedUpdate()
    {
        if (ball != null)
        {
            var uniqueCharges = ball.GetComponent<BallScript>().uniqueHoldCharges;
            int cloneNum = controller.cloneData.RoundNumber;

            if (!uniqueCharges.Contains(cloneNum))
            {
                chargeTime += Time.deltaTime;

                if (chargeTime > GameConfigurations.ballChargeTime)
                {
                    uniqueCharges.Add(cloneNum);
                    ball.GetComponent<BallScript>().AddCharge();
                    chargeTime = 0;
                }
            }
        }

        if ((throwBall || passBall) && ball)
        {
            playerToNotify.SetCloneWithBall(null);

            ball.transform.parent = null;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce((transform.forward * GameConfigurations.horizontalThrowingForce * GameConfigurations.speedBoostFactor * (1 + ball.GetComponent<BallScript>().GetCharge()))
                + Vector3.up * GameConfigurations.verticalThrowingForce);
            if (priorityLockedTarget || lockedTarget)
            {
                Rigidbody target = priorityLockedTarget ?? lockedTarget;
                Debug.Log("Clone throwing to target: " + target);
                ball.GetComponent<BallScript>().SetHomingTarget(target);
                target.GetComponent<CloneController>()?.Pause();
                lockedTarget = null;
                priorityLockedTarget = null;

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
            bool passInput = controller.passInput;

            // Check if ball gone
            if (ball && ball.transform.parent != transform)
            {
                ball = null;
                playerToNotify.SetCloneWithBall(null);
                return;
            }

            // Notify player, handle player input
            if (ball)
            {
                playerToNotify.SetCloneWithBall(this);

                if (lockedTarget)
                {
                    Vector3 vectorTowardsTarget = new Vector3(lockedTarget.transform.position.x, transform.position.y, lockedTarget.transform.position.z) - transform.position;
                    targetRotation = Quaternion.LookRotation(vectorTowardsTarget, Vector3.up);
                }
            }

            if (throwInput)
            {
                if (!throwHeldDown) // One throw per input down
                {
                    throwHeldDown = true;
                    if (ball)
                    {
                        throwBall = true;
                    }
                }
            }
            else
            {
                throwHeldDown = false;
            }

            if (passInput)
            {
                if (!passHeldDown) // One pass per input down
                {
                    passHeldDown = true;
                    if (ball)
                    {
                        LockOnClosestCloneOrPlayer();
                        if (lockedTarget != null)
                        {
                            passBall = true;
                        }
                    }
                }
            }
            else
            {
                passHeldDown = false;
            }

            transform.rotation = targetRotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!ball && collision.transform.tag == "Ball")
        {
            ball = collision.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is of player's color
            if (ballData.playerNumber == GetComponent<PlayerData>().playerNumber)
            {
                if (!ball.transform.parent)
                {
                    ClaimBall(ball);

                    var uniqueCloneCharges = ball.GetComponent<BallScript>().uniqueClones;
                    int cloneNum = controller.cloneData.RoundNumber;
                    if (!uniqueCloneCharges.Contains(cloneNum))
                    {
                        uniqueCloneCharges.Add(cloneNum);
                        ball.GetComponent<BallScript>().AddCharge(GameConfigurations.cloneBaseCharge, GameConfigurations.maxBallCharge);
                    }

                    BallScript ballScript = ball.GetComponent<BallScript>();
                    if (ballScript.IsHomingTarget(GetComponent<Rigidbody>()))
                    {
                        throwBall = false;
                    }
                    else if (ballScript.GetHomingTarget() != null && ballScript.GetHomingTarget().GetComponent<PlayerData>().playerNumber == GetComponent<PlayerData>().playerNumber)
                    {
                        throwBall = true;
                        lockedTarget = ballScript.GetHomingTarget();
                    }
                }
            }

            // if ball is of no player's color
            else if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer)
            {
                ClaimBall(ball);
                var uniqueCloneCharges = ball.GetComponent<BallScript>().uniqueClones;
                int cloneNum = controller.cloneData.RoundNumber;
                uniqueCloneCharges.Add(cloneNum);
                ball.GetComponent<BallScript>().AddCharge(GameConfigurations.cloneBaseCharge, GameConfigurations.maxBallCharge);
            }

            // if ball is of opponent's color
            else
            {
                if (ball.transform.parent == null && ball.GetComponent<BallScript>().GetCharge() > GameConfigurations.goalShieldBreakableCharge)
                {
                    cloneKnockdown = true;
                    ball.GetComponent<BallScript>().ClearCharge();
                }
                else
                {
                    ClaimBall(ball);
                    ball.GetComponent<BallScript>().ClearCharge();
                }
            }
        }
    }

    public void Fire()
    {
        if (ball)
        {
            throwBall = true;
            HandleThrowAnimation();
        }
    }

    void HandleThrowAnimation()
    {
        animator.CrossFade(throwAnimation, 0f);
    }

    public void SetTarget(Rigidbody target)
    {
        priorityLockedTarget = target;
    }

    public void ClaimBall(GameObject ball)
    {
        this.ball = ball;
        ball.transform.parent = transform;
        ball.GetComponent<PlayerData>().playerNumber = GetComponent<PlayerData>().playerNumber;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        playerToNotify.SetCloneWithBall(this);
        controller.Unpause();
        chargeTime = 0;
    }

    private void LockOnClosestCloneOrPlayer()
    {
        // Rediscover clones/player
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Clone");
        List<GameObject> playerClones = clones.Where((GameObject clone) => clone.GetComponent<PlayerData>().playerNumber == controller.cloneData.PlayerNumber).ToList();
        playerClones.Add(playerToNotify.gameObject);
        lockedTarget = null;
        float angle = GameConfigurations.passAngle / 2;

        // Find closest based on angle
        foreach (GameObject clone in playerClones)
        {
            Vector3 cloneDirection = DirectionTo(clone);

            float cloneAngle = Mathf.Abs(Vector3.Angle(transform.forward, cloneDirection));

            if (cloneAngle < angle)
            {
                lockedTarget = clone.GetComponent<Rigidbody>();
                angle = cloneAngle;
            }
        }
    }

    private Vector3 DirectionTo(GameObject clone)
    {
        return Vector3.Normalize(clone.transform.position - ball.transform.position);
    }

    public bool HasBall()
    {
        return ball != null;
    }

    public void KnockDownClone()
    {
        cloneKnockdown = true;
    }
}
