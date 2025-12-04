using UnityEngine;
using TMPro;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Simple UI feedback for Grid Puzzle progress.
    /// Shows step count and messages.
    /// </summary>
    public class GridPuzzleUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridPuzzleController puzzleController;
        [SerializeField] private GameObject uiPanel;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Messages")]
        [SerializeField] private string startMessage = "Follow the correct path...";
        [SerializeField] private string failMessage = "Wrong step! Try again.";
        [SerializeField] private string solvedMessage = "Path complete!";

        [Header("Settings")]
        [SerializeField] private float messageDisplayTime = 2f;

        private float messageTimer = 0f;

        private void Start()
        {
            if (puzzleController == null)
                puzzleController = FindObjectOfType<GridPuzzleController>();

            if (puzzleController != null)
            {
                puzzleController.OnPuzzleStarted += HandlePuzzleStarted;
                puzzleController.OnProgressChanged += HandleProgressChanged;
                puzzleController.OnPuzzleFailed += HandlePuzzleFailed;
                puzzleController.OnPuzzleSolved += HandlePuzzleSolved;
            }

            if (uiPanel != null)
                uiPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (puzzleController != null)
            {
                puzzleController.OnPuzzleStarted -= HandlePuzzleStarted;
                puzzleController.OnProgressChanged -= HandleProgressChanged;
                puzzleController.OnPuzzleFailed -= HandlePuzzleFailed;
                puzzleController.OnPuzzleSolved -= HandlePuzzleSolved;
            }
        }

        private void Update()
        {
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f && messageText != null)
                {
                    messageText.text = "";
                }
            }
        }

        private void HandlePuzzleStarted()
        {
            if (uiPanel != null)
                uiPanel.SetActive(true);

            UpdateProgress(0, puzzleController.Config.correctPath.Count);
            ShowMessage(startMessage);
        }

        private void HandleProgressChanged(int current, int total)
        {
            UpdateProgress(current, total);
        }

        private void HandlePuzzleFailed()
        {
            ShowMessage(failMessage);
            UpdateProgress(0, puzzleController.Config.correctPath.Count);
        }

        private void HandlePuzzleSolved()
        {
            ShowMessage(solvedMessage);
            
            // Hide UI after delay
            Invoke(nameof(HideUI), 3f);
        }

        private void UpdateProgress(int current, int total)
        {
            if (progressText != null)
            {
                progressText.text = $"Steps: {current} / {total}";
            }
        }

        private void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
                messageTimer = messageDisplayTime;
            }
        }

        private void HideUI()
        {
            if (uiPanel != null)
                uiPanel.SetActive(false);
        }
    }
}



