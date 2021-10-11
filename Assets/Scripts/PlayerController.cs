using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Config
    public float speed = 10;
    public float jumpSpeed = 150;
    public float groundDistance = 10;
    public float recordLength = GameSettings.roundDuration;
    public Vector3 spawnLocation;
    public Vector3 spawnRotation;

    public PlayerData.PlayerNumber playerNumber;

    private Rigidbody rb;

    // State info
    private float horizontalInput;
    private float verticalInput;
    private bool jump;
    private Vector3 lastRotation;

    // Experimental
    public Queue<Vector3> lastPositions;
    private float timeLeft = GameSettings.roundDuration;
    public int framesToSkip = 3;
    private int frame = 0;

    public void Reset()
    {
        rb.transform.position = spawnLocation;
        rb.transform.eulerAngles = spawnRotation;
        lastRotation = spawnRotation;
        rb.velocity = Vector3.zero;
        timeLeft = recordLength;
        lastPositions = new Queue<Vector3>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerNumber = GetComponent<PlayerData>().playerNumber;
    }

    private void FixedUpdate()
    {
        Vector3 forwardMovement = verticalInput * Vector3.forward;
        Vector3 sideMovement = horizontalInput * Vector3.right;
        Vector3 movementVector = (forwardMovement + sideMovement).normalized * speed;
        if (jump)
        {
            movementVector += Vector3.up * jumpSpeed;
            jump = false;
        }
        rb.AddRelativeForce(movementVector);

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime; 
            if (frame == 0)
            {
                lastPositions.Enqueue(transform.position);
                //Debug.Log(lastPositions.Count);
            }
            frame = (frame + 1) % (framesToSkip + 1);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        bool grounded = Physics.Raycast(groundRay, out hit, 2);
        grounded = grounded && hit.distance < groundDistance;

        bool jumpInput = false;
        // Keyboard inputs
        switch (playerNumber)
        {
            case PlayerData.PlayerNumber.PlayerOne:
                horizontalInput = Input.GetAxisRaw("P1Horizontal");
                verticalInput = Input.GetAxisRaw("P1Vertical");
                jumpInput = Input.GetButtonDown("P1Jump");
                break;
            case PlayerData.PlayerNumber.PlayerTwo:
                horizontalInput = Input.GetAxisRaw("P2Horizontal");
                verticalInput = Input.GetAxisRaw("P2Vertical");
                jumpInput = Input.GetButtonDown("P2Jump");
                break;
            default:
                Debug.LogError("Player object not assigned type.");
                break;
        }

        if (jumpInput && grounded)
        {
            jump = true;
        }

        float rotationInput = 0;
        switch (playerNumber)
        {
            case PlayerData.PlayerNumber.PlayerOne:
                rotationInput = Input.GetAxis("P1Camera");
                break;
            case PlayerData.PlayerNumber.PlayerTwo:
                // TODO: Getting button input rather than mouse, should be GetAxis
                rotationInput = Input.GetAxisRaw("P2Camera") * 500;
                break;
            default:
                Debug.LogError("Player object not assigned type.");
                break;
        }

        transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput * Time.deltaTime, lastRotation.z);
        lastRotation = transform.eulerAngles;
    }
}
