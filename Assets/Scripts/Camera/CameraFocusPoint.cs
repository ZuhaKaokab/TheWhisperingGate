using UnityEngine;

namespace WhisperingGate.Camera
{
    /// <summary>
    /// Defines a camera position and view direction for dialogue sequences.
    /// The camera will move to this transform's position and adopt its rotation.
    /// Use the blue arrow gizmo to set the view direction.
    /// </summary>
    public class CameraFocusPoint : MonoBehaviour
    {
        [Header("Identification")]
        [Tooltip("Unique ID for this focus point. Used in dialogue commands like 'cam:tree'")]
        [SerializeField] private string pointId;

        [Header("Preview")]
        [Tooltip("Show camera frustum preview in editor")]
        [SerializeField] private bool showFrustumPreview = true;
        [SerializeField] private float previewDistance = 5f;
        [SerializeField] private float previewFOV = 60f;

        [Header("Gizmo Settings")]
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private float gizmoSize = 0.3f;

        /// <summary>
        /// The unique identifier for this focus point.
        /// </summary>
        public string PointId => pointId;

        private void OnValidate()
        {
            // Auto-generate ID from GameObject name if empty
            if (string.IsNullOrWhiteSpace(pointId))
            {
                pointId = gameObject.name
                    .Replace("FocusPoint_", "")
                    .Replace("CamPoint_", "")
                    .Replace(" ", "_")
                    .ToLower();
            }
        }

        private void OnDrawGizmos()
        {
            // Camera icon representation
            Gizmos.color = gizmoColor;
            
            // Draw camera body (box)
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(gizmoSize * 1.5f, gizmoSize, gizmoSize * 0.8f));
            
            // Draw lens (cylinder-like)
            Gizmos.DrawWireCube(new Vector3(0, 0, gizmoSize * 0.6f), new Vector3(gizmoSize * 0.6f, gizmoSize * 0.6f, gizmoSize * 0.4f));
            Gizmos.matrix = oldMatrix;

            // Draw view direction arrow
            Gizmos.color = Color.blue;
            Vector3 forward = transform.forward;
            Gizmos.DrawRay(transform.position, forward * (gizmoSize * 4f));
            
            // Arrow head
            Vector3 arrowTip = transform.position + forward * (gizmoSize * 4f);
            Vector3 right = transform.right * gizmoSize * 0.5f;
            Vector3 up = transform.up * gizmoSize * 0.5f;
            Gizmos.DrawLine(arrowTip, arrowTip - forward * gizmoSize + right);
            Gizmos.DrawLine(arrowTip, arrowTip - forward * gizmoSize - right);
            Gizmos.DrawLine(arrowTip, arrowTip - forward * gizmoSize + up);
            Gizmos.DrawLine(arrowTip, arrowTip - forward * gizmoSize - up);

            // Draw frustum preview
            if (showFrustumPreview)
            {
                DrawFrustumPreview();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Highlight when selected
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.5f);

#if UNITY_EDITOR
            // Draw label
            if (!string.IsNullOrWhiteSpace(pointId))
            {
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * gizmoSize * 2f,
                    $"cam:{pointId}",
                    new GUIStyle { 
                        normal = { textColor = Color.cyan },
                        fontStyle = FontStyle.Bold,
                        fontSize = 12
                    }
                );
            }
#endif
        }

        private void DrawFrustumPreview()
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            
            float halfFOV = previewFOV * 0.5f * Mathf.Deg2Rad;
            float aspect = 16f / 9f; // Assume 16:9
            
            float nearHeight = Mathf.Tan(halfFOV) * 0.1f;
            float nearWidth = nearHeight * aspect;
            float farHeight = Mathf.Tan(halfFOV) * previewDistance;
            float farWidth = farHeight * aspect;

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 up = transform.up;

            // Near plane corners
            Vector3 nearCenter = transform.position + forward * 0.1f;
            Vector3 n1 = nearCenter + up * nearHeight + right * nearWidth;
            Vector3 n2 = nearCenter + up * nearHeight - right * nearWidth;
            Vector3 n3 = nearCenter - up * nearHeight - right * nearWidth;
            Vector3 n4 = nearCenter - up * nearHeight + right * nearWidth;

            // Far plane corners
            Vector3 farCenter = transform.position + forward * previewDistance;
            Vector3 f1 = farCenter + up * farHeight + right * farWidth;
            Vector3 f2 = farCenter + up * farHeight - right * farWidth;
            Vector3 f3 = farCenter - up * farHeight - right * farWidth;
            Vector3 f4 = farCenter - up * farHeight + right * farWidth;

            // Draw frustum edges
            Gizmos.DrawLine(n1, f1);
            Gizmos.DrawLine(n2, f2);
            Gizmos.DrawLine(n3, f3);
            Gizmos.DrawLine(n4, f4);

            // Draw far plane
            Gizmos.DrawLine(f1, f2);
            Gizmos.DrawLine(f2, f3);
            Gizmos.DrawLine(f3, f4);
            Gizmos.DrawLine(f4, f1);
        }
    }
}
