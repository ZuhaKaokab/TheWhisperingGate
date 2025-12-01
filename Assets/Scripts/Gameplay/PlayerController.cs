using UnityEngine;
using WhisperingGate.Dialogue;
using CameraFocus = WhisperingGate.Camera;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Simple hybrid controller that supports both first- and third-person views with smooth camera follow.
    /// Uses a CharacterController for movement and allows runtime toggling between view modes.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public enum ViewMode { FirstPerson, ThirdPerson }

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float rotationSmoothTime = 0.15f;

        [Header("Camera")]
        [SerializeField] private UnityEngine.Camera playerCamera;
        [SerializeField] private Transform firstPersonAnchor;
        [SerializeField] private Transform thirdPersonAnchor;
        [SerializeField] private float cameraSmoothTime = 0.08f;
        [SerializeField] private float mouseSensitivity = 150f;
        [SerializeField] private Vector2 pitchLimits = new(-60f, 80f);
        [SerializeField] private KeyCode toggleViewKey = KeyCode.V;

        [Header("Crouch")]
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float normalHeight = 2f;
        [SerializeField] private float crouchTransitionSpeed = 8f;

        private CharacterController controller;
        private ViewMode currentViewMode = ViewMode.FirstPerson;
        private bool inputEnabled = true;

        private float pitch;
        private float yaw;
        private float verticalSpeed;
        private Vector3 cameraVelocity;
        private float groundedTimer;

        [SerializeField] private float groundedGraceTime = 0.15f;

        // Jump animation event support
        private bool jumpRequested = false;
        private bool isJumping = false;

        // Crouch state
        private bool isCrouched = false;
        private float currentHeight;
        private float targetHeight;

        /// <summary>
        /// Exposes vertical movement speed for animation systems.
        /// </summary>
        public float VerticalSpeed => verticalSpeed;

        /// <summary>
        /// Exposes crouch state for animation systems.
        /// </summary>
        public bool IsCrouched => isCrouched;

        /// <summary>
        /// Exposes jump request state for animation systems.
        /// </summary>
        public bool JumpRequested => jumpRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            controller = GetComponent<CharacterController>();
            if (playerCamera == null)
                playerCamera = UnityEngine.Camera.main;

            if (playerCamera != null)
                playerCamera.transform.SetParent(null); // keep camera free for smooth follow

            // Initialize heights
            normalHeight = controller.height;
            currentHeight = normalHeight;
            targetHeight = normalHeight;
            crouchHeight = normalHeight * 0.5f;

            yaw = transform.eulerAngles.y;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += HandleDialogueStarted;
                DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
            }
        }

        private void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= HandleDialogueStarted;
                DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
            }
        }

        private void Update()
        {
            if (!inputEnabled)
                return;

            HandleViewToggle();
            HandleCrouch();
            HandleLook();
            HandleMovement();
            UpdateCrouchHeight();
            UpdateCamera();
        }

        private void HandleMovement()
        {
            // Track jump state - clear when grounded
            if (controller.isGrounded && isJumping)
            {
                isJumping = false;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            // Disable horizontal movement during jump
            Vector3 moveDirection;
            if (isJumping)
            {
                moveDirection = Vector3.zero; // No horizontal movement during jump
            }
            else
            {
                moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
            }

            // Determine speed based on crouch and sprint
            float targetSpeed = isCrouched ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
            Vector3 motion = moveDirection * targetSpeed;

            if (controller.isGrounded)
            {
                groundedTimer = groundedGraceTime;
                if (verticalSpeed < 0f)
                {
                    verticalSpeed = -2f; // small stick-to-ground
                }
            }
            else
            {
                groundedTimer -= Time.deltaTime;
            }

            // Request jump on button press (actual jump will be triggered by animation event)
            if (groundedTimer > 0f && Input.GetButtonDown("Jump") && !isCrouched && !isJumping)
            {
                jumpRequested = true;
                isJumping = true; // Set jumping state immediately
                groundedTimer = 0f;
            }

            verticalSpeed += gravity * Time.deltaTime;
            motion.y = verticalSpeed;

            controller.Move(motion * Time.deltaTime);
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch = Mathf.Clamp(pitch - mouseY, pitchLimits.x, pitchLimits.y);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void UpdateCamera()
        {
            if (playerCamera == null)
                return;

            // Skip camera updates if CameraFocusController is handling the camera
            if (CameraFocus.CameraFocusController.Instance != null && CameraFocus.CameraFocusController.Instance.IsFocusing)
                return;

            Transform targetAnchor = currentViewMode == ViewMode.FirstPerson ? firstPersonAnchor : thirdPersonAnchor;
            if (targetAnchor == null)
                return;

            Vector3 desiredPosition = targetAnchor.position;
            playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, desiredPosition, ref cameraVelocity, cameraSmoothTime);

            Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, desiredRotation, Time.deltaTime / cameraSmoothTime);
        }

        private void HandleViewToggle()
        {
            if (Input.GetKeyDown(toggleViewKey))
            {
                currentViewMode = currentViewMode == ViewMode.FirstPerson ? ViewMode.ThirdPerson : ViewMode.FirstPerson;
            }
        }

        private void HandleCrouch()
        {
            // Toggle crouch with Left Ctrl
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouched = !isCrouched;
                targetHeight = isCrouched ? crouchHeight : normalHeight;
            }
            
            // Exit crouch when pressing Shift (while crouched)
            if (isCrouched && Input.GetKey(KeyCode.LeftShift))
            {
                isCrouched = false;
                targetHeight = normalHeight;
            }
        }

        private void UpdateCrouchHeight()
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            float heightDifference = normalHeight - currentHeight;
            controller.height = currentHeight;
            controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (enabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleDialogueStarted(DialogueNode node)
        {
            SetInputEnabled(false);
        }

        private void HandleDialogueEnded()
        {
            SetInputEnabled(true);
        }

        /// <summary>
        /// Called from animation event at the specific frame when jump should occur.
        /// This allows precise control over when the jump force is applied.
        /// </summary>
        public void OnJumpAnimationEvent()
        {
            if (jumpRequested && controller.isGrounded)
            {
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpRequested = false;
            }
        }
    }
}

