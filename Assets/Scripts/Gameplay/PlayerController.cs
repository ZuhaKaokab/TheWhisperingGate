using UnityEngine;
using WhisperingGate.Dialogue;
using CameraFocus = WhisperingGate.Camera;
using System.Collections.Generic; // Added for List/Collections

namespace WhisperingGate.Gameplay
{
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

        // --- FOOTSTEPS SECTION START ---
        [Header("Footsteps")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float footstepIntervalWalk = 0.5f;
        [SerializeField] private float footstepIntervalSprint = 0.3f;
        [SerializeField] private float footstepIntervalCrouch = 0.7f;
        private float footstepTimer;
        // --- FOOTSTEPS SECTION END ---

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

        public ViewMode CurrentViewMode => currentViewMode;

        private float pitch;
        private float yaw;
        private float verticalSpeed;
        private Vector3 cameraVelocity;
        private float groundedTimer;

        [SerializeField] private float groundedGraceTime = 0.15f;

        private bool jumpRequested = false;
        private bool isJumping = false;

        private bool isCrouched = false;
        private float currentHeight;
        private float targetHeight;

        public float VerticalSpeed => verticalSpeed;
        public bool IsCrouched => isCrouched;
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
                playerCamera.transform.SetParent(null);

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
            HandleFootsteps(); // Footsteps update call
        }

        private void HandleMovement()
        {
            if (controller.isGrounded && isJumping)
            {
                isJumping = false;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection;
            if (isJumping)
            {
                moveDirection = Vector3.zero;
            }
            else
            {
                moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
            }

            float targetSpeed = isCrouched ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
            Vector3 motion = moveDirection * targetSpeed;

            if (controller.isGrounded)
            {
                groundedTimer = groundedGraceTime;
                if (verticalSpeed < 0f)
                {
                    verticalSpeed = -2f;
                }
            }
            else
            {
                groundedTimer -= Time.deltaTime;
            }

            if (groundedTimer > 0f && Input.GetButtonDown("Jump") && !isCrouched && !isJumping)
            {
                jumpRequested = true;
                isJumping = true;
                groundedTimer = 0f;
            }

            verticalSpeed += gravity * Time.deltaTime;
            motion.y = verticalSpeed;

            controller.Move(motion * Time.deltaTime);
        }

        // --- FOOTSTEPS LOGIC ---
        private void HandleFootsteps()
        {
            // Sirf tab awaz aaye jab player zameen par ho aur move kar raha ho
            if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
            {
                footstepTimer -= Time.deltaTime;

                if (footstepTimer <= 0)
                {
                    // Random clip select karein taake repetition na lage
                    if (footstepClips.Length > 0 && footstepAudioSource != null)
                    {
                        int n = Random.Range(0, footstepClips.Length);
                        footstepAudioSource.clip = footstepClips[n];
                        footstepAudioSource.PlayOneShot(footstepAudioSource.clip);

                        // Interval set karein based on speed
                        float currentInterval = walkSpeed;
                        if (isCrouched) currentInterval = footstepIntervalCrouch;
                        else if (Input.GetKey(KeyCode.LeftShift)) currentInterval = footstepIntervalSprint;
                        else currentInterval = footstepIntervalWalk;

                        footstepTimer = currentInterval;
                    }
                }
            }
            else
            {
                // Agar ruk jaye toh timer reset kardein taake agli baar foran awaz aaye
                footstepTimer = 0;
            }
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

            if (CameraFocus.CameraFocusController.Instance != null &&
                (CameraFocus.CameraFocusController.Instance.IsFocusing || CameraFocus.CameraFocusController.Instance.IsReturning))
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
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouched = !isCrouched;
                targetHeight = isCrouched ? crouchHeight : normalHeight;
            }

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