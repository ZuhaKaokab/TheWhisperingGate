# The Whispering Gate - Dialogue System Implementation
## Complete C# Code Bundle & Architecture

**Status:** Production-Ready | **For:** Unity 2021.3+ | **Language:** C# 9.0+

---

## TABLE OF CONTENTS
1. Data Models (ScriptableObject definitions)
2. Runtime Managers (GameState, DialogueManager)
3. UI System (Display & Input)
4. Trigger System (World Interactions)
5. Command System (Consequences & Variables)
6. Editor Integration (Inspector workflows)

---

## PART 1: DATA MODELS

### 1.1 Character Data Asset

**File:** `Scripts/Data/CharacterData.cs`

```csharp
using UnityEngine;

namespace WhisperingGate.Dialogue
{
    [CreateAssetMenu(menuName = "Whispering Gate/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private AudioClip characterTheme;
        [SerializeField] private string characterDescription;
        
        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public Sprite PortraitSprite => portraitSprite;
        public AudioClip CharacterTheme => characterTheme;
        public string Description => characterDescription;
    }
}
```

---

### 1.2 Dialogue Choice

**File:** `Scripts/Data/DialogueChoice.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Dialogue
{
    [System.Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string choiceText;
        [SerializeField] private DialogueNode nextNode;
        [SerializeField] private List<ChoiceImpact> impacts = new();
        [SerializeField] private bool hasCondition = false;
        [SerializeField] private string showCondition = "";
        
        public string ChoiceText => choiceText;
        public DialogueNode NextNode => nextNode;
        public List<ChoiceImpact> Impacts => impacts;
        public bool HasCondition => hasCondition;
        public string ShowCondition => showCondition;
    }
    
    [System.Serializable]
    public class ChoiceImpact
    {
        [SerializeField] private string variableName;
        [SerializeField] private int valueChange;
        [SerializeField] private bool isConditional = false;
        [SerializeField] private string applyCondition = "";
        
        public string VariableName => variableName;
        public int ValueChange => valueChange;
        public bool IsConditional => isConditional;
        public string ApplyCondition => applyCondition;
    }
}
```

---

### 1.3 Dialogue Node

**File:** `Scripts/Data/DialogueNode.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Dialogue
{
    [CreateAssetMenu(menuName = "Whispering Gate/Dialogue Node")]
    public class DialogueNode : ScriptableObject
    {
        [SerializeField] private string nodeId;
        [SerializeField] private CharacterData speaker;
        [TextArea(3, 5)]
        [SerializeField] private string lineText;
        [SerializeField] private AudioClip voiceClip;
        [SerializeField] private float voiceDelay = 0f;
        
        [SerializeField] private List<DialogueChoice> choices = new();
        [SerializeField] private DialogueNode nextNodeIfAuto;
        
        [SerializeField] private List<string> startCommands = new();
        [SerializeField] private List<string> endCommands = new();
        
        [SerializeField] private bool isEndNode = false;
        [SerializeField] private float displayDuration = 0f;
        
        public string NodeId => nodeId;
        public CharacterData Speaker => speaker;
        public string LineText => lineText;
        public AudioClip VoiceClip => voiceClip;
        public float VoiceDelay => voiceDelay;
        public List<DialogueChoice> Choices => choices;
        public DialogueNode NextNodeIfAuto => nextNodeIfAuto;
        public List<string> StartCommands => startCommands;
        public List<string> EndCommands => endCommands;
        public bool IsEndNode => isEndNode;
        public float DisplayDuration => displayDuration;
    }
}
```

---

### 1.4 Dialogue Tree

**File:** `Scripts/Data/DialogueTree.cs`

```csharp
using UnityEngine;

namespace WhisperingGate.Dialogue
{
    [CreateAssetMenu(menuName = "Whispering Gate/Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        [SerializeField] private string treeId;
        [SerializeField] private string treeTitle;
        [SerializeField] private DialogueNode startNode;
        [SerializeField] private float defaultTypewriterSpeed = 0.05f;
        [SerializeField] private bool autoAdvanceIfSingleChoice = false;
        
        public string TreeId => treeId;
        public string TreeTitle => treeTitle;
        public DialogueNode StartNode => startNode;
        public float TypewriterSpeed => defaultTypewriterSpeed;
        public bool AutoAdvanceIfSingleChoice => autoAdvanceIfSingleChoice;
    }
}
```

