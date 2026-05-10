using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhisperRandomizer : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource whisperSource;
    public AudioSource whisper2Source;
    public AudioSource birdSource;

    [Header("Audio Clips")]
    public AudioClip[] whisperClips;
    public AudioClip[] whisper2Clips;
    public AudioClip[] birdClips;

    [Header("General Settings")]
    public float fadeDuration = 2.0f;
    [Range(0f, 1f)] public float maxVolume = 0.7f;

    void Start()
    {
        // Har audio category ka apna independent loop
        // Format: Source, Clips, MinGap, MaxGap, MaxPlayTime
        StartCoroutine(PlayAudioLoop(whisperSource, whisperClips, 1f, 15f, 4f));
        StartCoroutine(PlayAudioLoop(whisper2Source, whisper2Clips, 3f, 20f, 5f));
        StartCoroutine(PlayAudioLoop(birdSource, birdClips, 10f, 25f, 6f));
    }

    IEnumerator PlayAudioLoop(AudioSource source, AudioClip[] clips, float minGap, float maxGap, float maxPlayTime)
    {
        while (true)
        {
            if (clips != null && clips.Length > 0)
            {
                // --- RANDOM GAP LOGIC ---
                float randomWait = Random.Range(minGap, maxGap);

                // 15% Chance ke gap "Extra Long" ho jaye (Silence create karne ke liye)
                if (Random.value > 0.85f)
                {
                    randomWait *= 2.5f;
                }

                yield return new WaitForSeconds(randomWait);

                // --- SETUP CLIP ---
                source.clip = clips[Random.Range(0, clips.Length)];
                source.pitch = Random.Range(0.80f, 1.15f); // Bhaari ya patli awaz ka randomness

                // --- FADE IN ---
                float targetVol = Random.Range(0.2f, maxVolume);
                yield return StartCoroutine(FadeAudio(source, 0, targetVol, fadeDuration));

                // --- PLAY DURATION ---
                // Clip kitni dair tak chale (Randomly cut karna)
                float playDuration = Random.Range(2f, maxPlayTime);
                float timer = 0;
                while (source.isPlaying && timer < playDuration)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                // --- FADE OUT ---
                yield return StartCoroutine(FadeAudio(source, source.volume, 0, fadeDuration));
            }
            else
            {
                yield return null; // Agar clips khali hain toh error na aaye
            }
        }
    }

    IEnumerator FadeAudio(AudioSource source, float start, float end, float duration)
    {
        float elapsed = 0;
        if (end > 0) source.Play();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        source.volume = end;
        if (end == 0) source.Stop();
    }
}