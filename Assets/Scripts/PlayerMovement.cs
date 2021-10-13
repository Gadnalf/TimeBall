using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    // Config
    public float speed;
    public float jumpSpeed;
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
    private bool jumped = false;
    private Vector3 lastRotation;
    private bool grounded;

    // Experimental
    public Queue<Vector3> lastPositions;
    private float timeLeft = GameSettings.roundDuration;
    public int framesToSkip = 3;
    private int frame = 0;

    PlayerControls controls;
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
        if (grounded) {
            jumped = context.action.triggered;
        }
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
        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        grounded = Physics.Raycast(groundRay, out hit, 2);
        grounded = grounded && hit.distance < groundDistance;

        Vector3 forwardMovement = movement.y * Vector3.forward;
        Vector3 sideMovement = movement.x * Vector3.right;
        Vector3 movementVector = (forwardMovement + sideMovement).normalized * speed;

        Vector3 vel = new Vector3();
        vel.x = movementVector.x;
        vel.y = rb.velocity.y;
        vel.z = movementVector.z;
        rb.velocity = transform.TransformDirection(vel);

        if (jumped) {
            Debug.Log("Player jumping.");
            jumped = false;
            Vector3 jumpVector = Vector3.up * jumpSpeed;
            rb.AddRelativeForce(jumpVector, ForceMode.Impulse);
        }

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (frame == 0)
            {
                lastPositions.Enqueue(transform.position);
            }
            frame = (frame + 1) % (framesToSkip + 1);
        }
    }

    // Update is called once per frame
    private void Update()
    {
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