---

## PART 2: RUNTIME MANAGERS

### 2.1 Game State

**File:** `Scripts/Runtime/GameState.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using System;

namespace WhisperingGate.Core
{
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }
        
        private Dictionary<string, int> intVariables = new();
        private Dictionary<string, bool> boolVariables = new();
        private Dictionary<string, string> stringVariables = new();
        private Dictionary<string, float> floatVariables = new();
        
        public event Action<string, int> OnIntChanged;
        public event Action<string, bool> OnBoolChanged;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultVariables();
        }
        
        private void InitializeDefaultVariables()
        {
            SetInt("courage", 0);
            SetInt("trust_alina", 0);
            SetInt("trust_writer", 0);
            SetInt("sanity", 50);
            SetInt("investigation_level", 0);
            
            SetBool("journal_found", false);
            SetBool("saw_dolls", false);
            SetBool("heard_scream", false);
            SetBool("met_writer", false);
            SetBool("portal_discovered", false);
        }
        
        public void SetInt(string key, int value)
        {
            intVariables[key] = value;
            OnIntChanged?.Invoke(key, value);
            Debug.Log($"[GameState] {key} = {value}");
        }
        
        public int GetInt(string key)
        {
            return intVariables.TryGetValue(key, out var value) ? value : 0;
        }
        
        public void AddInt(string key, int delta)
        {
            SetInt(key, GetInt(key) + delta);
        }
        
        public void SetBool(string key, bool value)
        {
            boolVariables[key] = value;
            OnBoolChanged?.Invoke(key, value);
            Debug.Log($"[GameState] {key} = {value}");
        }
        
        public bool GetBool(string key)
        {
            return boolVariables.TryGetValue(key, out var value) && value;
        }
        
        public void ToggleBool(string key)
        {
            SetBool(key, !GetBool(key));
        }
        
        public void SetString(string key, string value)
        {
            stringVariables[key] = value;
        }
        
        public string GetString(string key)
        {
            return stringVariables.TryGetValue(key, out var value) ? value : "";
        }
        
        public void SetFloat(string key, float value)
        {
            floatVariables[key] = value;
        }
        
        public float GetFloat(string key)
        {
            return floatVariables.TryGetValue(key, out var value) ? value : 0f;
        }
        
        public bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;
            
            condition = condition.Trim();
            
            if (condition.Contains(">="))
            {
                var parts = condition.Split(">=");
                var varName = parts[0].Trim();
                var value = int.Parse(parts[1].Trim());
                return GetInt(varName) >= value;
            }
            else if (condition.Contains("<="))
            {
                var parts = condition.Split("<=");
                var varName = parts[0].Trim();
                var value = int.Parse(parts[1].Trim());
                return GetInt(varName) <= value;
            }
            else if (condition.Contains(">"))
            {
                var parts = condition.Split(">");
                var varName = parts[0].Trim();
                var value = int.Parse(parts[1].Trim());
                return GetInt(varName) > value;
            }
            else if (condition.Contains("<"))
            {
                var parts = condition.Split("<");
                var varName = parts[0].Trim();
                var value = int.Parse(parts[1].Trim());
                return GetInt(varName) < value;
            }
            else if (condition.Contains("=="))
            {
                var parts = condition.Split("==");
                var varName = parts[0].Trim();
                var value = parts[1].Trim();
                return GetBool(varName) == bool.Parse(value);
            }
            else
            {
                return GetBool(condition);
            }
        }
        
        public void ResetAllVariables()
        {
            intVariables.Clear();
            boolVariables.Clear();
            stringVariables.Clear();
            floatVariables.Clear();
            InitializeDefaultVariables();
        }
    }
}
```

---

### 2.2 Dialogue Manager

