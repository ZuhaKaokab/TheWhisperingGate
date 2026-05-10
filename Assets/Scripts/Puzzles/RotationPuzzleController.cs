using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Core;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Puzzles
{
    public class RotationPuzzleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RotationPuzzleConfig config;

        [Header("Portal Activation")]
        [Tooltip("Portal ka particle system yahan drag kar ke dalein")]
        [SerializeField] private ParticleSystem portalParticleSystem;

        [Header("Grid Setup")]
        [SerializeField] private Transform elementsParent;
        [SerializeField] private GameObject elementPrefab;
        [SerializeField] private List<RotatableElement> preplacedElements = new List<RotatableElement>();

        [Header("Solve Mode Settings")]
        [SerializeField] private KeyCode exitKey = KeyCode.Escape;
        [SerializeField] private KeyCode exitKeyAlt = KeyCode.Tab;
        [SerializeField] private KeyCode rotateClockwiseKey = KeyCode.E;
        [SerializeField] private KeyCode rotateCounterClockwiseKey = KeyCode.Q;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip rotateSound;
        [SerializeField] private AudioClip solveSound;
        [SerializeField] private AudioClip selectSound;

        // Runtime state
        private List<RotatableElement> elements = new List<RotatableElement>();
        private bool isInSolveMode = false;
        private bool isSolved = false;
        private int selectedRow = 0;
        private int selectedCol = 0;
        private RotatableElement selectedElement;

        // --- PUBLIC PROPERTIES (Required by Lever and Save System) ---
        public bool IsInSolveMode => isInSolveMode;
        public bool IsSolved => isSolved;
        public RotationPuzzleConfig Config => config;
        public RotatableElement SelectedElement => selectedElement;

        // Events
        public event Action OnPuzzleSolved;
        public event Action OnSolveModeEntered;
        public event Action OnSolveModeExited;
        public event Action<RotatableElement> OnElementRotated;

        private void Start()
        {
            if (config == null) return;
            InitializeElements();
            if (portalParticleSystem != null) portalParticleSystem.Stop();
        }

        private void Update()
        {
            if (!isInSolveMode || isSolved) return;
            HandleSolveModeInput();
        }

        private void InitializeElements()
        {
            elements.Clear();
            if (preplacedElements.Count > 0)
            {
                elements.AddRange(preplacedElements);
                for (int i = 0; i < elements.Count && i < config.TotalElements; i++)
                {
                    elements[i].Initialize(config, i / config.columns, i % config.columns);
                    elements[i].OnRotationComplete += OnElementRotationComplete;
                }
            }
            else { SpawnElements(); }
        }

        private void SpawnElements()
        {
            Transform parent = elementsParent != null ? elementsParent : transform;
            for (int row = 0; row < config.rows; row++)
            {
                for (int col = 0; col < config.columns; col++)
                {
                    Vector3 localPos = new Vector3(col * config.elementSpacing, row * config.elementSpacing, 0);
                    localPos.x -= (config.columns - 1) * config.elementSpacing * 0.5f;
                    localPos.y -= (config.rows - 1) * config.elementSpacing * 0.5f;

                    GameObject obj = elementPrefab != null ? Instantiate(elementPrefab, parent) : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.localPosition = localPos;
                    obj.name = $"Element_{row}_{col}";

                    var element = obj.GetComponent<RotatableElement>() ?? obj.AddComponent<RotatableElement>();
                    element.Initialize(config, row, col);
                    element.OnRotationComplete += OnElementRotationComplete;
                    elements.Add(element);
                }
            }
        }

        public void EnterSolveMode()
        {
            if (isSolved || isInSolveMode) return;
            isInSolveMode = true;
            UpdateSelection();
            if (Gameplay.PlayerController.Instance != null) Gameplay.PlayerController.Instance.SetInputEnabled(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            OnSolveModeEntered?.Invoke();
        }

        public void ExitSolveMode()
        {
            if (!isInSolveMode) return;
            isInSolveMode = false;
            if (selectedElement != null) selectedElement.SetSelected(false);
            if (Gameplay.PlayerController.Instance != null) Gameplay.PlayerController.Instance.SetInputEnabled(true);
            OnSolveModeExited?.Invoke();
        }

        private void HandleSolveModeInput()
        {
            if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(exitKeyAlt)) { ExitSolveMode(); return; }
            int r = 0, c = 0;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) r = 1;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) r = -1;
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) c = -1;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) c = 1;
            if (r != 0 || c != 0) NavigateSelection(r, c);

            if (Input.GetKeyDown(rotateClockwiseKey)) RotateSelected(true);
            else if (Input.GetKeyDown(rotateCounterClockwiseKey)) RotateSelected(false);
        }

        private void NavigateSelection(int r, int c)
        {
            selectedRow = Mathf.Clamp(selectedRow + r, 0, config.rows - 1);
            selectedCol = Mathf.Clamp(selectedCol + c, 0, config.columns - 1);
            UpdateSelection();
            PlaySound(selectSound);
        }

        private void UpdateSelection()
        {
            if (selectedElement != null) selectedElement.SetSelected(false);
            selectedElement = GetElementAt(selectedRow, selectedCol);
            if (selectedElement != null) selectedElement.SetSelected(true);
        }

        private void RotateSelected(bool clockwise)
        {
            if (selectedElement == null || selectedElement.IsRotating) return;
            if (clockwise) selectedElement.RotateNext(); else selectedElement.RotatePrevious();
            PlaySound(rotateSound);
            OnElementRotated?.Invoke(selectedElement);
        }

        private void OnElementRotationComplete(RotatableElement element) { element.UpdateCorrectState(); CheckSolution(); }

        private void CheckSolution()
        {
            foreach (var el in elements) if (!el.IsCorrect) return;
            SolvePuzzle();
        }

        private void SolvePuzzle()
        {
            if (isSolved) return;
            isSolved = true;
            isInSolveMode = false;
            if (selectedElement != null) selectedElement.SetSelected(false);

            PlaySound(solveSound);

            // --- PORTAL ACTIVATION ---
            if (portalParticleSystem != null) portalParticleSystem.Play();

            ExecuteCommands(config.onSolvedCommands);
            if (Gameplay.PlayerController.Instance != null) Gameplay.PlayerController.Instance.SetInputEnabled(true);
            OnPuzzleSolved?.Invoke();
        }

        public RotatableElement GetElementAt(int r, int c) { int i = r * config.columns + c; return (i >= 0 && i < elements.Count) ? elements[i] : null; }

        private void ExecuteCommands(List<string> commands)
        {
            if (commands == null) return;
            foreach (string cmd in commands)
            {
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                int colon = cmd.IndexOf(':');
                if (colon > 0) ExecuteCommand(cmd.Substring(0, colon).ToLower().Trim(), cmd.Substring(colon + 1).Trim());
            }
        }

        private void ExecuteCommand(string type, string param)
        {
            switch (type)
            {
                case "flag": if (GameState.Instance != null) GameState.Instance.SetBool(param, true); break;
                case "activate": Interaction.ActivatableObject.ExecuteCommand("activate", param); break;
                case "cam": if (Camera.CameraFocusController.Instance != null) Camera.CameraFocusController.Instance.FocusOn(param); break;
            }
        }

        public void SetSolvedState(bool solved)
        {
            isSolved = solved;
            if (solved && config != null)
            {
                for (int i = 0; i < elements.Count && i < config.solutionIndices.Count; i++)
                {
                    if (elements[i] != null) { elements[i].SetRotationIndex(config.solutionIndices[i]); elements[i].UpdateCorrectState(); }
                }
                if (portalParticleSystem != null) portalParticleSystem.Play();
            }
        }

        private void PlaySound(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
        private void OnDestroy() { foreach (var el in elements) if (el != null) el.OnRotationComplete -= OnElementRotationComplete; }
    }
}