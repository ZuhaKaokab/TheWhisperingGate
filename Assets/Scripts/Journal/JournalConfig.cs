using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Journal
{
    /// <summary>
    /// Master configuration for the journal system.
    /// Contains all pages and visual settings.
    /// </summary>
    [CreateAssetMenu(fileName = "JournalConfig", menuName = "Whispering Gate/Journal/Journal Config")]
    public class JournalConfig : ScriptableObject
    {
        [Header("Journal Pages")]
        [Tooltip("All pages that can appear in the journal")]
        public List<JournalPage> allPages = new List<JournalPage>();

        [Header("Visual Settings")]
        [Tooltip("Journal cover image")]
        public Sprite coverImage;
        
        [Tooltip("Paper/page background texture")]
        public Sprite pageBackground;
        
        [Tooltip("Page edge/border decoration")]
        public Sprite pageBorder;

        [Header("Colors")]
        public Color pageColor = new Color(0.95f, 0.93f, 0.88f); // Aged paper
        public Color textColor = new Color(0.2f, 0.15f, 0.1f);   // Dark ink
        public Color titleColor = new Color(0.4f, 0.2f, 0.1f);   // Reddish brown

        [Header("Audio")]
        [Tooltip("Sound when opening the journal")]
        public AudioClip openSound;
        
        [Tooltip("Sound when closing the journal")]
        public AudioClip closeSound;
        
        [Tooltip("Sound when flipping pages")]
        public AudioClip pageFlipSound;
        
        [Tooltip("Sound when new page is unlocked")]
        public AudioClip unlockSound;

        [Header("Animation")]
        [Tooltip("Duration of page flip animation")]
        public float pageFlipDuration = 0.3f;
        
        [Tooltip("Duration of open/close animation")]
        public float openCloseDuration = 0.4f;

        /// <summary>
        /// Get pages sorted by their sort order.
        /// </summary>
        public List<JournalPage> GetSortedPages()
        {
            List<JournalPage> sorted = new List<JournalPage>(allPages);
            sorted.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
            return sorted;
        }

        /// <summary>
        /// Find a page by its ID.
        /// </summary>
        public JournalPage GetPageById(string pageId)
        {
            foreach (var page in allPages)
            {
                if (page != null && page.pageId == pageId)
                    return page;
            }
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Remove null entries
            allPages.RemoveAll(p => p == null);
        }
#endif
    }
}

