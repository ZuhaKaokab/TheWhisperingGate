using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections;

public class UIScreenManager : MonoBehaviour
{
    [Header("UI Screens")]
    public CanvasGroup homeScreen;      // Splash / Intro screen
    public CanvasGroup loadingScreen;   // Loading screen
    public CanvasGroup mainMenuScreen;  // Main menu screen
    public CanvasGroup settingsScreen;  // NEW: Settings panel
    public CanvasGroup InstructionScreen;

    [Header("Loading UI Elements")]
    public Scrollbar loadingBar;        // Scrollbar in loading screen
    public Text loadingText; // Loading text e.g. "Loading... 80%"

    [Header("Timings")]
    public float homeDuration = 3f;     // Time home screen stays visible
    public float fadeDuration = 1f;     // Transition duration
    public float loadingDuration = 3f;  // Duration of loading bar fill

    [Header("Music")]
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer mainAudioMixer;
    private void Start()
    {
        // Hide all screens first
        homeScreen.gameObject.SetActive(false);
        loadingScreen.gameObject.SetActive(false);
        mainMenuScreen.gameObject.SetActive(false);
        settingsScreen.gameObject.SetActive(false); // NEW

        // Start full UI sequence
        StartCoroutine(ScreenSequence());
    }

    private IEnumerator ScreenSequence()
    {
        // --- HOME SCREEN ---
        homeScreen.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(homeScreen));
        yield return new WaitForSeconds(homeDuration);
        yield return StartCoroutine(FadeOut(homeScreen));

        // --- LOADING SCREEN ---
        loadingScreen.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(loadingScreen));
        yield return StartCoroutine(LoadingProgress());
        yield return StartCoroutine(FadeOut(loadingScreen));

        // --- MAIN MENU SCREEN ---
        mainMenuScreen.gameObject.SetActive(true);
        yield return StartCoroutine(FadeIn(mainMenuScreen));
    }

    // 🔹 Smooth fade-in animation
    private IEnumerator FadeIn(CanvasGroup cg)
    {
        if (cg == null) yield break;
        cg.alpha = 0f;
        cg.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    // 🔹 Smooth fade-out animation
    private IEnumerator FadeOut(CanvasGroup cg)
    {
        if (cg == null) yield break;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }

    // 🔹 Fake loading bar + percentage text
    private IEnumerator LoadingProgress()
    {
        if (loadingBar == null || loadingText == null)
        {
            Debug.LogWarning("Loading bar or text not assigned in UIScreenManager!");
            yield break;
        }

        loadingBar.size = 0f;
        float timer = 0f;

        while (timer < loadingDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadingDuration);
            loadingBar.size = progress;
            loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            yield return null;
        }

        loadingBar.size = 1f;
        loadingText.text = "Loading... 100%";
        yield return new WaitForSeconds(0.5f);
    }

    // 🔥 SHOW settings panel function
    public void ShowSettings()
    {
        StartCoroutine(FadeIn(settingsScreen));
        StartCoroutine(FadeOut(mainMenuScreen)); // Hide menu
    }

    // 🔥 HIDE settings panel function
    public void HideSettings()
    {
        StartCoroutine(FadeIn(mainMenuScreen)); // Show menu again
        StartCoroutine(FadeOut(settingsScreen)); // Hide settings
    }
    public void ShowInstructions()
    {
        StartCoroutine(FadeIn(InstructionScreen));
        StartCoroutine(FadeOut(mainMenuScreen)); // Hide menu
    }
    public void HideInstructions()
    {
        StartCoroutine(FadeIn(mainMenuScreen)); // Show menu again
        StartCoroutine(FadeOut(InstructionScreen)); // Hide settings
    }
    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        Debug.Log("Graphics Quality changed to: " + QualitySettings.names[index]);
    }
    public void ChangeSoundVolume()
    {
        mainAudioMixer.SetFloat("Master", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("Musix", musicVol.value);
    }
    public void ChangeSFXVolume()
    {
        mainAudioMixer.SetFloat("SFX", sfxVol.value);
    }
}
