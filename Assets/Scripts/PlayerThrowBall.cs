using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowBall : MonoBehaviour
{
    // Config
    public PlayerData.PlayerNumber playerNumber;
    private CrosshairScript crosshair;
    public float lockDistance = 10000;

    // Pass config
    public int searchWidth = 100;
    public float searchStep = 0.2f;

    // State info
    private bool throwBall = false;
    private float chargeBall;
    private GameObject ball;
    private Rigidbody lockedTarget;
    private CloneHitByBall cloneWithBall;

    private ScoringManager scoringManager;

    PlayerControls controls;
    private bool throwInput = false;
    private bool lockInput;

    private CooldownTimer dashCooldown;

    private PlayerConfig playerConfig;
    private PlayerRecording records;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Throw.canceled += ctx =>
        {
            throwInput = false;
        };

        controls.Gameplay.Lockon.canceled += ctx =>
        {

        };
    }

    public void InitializePlayerConfig(PlayerConfig pc)
    {
        playerConfig = pc;
        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(InputAction.CallbackContext obj)
    {
        if (obj.action.name == controls.Gameplay.Throw.name)
        {
            OnThrow(obj);
        }
        else if (obj.action.name == controls.Gameplay.Lockon.name)
        {
            OnLock(obj);
        }
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        throwInput = context.action.triggered;
    }

    public void OnLock(InputAction.CallbackContext context)
    {
        lockInput = context.action.triggered;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerNumber = GetComponent<PlayerData>().playerNumber;
        CrosshairScript[] crosshairs = FindObjectsOfType<CrosshairScript>();
        foreach (CrosshairScript crosshair in crosshairs)
        {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && crosshair.name.StartsWith("P1"))
            {
                this.crosshair = crosshair;
            }
            else if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && crosshair.name.StartsWith("P2"))
            {
                this.crosshair = crosshair;
            }
        }

        scoringManager = FindObjectOfType<ScoringManager>();

        CooldownTimer[] dashCooldowns = FindObjectsOfType<CooldownTimer>();
        foreach (CooldownTimer dashCooldown in dashCooldowns) {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && dashCooldown.name.StartsWith("P1")) {
                this.dashCooldown = dashCooldown;
            }
            else if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && dashCooldown.name.StartsWith("P2")) {
                this.dashCooldown = dashCooldown;
            }
        }
    }

    private void FixedUpdate()
    {
        records.RecordInput(throwInput);
        if (throwBall)
        {
            throwBall = false;
            ball.transform.parent = null;
            ball.GetComponent<PlayerData>().playerNumber = playerNumber;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 throwForce = transform.forward * GameConfigurations.horizontalThrowingForce;
            throwForce *= GameConfigurations.speedBoostFactor * (1 + ball.GetComponent<BallScript>().GetCharge());
            throwForce += Vector3.up * GameConfigurations.verticalThrowingForce;
            ball.GetComponent<Rigidbody>().AddForce(throwForce);
            ball.GetComponent<BallScript>().SetHomingTarget(lockedTarget);
            Reset();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if ball gone
        if (ball && ball.transform.parent != transform)
        {
            Reset();
            return;
        }

        // Update throw inputs
        if (ball)
        {
            BallScript ballScript = ball.GetComponent<BallScript>();
            if (throwInput)
            {
                chargeBall += Time.deltaTime;
                if (chargeBall > GameConfigurations.ballChargeTime)
                {
                    ballScript.AddCharge();
                    chargeBall = 0;
                }
            }
            else if (chargeBall > 0)
            {
                throwBall = true;
                dashCooldown.AbilityEnabled();
            }
        }

        // Send input to clone if necessary
        else if (cloneWithBall)
        {
            if (lockInput)
            {
                cloneWithBall.SetTarget(GetComponent<Rigidbody>());
            }
            else
            {
                cloneWithBall.SetTarget(null);
            }

            if (throwInput)
            {
                cloneWithBall.Fire();
                throwInput = false;
            }
        }

        // Update lock on targets
        if (!lockedTarget && ball)
        {
            if (lockInput)
            {
                List<Rigidbody> availableClones = new List<Rigidbody>();

                Vector3 rayStart = transform.position;
                Debug.Log("Offset:" + transform.position + transform.right);
                for (int i = 1; i < searchWidth*2; i++)
                {
                    float offset = i/2 * searchStep;
                    if (i%2 == 0)
                    {
                        offset *= -1;
                    }
                    rayStart += transform.position + (transform.right * offset);
                    Ray lockRay = new Ray(rayStart, transform.forward); ;
                    RaycastHit[] hitInfos = Physics.RaycastAll(lockRay);
                    foreach (RaycastHit hitInfo in hitInfos)
                    {
                        if (hitInfo.rigidbody && hitInfo.rigidbody.tag == "Clone" && hitInfo.rigidbody.GetComponent<PlayerData>().playerNumber == playerNumber)
                        {
                            availableClones.Add(hitInfo.rigidbody);
                        }
                    }
                }
                if (availableClones.Count > 0)
                {
                    lockedTarget = availableClones[0];
                    crosshair.SetTarget(lockedTarget);
                }
            }
        }
        else if (!lockInput || !ball)
        {
            lockedTarget = null;
            crosshair.SetTarget(null);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if not holding ball and object is ball
        if (!ball && collision.transform.tag == "Ball")
        {
            //Debug.Log("balldetected");
            PlayerData ballData = collision.gameObject.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                ///Debug.Log("Claiming un-owned ball");
                ClaimBall(collision);
            }

            // if ball is of opponent's color
            else if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber)
            {
                

                if (!collision.transform.parent) {
                    ballData.playerNumber = playerNumber;
                    scoringManager.SetCurrentPlayer(playerNumber);
                    ClaimBall(collision);
                    ball.GetComponent<BallScript>().ClearCharge();
                }
                else {
                    // tag ball
                    if (GetComponent<PlayerMovement>().GetDashStatus() == true) {
                        var opponent = collision.transform.parent;
                        opponent.GetComponent<PlayerThrowBall>().dashCooldown.AbilityEnabled();
                        ClaimBall(collision);
                        ball.GetComponent<BallScript>().ClearCharge();
                        if (opponent.GetComponent<PlayerThrowBall>().CheckIfHasBall()) {
                            opponent.GetComponent<PlayerThrowBall>().ReleaseBall();
                        }
                    }
                }
            }

            // if ball is of player's color
            else {
                if (GetComponent<PlayerMovement>().GetStunStatus() == false) {
                    ClaimBall(collision);
                }
            }
        }

        // Stun code
        else if (collision.transform.tag == "Player" && GetComponent<PlayerMovement>().GetDashStatus() == true) {
            var opponentBall = collision.gameObject.GetComponent<PlayerThrowBall>().ball;
            
                Debug.Log("Stunning opponent player.");

            if (opponentBall != null) {
                opponentBall.transform.parent = null;
                opponentBall.GetComponent<Rigidbody>().isKinematic = false;
                opponentBall.GetComponent<Rigidbody>().AddForce(transform.forward * GameConfigurations.horizontalThrowingForce / 50);
                collision.gameObject.GetComponent<PlayerThrowBall>().dashCooldown.AbilityEnabled();
                collision.gameObject.GetComponent<PlayerThrowBall>().ReleaseBall();
            }

            collision.gameObject.GetComponent<PlayerMovement>().SetStunStatus(true);
            collision.gameObject.GetComponent<PlayerMovement>().StartExplosion(GameConfigurations.stunningSpeed, GameConfigurations.stunningFrame, transform.position);
        }
    }

    public void SetCloneWithBall(CloneHitByBall clone)
    {
        cloneWithBall = clone;
    }

    private void ClaimBall(Collision collision) {
        // Set ball attributes to current player
        ball = collision.gameObject;
        ball.GetComponent<PlayerData>().playerNumber = playerNumber;

        // Pick up ball
        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        dashCooldown.AbilityDisabled();
    }

    public void Reset()
    {
        ball = null;
        chargeBall = 0;
        lockedTarget = null;
        crosshair.SetTarget(null);
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    public void ReleaseBall() {
        ball = null;
    }

    public bool CheckIfHasBall() {
        return (ball != null);
    }

    public bool CheckIfCharging()
    {
        return (chargeBall > 0 || throwInput);
    }
}