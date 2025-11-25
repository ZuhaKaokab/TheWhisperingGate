using UnityEngine;
using WhisperingGate.Dialogue;

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
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -25f;

        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform firstPersonAnchor;
        [SerializeField] private Transform thirdPersonAnchor;
        [SerializeField] private float cameraSmoothTime = 0.08f;
        [SerializeField] private float mouseSensitivity = 150f;
        [SerializeField] private Vector2 pitchLimits = new(-60f, 80f);
        [SerializeField] private KeyCode toggleViewKey = KeyCode.V;

        private CharacterController controller;
        private ViewMode currentViewMode = ViewMode.FirstPerson;
        private bool inputEnabled = true;

        private float pitch;
        private float yaw;
        private float verticalSpeed;
        private Vector3 cameraVelocity;
        private float groundedTimer;

        [SerializeField] private float groundedGraceTime = 0.15f;

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
                playerCamera = Camera.main;

            if (playerCamera != null)
                playerCamera.transform.SetParent(null); // keep camera free for smooth follow

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
            HandleLook();
            HandleMovement();
            UpdateCamera();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;

            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
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

            if (groundedTimer > 0f && Input.GetButtonDown("Jump"))
            {
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
    }
}

