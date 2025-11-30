using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WhisperingGate.Dialogue;
using WhisperingGate.Core;
using UnityEngine.UI;
using TMPro;

namespace WhisperingGate.UI
{
    /// <summary>
    /// UI component that displays dialogue nodes, handles typewriter effect, and manages choice buttons.
    /// Subscribes to DialogueManager events for event-driven communication.
    /// </summary>
    public class DialogueUIPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button skipButton;
        [SerializeField] private VerticalLayoutGroup choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        
        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;
        [SerializeField] private float autoAdvanceDelay = 2f;
        
        private List<Button> currentChoiceButtons = new();
        private Coroutine typewriterCoroutine;
        private bool isWaitingForInput = false;
        
        void Start()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed += DisplayNode;
                DialogueManager.Instance.OnDialogueEnded += HidePanel;
                DialogueManager.Instance.OnItemGiven += HandleItemGiven;
                DialogueManager.Instance.OnImpactApplied += HandleImpactApplied;
            }
            else
            {
                Debug.LogError("[DialogueUIPanel] DialogueManager.Instance is null. Make sure DialogueManager exists in scene.");
            }
            
            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTypewriter);
            
            HidePanel();
        }
        
        void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= DisplayNode;
                DialogueManager.Instance.OnDialogueEnded -= HidePanel;
                DialogueManager.Instance.OnItemGiven -= HandleItemGiven;
                DialogueManager.Instance.OnImpactApplied -= HandleImpactApplied;
            }
            
            if (skipButton != null)
                skipButton.onClick.RemoveListener(SkipTypewriter);
        }
        
        private void HandleItemGiven(string itemId)
        {
            if (impactNotificationUI != null)
            {
                // Get item name from InventoryManager if available
                string itemName = itemId;
                if (Gameplay.InventoryManager.Instance != null)
                {
                    var itemData = Gameplay.InventoryManager.Instance.GetItemData(itemId);
                    if (itemData != null && !string.IsNullOrEmpty(itemData.itemName))
                        itemName = itemData.itemName;
                }
                
                impactNotificationUI.ShowItemGained(itemName);
            }
        }
        
        private void DisplayNode(DialogueNode node)
        {
            if (node == null) return;
            
            StartCoroutine(FadePanel(true, 0.3f));
            
            // Update speaker info
            if (node.Speaker != null)
            {
                if (speakerNameText != null)
                    speakerNameText.text = node.Speaker.DisplayName;
                
                if (portraitImage != null && node.Speaker.PortraitSprite != null)
                    portraitImage.sprite = node.Speaker.PortraitSprite;
            }
            
            // Stop any existing typewriter and start new one
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            
            typewriterCoroutine = StartCoroutine(TypewriteDialogue(node));
        }
        
        private IEnumerator TypewriteDialogue(DialogueNode node)
        {
            isWaitingForInput = false;
            if (dialogueText != null)
                dialogueText.text = "";
            
            if (node == null || string.IsNullOrEmpty(node.LineText))
            {
                isWaitingForInput = true;
                ShowChoices(node);
                yield break;
            }
            
            // Typewriter effect
            foreach (char c in node.LineText)
            {
                if (dialogueText != null)
                    dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
            
            isWaitingForInput = true;
            ShowChoices(node);
        }
        
        private void SkipTypewriter()
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            
            if (DialogueManager.Instance != null && DialogueManager.Instance.CurrentNode != null)
            {
                if (dialogueText != null)
                    dialogueText.text = DialogueManager.Instance.CurrentNode.LineText;
            }
            
            isWaitingForInput = true;
            if (DialogueManager.Instance != null)
                ShowChoices(DialogueManager.Instance.CurrentNode);
        }
        
        private void ShowChoices(DialogueNode node)
        {
            // Clear existing choice buttons
            foreach (var btn in currentChoiceButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            currentChoiceButtons.Clear();
            
            if (node == null)
            {
                StartCoroutine(WaitThenAdvance(autoAdvanceDelay));
                return;
            }
            
            // Get visible choices from DialogueManager (already filtered by conditions)
            var visibleChoices = DialogueManager.Instance != null ? DialogueManager.Instance.GetVisibleChoices() : new List<DialogueChoice>();
            
            if (visibleChoices.Count == 0)
            {
                // No visible choices, check if node has NextNodeIfAuto or is an end node
                if (node != null && node.NextNodeIfAuto != null)
                {
                    // Has auto-advance node, wait then advance
                    Debug.Log($"[DialogueUIPanel] No visible choices, auto-advancing to next node after {autoAdvanceDelay} seconds");
                    StartCoroutine(WaitThenAdvance(autoAdvanceDelay));
                }
                else if (node != null && node.IsEndNode)
                {
                    // Is end node, dialogue will end automatically (DialogueManager handles this)
                    Debug.Log($"[DialogueUIPanel] Node is an end node with no choices. Dialogue will end automatically.");
                }
                else
                {
                    // No choices and no next node, end dialogue
                    Debug.Log($"[DialogueUIPanel] No visible choices and no next node. Ending dialogue.");
                    StartCoroutine(WaitThenAdvance(autoAdvanceDelay));
                }
                return;
            }
            
            // Create buttons for visible choices
            if (choiceButtonPrefab == null || choicesContainer == null)
            {
                Debug.LogWarning("[DialogueUIPanel] Choice button prefab or container not assigned");
                return;
            }
            
            for (int i = 0; i < visibleChoices.Count; i++)
            {
                var choice = visibleChoices[i];
                var btn = Instantiate(choiceButtonPrefab, choicesContainer.transform);
                
                // Set button text
                var textComponent = btn.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                    textComponent.text = choice.ChoiceText;
                
                // Set up click handler
                int buttonIndex = i;
                btn.onClick.AddListener(() => SelectChoice(buttonIndex));
                currentChoiceButtons.Add(btn);
            }
        }
        
        [Header("Impact Notifications")]
        [SerializeField] private ImpactNotificationUI impactNotificationUI;
        
        private void SelectChoice(int index)
        {
            if (DialogueManager.Instance == null) return;
            
            var visibleChoices = DialogueManager.Instance.GetVisibleChoices();
            if (index < 0 || index >= visibleChoices.Count)
            {
                Debug.LogWarning($"[DialogueUIPanel] Invalid choice index: {index}");
                return;
            }
            
            DialogueManager.Instance.SelectChoice(index);
        }
        
        private void HandleImpactApplied(string variableName, int change)
        {
            if (impactNotificationUI != null)
            {
                impactNotificationUI.ShowVariableChange(variableName, change);
            }
        }
        
        private IEnumerator WaitThenAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.AdvanceToNextNode();
        }
        
        private void HidePanel()
        {
            StartCoroutine(FadePanel(false, 0.3f));
        }
        
        private IEnumerator FadePanel(bool fadeIn, float duration)
        {
            if (panelCanvasGroup == null) yield break;
            
            float elapsed = 0f;
            float startAlpha = panelCanvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            
            panelCanvasGroup.alpha = targetAlpha;
        }
    }
}

