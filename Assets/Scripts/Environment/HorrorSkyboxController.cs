using UnityEngine;
using System;

namespace WhisperingGate.Environment
{
    /// <summary>
    /// Controls a procedural horror skybox with mood transitions.
    /// Supports blood red bleeding sky, dark night, and smooth transitions between moods.
    /// </summary>
    [ExecuteInEditMode]
    public class HorrorSkyboxController : MonoBehaviour
    {
        public static HorrorSkyboxController Instance { get; private set; }

        [Header("Skybox Material")]
        [Tooltip("Assign the HorrorSkybox material here")]
        [SerializeField] private Material skyboxMaterial;

        [Header("Current Sky State")]
        [Range(0f, 1f)]
        [Tooltip("0 = Mood A (Blood Red), 1 = Mood B (Dark Night)")]
        [SerializeField] private float moodBlend = 0f;

        [Header("Mood A - Blood Red Sky (Prologue Start)")]
        [SerializeField] private Color moodA_TopColor = new Color(0.15f, 0.02f, 0.02f, 1f);      // Deep blood red
        [SerializeField] private Color moodA_HorizonColor = new Color(0.6f, 0.1f, 0.05f, 1f);   // Bright blood red
        [SerializeField] private Color moodA_BottomColor = new Color(0.08f, 0.02f, 0.02f, 1f);  // Dark crimson
        [SerializeField] private Color moodA_SunColor = new Color(1f, 0.2f, 0.1f, 1f);          // Red sun
        [SerializeField] private Color moodA_CloudColor = new Color(0.3f, 0.05f, 0.02f, 1f);    // Dark red clouds
        [Range(0f, 1f)]
        [SerializeField] private float moodA_SunIntensity = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float moodA_CloudDensity = 0.6f;
        [Range(0f, 1f)]
        [SerializeField] private float moodA_HorizonSharpness = 0.3f;

        [Header("Mood B - Dark Night Sky (After Transition)")]
        [SerializeField] private Color moodB_TopColor = new Color(0.02f, 0.02f, 0.08f, 1f);      // Deep dark blue
        [SerializeField] private Color moodB_HorizonColor = new Color(0.05f, 0.08f, 0.15f, 1f);  // Slightly lighter blue
        [SerializeField] private Color moodB_BottomColor = new Color(0.01f, 0.01f, 0.03f, 1f);   // Almost black
        [SerializeField] private Color moodB_SunColor = new Color(0.6f, 0.7f, 0.9f, 1f);         // Pale moon
        [SerializeField] private Color moodB_CloudColor = new Color(0.03f, 0.04f, 0.08f, 1f);    // Dark blue clouds
        [Range(0f, 1f)]
        [SerializeField] private float moodB_SunIntensity = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float moodB_CloudDensity = 0.4f;
        [Range(0f, 1f)]
        [SerializeField] private float moodB_HorizonSharpness = 0.5f;

        [Header("Sun/Moon Settings")]
        [Range(-1f, 1f)]
        [Tooltip("Vertical position: -1 = below horizon, 0 = at horizon, 1 = overhead")]
        [SerializeField] private float sunHeight = 0.1f;
        [Range(0f, 360f)]
        [SerializeField] private float sunRotation = 45f;
        [Range(0.01f, 0.3f)]
        [SerializeField] private float sunSize = 0.08f;
        [Range(0f, 1f)]
        [SerializeField] private float sunGlow = 0.5f;

        [Header("Fog/Haze")]
        [SerializeField] private bool enableFog = true;
        [SerializeField] private Color fogColor = new Color(0.1f, 0.05f, 0.05f, 1f);
        [Range(0f, 0.1f)]
        [SerializeField] private float fogDensity = 0.02f;

        [Header("Stars (Night)")]
        [Range(0f, 1f)]
        [SerializeField] private float starsIntensity = 0f;
        [SerializeField] private float starsSpeed = 0.01f;

        [Header("Animation")]
        [SerializeField] private float cloudSpeed = 0.02f;
        [SerializeField] private float distortionSpeed = 0.01f;

        [Header("Transition Settings")]
        [SerializeField] private float defaultTransitionDuration = 5f;

        // Transition state
        private bool isTransitioning = false;
        private float transitionStartValue;
        private float transitionEndValue;
        private float transitionDuration;
        private float transitionElapsed;
        private Action onTransitionComplete;

