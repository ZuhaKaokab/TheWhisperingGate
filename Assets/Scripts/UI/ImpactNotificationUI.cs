using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhisperingGate.UI
{
    /// <summary>
    /// Displays temporary notifications when dialogue choices have impacts (like "He will remember that").
    /// Shows variable changes, item gains, and flag changes in a stylish notification panel.
    /// </summary>
    public class ImpactNotificationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private GameObject notificationPrefab;
        
        [Header("Settings")]
        [SerializeField] private float notificationDuration = 3f;
        [SerializeField] private float notificationSpacing = 10f;
        [SerializeField] private int maxNotifications = 5;
        
        private Queue<GameObject> activeNotifications = new Queue<GameObject>();
        private Dictionary<string, string> impactMessages = new Dictionary<string, string>
        {
            // Variable messages
            { "courage", "Courage" },
            { "trust_alina", "Alina's Trust" },
            { "trust_writer", "Writer's Trust" },
            { "sanity", "Sanity" },
            { "investigation_level", "Investigation" },
            
            // Flag messages (will be handled separately)
        };
        
        void Start()
        {
            if (notificationContainer == null)
                notificationContainer = transform;
        }
        
        /// <summary>
        /// Shows a notification for a variable change.
        /// </summary>
        public void ShowVariableChange(string variableName, int change)
        {
            if (change == 0) return;
            
            string displayName = impactMessages.ContainsKey(variableName) 
                ? impactMessages[variableName] 
                : variableName;
            
            string message = change > 0 
                ? $"{displayName} +{change}" 
                : $"{displayName} {change}";
            
            Color color = GetVariableColor(variableName);
            ShowNotification(message, color);
        }
        
        /// <summary>
        /// Shows a notification for a flag change.
        /// </summary>
        public void ShowFlagChange(string flagName, bool value)
        {
            string message = value 
                ? $"Flag set: {flagName}" 
                : $"Flag cleared: {flagName}";
            
            ShowNotification(message, Color.cyan);
        }
        
        /// <summary>
        /// Shows a notification for an item gain.
        /// </summary>
        public void ShowItemGained(string itemName)
        {
            ShowNotification($"Item gained: {itemName}", Color.yellow);
        }
        
        /// <summary>
        /// Shows a custom notification (e.g., "He will remember that").
        /// </summary>
        public void ShowCustomNotification(string message, Color? color = null)
        {
            ShowNotification(message, color ?? Color.white);
        }
        
        private void ShowNotification(string message, Color color)
        {
            if (notificationPrefab == null || notificationContainer == null)
            {
                Debug.LogWarning("[ImpactNotificationUI] Prefab or container not assigned");
                return;
            }
            
            // Limit number of notifications
            if (activeNotifications.Count >= maxNotifications)
            {
                var oldest = activeNotifications.Dequeue();
                if (oldest != null)
                    Destroy(oldest);
            }
            
            // Create notification
            GameObject notification = Instantiate(notificationPrefab, notificationContainer);
            notification.SetActive(true);
            
            // Set text
            var textComponent = notification.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = color;
            }
            
            // Set background color (if Image component exists)
            var image = notification.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(color.r, color.g, color.b, 0.2f);
            }
            
            // Position notification
            RectTransform rect = notification.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, -activeNotifications.Count * (rect.sizeDelta.y + notificationSpacing));
            }
            
            activeNotifications.Enqueue(notification);
            
            // Animate in
            StartCoroutine(AnimateNotification(notification));
        }
        
        private IEnumerator AnimateNotification(GameObject notification)
        {
            if (notification == null) yield break;
            
            RectTransform rect = notification.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = notification.AddComponent<CanvasGroup>();
            
            // Fade in
            float elapsed = 0f;
            float fadeInTime = 0.3f;
            
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInTime);
                if (rect != null)
                    rect.anchoredPosition = new Vector2(
                        Mathf.Lerp(-200f, 0f, elapsed / fadeInTime),
                        rect.anchoredPosition.y
                    );
                yield return null;
            }
            
            // Wait
            yield return new WaitForSeconds(notificationDuration);
            
            // Fade out
            elapsed = 0f;
            float fadeOutTime = 0.5f;
            Vector2 startPos = rect != null ? rect.anchoredPosition : Vector2.zero;
            
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutTime);
                if (rect != null)
                    rect.anchoredPosition = new Vector2(
                        Mathf.Lerp(startPos.x, 200f, elapsed / fadeOutTime),
                        startPos.y
                    );
                yield return null;
            }
            
            // Remove from queue and destroy
            if (activeNotifications.Contains(notification))
            {
                var tempQueue = new Queue<GameObject>();
                foreach (var notif in activeNotifications)
                {
                    if (notif != notification)
                        tempQueue.Enqueue(notif);
                }
                activeNotifications = tempQueue;
                
                // Reposition remaining notifications
                RepositionNotifications();
            }
            
            if (notification != null)
                Destroy(notification);
        }
        
        private void RepositionNotifications()
        {
            int index = 0;
            foreach (var notif in activeNotifications)
            {
                if (notif == null) continue;
                RectTransform rect = notif.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(0, -index * (rect.sizeDelta.y + notificationSpacing));
                }
                index++;
            }
        }
        
        private Color GetVariableColor(string variableName)
        {
            switch (variableName.ToLower())
            {
                case "courage":
                    return new Color(1f, 0.8f, 0f); // Gold
                case "trust_alina":
                case "trust_writer":
                    return new Color(0.2f, 0.8f, 1f); // Light blue
                case "sanity":
                    return new Color(0.8f, 0.2f, 0.2f); // Red
                case "investigation_level":
                    return new Color(0.6f, 0.4f, 1f); // Purple
                default:
                    return Color.white;
            }
        }
    }
}







