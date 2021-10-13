using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    // Config
    public float speed;
    public float lockDistance = 100;
    public Vector3 spawnLocation;
    public Vector3 spawnRotation;

    public PlayerData.PlayerNumber playerNumber;

    private Rigidbody rb;

    // State info
    private Vector2 movement = Vector2.zero;
    private int dashingFrame;
    private Vector3 lastRotation;
    private int dashCD;

    // Experimental
    public Queue<Vector3> lastPositions;
    private float timeLeft = GameConfigurations.roundDuration;
    public int framesToSkip = 3;
    private int frame = 0;

    PlayerControls controls;
    private float rotationInput = 0;
    private bool lockInput;

    public Rigidbody lockedTarget;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Dash.canceled += ctx =>
        {

        };

        controls.Gameplay.Lockon.canceled += ctx =>
        {

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

    public void OnDash(InputAction.CallbackContext context)
    {
        //jumped = context.ReadValue<bool>();
        if (context.action.triggered && dashCD == 0) {
            // Debug.Log("Player dashing.");
            dashingFrame = GameConfigurations.dashingFrame;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>().x;
        //Debug.Log(rotationInput);
    }

    public void OnLock(InputAction.CallbackContext context)
    {
        lockInput = context.action.triggered;
    }

    private void Start()
    {
        dashingFrame = 0;
        dashCD = 0;
        rb = GetComponent<Rigidbody>();
        playerNumber = GetComponent<PlayerData>().playerNumber;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Reset();
    }

    private void FixedUpdate()
    {
        Vector3 movementVector = new Vector3(movement.x, 0, movement.y).normalized * speed;

        if (dashingFrame > 0) {
            float dashFactor = GameConfigurations.dashSpeed / GameConfigurations.dashingFrame;
            float dashBonus = GameConfigurations.dashSpeed - dashFactor * (GameConfigurations.dashingFrame - dashingFrame);

            dashingFrame --;
            if (dashingFrame == 0)
                dashCD = GameConfigurations.dashCD;

            Vector3 dashVector = Vector3.forward * dashBonus;
            movementVector += dashVector;
        }

        Vector3 vel = new Vector3();
        vel.x = movementVector.x;
        vel.y = rb.velocity.y;
        vel.z = movementVector.z;

        rb.velocity = transform.TransformDirection(vel);

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (frame == 0)
            {
                lastPositions.Enqueue(transform.position);
            }
            frame = (frame + 1) % (framesToSkip + 1);
        }

        if (dashCD != 0) {
            dashCD --;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!lockedTarget)
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
                        break;
                    }
                }
            }
        }

        if (lockedTarget)
        {
            if (!lockInput)
            {
                lockedTarget = null;
            }
            else
            {
                transform.LookAt(lockedTarget.position);
            }
        }
        else
        {
            transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput, lastRotation.z);
            lastRotation = transform.eulerAngles;
        }
    }


    public void Reset()
    {
        rb.transform.position = spawnLocation;
        rb.transform.eulerAngles = spawnRotation;
        lastRotation = spawnRotation;
        rb.velocity = Vector3.zero;
        timeLeft = GameConfigurations.roundDuration;
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

    public int getDashFrame() {
        return dashingFrame;
    }

    public Rigidbody getRb() {
        return rb;
    }
}