**File:** `Scripts/Runtime/DialogueManager.cs`

```csharp
using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Dialogue;
using WhisperingGate.Core;

namespace WhisperingGate.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        
        public event Action<DialogueNode> OnNodeDisplayed;
        public event Action OnDialogueEnded;
        public event Action<int> OnChoicesUpdated;
        
        private DialogueNode currentNode;
        private DialogueTree currentTree;
        private bool isDialogueActive = false;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void StartDialogue(DialogueTree tree)
        {
            if (tree == null)
            {
                Debug.LogError("[DialogueManager] Tried to start null dialogue tree");
                return;
            }
            
            currentTree = tree;
            isDialogueActive = true;
            ShowNode(tree.StartNode);
        }
        
        public void SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive || currentNode == null) return;
            if (choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count) return;
            
            var choice = currentNode.Choices[choiceIndex];
            
            Debug.Log($"[DialogueManager] Choice selected: {choice.ChoiceText}");
            
            ApplyImpacts(choice.Impacts);
            
            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);
            
            ShowNode(choice.NextNode);
        }
        
        public void AdvanceToNextNode()
        {
            if (!isDialogueActive || currentNode == null) return;
            
            if (currentNode.Choices.Count == 0)
            {
                ShowNode(currentNode.NextNodeIfAuto);
            }
        }
        
        public bool IsDialogueActive => isDialogueActive;
        public DialogueNode CurrentNode => currentNode;
        
        private void ShowNode(DialogueNode node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }
            
            currentNode = node;
            
            Debug.Log($"[DialogueManager] Showing node: {node.NodeId}");
            
            foreach (var cmd in node.StartCommands)
                ExecuteCommand(cmd);
            
            OnNodeDisplayed?.Invoke(node);
            
            int visibleChoices = CountVisibleChoices(node);
            OnChoicesUpdated?.Invoke(visibleChoices);
            
            if (node.IsEndNode)
            {
                Invoke(nameof(EndDialogue), 3f);
            }
        }
        
        private int CountVisibleChoices(DialogueNode node)
        {
            int count = 0;
            foreach (var choice in node.Choices)
            {
                if (!choice.HasCondition || GameState.Instance.EvaluateCondition(choice.ShowCondition))
                    count++;
            }
            return count;
        }
        
        private void ApplyImpacts(List<ChoiceImpact> impacts)
        {
            foreach (var impact in impacts)
            {
                if (impact.IsConditional && !GameState.Instance.EvaluateCondition(impact.ApplyCondition))
                    continue;
                
                GameState.Instance.AddInt(impact.VariableName, impact.ValueChange);
                Debug.Log($"[DialogueManager] Impact: {impact.VariableName} += {impact.ValueChange}");
            }
        }
        
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            
            command = command.Trim();
            var parts = command.Split(':');
            var cmd = parts[0].ToLower();
            var param = parts.Length > 1 ? parts[1] : "";
            
            Debug.Log($"[DialogueManager] Executing command: {cmd} | {param}");
            
            switch (cmd)
            {
                case "item":
                    GiveItem(param);
                    break;
                
                case "flag":
                    GameState.Instance.SetBool(param, true);
                    break;
                
                case "unflag":
                    GameState.Instance.SetBool(param, false);
                    break;
                
                case "var":
                    if (param.Contains("+"))
                    {
                        var subparts = param.Split('+');
                        var varName = subparts[0].Trim();
                        var delta = int.Parse(subparts[1].Trim());
                        GameState.Instance.AddInt(varName, delta);
                    }
                    break;
                
                case "ending":
                    GameState.Instance.SetString("current_ending_path", param);
                    break;
                
                default:
                    Debug.LogWarning($"[DialogueManager] Unknown command: {cmd}");
                    break;
            }
        }
        
        private void GiveItem(string itemId)
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddItem(itemId);
            else
                Debug.LogWarning("[DialogueManager] InventoryManager not found");
        }
        
        private void EndDialogue()
        {
            if (!isDialogueActive) return;
            
            isDialogueActive = false;
            currentNode = null;
            OnDialogueEnded?.Invoke();
            
            Debug.Log("[DialogueManager] Dialogue ended. Player control restored.");
        }
    }
}
```

