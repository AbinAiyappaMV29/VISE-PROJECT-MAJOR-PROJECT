using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    [SerializeField] private float minX = -36.1f;  // Minimum X boundary
    [SerializeField] private float maxX = 36.7f; // Maximum X boundary
    [SerializeField] private float minZ = -46.8f;   // Minimum Z boundary
    [SerializeField] private float maxZ = 45.6f;  // Maximum Z boundary

    void Update()
    {
        // Get input from WASD or arrow keys
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Create movement vector
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // Move the player
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // Clamp position within boundaries
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, minX, maxX),
            transform.localPosition.y,
            Mathf.Clamp(transform.localPosition.z, minZ, maxZ)
        );
    }
}
