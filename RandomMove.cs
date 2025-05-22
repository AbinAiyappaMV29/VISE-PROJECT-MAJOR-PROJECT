using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float moveTime = 3f;
    private float timer;
    private Vector3 moveDirection;

    [SerializeField] private float minX = -31f;  // Minimum X boundary
    [SerializeField] private float maxX = 33.4f; // Maximum X boundary
    [SerializeField] private float minZ = -42.8f;   // Minimum Z boundary
    [SerializeField] private float maxZ = 41.5f;  // Maximum Z boundary

    void Start()
    {
        SetRandomDirection();
        timer = moveTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Move the player in the random direction
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // After a certain amount of time, change the movement direction
        if (timer <= 0)
        {
            SetRandomDirection();
            timer = moveTime;
        }

        // Ensure the player stays within the boundaries using localPosition
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, minX, maxX),
            transform.localPosition.y,
            Mathf.Clamp(transform.localPosition.z, minZ, maxZ)
        );
    }

    private void SetRandomDirection()
    {
        // Set a random direction (up, down, left, right), ensuring it stays within the boundary
        moveDirection = new Vector3(
            Random.Range(minX, maxX), // Limit random range for X-axis
            0,
            Random.Range(minZ, maxZ)  // Limit random range for Z-axis
        ).normalized;
    }
}