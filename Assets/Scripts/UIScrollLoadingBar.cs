using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScrollLoadingBar : MonoBehaviour
{
    [Header("UI References")]
    public Scrollbar scrollBar;
    public Text loadingText;

    [Header("Settings")]
    public float loadTime = 3f;
    private float timer = 0f;
    private bool loadingComplete = false;

    private void OnEnable()
    {
        timer = 0f;
        loadingComplete = false;

        if (scrollBar != null)
            scrollBar.size = 0f;

        if (loadingText != null)
            loadingText.text = "Loading... 0%";
    }

    private void Update()
    {
        if (scrollBar == null || loadingText == null)
            return;

        if (!loadingComplete)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadTime);
            scrollBar.size = progress;
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";

            if (progress >= 1f)
            {
                loadingComplete = true;
                loadingText.text = "Loading Complete!";
                Invoke(nameof(ShowMainMenu), 0.5f);
            }
        }
    }

    private void ShowMainMenu()
    {
        UIScreenManager manager = FindObjectOfType<UIScreenManager>();
        if (manager != null)
        {
         //   manager.ShowMainMenuScreen(); // ✅ direct safe call
        }
        else
        {
            Debug.LogWarning("UIScreenManager not found in scene!");
        }
    }
}
