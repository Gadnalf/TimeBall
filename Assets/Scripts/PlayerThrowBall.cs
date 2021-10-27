using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowBall : MonoBehaviour
{
    // Config
    public PlayerData.PlayerNumber playerNumber;
    public CrosshairScript crosshair;
    public float lockDistance = 10000;

    // State info
    private bool throwBall = false;
    private bool throwBoost = false;
    private GameObject ball;
    private Rigidbody lockedTarget;
    private CloneHitByBall cloneWithBall;

    [SerializeField]
    private ScoringManager scoringManager;

    PlayerControls controls;
    private bool throwInput = false;
    private bool lockInput;

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
    }

    private void FixedUpdate()
    {
        if (throwBall)
        {
            throwBall = false;
            ball.transform.parent = null;
            ball.GetComponent<PlayerData>().playerNumber = playerNumber;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 throwForce = transform.forward * GameConfigurations.throwingForce;
            if (throwBoost)
            {
                Debug.Log("boosted");
                throwBoost = false;
                throwForce *= GameConfigurations.speedBoostFactor;
            }
            throwForce += Vector3.up * GameConfigurations.verticalThrowingForce;
            ball.GetComponent<Rigidbody>().AddForce(throwForce);
            ball.GetComponent<BallScript>().SetHomingTarget(lockedTarget);
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
            return;
        }

        // Update throw inputs
        if (ball)
        {
            if (throwInput)
            {
                throwBall = true;
            }
            throwInput = false;
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
                Ray lockRay = new Ray(transform.position, transform.forward);
                RaycastHit[] hitInfos = Physics.RaycastAll(lockRay);
                foreach (RaycastHit hitInfo in hitInfos)
                {
                    if (hitInfo.rigidbody && hitInfo.rigidbody.tag == "Clone" && hitInfo.rigidbody.GetComponent<PlayerData>().playerNumber == playerNumber)
                    {
                        lockedTarget = hitInfo.rigidbody;
                        crosshair.SetTarget(lockedTarget);
                        break;
                    }
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
                }
                else {
                    if (GetComponent<PlayerMovement>().GetDashStatus() == true) {
                        Debug.Log("Tag ball.");
                        ClaimBall(collision);
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

        else if (collision.transform.tag == "Player" && GetComponent<PlayerMovement>().GetDashStatus() == true) {
            var opponentBall = collision.gameObject.GetComponent<PlayerThrowBall>().ball;
            if (opponentBall != null) {
                Debug.Log("Stunning opponent player.");

                opponentBall.transform.parent = null;
                opponentBall.GetComponent<Rigidbody>().isKinematic = false;
                opponentBall.GetComponent<Rigidbody>().AddForce(transform.forward * GameConfigurations.throwingForce / 50);
                collision.gameObject.GetComponent<PlayerThrowBall>().ReleaseBall();

                collision.gameObject.GetComponent<PlayerMovement>().SetStunStatus(true);
                collision.gameObject.GetComponent<PlayerMovement>().StartExplosion(GameConfigurations.stunningSpeed, GameConfigurations.stunningFrame, transform.position);
            }
        }
    }

    public void SetCloneWithBall(CloneHitByBall clone)
    {
        cloneWithBall = clone;
    }

    private void ClaimBall(Collision collision) {
        ball = collision.gameObject;
        ball.GetComponent<PlayerData>().playerNumber = playerNumber;
        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;

        BallScript ballscript = ball.GetComponent<BallScript>();
        if (ballscript.IsHomingTarget(GetComponent<Rigidbody>()))
        {
            throwBoost = true;
        }
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
}