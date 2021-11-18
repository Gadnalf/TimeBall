using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowBall : MonoBehaviour
{
    // Config
    public PlayerData.PlayerNumber playerNumber;
    //private CrosshairScript crosshair;
    public float lockDistance = 10000;

    // Pass config
    public int searchWidth = 100;
    public float searchStep = 0.2f;

    // State info
    private bool throwBall = false;
    private bool passBall = false;
    private float throwHeldDown;
    private float chargeTime;
    private bool passHeldDown;
    private GameObject ball;
    private Rigidbody lockedTarget;
    private CloneHitByBall cloneWithBall;

    private ScoringManager scoringManager;

    PlayerControls controls;
    private bool throwInput = false;
    private bool passInput = false;

    private CooldownTimer dashCooldown;

    private int frame;
    private PlayerConfig playerConfig;
    private PlayerRecording records;

    private GameObject[] clones;
    private List<GameObject> playerClones = new List<GameObject>();

    // sound effects
    [HideInInspector]
    public AudioManager audioManager;
    private AudioSource throwBallSound;
    private AudioSource stunSound;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Throw.canceled += ctx =>
        {
            throwInput = false;
        };

        controls.Gameplay.Lockon.canceled += ctx =>
        {
            passInput = false;
            passBall = false;
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
        passInput = context.action.triggered;
    }

    // Start is called before the first frame update
    void Start()
    {
        records = GetComponent<PlayerRecording>();
        playerNumber = GetComponent<PlayerData>().playerNumber;
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

        ChargeBorderScript[] borders = FindObjectsOfType<ChargeBorderScript>();
        foreach (ChargeBorderScript border in borders)
        {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && border.name.StartsWith("P1")
                || GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && border.name.StartsWith("P2")) {
                border.playerBallScript = this;
                border.enabled = true;
            }
        }

        audioManager = FindObjectOfType<AudioManager>();
        throwBallSound = audioManager.GetAudio("ThrowBall");
        stunSound = audioManager.GetAudio("Stunning");
    }

    private void FixedUpdate() {
        //int? passTargetId = lockedTarget?.GetComponent<CloneController>().cloneData.RoundNumber;

        if (ball != null) {
            chargeTime += Time.deltaTime;

            if (chargeTime > GameConfigurations.ballChargeTime) {
                ball.GetComponent<BallScript>().AddCharge(1, GameConfigurations.goalShieldBreakableCharge - 1);
                chargeTime = 0;
            }
        }

        if (throwBall || passBall) {
            throwBall = false;
            passBall = false;
            ball.transform.parent = null;
            ball.GetComponent<PlayerData>().playerNumber = playerNumber;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 throwForce = transform.forward * GameConfigurations.horizontalThrowingForce;
            throwForce *= GameConfigurations.speedBoostFactor * (1 + ball.GetComponent<BallScript>().GetCharge());
            throwForce += Vector3.up * GameConfigurations.verticalThrowingForce;
            ball.GetComponent<Rigidbody>().AddForce(throwForce);
            ball.GetComponent<BallScript>().SetHomingTarget(lockedTarget);
            lockedTarget?.GetComponent<CloneController>().Pause();
            ball = null;
            lockedTarget = null;
            throwHeldDown = 0;
            chargeTime = 0;
            dashCooldown.AbilityEnabled();
            throwBallSound.Play();
        }

        records.RecordThrowInput(throwInput, frame);
        //records.RecordPassInput(passTargetId, frame);
        frame++;

    }

    // Update is called once per frame
    void Update()
    {
        // Check if ball gone
        if (ball && ball.transform.parent != transform)
        {
            ball = null;
            throwHeldDown = 0;
            lockedTarget = null;
            return;
        }

        // Increment hold time if throwInput is held
        if (throwInput)
        {
            throwHeldDown += Time.deltaTime;
        }
        // If throw input was released
        else
        {
            if (throwHeldDown > 0)
            {
                // If ball, charge and throw it
                if (ball) {
                    throwHeldDown = 0;
                    throwBall = true;
                }
                // If no ball
                else
                {
                    // Send input to clone if exists
                    if (cloneWithBall)
                    {
                        cloneWithBall.Fire();
                    }
                }
                throwHeldDown = 0;
            }
        }

        // Pass input is held
        if (passInput)
        {
            // If passHeldDown hasn't cleared, passInput has not been released or the pass hasn't happened yet.
            if (!passHeldDown)
            {
                passHeldDown = true;
                if (ball)
                {
                    playerClones.Clear();
                    clones = GameObject.FindGameObjectsWithTag("Clone");
                    foreach (GameObject clone in clones)
                    {
                        if (clone.GetComponent<PlayerData>().playerNumber == playerNumber)
                            playerClones.Add(clone);
                    }

                    lockedTarget = GetClosestClone();
                    if (lockedTarget != null)
                    {
                        passBall = true;
                    }
                }
                else
                {
                    // If the clone has the ball instead
                    if (cloneWithBall)
                    {
                        cloneWithBall.SetTarget(GetComponent<Rigidbody>());
                        cloneWithBall.Fire();
                    }
                }
            }
        }
        // Pass input is released
        else
        {
            // Wait for pass to resolve before clearing locked target
            if (!passBall)
            {
                passHeldDown = false;
                if (cloneWithBall)
                {
                    cloneWithBall.SetTarget(null);
                }
            }
        }
    }

    private Rigidbody GetClosestClone()
    {
        lockedTarget = null;
        float angle = GameConfigurations.passAngle/2;

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

        if (lockedTarget == null)
        {
            Debug.Log("No clones located.");
        }
        return lockedTarget;
    }

    private Vector3 DirectionTo(GameObject clone)
    {
        return Vector3.Normalize(clone.transform.position - ball.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if not holding ball and object is ball
        if (!ball && collision.transform.tag == "Ball")
        {
            ball = collision.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                ///Debug.Log("Claiming un-owned ball");
                ClaimBall(ball);
                ball.GetComponent<BallScript>().ClearCharge();
            }

            // if ball is of opponent's color
            else if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber)
            {

                if (!collision.transform.parent) {
                    ballData.playerNumber = playerNumber;
                    scoringManager.SetCurrentPlayer(playerNumber);
                    ClaimBall(ball);
                    ball.GetComponent<BallScript>().ClearCharge();
                }
                else {
                    // tag ball
                    if (GetComponent<PlayerMovement>().GetDashStatus() == true) {
                        var opponent = collision.transform.parent;
                        if (opponent.tag == "Player") {
                            opponent.GetComponent<PlayerThrowBall>().dashCooldown.AbilityEnabled();
                            if (opponent.GetComponent<PlayerThrowBall>().CheckIfHasBall())
                                opponent.GetComponent<PlayerThrowBall>().ReleaseBall();
                        }
                        ClaimBall(ball);
                        ball.GetComponent<BallScript>().ClearCharge();
                        
                    }
                }
            }

            // if ball is of player's color
            else {
                if (GetComponent<PlayerMovement>().GetStunStatus() == false) {
                    ClaimBall(ball);
                }
            }
        }

        // Stun code
        else if (collision.transform.tag == "Player" && GetComponent<PlayerMovement>().GetDashStatus() == true) {
            var opponentBall = collision.gameObject.GetComponent<PlayerThrowBall>().ball;
            
            Debug.Log("Stunning opponent player.");

            if (opponentBall != null) {
                opponentBall.transform.parent = null;
                collision.gameObject.GetComponent<PlayerThrowBall>().dashCooldown.AbilityEnabled();
                collision.gameObject.GetComponent<PlayerThrowBall>().ReleaseBall();
                ball = opponentBall;
                ball.GetComponent<PlayerData>().playerNumber = playerNumber;

                // Pick up ball
                opponentBall.transform.parent = transform;
                opponentBall.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
                ball.GetComponent<Rigidbody>().isKinematic = true;
                dashCooldown.AbilityDisabled();
            }

            collision.gameObject.GetComponent<PlayerMovement>().SetStunStatus(true);
            stunSound.Play();
            collision.gameObject.GetComponent<PlayerMovement>().StartExplosion(GameConfigurations.stunningSpeed, GameConfigurations.stunningFrame, transform.position);
        }
    }

    public void SetCloneWithBall(CloneHitByBall clone)
    {
        cloneWithBall = clone;
    }

    public void ClaimBall(GameObject ball) {
        // Set ball attributes to current player
        this.ball = ball;
        ball.GetComponent<PlayerData>().playerNumber = playerNumber;

        // Pick up ball
        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        dashCooldown.AbilityDisabled();
        chargeTime = 0;
    }

    public void Reset()
    {
        ball = null;
        throwHeldDown = 0;
        lockedTarget = null;
        frame = 0;
        chargeTime = 0;
        clones = GameObject.FindGameObjectsWithTag("Clone");
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
        return throwHeldDown > 0 || throwInput;
    }

    public int GetPotentialCharge()
    {
        return (int)(throwHeldDown / GameConfigurations.ballChargeTime);
    }
}