using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = 9.81f;  // Simulate gravity for the character
    private float moveSpeed;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveSpeed = walkSpeed;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // A (-1) / D (+1)
        float moveZ = Input.GetAxisRaw("Vertical");   // W (+1) / S (-1)

        moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Sprinting logic
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = walkSpeed;
        }

        // Apply gravity
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the player
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
