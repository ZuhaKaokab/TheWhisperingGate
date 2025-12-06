using UnityEngine;
using System.Collections;
using WhisperingGate.Core;

namespace WhisperingGate.Interaction
{
    /// <summary>
    /// A door that can be opened/closed via commands, puzzles, or direct calls.
    /// Supports animation, rotation, or sliding open methods.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique ID for command system (door:open:this_id)")]
        [SerializeField] private string doorId = "door_1";

        [Header("Open Method")]
        [SerializeField] private DoorOpenMethod openMethod = DoorOpenMethod.Rotate;
        
        [Header("Rotation Settings (if Rotate)")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private Transform pivotPoint;

        [Header("Slide Settings (if Slide)")]
        [SerializeField] private Vector3 slideDirection = Vector3.up;
        [SerializeField] private float slideDistance = 3f;

        [Header("Animation Settings (if Animate)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";
        [SerializeField] private string closeTrigger = "Close";

        [Header("Timing")]
        [SerializeField] private float openDuration = 1f;
        [SerializeField] private float openDelay = 0f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("State")]
        [SerializeField] private bool startsOpen = false;
        [SerializeField] private bool canClose = true;
        [SerializeField] private bool isLocked = false;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;

        [Header("Events")]
        [Tooltip("GameState flag to set when opened")]
        [SerializeField] private string onOpenFlag = "";
        
        [Tooltip("Commands to execute when opened")]
        [SerializeField] private string[] onOpenCommands;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime state
        private bool isOpen = false;
        private bool isAnimating = false;
        private Vector3 closedPosition;
        private Quaternion closedRotation;
        private Vector3 openPosition;
        private Quaternion openRotation;

        // Static registry for command access
        private static System.Collections.Generic.Dictionary<string, Door> doorRegistry = 
            new System.Collections.Generic.Dictionary<string, Door>();

        public string DoorId => doorId;
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;
        public bool IsAnimating => isAnimating;

        private void Awake()
        {
            // Register this door
            if (!string.IsNullOrWhiteSpace(doorId))
            {
                doorRegistry[doorId] = this;
            }

            // Store initial state
            Transform target = pivotPoint != null ? pivotPoint : transform;
            closedPosition = target.localPosition;
            closedRotation = target.localRotation;

            // Calculate open state
            switch (openMethod)
            {
                case DoorOpenMethod.Rotate:
                    openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
                    openPosition = closedPosition;
                    break;
                case DoorOpenMethod.Slide:
                    openPosition = closedPosition + slideDirection.normalized * slideDistance;
                    openRotation = closedRotation;
                    break;
                default:
                    openPosition = closedPosition;
                    openRotation = closedRotation;
                    break;
            }

            // Set initial state
            if (startsOpen)
            {
                isOpen = true;
                ApplyState(true, immediate: true);
            }
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrWhiteSpace(doorId) && doorRegistry.ContainsKey(doorId))
            {
                doorRegistry.Remove(doorId);
            }
        }

        /// <summary>
        /// Get a door by its ID.
        /// </summary>
        public static Door GetDoor(string id)
        {
            if (doorRegistry.TryGetValue(id, out Door door))
                return door;
            return null;
        }

        /// <summary>
        /// Open the door.
        /// </summary>
        public void Open()
        {
            if (isOpen || isAnimating) return;

            if (isLocked)
            {
                PlaySound(lockedSound);
                if (enableDebugLogs) Debug.Log($"[Door] '{doorId}' is locked");
                return;
            }

            if (enableDebugLogs) Debug.Log($"[Door] Opening '{doorId}'");
            StartCoroutine(AnimateDoor(true));
        }

        /// <summary>
        /// Close the door.
        /// </summary>
        public void Close()
        {
            if (!isOpen || isAnimating || !canClose) return;

            if (enableDebugLogs) Debug.Log($"[Door] Closing '{doorId}'");
            StartCoroutine(AnimateDoor(false));
        }

        /// <summary>
        /// Toggle door state.
        /// </summary>
        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Lock/unlock the door.
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            if (enableDebugLogs) Debug.Log($"[Door] '{doorId}' locked: {locked}");
        }

