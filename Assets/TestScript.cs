using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{

    private Animator animator;
    PlayerControls pc;
    private Rigidbody rb;

    private Vector2 movement;

    private float VelocityZ, VelocityX;

    private int dashAnimation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        pc = new PlayerControls();

        dashAnimation = Animator.StringToHash("Dash");

        pc.Gameplay.Move.canceled += ctx =>
        {
            movement = Vector2.zero;
            VelocityZ = 0;
            VelocityX = 0;
        };

        pc.Gameplay.Move.performed += ctx =>
        {
            movement = ctx.ReadValue<Vector2>();
        };

        pc.Gameplay.Move.started += ctx =>
        {
            movement = ctx.ReadValue<Vector2>();
        };

        pc.Gameplay.Dash.started += ctx =>
        {
            animator.CrossFade(dashAnimation, 0.15f);
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        movement = Vector2.zero;
    }

    void HandleAnimation(Vector3 move, bool moving)
    {
        if (moving)
        {
            VelocityZ = Vector3.Dot(move.normalized, transform.forward);
            VelocityX = Vector3.Dot(move.normalized, transform.right);
        } else
        {
            VelocityX = 0;
            VelocityZ = 0;
        }

        animator.SetFloat("VelocityZ", VelocityZ, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityX", VelocityX, 0.1f, Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (movement.magnitude > 0)
        {
            movement.Normalize();
            Vector3 move = new Vector3(movement.x, 0, movement.y) * Time.deltaTime;
            HandleAnimation(move, true);
            rb.MovePosition(transform.position + move);
        } else
        {
            HandleAnimation(Vector3.zero, false);
        }
    }


    private void OnEnable()
    {
        pc.Gameplay.Enable();
    }

    private void OnDisable()
    {
        pc.Gameplay.Disable();
    }


}
