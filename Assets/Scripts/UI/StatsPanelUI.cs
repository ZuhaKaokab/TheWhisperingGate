using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhisperingGate.Core;

namespace WhisperingGate.UI
{
    /// <summary>
    /// Displays player stats (courage, trust, sanity, etc.) in a persistent UI panel.
    /// Updates in real-time as variables change. Can be toggled on/off.
    /// </summary>
    public class StatsPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text courageText;
        [SerializeField] private TMP_Text trustAlinaText;
        [SerializeField] private TMP_Text trustWriterText;
        [SerializeField] private TMP_Text sanityText;
        [SerializeField] private TMP_Text investigationText;
        
        [Header("Progress Bars (Optional)")]
        [SerializeField] private Image courageBar;
        [SerializeField] private Image trustAlinaBar;
        [SerializeField] private Image trustWriterBar;
        [SerializeField] private Image sanityBar;
        [SerializeField] private Image investigationBar;
        
        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private bool showByDefault = false;
        [SerializeField] private bool updateInRealTime = true;
        
        private void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(showByDefault);
            
            if (GameState.Instance != null)
            {
                GameState.Instance.OnIntChanged += OnVariableChanged;
                UpdateAllStats();
            }
            else
            {
                Debug.LogWarning("[StatsPanelUI] GameState.Instance not found. Stats won't update.");
            }
        }
        
        private void OnDestroy()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnIntChanged -= OnVariableChanged;
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(toggleKey) && panelRoot != null)
            {
                panelRoot.SetActive(!panelRoot.activeSelf);
            }
        }
        
        private void OnVariableChanged(string variableName, int value)
        {
            if (!updateInRealTime) return;
            
            UpdateStat(variableName, value);
        }
        
        private void UpdateAllStats()
        {
            if (GameState.Instance == null) return;
            
            UpdateStat("courage", GameState.Instance.GetInt("courage"));
            UpdateStat("trust_alina", GameState.Instance.GetInt("trust_alina"));
            UpdateStat("trust_writer", GameState.Instance.GetInt("trust_writer"));
            UpdateStat("sanity", GameState.Instance.GetInt("sanity"));
            UpdateStat("investigation_level", GameState.Instance.GetInt("investigation_level"));
        }
        
        private void UpdateStat(string variableName, int value)
        {
            // Clamp values for display
            int clampedValue = Mathf.Clamp(value, 0, 100);
            
            switch (variableName.ToLower())
            {
                case "courage":
                    if (courageText != null)
                        courageText.text = $"Courage: {clampedValue}/100";
                    if (courageBar != null)
                        courageBar.fillAmount = clampedValue / 100f;
                    break;
                    
                case "trust_alina":
                    if (trustAlinaText != null)
                        trustAlinaText.text = $"Alina's Trust: {clampedValue}/100";
                    if (trustAlinaBar != null)
                        trustAlinaBar.fillAmount = clampedValue / 100f;
                    break;
                    
                case "trust_writer":
                    if (trustWriterText != null)
                        trustWriterText.text = $"Writer's Trust: {clampedValue}/100";
                    if (trustWriterBar != null)
                        trustWriterBar.fillAmount = clampedValue / 100f;
                    break;
                    
                case "sanity":
                    if (sanityText != null)
                        sanityText.text = $"Sanity: {clampedValue}/100";
                    if (sanityBar != null)
                        sanityBar.fillAmount = clampedValue / 100f;
                    break;
                    
                case "investigation_level":
                    if (investigationText != null)
                        investigationText.text = $"Investigation: {clampedValue}/100";
                    if (investigationBar != null)
                        investigationBar.fillAmount = clampedValue / 100f;
                    break;
            }
        }
        
        /// <summary>
        /// Manually refresh all stats display. Useful for initialization.
        /// </summary>
        public void RefreshStats()
        {
            UpdateAllStats();
        }
        
        /// <summary>
        /// Show or hide the stats panel.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
                panelRoot.SetActive(visible);
        }
    }
}