        private IEnumerator AnimateDoor(bool opening)
        {
            isAnimating = true;

            if (openDelay > 0)
                yield return new WaitForSeconds(openDelay);

            // Play sound
            PlaySound(opening ? openSound : closeSound);

            // Handle animation method
            if (openMethod == DoorOpenMethod.Animate && animator != null)
            {
                animator.SetTrigger(opening ? openTrigger : closeTrigger);
                yield return new WaitForSeconds(openDuration);
            }
            else
            {
                // Lerp position/rotation
                Transform target = pivotPoint != null ? pivotPoint : transform;
                Vector3 startPos = target.localPosition;
                Quaternion startRot = target.localRotation;
                Vector3 endPos = opening ? openPosition : closedPosition;
                Quaternion endRot = opening ? openRotation : closedRotation;

                float elapsed = 0f;
                while (elapsed < openDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = openCurve.Evaluate(elapsed / openDuration);
                    
                    target.localPosition = Vector3.Lerp(startPos, endPos, t);
                    target.localRotation = Quaternion.Slerp(startRot, endRot, t);
                    
                    yield return null;
                }

                target.localPosition = endPos;
                target.localRotation = endRot;
            }

            isOpen = opening;
            isAnimating = false;

            // Set flag and execute commands on open
            if (opening)
            {
                if (!string.IsNullOrWhiteSpace(onOpenFlag) && GameState.Instance != null)
                {
                    GameState.Instance.SetBool(onOpenFlag, true);
                }

                ExecuteCommands(onOpenCommands);
            }

            if (enableDebugLogs) Debug.Log($"[Door] '{doorId}' is now {(isOpen ? "open" : "closed")}");
        }

        private void ApplyState(bool open, bool immediate)
        {
            Transform target = pivotPoint != null ? pivotPoint : transform;
            target.localPosition = open ? openPosition : closedPosition;
            target.localRotation = open ? openRotation : closedRotation;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        private void ExecuteCommands(string[] commands)
        {
            if (commands == null) return;

            foreach (string cmd in commands)
            {
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                // Commands would be executed via a command system
                // For now, just log them
                if (enableDebugLogs) Debug.Log($"[Door] Execute command: {cmd}");
            }
        }

        /// <summary>
        /// Execute a door command from the command system.
        /// Format: door:action:door_id
        /// Actions: open, close, toggle, lock, unlock
        /// </summary>
        public static void ExecuteCommand(string action, string targetId)
        {
            Door door = GetDoor(targetId);
            if (door == null)
            {
                Debug.LogWarning($"[Door] Door not found: {targetId}");
                return;
            }

            switch (action.ToLower())
            {
                case "open":
                    door.Open();
                    break;
                case "close":
                    door.Close();
                    break;
                case "toggle":
                    door.Toggle();
                    break;
                case "lock":
                    door.SetLocked(true);
                    break;
                case "unlock":
                    door.SetLocked(false);
                    break;
                default:
                    Debug.LogWarning($"[Door] Unknown action: {action}");
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform target = pivotPoint != null ? pivotPoint : transform;
            
            // Draw closed position
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(target.position, Vector3.one * 0.2f);
            
            // Draw open position preview
            Gizmos.color = Color.green;
            if (openMethod == DoorOpenMethod.Slide)
            {
                Vector3 openPos = target.position + slideDirection.normalized * slideDistance;
                Gizmos.DrawWireCube(openPos, Vector3.one * 0.2f);
                Gizmos.DrawLine(target.position, openPos);
            }
            else if (openMethod == DoorOpenMethod.Rotate)
            {
                // Draw rotation arc
                Gizmos.DrawRay(target.position, target.TransformDirection(rotationAxis) * 0.5f);
            }
        }
#endif
    }

    public enum DoorOpenMethod
    {
        Rotate,     // Rotates around pivot (standard door)
        Slide,      // Slides in a direction (sliding door, gate)
        Animate     // Uses Animator component
    }
}

