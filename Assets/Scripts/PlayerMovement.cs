using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    // Config
    private float speed;
    public Vector3 spawnLocation;
    public Vector3 spawnRotation;

    public PlayerData.PlayerNumber playerNumber;

    private Rigidbody rb;

    // State info
    private Vector2 movement;
    private int dashingFrame;
    private Vector3 lastRotation;
    private int dashCD;

    private int currentExplosionFrame;
    private int explosionFrameDuration;
    private Vector3 explosionDirection;
    private float explosionSpeed;

    private bool stunned;

    // Experimental
    public Queue<Vector3> lastPositions;
    private float timeLeft = GameConfigurations.roundDuration;
    public int framesToSkip = 3;
    private int frame = 0;

    PlayerControls controls;
    private float rotationInput = 0;

    [SerializeField]
    private GameObject stunText;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Dash.canceled += ctx =>
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
        if (currentExplosionFrame == 0) {
            movement = context.ReadValue<Vector2>();
        }
        else {
            movement = Vector2.zero;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.action.triggered && dashCD == 0) {
            dashingFrame = GameConfigurations.dashingFrame;
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotationInput = context.ReadValue<Vector2>().x;
    }

    private void Start()
    {
        movement = Vector2.zero;
        speed = GameConfigurations.baseMovementSpeed;
        dashingFrame = 0;
        dashCD = 0;

        currentExplosionFrame = 0;
        explosionFrameDuration = 0;
        explosionDirection = Vector3.zero;
        explosionSpeed = 0;

        stunned = false;

        rb = GetComponent<Rigidbody>();
        playerNumber = GetComponent<PlayerData>().playerNumber;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Reset();
    }

    private void FixedUpdate()
    {
        Vector3 movementVector = new Vector3(movement.x, 0, movement.y).normalized * speed;

        if (currentExplosionFrame > 0) {
            float explosionFactor = explosionSpeed / explosionFrameDuration;
            float explosionBonus = explosionSpeed - explosionFactor * (explosionFrameDuration - currentExplosionFrame);

            currentExplosionFrame--;

            rb.AddForce(explosionDirection * explosionBonus, ForceMode.Impulse);

            if (currentExplosionFrame == 0) {
                SetStunStatus(false);
            }
        }

        if (dashingFrame > 0) {
            float dashFactor = GameConfigurations.dashSpeed / GameConfigurations.dashingFrame;
            float dashBonus = GameConfigurations.dashSpeed - dashFactor * (GameConfigurations.dashingFrame - dashingFrame);

            dashingFrame --;
            if (dashingFrame == 0)
                dashCD = GameConfigurations.dashCD;

            Vector3 dashVector;
            if (movement == Vector2.zero) {
                dashVector = Vector3.forward * dashBonus;
            }
            else {
                dashVector = movementVector.normalized * dashBonus;
            }
            movementVector += dashVector;
        }

        Vector3 vel;
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

        transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput, lastRotation.z);
        lastRotation = transform.eulerAngles;
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

    public bool GetDashStatus() {
        return dashingFrame > 0;
    }

    public bool GetStunStatus() {
        return stunned;
    }

    public void SetStunStatus(bool ifStun) {
        stunned = ifStun;
        stunText.SetActive(ifStun);
    }

    public void StartExplosion(float explosionSpeed, int explosionFrameDuration, Vector3 from) {
        this.explosionSpeed = explosionSpeed;
        this.explosionFrameDuration = explosionFrameDuration;

        var direction = transform.position - from;
        direction.y = 0;
        this.explosionDirection = direction.normalized;

        this.currentExplosionFrame = explosionFrameDuration;
    }
}