---

## PART 3: UI SYSTEM

### 3.1 Dialogue UI Panel

**File:** `Scripts/UI/DialogueUIPanel.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using WhisperingGate.Dialogue;
using WhisperingGate.Core;

namespace WhisperingGate.UI
{
    public class DialogueUIPanel : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Button skipButton;
        [SerializeField] private VerticalLayoutGroup choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        
        private List<Button> currentChoiceButtons = new();
        private Coroutine typewriterCoroutine;
        private bool isWaitingForInput = false;
        
        void Start()
        {
            DialogueManager.Instance.OnNodeDisplayed += DisplayNode;
            DialogueManager.Instance.OnDialogueEnded += HidePanel;
            skipButton.onClick.AddListener(SkipTypewriter);
            HidePanel();
        }
        
        void OnDestroy()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeDisplayed -= DisplayNode;
                DialogueManager.Instance.OnDialogueEnded -= HidePanel;
            }
        }
        
        private void DisplayNode(DialogueNode node)
        {
            StartCoroutine(FadePanel(true, 0.3f));
            
            if (node.Speaker != null)
            {
                speakerNameText.text = node.Speaker.DisplayName;
                portraitImage.sprite = node.Speaker.PortraitSprite;
            }
            
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriteDialogue(node));
        }
        
        private IEnumerator TypewriteDialogue(DialogueNode node)
        {
            isWaitingForInput = false;
            dialogueText.text = "";
            
            foreach (char c in node.LineText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
            
            isWaitingForInput = true;
            ShowChoices(node);
        }
        
        private void SkipTypewriter()
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            
            if (DialogueManager.Instance.CurrentNode != null)
                dialogueText.text = DialogueManager.Instance.CurrentNode.LineText;
            
            isWaitingForInput = true;
            ShowChoices(DialogueManager.Instance.CurrentNode);
        }
        
        private void ShowChoices(DialogueNode node)
        {
            foreach (var btn in currentChoiceButtons)
                Destroy(btn.gameObject);
            currentChoiceButtons.Clear();
            
            if (node.Choices.Count == 0)
            {
                StartCoroutine(WaitThenAdvance(2f));
                return;
            }
            
            int choiceCount = 0;
            foreach (var choice in node.Choices)
            {
                if (choice.HasCondition && !GameState.Instance.EvaluateCondition(choice.ShowCondition))
                    continue;
                
                var btn = Instantiate(choiceButtonPrefab, choicesContainer.transform);
                btn.GetComponentInChildren<Text>().text = choice.ChoiceText;
                
                int buttonIndex = choiceCount;
                btn.onClick.AddListener(() => SelectChoice(buttonIndex));
                currentChoiceButtons.Add(btn);
                choiceCount++;
            }
        }
        
        private void SelectChoice(int index)
        {
            if (index < 0 || index >= currentChoiceButtons.Count) return;
            DialogueManager.Instance.SelectChoice(index);
        }
        
        private IEnumerator WaitThenAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            DialogueManager.Instance.AdvanceToNextNode();
        }
        
        private void HidePanel()
        {
            StartCoroutine(FadePanel(false, 0.3f));
        }
        
        private IEnumerator FadePanel(bool fadeIn, float duration)
        {
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
```

---

## PART 4: TRIGGER SYSTEM

### 4.1 Dialogue Trigger

**File:** `Scripts/Interaction/DialogueTrigger.cs`

