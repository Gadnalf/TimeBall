using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    // Config
    private PlayerConfig playerConfig;

    private float baseMovementSpeed;
    private float withBallMovementSpeed;
    private float ballChargingMovementSpeed;
    public Vector3 spawnLocation;
    public Vector3 spawnRotation;

    public PlayerData.PlayerNumber playerNumber;

    private Rigidbody rb;
    private PlayerRecording records;

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

    private int frame;

    // Input
    PlayerControls controls;
    private float rotationInput = 0;

    [SerializeField]
    private GameObject stunText;

    private CooldownTimer cooldownTimer;

    // Sound effects
    [HideInInspector]
    public AudioManager audioManager;
    private AudioSource runningWithoutBall;

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

    public void InitializePlayer(PlayerConfig pc)
    {
        playerConfig = pc;
        GetComponent<PlayerThrowBall>().InitializePlayerConfig(pc);
        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(InputAction.CallbackContext obj)
    {
        if (obj.action.name == controls.Gameplay.Move.name)
        {
            OnMove(obj);
        }
        else if (obj.action.name == controls.Gameplay.Dash.name)
        {
            OnDash(obj);
        }
        else if (obj.action.name == controls.Gameplay.Rotate.name)
        {
            OnRotate(obj);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.action.triggered && ifCanDash()) {
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
        baseMovementSpeed = GameConfigurations.baseMovementSpeed;
        withBallMovementSpeed = GameConfigurations.withBallMovementSpeed;
        ballChargingMovementSpeed = GameConfigurations.ballChargingMovementSpeed;
        dashingFrame = 0;
        dashCD = 0;

        currentExplosionFrame = 0;
        explosionFrameDuration = 0;
        explosionDirection = Vector3.zero;
        explosionSpeed = 0;

        stunned = false;

        rb = GetComponent<Rigidbody>();
        records = GetComponent<PlayerRecording>();
        playerNumber = GetComponent<PlayerData>().playerNumber;

        CooldownTimer[] cooldownTimers = FindObjectsOfType<CooldownTimer>();
        foreach (CooldownTimer cooldownTimer in cooldownTimers) {
            if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerOne && cooldownTimer.name.StartsWith("P1")) {
                this.cooldownTimer = cooldownTimer;
            }
            else if (GetComponent<PlayerData>().playerNumber == PlayerData.PlayerNumber.PlayerTwo && cooldownTimer.name.StartsWith("P2")) {
                this.cooldownTimer = cooldownTimer;
            }
        }

        audioManager = FindObjectOfType<AudioManager>();
        runningWithoutBall = audioManager.GetAudio("Running");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Reset();
    }

    private void FixedUpdate()
    {
        Vector3 movementVector;
        if (currentExplosionFrame != 0 || (Math.Abs(movement.x) < 0.05f && Math.Abs(movement.y) < 0.05f)) {
            movementVector = Vector3.zero;
        }
        else {
            PlayerThrowBall playerBall = GetComponent<PlayerThrowBall>();

            if (playerBall.CheckIfHasBall()) {

                if (playerBall.CheckIfCharging())
                    movementVector = new Vector3(movement.x, 0, movement.y).normalized * ballChargingMovementSpeed;

                else
                    movementVector = new Vector3(movement.x, 0, movement.y).normalized * withBallMovementSpeed;
            }

            else {
                movementVector = new Vector3(movement.x, 0, movement.y).normalized * baseMovementSpeed;

                if (runningWithoutBall.isPlaying == false)
                    runningWithoutBall.Play();
            }
        }     
        

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
            if (dashingFrame == 0) {
                dashCD = GameConfigurations.dashCDinFrames;
                if (cooldownTimer.gameObject.activeInHierarchy) {
                    cooldownTimer.StartCooldown(GameConfigurations.dashCDinSeconds);
                }
            }

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
        vel.y = 0;
        vel.z = movementVector.z;

        rb.velocity = transform.TransformDirection(vel);

        if (dashCD != 0) {
            dashCD --;
        }

        records.RecordLocation(frame);
        frame++;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.eulerAngles = new Vector3(lastRotation.x, lastRotation.y + rotationInput * Time.deltaTime * GameConfigurations.rotationSpeed, lastRotation.z);
        lastRotation = transform.eulerAngles;
    }


    public void Reset()
    {
        rb.transform.position = spawnLocation;
        rb.transform.eulerAngles = spawnRotation;
        lastRotation = spawnRotation;
        rb.velocity = Vector3.zero;

        cooldownTimer.AbilityEnabled();
        SetStunStatus(false);
        currentExplosionFrame = 0;

        frame = 0;

        GetComponent<PlayerThrowBall>().Reset();
        GetComponent<PlayerRecording>().Reset();
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

    public bool ShouldStopRunningSound ()
    {
        return (currentExplosionFrame != 0 || (Math.Abs(movement.x) < 0.05f && Math.Abs(movement.y) < 0.05f));
    }

    public void StartExplosion(float explosionSpeed, int explosionFrameDuration, Vector3 from) {
        this.explosionSpeed = explosionSpeed;
        this.explosionFrameDuration = explosionFrameDuration;

        var direction = transform.position - from;
        direction.y = 0;
        this.explosionDirection = direction.normalized;

        this.currentExplosionFrame = explosionFrameDuration;
    }

    private bool ifCanDash() {
        if (GetComponent<PlayerThrowBall>()) {
            return dashingFrame == 0 && dashCD == 0 && GetComponent<PlayerThrowBall>().CheckIfHasBall() == false;
        }
        else {
            return true;
        }
    }
}
