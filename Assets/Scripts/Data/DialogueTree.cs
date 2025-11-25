using UnityEngine;

namespace WhisperingGate.Dialogue
{
    /// <summary>
    /// ScriptableObject asset representing a complete dialogue tree.
    /// Contains the entry point (start node) and tree-level settings.
    /// </summary>
    [CreateAssetMenu(menuName = "Whispering Gate/Dialogue Tree", fileName = "Tree_")]
    public class DialogueTree : ScriptableObject
    {
        [Header("Tree Identity")]
        [SerializeField] private string treeId;
        [SerializeField] private string treeTitle;
        
        [Header("Entry Point")]
        [SerializeField] private DialogueNode startNode;
        
        [Header("Tree Settings")]
        [SerializeField] private float defaultTypewriterSpeed = 0.05f;
        [SerializeField] private bool autoAdvanceIfSingleChoice = false;
        
        public string TreeId => treeId;
        public string TreeTitle => treeTitle;
        public DialogueNode StartNode => startNode;
        public float TypewriterSpeed => defaultTypewriterSpeed;
        public bool AutoAdvanceIfSingleChoice => autoAdvanceIfSingleChoice;
    }
}