```csharp
using UnityEngine;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private InteractionMode interactionMode = InteractionMode.OnInteract;
        [SerializeField] private bool singleUse = false;
        [SerializeField] private bool pausePlayerDuringDialogue = true;
        
        private bool hasTriggered = false;
        private bool playerInRange = false;
        private PlayerController playerController;
        
        public enum InteractionMode { OnEnter, OnInteract }
        
        void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
            GetComponent<Collider>().isTrigger = true;
        }
        
        void Update()
        {
            if (interactionMode == InteractionMode.OnInteract && playerInRange && Input.GetKeyDown(KeyCode.E))
                TriggerDialogue();
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (interactionMode == InteractionMode.OnEnter)
                    TriggerDialogue();
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                playerInRange = false;
        }
        
        private void TriggerDialogue()
        {
            if (singleUse && hasTriggered)
                return;
            
            if (dialogueTree == null)
            {
                Debug.LogError($"[DialogueTrigger] No dialogue tree assigned on {gameObject.name}");
                return;
            }
            
            hasTriggered = true;
            
            if (pausePlayerDuringDialogue && playerController != null)
                playerController.SetInputEnabled(false);
            
            DialogueManager.Instance.StartDialogue(dialogueTree);
            
            if (pausePlayerDuringDialogue)
                DialogueManager.Instance.OnDialogueEnded += () =>
                {
                    if (playerController != null)
                        playerController.SetInputEnabled(true);
                };
        }
    }
}
```

---

## PART 5: INVENTORY MANAGER

### 5.1 Inventory Manager

**File:** `Scripts/Gameplay/InventoryManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using System;

namespace WhisperingGate.Gameplay
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        
        [System.Serializable]
        public class InventoryItem
        {
            public string itemId;
            public string itemName;
            public Sprite itemIcon;
            public string description;
        }
        
        [SerializeField] private List<InventoryItem> allItems = new();
        private List<string> playerInventory = new();
        
        public event Action<string> OnItemAdded;
        public event Action<string> OnItemRemoved;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void AddItem(string itemId)
        {
            if (!playerInventory.Contains(itemId))
            {
                playerInventory.Add(itemId);
                OnItemAdded?.Invoke(itemId);
                Debug.Log($"[Inventory] Added: {itemId}");
            }
        }
        
        public void RemoveItem(string itemId)
        {
            if (playerInventory.Contains(itemId))
            {
                playerInventory.Remove(itemId);
                OnItemRemoved?.Invoke(itemId);
                Debug.Log($"[Inventory] Removed: {itemId}");
            }
        }
        
        public bool HasItem(string itemId)
        {
            return playerInventory.Contains(itemId);
        }
        
        public List<string> GetAllItems()
        {
            return new List<string>(playerInventory);
        }
        
        public InventoryItem GetItemData(string itemId)
        {
            return allItems.Find(i => i.itemId == itemId);
        }
    }
}
```

---

## PART 6: PLAYER CONTROLLER TEMPLATE

### 6.1 Player Controller

**File:** `Scripts/Gameplay/PlayerController.cs`

```csharp
using UnityEngine;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }
        
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Camera playerCamera;
        
        private bool inputEnabled = true;
        private float xRotation = 0f;
        private Vector3 moveDirection;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            DialogueManager.Instance.OnNodeDisplayed += (node) => SetInputEnabled(false);
            DialogueManager.Instance.OnDialogueEnded += () => SetInputEnabled(true);
        }
        
        void Update()
        {
            if (!inputEnabled) return;
            
            HandleMovement();
            HandleMouseLook();
        }
        
        private void HandleMovement()
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
            
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            moveDirection = transform.forward * vertical + transform.right * horizontal;
            rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        
        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            transform.Rotate(Vector3.up * mouseX);
            
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
        
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }
    }
}
```

---

## SETUP CHECKLIST

- [ ] Create folder structure: `Assets/Scripts/Data/`, `Runtime/`, `UI/`, `Interaction/`, `Gameplay/`
- [ ] Create empty GameObject: "DialogueManager" → add DialogueManager script
- [ ] Create empty GameObject: "GameState" → add GameState script
- [ ] Create Canvas for UI → add DialogueUIPanel script
- [ ] Create Character assets for: Protagonist, Alina, Writer
- [ ] Create Dialogue Nodes for jungle intro scene
- [ ] Create Dialogue Tree: "JungleAwakens"
- [ ] Place DialogueTrigger in first scene
- [ ] Test: Start game, walk to trigger, press E

---

**Next: See 2-Implementation-Roadmap.md for step-by-step execution**
