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
    private float holdThrow;
    private float chargeTime;

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

        chargeTime = 0;
    }

    private void FixedUpdate()
    {
        if (ball != null) {
            var uniqueCharges = ball.GetComponent<BallScript>().uniqueHoldCharges;
            int cloneNum = controller.cloneData.RoundNumber;

            if (!uniqueCharges.Contains(cloneNum)) {
                chargeTime += Time.deltaTime;

                if (chargeTime > GameConfigurations.ballChargeTime) {
                    uniqueCharges.Add(cloneNum);
                    ball.GetComponent<BallScript>().AddCharge();
                    chargeTime = 0;
                }
            }
        }

        if ((throwBall && ball && ball.GetComponent<BallScript>().GetCharge() >= GameConfigurations.goalShieldBreakableCharge) || (throwBall && ball && lockTarget))
        {
            playerToNotify.SetCloneWithBall(null);

            // Debug.Log("Throwing");
            
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
            holdThrow = 0;
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

            // Charge the ball while input is held. When throw input is released, release the charge if the ball isn't held.
            if (throwInput)
            {
                holdThrow += Time.deltaTime;
            }
            else if (!ball)
            {
                holdThrow = 0;
            }

            if (ball)
            {
                playerToNotify.SetCloneWithBall(this);
                
                // When throw input is released, if the ball is held and charged, throw the ball.
                if (!throwInput && holdThrow > 0)
                {
                    throwBall = true;
                    holdThrow = 0;
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
                    ClaimBall(ball);

                    var uniqueCloneCharges = ball.GetComponent<BallScript>().uniqueClones;
                    int cloneNum = controller.cloneData.RoundNumber;
                    if (!uniqueCloneCharges.Contains(cloneNum)) {
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
                        lockTarget = ballScript.GetHomingTarget();
                    }
                }
            }

            // if ball is of no player's color
            else if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                ClaimBall(ball);
                var uniqueCloneCharges = ball.GetComponent<BallScript>().uniqueClones;
                int cloneNum = controller.cloneData.RoundNumber;
                uniqueCloneCharges.Add(cloneNum);
                ball.GetComponent<BallScript>().AddCharge(GameConfigurations.cloneBaseCharge, GameConfigurations.maxBallCharge);
            }

            // if ball is of opponent's color
            else {
                if (ball.transform.parent == null && ball.GetComponent<BallScript>().GetCharge() > GameConfigurations.goalShieldBreakableCharge) {
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
            throwBall = true;
    }

    public void SetTarget(Rigidbody target)
    {
        // If the clone is not currently throwing the ball, set target
        if (!throwBall)
        {
            lockTarget = target;
        }
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

    public bool HasBall()
    {
        return ball != null;
    }

    public void KnockDownClone()
    {
        cloneKnockdown = true;
    }
}
