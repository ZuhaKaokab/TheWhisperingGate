using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace WhisperingGate.Journal
{
    /// <summary>
    /// UI controller for the journal interface.
    /// Displays pages in a two-page spread with Q/E navigation.
    /// </summary>
    public class JournalUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject journalPanel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Page Display - Left Page")]
        [SerializeField] private GameObject leftPageContainer;
        [SerializeField] private Image leftPageBackground;
        [SerializeField] private TextMeshProUGUI leftPageTitle;
        [SerializeField] private TextMeshProUGUI leftPageText;
        [SerializeField] private Image leftPageImage;
        [SerializeField] private GameObject leftPageNewIndicator;

        [Header("Page Display - Right Page")]
        [SerializeField] private GameObject rightPageContainer;
        [SerializeField] private Image rightPageBackground;
        [SerializeField] private TextMeshProUGUI rightPageTitle;
        [SerializeField] private TextMeshProUGUI rightPageText;
        [SerializeField] private Image rightPageImage;
        [SerializeField] private GameObject rightPageNewIndicator;

        [Header("Navigation")]
        [SerializeField] private TextMeshProUGUI pageNumberText;
        [SerializeField] private GameObject prevButton;
        [SerializeField] private GameObject nextButton;
        [SerializeField] private KeyCode prevPageKey = KeyCode.Q;
        [SerializeField] private KeyCode nextPageKey = KeyCode.E;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;
        [SerializeField] private KeyCode closeKeyAlt = KeyCode.Tab;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float pageFlipDuration = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        // Runtime state
        private List<JournalPage> currentPages = new List<JournalPage>();
        private int currentSpreadIndex = 0; // Index of left page (0, 2, 4, etc.)
        private bool isAnimating = false;
        private bool isOpen = false;

        private JournalManager manager;
        private JournalConfig config;

        private void Awake()
        {
            if (journalPanel != null)
            {
                journalPanel.SetActive(false);
            }

            if (canvasGroup == null && journalPanel != null)
            {
                canvasGroup = journalPanel.GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            manager = JournalManager.Instance;
            if (manager != null)
            {
                config = manager.Config;
            }
        }

        private void Update()
        {
            if (!isOpen || isAnimating) return;

            // Navigation
            if (Input.GetKeyDown(prevPageKey))
            {
                PreviousPage();
            }
            else if (Input.GetKeyDown(nextPageKey))
            {
                NextPage();
            }

            // Close
            if (Input.GetKeyDown(closeKey) || Input.GetKeyDown(closeKeyAlt))
            {
                manager?.CloseJournal();
            }
        }

        /// <summary>
        /// Open the journal UI.
        /// </summary>
        public void Open(string gotoPageId = null)
        {
            if (isOpen) return;

            isOpen = true;
            
            // Refresh unlocked pages
            RefreshPages();

            // Find target page index
            currentSpreadIndex = 0;
            if (!string.IsNullOrWhiteSpace(gotoPageId))
            {
                int pageIndex = FindPageIndex(gotoPageId);
                if (pageIndex >= 0)
                {
                    // Convert to spread index (even number)
                    currentSpreadIndex = (pageIndex / 2) * 2;
                }
            }

            // Show panel
            if (journalPanel != null)
            {
                journalPanel.SetActive(true);
            }

            // Display current spread
            DisplayCurrentSpread();

            // Animate in
            StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Close the journal UI.
        /// </summary>
        public void Close()
        {
            if (!isOpen) return;

            isOpen = false;
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// Go to previous page spread.
        /// </summary>
        public void PreviousPage()
        {
            if (isAnimating || currentSpreadIndex <= 0) return;

            currentSpreadIndex -= 2;
            StartCoroutine(FlipPage(-1));
            PlayFlipSound();
        }

        /// <summary>
        /// Go to next page spread.
        /// </summary>
        public void NextPage()
        {
            if (isAnimating || currentSpreadIndex + 2 >= currentPages.Count) return;

            currentSpreadIndex += 2;
            StartCoroutine(FlipPage(1));
            PlayFlipSound();
        }

        /// <summary>
        /// Refresh the list of unlocked pages.
        /// </summary>
        private void RefreshPages()
        {
            currentPages.Clear();
            
            if (manager != null)
            {
                currentPages = manager.GetUnlockedPages();
            }
        }

        /// <summary>
        /// Find index of a page by ID.
        /// </summary>
        private int FindPageIndex(string pageId)
        {
            for (int i = 0; i < currentPages.Count; i++)
            {
                if (currentPages[i].pageId == pageId)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Display the current two-page spread.
        /// </summary>
        private void DisplayCurrentSpread()
        {
            // Left page
            JournalPage leftPage = GetPageAt(currentSpreadIndex);
            DisplayPage(leftPage, true);

            // Right page
            JournalPage rightPage = GetPageAt(currentSpreadIndex + 1);
            DisplayPage(rightPage, false);

            // Update navigation
            UpdateNavigation();
        }

        /// <summary>
        /// Get page at index, or null if out of bounds.
        /// </summary>
        private JournalPage GetPageAt(int index)
        {
            if (index >= 0 && index < currentPages.Count)
                return currentPages[index];
            return null;
        }

        /// <summary>
        /// Display a single page.
        /// </summary>
        private void DisplayPage(JournalPage page, bool isLeftPage)
        {
            GameObject container = isLeftPage ? leftPageContainer : rightPageContainer;
            TextMeshProUGUI titleText = isLeftPage ? leftPageTitle : rightPageTitle;
            TextMeshProUGUI contentText = isLeftPage ? leftPageText : rightPageText;
            Image pageImage = isLeftPage ? leftPageImage : rightPageImage;
            GameObject newIndicator = isLeftPage ? leftPageNewIndicator : rightPageNewIndicator;
            Image background = isLeftPage ? leftPageBackground : rightPageBackground;

            if (page == null)
            {
                // Empty page
                if (container != null) container.SetActive(false);
                return;
            }

            if (container != null) container.SetActive(true);

            // Background
            if (background != null && config != null)
            {
                if (config.pageBackground != null)
                    background.sprite = config.pageBackground;
                background.color = config.pageColor;
            }

            // Title
            if (titleText != null)
            {
                titleText.text = page.pageTitle;
                if (config != null)
                    titleText.color = config.titleColor;
            }

            // Content based on type
            bool showText = page.contentType == PageContentType.Text || 
                           page.contentType == PageContentType.TextAndImage;
            bool showImage = page.contentType == PageContentType.Image || 
                            page.contentType == PageContentType.TextAndImage;

            // Text
            if (contentText != null)
            {
                contentText.gameObject.SetActive(showText);
                if (showText)
                {
                    contentText.text = page.textContent;
                    if (config != null)
                        contentText.color = config.textColor;
                }
            }

            // Image
            if (pageImage != null)
            {
                pageImage.gameObject.SetActive(showImage && page.pageImage != null);
                if (showImage && page.pageImage != null)
                {
                    pageImage.sprite = page.pageImage;
                    pageImage.preserveAspect = true;
                    
                    // Position based on setting
                    RectTransform rt = pageImage.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        switch (page.imagePosition)
                        {
                            case ImagePosition.Top:
                                rt.anchorMin = new Vector2(0.5f, 1f);
                                rt.anchorMax = new Vector2(0.5f, 1f);
                                rt.pivot = new Vector2(0.5f, 1f);
                                rt.anchoredPosition = new Vector2(0, -10);
                                break;
                            case ImagePosition.Bottom:
                                rt.anchorMin = new Vector2(0.5f, 0f);
                                rt.anchorMax = new Vector2(0.5f, 0f);
                                rt.pivot = new Vector2(0.5f, 0f);
                                rt.anchoredPosition = new Vector2(0, 10);
                                break;
                            case ImagePosition.Full:
                            case ImagePosition.Background:
                                rt.anchorMin = Vector2.zero;
                                rt.anchorMax = Vector2.one;
                                rt.offsetMin = Vector2.zero;
                                rt.offsetMax = Vector2.zero;
                                break;
                        }
                    }
                }
            }

            // New indicator
            if (newIndicator != null && manager != null)
            {
                bool isNew = !manager.IsPageViewed(page.pageId);
                newIndicator.SetActive(isNew);
                
                // Mark as viewed
                manager.MarkPageViewed(page.pageId);
            }
        }

        /// <summary>
        /// Update navigation buttons and page number.
        /// </summary>
        private void UpdateNavigation()
        {
            // Page number
            if (pageNumberText != null)
            {
                int currentLeft = currentSpreadIndex + 1;
                int currentRight = Mathf.Min(currentSpreadIndex + 2, currentPages.Count);
                int total = currentPages.Count;
                
                if (total > 0)
                {
                    pageNumberText.text = $"Pages {currentLeft}-{currentRight} of {total}";
                }
                else
                {
                    pageNumberText.text = "No entries";
                }
            }

            // Prev button
            if (prevButton != null)
            {
                prevButton.SetActive(currentSpreadIndex > 0);
            }

            // Next button
            if (nextButton != null)
            {
                nextButton.SetActive(currentSpreadIndex + 2 < currentPages.Count);
            }
        }

        /// <summary>
        /// Play page flip sound.
        /// </summary>
        private void PlayFlipSound()
        {
            if (config != null && config.pageFlipSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(config.pageFlipSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(config.pageFlipSound, 
                        UnityEngine.Camera.main.transform.position, 0.5f);
                }
            }
        }

        private IEnumerator FadeIn()
        {
            isAnimating = true;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float elapsed = 0f;
                
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }

            isAnimating = false;
        }

        private IEnumerator FadeOut()
        {
            isAnimating = true;
            
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 0f;
            }

            if (journalPanel != null)
            {
                journalPanel.SetActive(false);
            }

            isAnimating = false;
        }

        private IEnumerator FlipPage(int direction)
        {
            isAnimating = true;
            
            // Simple fade transition for now
            // Could be enhanced with actual page flip animation
            if (canvasGroup != null)
            {
                float halfDuration = pageFlipDuration / 2f;
                
                // Fade out
                float elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0.5f, elapsed / halfDuration);
                    yield return null;
                }
                
                // Update display
                DisplayCurrentSpread();
                
                // Fade in
                elapsed = 0f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0.5f, 1f, elapsed / halfDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }
            else
            {
                DisplayCurrentSpread();
            }

            isAnimating = false;
        }

        // UI Button callbacks
        public void OnPrevButtonClicked() => PreviousPage();
        public void OnNextButtonClicked() => NextPage();
        public void OnCloseButtonClicked() => manager?.CloseJournal();
    }
}

