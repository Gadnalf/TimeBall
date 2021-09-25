using UnityEngine;

[RequireComponent(typeof(PlayerData))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Config
    public float speed = 10;
    public float jumpSpeed = 150;
    public float groundDistance = 10;
    private PlayerData.PlayerNumber playerNumber;

    private Rigidbody rb;

    // State info
    private float horizontalInput;
    private float verticalInput;
    private bool jump;

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
    }
}
