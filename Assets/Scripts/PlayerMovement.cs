using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    // Config
    public float speed = 10;
    public float jumpSpeed = 150;
    public float groundDistance = 10;
    public float recordLength = GameSettings.roundDuration;
    public Vector3 spawnLocation;
    public Vector3 spawnRotation;

    public PlayerData.PlayerNumber playerNumber;
    private Camera playerCamera;

    private Rigidbody rb;

    //private float horizontalInput;
    //private float verticalInput;

    // State info
    private Vector2 movement = Vector2.zero;

    private bool jump;
    private Vector3 lastRotation;

    // Experimental
    public Queue<Vector3> lastPositions;
    private float timeLeft = GameSettings.roundDuration;
    public int framesToSkip = 3;
    private int frame = 0;

    PlayerControls controls;
    private bool jumped = false;

    private float rotationInput = 0;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Jump.canceled += ctx =>
        {
            jumped = false;
        };

        controls.Gameplay.Move.canceled += ctx =>
        {
            movement = Vector2.zero;
        };

        controls.Gameplay.Rotate.canceled += ctx =>
        {
            rotationInput = 0f;
        };
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //jumped = context.ReadValue<bool>();
        jumped = context.action.triggered;
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>().x;
        //Debug.Log(rotationInput);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerNumber = GetComponent<PlayerData>().playerNumber;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponentInChildren<Camera>();
        Reset();
    }

    private void FixedUpdate()
    {
        transform.eulerAngles = playerCamera.transform.eulerAngles;

        Vector3 forwardMovement = movement.y * Vector3.forward;
        Vector3 sideMovement = movement.x * Vector3.right;
        Vector3 movementVector = (forwardMovement + sideMovement).normalized * speed;
        if (jumped)
        {
            movementVector += Vector3.up * jumpSpeed;
            jumped = false;
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

        //bool jumpInput = false;
        //// Keyboard inputs
        //switch (playerNumber)
        //{
        //    case PlayerData.PlayerNumber.PlayerOne:
        //        horizontalInput = Input.GetAxisRaw("P1Horizontal");
        //        verticalInput = Input.GetAxisRaw("P1Vertical");
        //        jumpInput = Input.GetButtonDown("P1Jump");
        //        break;
        //    case PlayerData.PlayerNumber.PlayerTwo:
        //        horizontalInput = Input.GetAxisRaw("P2Horizontal");
        //        verticalInput = Input.GetAxisRaw("P2Vertical");
        //        jumpInput = Input.GetButtonDown("P2Jump");
        //        break;
        //    default:
        //        Debug.LogError("Player object not assigned type.");
        //        break;
        //}

        if (jumped && grounded)
        {
            jumped = true;
        }

        //switch (playerNumber)
        //{
        //    case PlayerData.PlayerNumber.PlayerOne:
        //        rotationInput = Input.GetAxis("P1Camera");
        //        break;
        //    case PlayerData.PlayerNumber.PlayerTwo:
        //        // TODO: Getting button input rather than mouse, should be GetAxis
        //        rotationInput = Input.GetAxisRaw("P2Camera") * 500;
        //        break;
        //    default:
        //        Debug.LogError("Player object not assigned type.");
        //        break;
        //}

        transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput, lastRotation.z);
        lastRotation = transform.eulerAngles;
    }


    public void Reset()
    {
        rb.transform.position = spawnLocation;
        rb.transform.eulerAngles = spawnRotation;
        lastRotation = spawnRotation;
        rb.velocity = Vector3.zero;
        timeLeft = recordLength;
        lastPositions = new Queue<Vector3>();
    }


    private void OnEnable()
    {
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

}
