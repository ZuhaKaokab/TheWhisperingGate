using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhisperingGate.Core
{
    /// <summary>
    /// Centralized variable store that tracks narrative state (ints, bools, floats, strings)
    /// and evaluates simple string conditions for dialogue/command usage.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        [Serializable]
        private struct IntVariableSeed
        {
            public string key;
            public int value;
        }

        [Serializable]
        private struct BoolVariableSeed
        {
            public string key;
            public bool value;
        }

        [Serializable]
        private struct FloatVariableSeed
        {
            public string key;
            public float value;
        }

        [Serializable]
        private struct StringVariableSeed
        {
            public string key;
            public string value;
        }

        public static GameState Instance { get; private set; }

        [Header("Default Values (overridable in Inspector)")]
        [SerializeField] private List<IntVariableSeed> defaultIntVariables = new()
        {
            new IntVariableSeed { key = "courage", value = 0 },
            new IntVariableSeed { key = "trust_alina", value = 0 },
            new IntVariableSeed { key = "trust_writer", value = 0 },
            new IntVariableSeed { key = "sanity", value = 50 },
            new IntVariableSeed { key = "investigation", value = 0 }
        };

        [SerializeField] private List<BoolVariableSeed> defaultBoolVariables = new()
        {
            new BoolVariableSeed { key = "journal_found", value = false },
            new BoolVariableSeed { key = "saw_dolls", value = false },
            new BoolVariableSeed { key = "heard_scream", value = false },
            new BoolVariableSeed { key = "met_writer", value = false },
            new BoolVariableSeed { key = "portal_discovered", value = false }
        };

        [SerializeField] private List<FloatVariableSeed> defaultFloatVariables = new();
        [SerializeField] private List<StringVariableSeed> defaultStringVariables = new();

        private readonly Dictionary<string, int> intVariables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> boolVariables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> floatVariables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> stringVariables = new(StringComparer.OrdinalIgnoreCase);

        public event Action<string, int> OnIntChanged;
        public event Action<string, bool> OnBoolChanged;
        public event Action<string, float> OnFloatChanged;
        public event Action<string, string> OnStringChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaults();
        }

        #region Initialization

        private void InitializeDefaults()
        {
            intVariables.Clear();
            boolVariables.Clear();
            floatVariables.Clear();
            stringVariables.Clear();

            foreach (var seed in defaultIntVariables)
            {
                if (string.IsNullOrWhiteSpace(seed.key)) continue;
                SetInt(seed.key, seed.value, false);
            }

            foreach (var seed in defaultBoolVariables)
            {
                if (string.IsNullOrWhiteSpace(seed.key)) continue;
                SetBool(seed.key, seed.value, false);
            }

            foreach (var seed in defaultFloatVariables)
            {
                if (string.IsNullOrWhiteSpace(seed.key)) continue;
                SetFloat(seed.key, seed.value, false);
            }

            foreach (var seed in defaultStringVariables)
            {
                if (string.IsNullOrWhiteSpace(seed.key)) continue;
                SetString(seed.key, seed.value, false);
            }
        }

        #endregion

        #region Int Variables

        public void SetInt(string key, int value) => SetInt(key, value, true);

        private void SetInt(string key, int value, bool raiseEvent)
        {
            if (!ValidateKey(key)) return;

            if (key.Equals("sanity", StringComparison.OrdinalIgnoreCase))
            {
                value = Mathf.Clamp(value, 0, 100);
            }

            intVariables[key] = value;
            if (raiseEvent)
            {
                OnIntChanged?.Invoke(key, value);
                Debug.Log($"[GameState] {key} = {value}");
            }
        }

        public int GetInt(string key)
        {
            return ValidateKey(key) && intVariables.TryGetValue(key, out var value) ? value : 0;
        }

        public bool TryGetInt(string key, out int value)
        {
            value = 0;
            if (!ValidateKey(key)) return false;
            return intVariables.TryGetValue(key, out value);
        }

        public void AddInt(string key, int delta)
        {
            int current = GetInt(key);
            SetInt(key, current + delta);
        }

        #endregion

        #region Bool Variables

        public void SetBool(string key, bool value) => SetBool(key, value, true);

        private void SetBool(string key, bool value, bool raiseEvent)
        {
            if (!ValidateKey(key)) return;
            boolVariables[key] = value;
            if (raiseEvent)
            {
                OnBoolChanged?.Invoke(key, value);
                Debug.Log($"[GameState] {key} = {value}");
            }
        }

        public bool GetBool(string key)
        {
            return ValidateKey(key) && boolVariables.TryGetValue(key, out var value) && value;
        }

        public void ToggleBool(string key)
        {
            SetBool(key, !GetBool(key));
        }

        #endregion

        #region Float Variables

        public void SetFloat(string key, float value) => SetFloat(key, value, true);

        private void SetFloat(string key, float value, bool raiseEvent)
        {
            if (!ValidateKey(key)) return;
            floatVariables[key] = value;
            if (raiseEvent)
            {
                OnFloatChanged?.Invoke(key, value);
                Debug.Log($"[GameState] {key} = {value}");
            }
        }

        public float GetFloat(string key)
        {
            return ValidateKey(key) && floatVariables.TryGetValue(key, out var value) ? value : 0f;
        }

        #endregion

        #region String Variables

        public void SetString(string key, string value) => SetString(key, value, true);

        private void SetString(string key, string value, bool raiseEvent)
        {
            if (!ValidateKey(key)) return;
            stringVariables[key] = value ?? string.Empty;
            if (raiseEvent)
            {
                OnStringChanged?.Invoke(key, stringVariables[key]);
                Debug.Log($"[GameState] {key} = {stringVariables[key]}");
            }
        }

        public string GetString(string key)
        {
            return ValidateKey(key) && stringVariables.TryGetValue(key, out var value) ? value : string.Empty;
        }

        #endregion

        #region Condition Evaluation

        /// <summary>
        /// Evaluates simple expressions such as "courage >= 30" or "journal_found".
        /// Supports operators: >=, <=, >, <, ==, != on int/bool variables.
        /// </summary>
        public bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            condition = condition.Trim();
            try
            {
                if (TryParseComparison(condition, ">=", out var left, out var right))
                    return GetInt(left) >= right;

                if (TryParseComparison(condition, "<=", out left, out right))
                    return GetInt(left) <= right;

                if (TryParseComparison(condition, ">", out left, out right))
                    return GetInt(left) > right;

                if (TryParseComparison(condition, "<", out left, out right))
                    return GetInt(left) < right;

                if (TryParseEquality(condition, "==", out var eqLeft, out var eqRight))
                    return EvaluateEquality(eqLeft, eqRight);

                if (TryParseEquality(condition, "!=", out var neqLeft, out var neqRight))
                    return !EvaluateEquality(neqLeft, neqRight);

                // Fallback: treat as boolean flag name.
                return GetBool(condition);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameState] Failed to evaluate condition '{condition}': {ex.Message}");
                return false;
            }
        }

        private static bool TryParseComparison(string condition, string token, out string variable, out int value)
        {
            variable = string.Empty;
            value = 0;
            int index = condition.IndexOf(token, StringComparison.Ordinal);
            if (index <= 0) return false;

            variable = condition[..index].Trim();
            string rhs = condition[(index + token.Length)..].Trim();
            return !string.IsNullOrEmpty(variable) && int.TryParse(rhs, out value);
        }

        private static bool TryParseEquality(string condition, string token, out string left, out string right)
        {
            left = string.Empty;
            right = string.Empty;
            int index = condition.IndexOf(token, StringComparison.Ordinal);
            if (index <= 0) return false;

            left = condition[..index].Trim();
            right = condition[(index + token.Length)..].Trim();
            return !string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right);
        }

        private bool EvaluateEquality(string left, string right)
        {
            if (bool.TryParse(right, out var boolTarget))
            {
                return GetBool(left) == boolTarget;
            }

            if (int.TryParse(right, out var intTarget))
            {
                return GetInt(left) == intTarget;
            }

            return string.Equals(GetString(left), right, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Utility

        public void ResetAllVariables()
        {
            InitializeDefaults();
        }

        private static bool ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("[GameState] Attempted to use an empty key.");
                return false;
            }

            return true;
        }

        #endregion
    }
}

