using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGuard : MonoBehaviour
{
    private PlayerThrowBall playerBallScript;
    private Collider col;

    PlayerControls controls;
    private PlayerConfig playerConfig;
    private PlayerRecording records;

    private bool guardInput;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Guard.canceled += ctx =>
        {
            guardInput = false;
        };
    }

    public void InitializePlayerConfig(PlayerConfig pc)
    {
        playerConfig = pc;
        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(InputAction.CallbackContext obj)
    {
        if (obj.action.name == controls.Gameplay.Guard.name)
        {
            OnGuard(obj);
        }
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        guardInput = context.action.triggered;
    }

    void Start()
    {
        playerBallScript = GetComponentInParent<PlayerThrowBall>();
        col = GetComponent<Collider>();
        records = GetComponentInParent<PlayerRecording>();
    }

    private void FixedUpdate()
    {
        records.RecordGuardInput(guardInput);

        if (guardInput && !playerBallScript.CheckIfHasBall())
        {
            col.enabled = true;
            float currentScale = transform.localScale.x;
            float scale = Mathf.Min(currentScale + GameConfigurations.guardExpandRate, GameConfigurations.guardMaxSize);
            transform.localScale = Vector3.one * scale;
        }
        else
        {
            col.enabled = false;
            transform.localScale = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Ball")
        {
            GameObject ball = other.gameObject;
            PlayerData ballData = ball.GetComponent<PlayerData>();

            // if ball is of no player's color
            if (ballData.playerNumber == PlayerData.PlayerNumber.NoPlayer)
            {
                playerBallScript.ClaimBall(ball);
                ball.GetComponent<BallScript>().ClearCharge();
            }

            // if ball is of opponent's color
            else if (ballData.playerNumber != GetComponentInParent<PlayerData>().playerNumber)
            {
                playerBallScript.ClaimBall(ball);
                ball.GetComponent<BallScript>().ClearCharge();
            }
        }
    }

    public bool IsGuarding()
    {
        return guardInput;
    }

    public void Reset()
    {
        guardInput = false;
    }

    /*public void ResetOnGoal() { guardInput = false; }*/
}