        // Shader property IDs (cached for performance)
        private static readonly int _TopColor = Shader.PropertyToID("_TopColor");
        private static readonly int _HorizonColor = Shader.PropertyToID("_HorizonColor");
        private static readonly int _BottomColor = Shader.PropertyToID("_BottomColor");
        private static readonly int _HorizonSharpness = Shader.PropertyToID("_HorizonSharpness");
        private static readonly int _SunColor = Shader.PropertyToID("_SunColor");
        private static readonly int _SunDirection = Shader.PropertyToID("_SunDirection");
        private static readonly int _SunSize = Shader.PropertyToID("_SunSize");
        private static readonly int _SunIntensity = Shader.PropertyToID("_SunIntensity");
        private static readonly int _SunGlow = Shader.PropertyToID("_SunGlow");
        private static readonly int _CloudColor = Shader.PropertyToID("_CloudColor");
        private static readonly int _CloudDensity = Shader.PropertyToID("_CloudDensity");
        private static readonly int _CloudSpeed = Shader.PropertyToID("_CloudSpeed");
        private static readonly int _StarsIntensity = Shader.PropertyToID("_StarsIntensity");
        private static readonly int _StarsSpeed = Shader.PropertyToID("_StarsSpeed");
        private static readonly int _Time = Shader.PropertyToID("_CustomTime");

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
            }
        }

        private void Update()
        {
            // Handle transition
            if (isTransitioning)
            {
                transitionElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(transitionElapsed / transitionDuration);
                
                // Smooth step for nicer easing
                t = t * t * (3f - 2f * t);
                
                moodBlend = Mathf.Lerp(transitionStartValue, transitionEndValue, t);

                if (transitionElapsed >= transitionDuration)
                {
                    isTransitioning = false;
                    moodBlend = transitionEndValue;
                    onTransitionComplete?.Invoke();
                    onTransitionComplete = null;
                }
            }

            UpdateSkybox();
            UpdateFog();
        }

        private void UpdateSkybox()
        {
            if (skyboxMaterial == null) return;

            // Lerp all colors based on mood blend
            Color topColor = Color.Lerp(moodA_TopColor, moodB_TopColor, moodBlend);
            Color horizonColor = Color.Lerp(moodA_HorizonColor, moodB_HorizonColor, moodBlend);
            Color bottomColor = Color.Lerp(moodA_BottomColor, moodB_BottomColor, moodBlend);
            Color sunColor = Color.Lerp(moodA_SunColor, moodB_SunColor, moodBlend);
            Color cloudColor = Color.Lerp(moodA_CloudColor, moodB_CloudColor, moodBlend);
            
            float sunIntensity = Mathf.Lerp(moodA_SunIntensity, moodB_SunIntensity, moodBlend);
            float cloudDensity = Mathf.Lerp(moodA_CloudDensity, moodB_CloudDensity, moodBlend);
            float horizonSharpness = Mathf.Lerp(moodA_HorizonSharpness, moodB_HorizonSharpness, moodBlend);

            // Calculate sun direction from height and rotation
            float sunAngleRad = sunRotation * Mathf.Deg2Rad;
            Vector3 sunDir = new Vector3(
                Mathf.Cos(sunAngleRad) * Mathf.Cos(sunHeight * Mathf.PI * 0.5f),
                sunHeight,
                Mathf.Sin(sunAngleRad) * Mathf.Cos(sunHeight * Mathf.PI * 0.5f)
            ).normalized;

            // Apply to material
            skyboxMaterial.SetColor(_TopColor, topColor);
            skyboxMaterial.SetColor(_HorizonColor, horizonColor);
            skyboxMaterial.SetColor(_BottomColor, bottomColor);
            skyboxMaterial.SetFloat(_HorizonSharpness, horizonSharpness);
            
            skyboxMaterial.SetColor(_SunColor, sunColor);
            skyboxMaterial.SetVector(_SunDirection, sunDir);
            skyboxMaterial.SetFloat(_SunSize, sunSize);
            skyboxMaterial.SetFloat(_SunIntensity, sunIntensity);
            skyboxMaterial.SetFloat(_SunGlow, sunGlow);
            
            skyboxMaterial.SetColor(_CloudColor, cloudColor);
            skyboxMaterial.SetFloat(_CloudDensity, cloudDensity);
            skyboxMaterial.SetFloat(_CloudSpeed, cloudSpeed);
            
            skyboxMaterial.SetFloat(_StarsIntensity, starsIntensity);
            skyboxMaterial.SetFloat(_StarsSpeed, starsSpeed);
            
            // Custom time for animation
            skyboxMaterial.SetFloat(_Time, Time.time);

            // Update directional light if exists
            UpdateDirectionalLight(sunDir, sunColor, sunIntensity);
        }

        private void UpdateDirectionalLight(Vector3 sunDir, Color sunColor, float intensity)
        {
            // Find main directional light and sync with skybox
            Light mainLight = RenderSettings.sun;
            if (mainLight != null)
            {
                mainLight.transform.forward = -sunDir;
                mainLight.color = sunColor;
                mainLight.intensity = intensity;
            }
        }

        private void UpdateFog()
        {
            RenderSettings.fog = enableFog;
            if (enableFog)
            {
                // Blend fog color with mood
                Color currentFogColor = Color.Lerp(
                    new Color(moodA_HorizonColor.r * 0.5f, moodA_HorizonColor.g * 0.3f, moodA_HorizonColor.b * 0.3f),
                    new Color(moodB_HorizonColor.r * 0.5f, moodB_HorizonColor.g * 0.5f, moodB_HorizonColor.b * 0.5f),
                    moodBlend
                );
                
                RenderSettings.fogColor = currentFogColor;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = fogDensity;
            }
        }

        #region Public API

        /// <summary>
        /// Instantly set the mood blend value.
        /// 0 = Blood Red, 1 = Dark Night
        /// </summary>
        public void SetMood(float blend)
        {
            moodBlend = Mathf.Clamp01(blend);
            isTransitioning = false;
        }

        /// <summary>
        /// Transition to blood red sky (Mood A).
        /// </summary>
        public void TransitionToBloodSky(float duration = -1f, Action onComplete = null)
        {
            TransitionToMood(0f, duration > 0 ? duration : defaultTransitionDuration, onComplete);
        }

        /// <summary>
        /// Transition to dark night sky (Mood B).
        /// </summary>
        public void TransitionToNightSky(float duration = -1f, Action onComplete = null)
        {
            TransitionToMood(1f, duration > 0 ? duration : defaultTransitionDuration, onComplete);
        }

        /// <summary>
        /// Transition to a specific mood blend value.
        /// </summary>
        public void TransitionToMood(float targetMood, float duration, Action onComplete = null)
        {
            transitionStartValue = moodBlend;
            transitionEndValue = Mathf.Clamp01(targetMood);
            transitionDuration = duration;
            transitionElapsed = 0f;
            onTransitionComplete = onComplete;
            isTransitioning = true;

            Debug.Log($"[Skybox] Starting transition from {transitionStartValue:F2} to {transitionEndValue:F2} over {duration}s");
        }

        /// <summary>
        /// Set sun/moon position.
        /// </summary>
        public void SetSunPosition(float height, float rotation)
        {
            sunHeight = Mathf.Clamp(height, -1f, 1f);
            sunRotation = rotation % 360f;
        }

        /// <summary>
        /// Set stars intensity (0 = no stars, 1 = full stars).
        /// </summary>
        public void SetStarsIntensity(float intensity)
        {
            starsIntensity = Mathf.Clamp01(intensity);
        }

        /// <summary>
        /// Current mood blend value.
        /// </summary>
        public float CurrentMood => moodBlend;

        /// <summary>
        /// Whether a transition is currently in progress.
        /// </summary>
        public bool IsTransitioning => isTransitioning;

        #endregion

        #region Presets

        [ContextMenu("Apply Preset: Blood Sky (Start)")]
        public void ApplyPresetBloodSky()
        {
            SetMood(0f);
            sunHeight = 0.05f;
            sunSize = 0.15f;
            sunGlow = 0.7f;
            starsIntensity = 0f;
            fogDensity = 0.03f;
        }

        [ContextMenu("Apply Preset: Dark Night")]
        public void ApplyPresetDarkNight()
        {
            SetMood(1f);
            sunHeight = 0.3f;
            sunSize = 0.05f;
            sunGlow = 0.3f;
            starsIntensity = 0.5f;
            fogDensity = 0.015f;
        }

        [ContextMenu("Apply Preset: Twilight Horror")]
        public void ApplyPresetTwilightHorror()
        {
            SetMood(0.4f);
            sunHeight = -0.1f;
            sunSize = 0.2f;
            sunGlow = 0.8f;
            starsIntensity = 0.1f;
            fogDensity = 0.025f;
        }

        #endregion
    }
}

