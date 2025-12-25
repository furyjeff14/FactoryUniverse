using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSControllerPolished : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Jump / Gravity")]
    private float jumpHeight = 2.2f;   // meters
    private float gravity = -25f;      // units/sec²

    [Header("Mouse")]
    public float lookSensitivity = 2f;
    public float maxLookX = 90f;
    public float minLookX = -90f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("References")]
    public Camera playerCamera;

    private CharacterController controller;
    private Vector3 moveVelocity;
    private float verticalVelocity;
    private float verticalLookRotation;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (groundCheck == null)
        {
            groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(transform);
            groundCheck.localPosition = new Vector3(0, 0.1f, 0);
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleLook();
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f; // small negative to stick to ground
    }

    void HandleMovement()
    {
        // Get input
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 targetMove = transform.TransformDirection(inputDir);

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Smooth horizontal movement
        moveVelocity = Vector3.MoveTowards(moveVelocity, targetMove * speed, (inputDir.magnitude > 0 ? acceleration : deceleration) * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity (no SmoothDamp)
        verticalVelocity += gravity * Time.deltaTime;

        // Apply movement
        Vector3 finalVelocity = moveVelocity + Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, minLookX, maxLookX);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
