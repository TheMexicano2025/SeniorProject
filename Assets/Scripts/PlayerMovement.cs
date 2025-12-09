using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script handles player movement including walking sprinting and jumping
// it uses physics based movement with a rigidbody
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier; // how much control you have while in air
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Player Rotation")]
    public Transform playerObj;
    public float rotationSpeed = 10f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        if (playerObj == null)
        {
            playerObj = transform.Find("PlayerObj");
        }
    }

    private void Update()
    {
        // check if player is touching ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);

        MyInput();
        SpeedControl();
        StateHandler();

        // apply drag when grounded to slow down naturally
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        
        // prevent player from bouncing on top of cows
        Collider[] cowColliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider col in cowColliders)
        {
            if (col.CompareTag("Cow") && rb.velocity.y > 0.5f)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
        }
    }

    private void LateUpdate()
    {
        RotatePlayerModel();
    }

    // read player input for movement and jumping
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // switch between walking sprinting and air states
    private void StateHandler()
    {
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking; 
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    // apply movement force based on input direction
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    // rotate the player model to face movement direction
    private void RotatePlayerModel()
    {
        if (playerObj == null)
            return;

        if (horizontalInput != 0 || verticalInput != 0)
        {
            Vector3 targetDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                playerObj.rotation = Quaternion.Slerp(playerObj.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    // prevent player from moving faster than max speed
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    // make player jump upward
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // prevent bouncing when landing on cows
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cow"))
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, 0), rb.velocity.z);
            
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                    break;
                }
            }
        }
    }

    // keep player from bouncing while standing on cows
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cow"))
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
        }
    }
}
