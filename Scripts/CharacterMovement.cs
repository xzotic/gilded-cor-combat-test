using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.6f;

    [Header("Look")]
    public float mouseSensitivity = 0.1f;
    public Transform cameraPivot;

    [Header("Vertical Look Limits")]
    public float minPitch = -10f;
    public float maxPitch = 30f;

    [Header("Camera Lock")]
    public bool lockCamera = false;

    [Header("Camera Collision")]
    public float cameraRadius = 0.25f;
    public float cameraSmooth = 12f;
    public LayerMask cameraCollisionMask;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;

    // Camera
    private Transform cam;
    private Vector3 cameraDefaultLocalPos;

    // Input (PER PlayerInput)
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Get actions from THIS PlayerInput
        InputActionMap map = playerInput.currentActionMap;
        moveAction = map.FindAction("Move");
        lookAction = map.FindAction("Look");
        jumpAction = map.FindAction("Jump");

        cam = Camera.main.transform;
        cameraDefaultLocalPos = cam.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadInput();
        HandleMovement();

        if (!lockCamera)
        {
            HandleLook();
            HandleCameraCollision();
        }
    }

    // ---------------- INPUT ----------------

    void ReadInput()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame())
            Jump();

        cachedMove = moveInput;
        cachedLook = lookInput;
    }

    private Vector2 cachedMove;
    private Vector2 cachedLook;

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector3 move =
            transform.right * cachedMove.x +
            transform.forward * cachedMove.y;

        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // ---------------- LOOK ----------------

    void HandleLook()
    {
        float mouseX = cachedLook.x * mouseSensitivity;
        float mouseY = cachedLook.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ---------------- CAMERA COLLISION ----------------

    void HandleCameraCollision()
    {
        Vector3 pivotPos = cameraPivot.position;
        Vector3 desiredWorldPos =
            pivotPos + cameraPivot.TransformDirection(cameraDefaultLocalPos);

        Vector3 direction = desiredWorldPos - pivotPos;
        float maxDistance = direction.magnitude;
        direction.Normalize();

        float distance = maxDistance;

        if (Physics.SphereCast(
            pivotPos,
            cameraRadius,
            direction,
            out RaycastHit hit,
            maxDistance,
            cameraCollisionMask,
            QueryTriggerInteraction.Ignore))
        {
            distance = hit.distance - cameraRadius;
        }

        distance = Mathf.Clamp(distance, 0.6f, maxDistance);

        cam.position = Vector3.Lerp(
            cam.position,
            pivotPos + direction * distance,
            cameraSmooth * Time.deltaTime);
    }

    // ---------------- EXTERNAL ----------------

    public void SetCameraLock(bool locked)
    {
        lockCamera = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}
