using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Camera
{
    /// <summary>
    /// Controls camera movement during dialogue sequences.
    /// Moves the camera to predefined positions with specific view directions.
    /// The camera physically moves to the focus point and uses its rotation.
    /// </summary>
    public class CameraFocusController : MonoBehaviour
    {
        public static CameraFocusController Instance { get; private set; }

        [Header("Transition Settings")]
        [Tooltip("How fast camera moves to focus point position")]
        [SerializeField] private float positionTransitionSpeed = 5f;
        [Tooltip("How fast camera rotates to focus point direction")]
        [SerializeField] private float rotationTransitionSpeed = 5f;
        [Tooltip("How fast camera returns to player")]
        [SerializeField] private float returnSpeed = 8f;

        [Header("Auto Return Settings")]
        [Tooltip("Default duration to hold focus before auto-returning (0 = no auto-return)")]
        [SerializeField] private float defaultHoldDuration = 0f;

        [Header("Player Look During Focus")]
        [Tooltip("Allow player to look around slightly while at focus point")]
        [SerializeField] private bool allowPlayerLook = true;
        [SerializeField] private float allowedPitchRange = 15f;
        [SerializeField] private float allowedYawRange = 20f;
        [SerializeField] private float playerLookSensitivity = 50f;
        [SerializeField] private float lookReturnSpeed = 3f;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera targetCamera;

        // Focus state
        private bool isFocusing = false;
        private bool isReturning = false;
        private Transform currentFocusTarget;
        private Coroutine autoReturnCoroutine;
        
        // Player look offsets
        private float pitchOffset = 0f;
        private float yawOffset = 0f;

        // Cached focus points
        private Dictionary<string, Transform> focusPoints = new Dictionary<string, Transform>();

        /// <summary>
        /// Returns true if camera is currently at a focus point or transitioning.
        /// </summary>
        public bool IsFocusing => isFocusing;

        /// <summary>
        /// Returns true if camera is returning to player control.
        /// </summary>
        public bool IsReturning => isReturning;

        /// <summary>
        /// Returns the current focus target transform.
        /// </summary>
        public Transform CurrentFocusTarget => currentFocusTarget;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (targetCamera == null)
                targetCamera = UnityEngine.Camera.main;
        }

        private void Start()
        {
            RefreshFocusPoints();

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
            }
        }

        private void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                return;

            if (isFocusing && currentFocusTarget != null)
            {
                UpdateFocusCamera();
            }
        }

        /// <summary>
        /// Scans the scene for all CameraFocusPoint components and caches them.
        /// </summary>
        public void RefreshFocusPoints()
        {
            focusPoints.Clear();
            
            var points = FindObjectsOfType<CameraFocusPoint>();
            foreach (var point in points)
            {
                if (!string.IsNullOrWhiteSpace(point.PointId))
                {
                    string key = point.PointId.ToLower().Trim();
                    if (!focusPoints.ContainsKey(key))
                    {
                        focusPoints[key] = point.transform;
                        Debug.Log($"[CameraFocus] Registered focus point: {key}");
                    }
                    else
                    {
                        Debug.LogWarning($"[CameraFocus] Duplicate focus point ID: {key}");
                    }
                }
            }

            Debug.Log($"[CameraFocus] Total focus points registered: {focusPoints.Count}");
        }

        /// <summary>
        /// Move camera to a named focus point. Camera adopts the point's position and rotation.
        /// </summary>
        /// <param name="pointId">The ID of the focus point</param>
        /// <param name="holdDuration">Duration to hold before auto-return. 0 or negative = no auto-return</param>
        public void FocusOn(string pointId, float holdDuration = -1f)
        {
            if (string.IsNullOrWhiteSpace(pointId))
            {
                Debug.LogWarning("[CameraFocus] FocusOn called with empty pointId");
                return;
            }

            string key = pointId.ToLower().Trim();

            if (!focusPoints.TryGetValue(key, out Transform target))
            {
                Debug.LogWarning($"[CameraFocus] Focus point not found: {pointId}");
                return;
            }

            // Use default duration if not specified
            float duration = holdDuration < 0 ? defaultHoldDuration : holdDuration;
            StartFocus(target, duration);
            Debug.Log($"[CameraFocus] Moving camera to: {pointId}" + (duration > 0 ? $" (auto-return in {duration}s)" : ""));
        }

        /// <summary>
        /// Move camera to a specific transform directly.
        /// </summary>
        /// <param name="target">The transform to focus on</param>
        /// <param name="holdDuration">Duration to hold before auto-return. 0 or negative = no auto-return</param>
        public void FocusOn(Transform target, float holdDuration = -1f)
        {
            if (target == null)
            {
                Debug.LogWarning("[CameraFocus] FocusOn called with null target");
                return;
            }

            // Use default duration if not specified
            float duration = holdDuration < 0 ? defaultHoldDuration : holdDuration;
            StartFocus(target, duration);
            Debug.Log($"[CameraFocus] Moving camera to: {target.name}" + (duration > 0 ? $" (auto-return in {duration}s)" : ""));
        }

        private void StartFocus(Transform target, float holdDuration = 0f)
        {
            // Cancel any existing auto-return
            if (autoReturnCoroutine != null)
            {
                StopCoroutine(autoReturnCoroutine);
                autoReturnCoroutine = null;
            }

            currentFocusTarget = target;
            isFocusing = true;
            isReturning = false;
            pitchOffset = 0f;
            yawOffset = 0f;

            // Start auto-return if duration is positive
            if (holdDuration > 0f)
            {
                autoReturnCoroutine = StartCoroutine(AutoReturnAfterDelay(holdDuration));
            }
        }

        /// <summary>
        /// Coroutine that waits for the specified duration then releases focus.
        /// </summary>
        private IEnumerator AutoReturnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            Debug.Log($"[CameraFocus] Auto-returning after {delay}s hold");
            ReleaseFocus();
            autoReturnCoroutine = null;
        }

        /// <summary>
        /// Release focus and return camera control to player.
        /// PlayerController will resume handling camera position/rotation.
        /// </summary>
        public void ReleaseFocus()
        {
            if (!isFocusing) return;

            // Cancel any pending auto-return
            if (autoReturnCoroutine != null)
            {
                StopCoroutine(autoReturnCoroutine);
                autoReturnCoroutine = null;
            }

            isFocusing = false;
            isReturning = true;
            currentFocusTarget = null;
            pitchOffset = 0f;
            yawOffset = 0f;

            // isReturning will be cleared when PlayerController takes over
            // We just need to stop controlling the camera
            Debug.Log("[CameraFocus] Focus released, returning to player control");
            
            // Clear returning flag after a short delay (PlayerController will take over)
            Invoke(nameof(ClearReturning), 0.5f);
        }

        private void ClearReturning()
        {
            isReturning = false;
        }

        private void UpdateFocusCamera()
        {
            // Target position and rotation from focus point
            Vector3 targetPosition = currentFocusTarget.position;
            Quaternion baseRotation = currentFocusTarget.rotation;

            // Apply player look offset if allowed
            if (allowPlayerLook)
            {
                HandlePlayerLookInput();
                
                Quaternion pitchRot = Quaternion.AngleAxis(pitchOffset, Vector3.right);
                Quaternion yawRot = Quaternion.AngleAxis(yawOffset, Vector3.up);
                baseRotation = yawRot * baseRotation * pitchRot;
            }

            // Smoothly move camera to focus point position
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                targetPosition,
                positionTransitionSpeed * Time.deltaTime
            );

            // Smoothly rotate camera to focus point direction
            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                baseRotation,
                rotationTransitionSpeed * Time.deltaTime
            );
        }

        private void HandlePlayerLookInput()
        {
            float mouseX = Input.GetAxis("Mouse X") * playerLookSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * playerLookSensitivity * Time.deltaTime;

            yawOffset = Mathf.Clamp(yawOffset + mouseX, -allowedYawRange, allowedYawRange);
            pitchOffset = Mathf.Clamp(pitchOffset - mouseY, -allowedPitchRange, allowedPitchRange);

            // Gradually return to center
            if (Mathf.Abs(mouseX) < 0.01f)
                yawOffset = Mathf.Lerp(yawOffset, 0f, lookReturnSpeed * Time.deltaTime);
            if (Mathf.Abs(mouseY) < 0.01f)
                pitchOffset = Mathf.Lerp(pitchOffset, 0f, lookReturnSpeed * Time.deltaTime);
        }

        private void OnDialogueEnded()
        {
            ReleaseFocus();
        }

        /// <summary>
        /// Check if a focus point exists by ID.
        /// </summary>
        public bool HasFocusPoint(string pointId)
        {
            if (string.IsNullOrWhiteSpace(pointId)) return false;
            return focusPoints.ContainsKey(pointId.ToLower().Trim());
        }

        /// <summary>
        /// Get all registered focus point IDs.
        /// </summary>
        public string[] GetAllFocusPointIds()
        {
            var ids = new string[focusPoints.Count];
            focusPoints.Keys.CopyTo(ids, 0);
            return ids;
        }
    }
}
