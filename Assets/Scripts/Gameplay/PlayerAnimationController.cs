using UnityEngine;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Drives the Animator parameters for the player character based on PlayerController movement.
    /// Supports simple Idle/Walk/Run/Jump using existing hybrid FP/TP controller.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;

        [Header("Parameters")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string isGroundedParam = "IsGrounded";
        [SerializeField] private string jumpTriggerParam = "Jump";
        [SerializeField] private string isCrouchedParam = "IsCrouched";
        [SerializeField] private string isCrouchMovingParam = "IsCrouchMoving";

        [Header("Tuning")]
        [SerializeField] private float runSpeedForNormalization = 7f; // should match sprintSpeed
        [SerializeField] private float minMoveThreshold = 0.05f;

        private Vector3 lastPosition;
        private bool wasGrounded = true;
        private bool lastJumpRequested = false;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            lastPosition = transform.position;
        }

        private void Update()
        {
            if (animator == null || playerController == null || characterController == null)
                return;

            UpdateAnimatorParameters();
        }

        private void UpdateAnimatorParameters()
        {
            // World-space horizontal speed
            Vector3 currentPos = transform.position;
            Vector3 displacement = currentPos - lastPosition;
            displacement.y = 0f; // ignore vertical for speed
            float speed = displacement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPosition = currentPos;

            float normalizedSpeed = runSpeedForNormalization > 0f
                ? Mathf.Clamp01(speed / runSpeedForNormalization)
                : 0f;

            bool isGrounded = characterController.isGrounded;
            bool isCrouched = playerController.IsCrouched;
            bool isCrouchMoving = isCrouched && normalizedSpeed > minMoveThreshold;
            bool jumpRequested = playerController.JumpRequested;

            // Jump trigger: fire when jump is requested (on button press)
            if (jumpRequested && !lastJumpRequested && isGrounded)
            {
                if (!string.IsNullOrEmpty(jumpTriggerParam))
                    animator.SetTrigger(jumpTriggerParam);
            }
            lastJumpRequested = jumpRequested;
            wasGrounded = isGrounded;

            // Push into Animator
            if (!string.IsNullOrEmpty(speedParam))
                animator.SetFloat(speedParam, normalizedSpeed < minMoveThreshold ? 0f : normalizedSpeed);

            if (!string.IsNullOrEmpty(isGroundedParam))
                animator.SetBool(isGroundedParam, isGrounded);

            if (!string.IsNullOrEmpty(isCrouchedParam))
                animator.SetBool(isCrouchedParam, isCrouched);

            if (!string.IsNullOrEmpty(isCrouchMovingParam))
                animator.SetBool(isCrouchMovingParam, isCrouchMoving);
        }
    }
}


