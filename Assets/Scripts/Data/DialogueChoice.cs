using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Dialogue
{
    /// <summary>
    /// Serializable class representing a player choice option in a dialogue node.
    /// Contains choice text, next node reference, impacts, and conditional visibility.
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        [SerializeField] private string choiceText;
        [SerializeField] private DialogueNode nextNode;
        [SerializeField] private List<ChoiceImpact> impacts = new();
        [SerializeField] private bool hasCondition = false;
        [SerializeField] private string showCondition = "";
        
        public string ChoiceText => choiceText;
        public DialogueNode NextNode => nextNode;
        public List<ChoiceImpact> Impacts => impacts;
        public bool HasCondition => hasCondition;
        public string ShowCondition => showCondition;
    }
    
    /// <summary>
    /// Serializable class representing a variable impact from a dialogue choice.
    /// Modifies game state variables when the choice is selected.
    /// </summary>
    [System.Serializable]
    public class ChoiceImpact
    {
        [SerializeField] private string variableName;
        [SerializeField] private int valueChange;
        [SerializeField] private bool isConditional = false;
        [SerializeField] private string applyCondition = "";
        
        public string VariableName => variableName;
        public int ValueChange => valueChange;
        public bool IsConditional => isConditional;
        public string ApplyCondition => applyCondition;
    }
}

