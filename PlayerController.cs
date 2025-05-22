using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float gravity = 9.8f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");  // A, D
        float moveZ = Input.GetAxis("Vertical");    // W, S

        bool isMoving = moveX != 0 || moveZ != 0;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving;

        float speed = isRunning ? runSpeed : walkSpeed;
        moveDirection = new Vector3(moveX, 0, moveZ).normalized * speed;

        if (controller.isGrounded)
        {
            if (isMoving)
            {
                animator.ResetTrigger("Breathing");
                animator.ResetTrigger("StopWalk");
                animator.ResetTrigger("StopRun");

                if (isRunning)
                {
                    animator.SetTrigger("Running");
                }
                else
                {
                    animator.SetTrigger("Walking");
                }
            }
            else
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Running"))
                {
                    animator.SetTrigger("StopRun");
                }
                else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                {
                    animator.SetTrigger("StopWalk");
                }
                else
                {
                    animator.SetTrigger("Breathing");
                }
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
