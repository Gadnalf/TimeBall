using System.Collections.Generic;
using System.Linq;
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
    private bool throwHeldDown;
    private float chargeTime;
    private bool passHeldDown;
    private GameObject ball;
    private Rigidbody lockedTarget;
    private CloneHitByBall cloneWithBall;
    private PlayerGuard guardScript;

    private ScoringManager scoringManager;

    PlayerControls controls;
    private bool throwInput = false;
    private bool passInput = false;

    private CooldownTimer dashCooldown;

    private PlayerConfig playerConfig;
    private PlayerRecording records;

    private Animator animator;
    private bool holdingBall;
    private int throwAnimation;

    // sound effects
    [HideInInspector]
    public AudioManager audioManager;
    private AudioSource throwBallSound;
    private AudioSource tagSound;
    private AudioSource receiveSound;


    private void Awake()
    {
        controls = new PlayerControls();
        animator = GetComponent<Animator>();

        throwAnimation = Animator.StringToHash("Throw");

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
        holdingBall = false;
        records = GetComponent<PlayerRecording>();
        playerNumber = GetComponent<PlayerData>().playerNumber;
        guardScript = GetComponentInChildren<PlayerGuard>();
        scoringManager = FindObjectOfType<ScoringManager>();

        CooldownTimer[] dashCooldowns = FindObjectsOfType<CooldownTimer>(true);
        foreach (CooldownTimer dashCooldown in dashCooldowns)
        {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && dashCooldown.name.StartsWith("P1"))
            {
                this.dashCooldown = dashCooldown;
            }
            else if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && dashCooldown.name.StartsWith("P2"))
            {
                this.dashCooldown = dashCooldown;
            }
        }
        if (dashCooldown == null)
        {
            Debug.LogError("DashCD discovery failed");
        }

        ChargeBorderScript[] borders = FindObjectsOfType<ChargeBorderScript>();
        foreach (ChargeBorderScript border in borders)
        {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && border.name.StartsWith("P1")
                || GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && border.name.StartsWith("P2"))
            {
                border.playerBallScript = this;
                border.enabled = true;
            }
        }

        audioManager = FindObjectOfType<AudioManager>();
        throwBallSound = audioManager.GetAudio("ThrowBall");
        tagSound = audioManager.GetAudio("TagBall");
        receiveSound = audioManager.GetAudio("ReceivePass");
    }

    private void FixedUpdate()
    {
        //int? passTargetId = lockedTarget?.GetComponent<CloneController>().cloneData.RoundNumber;

        if (ball != null)
        {
            HashSet<int> uniqueCharges = ball.GetComponent<BallScript>().uniqueHoldCharges;

            if (!uniqueCharges.Contains(0))
            {
                chargeTime += Time.deltaTime;

                if (chargeTime > GameConfigurations.ballChargeTime)
                {
                    uniqueCharges.Add(0);
                    ball.GetComponent<BallScript>().AddCharge();
                    chargeTime = 0;
                }
            }
        }

        if (throwBall || passBall)
        {
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
            chargeTime = 0;
            dashCooldown.AbilityEnabled();
            throwBallSound.Play();
        }

        records.RecordThrowInput(throwInput);
        records.RecordPassInput(passInput);
    }

    void HandleThrowAnimation()
    {
        animator.CrossFade(throwAnimation, 0f);
    }

    // Update is called once per frame
    void Update()
    {

        if (CheckIfHasBall())
        {
            holdingBall = true;
        } else
        {
            holdingBall = false;
        }

        animator.SetBool("holdingBall", holdingBall);

        // Check if ball gone
        if (ball && ball.transform.parent != transform)
        {
            ball = null;
            lockedTarget = null;
            return;
        }

        if (throwInput)
        {
            if (!throwHeldDown) // One throw per input down
            {
                throwHeldDown = true;
                if (ball)
                {
                    throwBall = true;
                    HandleThrowAnimation();
                }
                else
                {
                    // Send input to clone
                    if (cloneWithBall)
                    {
                        cloneWithBall.Fire();
                    }
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
                    LockOnClosestClone();
                    if (lockedTarget != null)
                    {
                        passBall = true;
                        HandleThrowAnimation();
                    }
                }
                else
                {
                    // Send input to clone
                    if (cloneWithBall)
                    {
                        cloneWithBall.SetTarget(GetComponent<Rigidbody>());
                        cloneWithBall.Fire();
                    }
                }
            }
        }
        else
        {
            passHeldDown = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if not holding ball and object is ball
        if (!ball && collision.transform.tag == "Ball")
        {
            ball = collision.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.NoPlayer)
            {
                ///Debug.Log("Claiming un-owned ball");
                ClaimBall(ball);
                ball.GetComponent<BallScript>().ClearCharge();
            }

            // if ball is of opponent's color
            else if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber)
            {
                if (!collision.transform.parent)
                {
                    ballData.playerNumber = playerNumber;
                    scoringManager.SetCurrentPlayer(playerNumber);
                    ClaimBall(ball);
                    ball.GetComponent<BallScript>().ClearCharge();
                }
                else
                {
                    // tag ball
                    if (GetComponent<PlayerMovement>().GetDashStatus() == true)
                    {
                        var opponent = collision.transform.parent;
                        if (opponent.tag == "Player")
                        {
                            opponent.GetComponent<PlayerThrowBall>().dashCooldown.AbilityEnabled();
                            if (opponent.GetComponent<PlayerThrowBall>().CheckIfHasBall())
                                opponent.GetComponent<PlayerThrowBall>().ReleaseBall();
                        }
                        ClaimBall(ball);
                        tagSound.Play();
                        ball.GetComponent<BallScript>().ClearCharge();
                    }
                }
            }

            // if ball is of player's color
            else
            {
                if (GetComponent<PlayerMovement>().GetStunStatus() == false)
                {
                    ClaimBall(ball);
                    if (ball.GetComponent<BallScript>().fromClone)
                    {
                        ball.GetComponent<BallScript>().fromClone = false;
                        receiveSound.Play();
                    }
                }
            }
        }

        // Stun code
        else if (collision.transform.tag == "Player" && GetComponent<PlayerMovement>().GetDashStatus() == true)
        {
            var opponentBall = collision.gameObject.GetComponent<PlayerThrowBall>().ball;

            Debug.Log("Stunning opponent player.");

            if (opponentBall != null)
            {
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
            tagSound.Play();
            collision.gameObject.GetComponent<PlayerMovement>().StartExplosion(GameConfigurations.stunningSpeed, GameConfigurations.stunningFrame, transform.position);
        }
    }

    private void LockOnClosestClone()
    {
        // Rediscover clones
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Clone");
        GameObject[] playerClones = clones.Where((GameObject clone) => clone.GetComponent<PlayerData>().playerNumber == playerNumber).ToArray();
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

    public void SetCloneWithBall(CloneHitByBall clone)
    {
        cloneWithBall = clone;
    }

    public void ClaimBall(GameObject ball)
    {
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
        lockedTarget = null;
        chargeTime = 0;
    }

    /*public void ResetOnGoal()
    {
        ball = null;
        throwHeldDown = 0;
        lockedTarget = null;
        chargeTime = 0;
    }*/

    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    public void ReleaseBall()
    {
        ball = null;
    }

    public bool CheckIfHasBall()
    {
        return (ball != null);
    }

    /*public bool CheckIfCharging()
    {
        return throwHeldDown > 0 || throwInput;
    }*/

    public bool CheckIfGuarding()
    {
        return guardScript.IsGuarding();
    }

    public int GetCurrentCharge()
    {
        return ball.GetComponent<BallScript>().GetCharge();
    }
}