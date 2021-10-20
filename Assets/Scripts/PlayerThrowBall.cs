using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowBall : MonoBehaviour
{
    // Config
    public PlayerData.PlayerNumber playerNumber;

    // State info
    private bool throwBall = false;
    private GameObject ball;

    [SerializeField]
    private ScoringManager scoringManager;

    PlayerControls controls;
    private bool throwInput = false;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Throw.canceled += ctx =>
        {
            throwInput = false;
        };
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        throwInput = context.action.triggered;
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
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce(transform.forward * GameConfigurations.throwingForce);
            Rigidbody lockedTarget = gameObject.GetComponent<PlayerMovement>().lockedTarget;
            ball.GetComponent<BallScript>().SetHomingTarget(lockedTarget);
            ball = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ball)
        {
            if (ball.GetComponent<PlayerData>().playerNumber != playerNumber)
            {
                ball = null;
                return;
            }

            if (throwInput)
            {
                throwBall = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("bonk");
        //Debug.Log(collision.transform.tag);
        //Debug.Log(!ball);
        // if not holding ball and object is ball
        if (!ball && collision.transform.tag == "Ball")
        {
            //Debug.Log("balldetected");
            PlayerData ballData = collision.gameObject.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.NoPlayer) {
                claimBall(collision);
            }

            // if ball is of opponent's color
            else if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber)
            {
                if (collision.transform.parent == null) {
                    ballData.playerNumber = playerNumber;
                    scoringManager.SetCurrentPlayer(playerNumber);
                    claimBall(collision);
                }
                else {
                    if (GetComponent<PlayerMovement>().GetDashStatus() == true) {
                        Debug.Log("Tag ball.");
                        claimBall(collision);
                    }
                }
            }

            // if ball is of player's color
            else {
                if (GetComponent<PlayerMovement>().GetStunStatus() == false) {
                    claimBall(collision);
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

    private void claimBall(Collision collision) {
        ball = collision.gameObject;
        ball.GetComponent<PlayerData>().playerNumber = playerNumber;

        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, GameConfigurations.ballHeight, GameConfigurations.ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
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