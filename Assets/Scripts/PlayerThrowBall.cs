using UnityEngine;

public class PlayerThrowBall : MonoBehaviour
{
    // Config
    [SerializeField]
    private float ballDistance = 2f;
    [SerializeField]
    private float throwingForce = 1000f;
    private PlayerData.PlayerNumber playerNumber;

    // State info
    private bool throwBall = false;
    private GameObject ball;

    public bool isClone = false;
    private bool cloneKnockdown;

    [SerializeField]
    private ScoringManager scoringManager;

    // Start is called before the first frame update
    void Start()
    {
        playerNumber = GetComponent<PlayerData>().playerNumber;
        cloneKnockdown = false;
    }

    private void FixedUpdate()
    {
        if (throwBall)
        {
            throwBall = false;
            ball.transform.parent = null;
            ball.GetComponent<Rigidbody>().isKinematic = false;
            ball.GetComponent<Rigidbody>().AddForce(transform.forward * throwingForce);
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

            bool throwInput = false;
            switch (playerNumber)
            {
                case PlayerData.PlayerNumber.PlayerOne:
                    throwInput = Input.GetButtonDown("P1Fire");
                    break;
                case PlayerData.PlayerNumber.PlayerTwo:
                    throwInput = Input.GetButtonDown("P2Fire");
                    break;
                default:
                    Debug.LogError("Error: player object not assigned type.");
                    break;
            }

            if (throwInput)
            {
                throwBall = true;
            }
        }

        if (isClone && cloneKnockdown) {
            int knockdownSpeed = 90;
            transform.Rotate(Vector3.forward, knockdownSpeed * Time.deltaTime);

            if (transform.localEulerAngles.z >= 90) {
                cloneKnockdown = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("bonk");
        Debug.Log(collision.transform.tag);
        Debug.Log(!ball);
        // if not holding ball and object is ball
        if (!ball && collision.transform.tag == "Ball")
        {
            Debug.Log("balldetected");
            PlayerData ballData = collision.gameObject.GetComponent<PlayerData>();

            // if ball is not of player's color
            if (collision.gameObject.GetComponent<PlayerData>().playerNumber != playerNumber)
            {
                if (isClone) {
                    Debug.Log("ball passed to enemy clone");
                    cloneKnockdown = true;
                }

                else {
                    Debug.Log("ball passed to enemy player");

                    ballData.playerNumber = playerNumber;
                    scoringManager.SetCurrentPlayer(playerNumber);
                    claimBall(collision);
                }
            }

            // if ball is of player's color
            else {

                if (isClone) {
                    Debug.Log("ball passed to friendly clone");
                }

                else {
                    claimBall(collision);
                }
            }
        }
    }

    private void claimBall(Collision collision) {
        ball = collision.gameObject;
        ball.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;
        ball.transform.parent = transform;
        ball.transform.localPosition = new Vector3(0, 0, ballDistance);
        ball.GetComponent<Rigidbody>().isKinematic = true;
    }
}