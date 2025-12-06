using UnityEngine;

namespace WhisperingGate.Journal
{
    /// <summary>
    /// Represents a single page in the journal.
    /// Create these as ScriptableObjects to define journal content.
    /// </summary>
    [CreateAssetMenu(fileName = "JournalPage_New", menuName = "Whispering Gate/Journal/Journal Page")]
    public class JournalPage : ScriptableObject
    {
        [Header("Page Identity")]
        [Tooltip("Unique identifier for this page")]
        public string pageId = "page_1";
        
        [Tooltip("Display title at top of page")]
        public string pageTitle = "Page Title";
        
        [Tooltip("Order in the journal (lower = earlier)")]
        public int sortOrder = 0;

        [Header("Content Type")]
        public PageContentType contentType = PageContentType.Text;

        [Header("Text Content")]
        [TextArea(5, 15)]
        [Tooltip("Main text content (supports rich text: <b>, <i>, <color>, <size>)")]
        public string textContent = "";

        [Header("Image Content")]
        [Tooltip("Image/illustration for this page")]
        public Sprite pageImage;
        
        [Tooltip("Where to position the image")]
        public ImagePosition imagePosition = ImagePosition.Top;
        
        [Tooltip("Image size (0-1, percentage of page width)")]
        [Range(0.3f, 1f)]
        public float imageScale = 0.8f;

        [Header("Unlock Settings")]
        [Tooltip("Is this page available from the start?")]
        public bool unlockedByDefault = false;
        
        [Tooltip("GameState condition to auto-unlock (e.g., 'talked_to_writer' or 'courage >= 10')")]
        public string unlockCondition = "";
        
        [Tooltip("Flag that gets set when this page is unlocked (for tracking)")]
        public string unlockFlag = "";

        [Header("Audio (Optional)")]
        [Tooltip("Sound to play when this page is first viewed")]
        public AudioClip firstViewSound;
    }

    public enum PageContentType
    {
        Text,           // Text only
        Image,          // Image only
        TextAndImage    // Both text and image
    }

    public enum ImagePosition
    {
        Top,            // Image at top, text below
        Bottom,         // Text at top, image below
        Full,           // Image fills entire page
        Background      // Image as background, text overlay
    }
}

