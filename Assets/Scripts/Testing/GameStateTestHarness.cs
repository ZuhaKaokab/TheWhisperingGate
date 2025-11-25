using UnityEngine;
using WhisperingGate.Core;

namespace WhisperingGate.Testing
{
    /// <summary>
    /// Utility MonoBehaviour that lets you poke GameState by pressing hotkeys.
    /// Attach this to any scene object while testing to verify variable storage and condition evaluation.
    /// </summary>
    public class GameStateTestHarness : MonoBehaviour
    {
        [Header("Int Variable Test")]
        [SerializeField] private string intVariableName = "courage";
        [SerializeField] private int intDelta = 10;
        [SerializeField] private KeyCode addIntKey = KeyCode.Alpha1;

        [Header("Bool Variable Test")]
        [SerializeField] private string boolVariableName = "journal_found";
        [SerializeField] private KeyCode toggleBoolKey = KeyCode.Alpha2;

        [Header("Condition Test")]
        [SerializeField] private string conditionToEvaluate = "courage >= 10";
        [SerializeField] private KeyCode evaluateConditionKey = KeyCode.Alpha3;

        private void Update()
        {
            var state = GameState.Instance;
            if (state == null)
                return;

            if (Input.GetKeyDown(addIntKey))
            {
                state.AddInt(intVariableName, intDelta);
                Debug.Log($"[GameStateTest] {intVariableName} changed by {intDelta}. New value = {state.GetInt(intVariableName)}");
            }

            if (Input.GetKeyDown(toggleBoolKey))
            {
                state.ToggleBool(boolVariableName);
                Debug.Log($"[GameStateTest] {boolVariableName} toggled. New value = {state.GetBool(boolVariableName)}");
            }

            if (Input.GetKeyDown(evaluateConditionKey))
            {
                bool result = state.EvaluateCondition(conditionToEvaluate);
                Debug.Log($"[GameStateTest] Condition '{conditionToEvaluate}' evaluated to {result}");
            }
        }
    }
}

