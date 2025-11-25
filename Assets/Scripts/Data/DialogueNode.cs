using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Dialogue
{
    /// <summary>
    /// ScriptableObject asset representing a single dialogue node in the conversation tree.
    /// Contains speaker info, dialogue text, choices, commands, and navigation logic.
    /// </summary>
    [CreateAssetMenu(menuName = "Whispering Gate/Dialogue Node", fileName = "Node_")]
    public class DialogueNode : ScriptableObject
    {
        [Header("Node Identity")]
        [SerializeField] private string nodeId;
        
        [Header("Speaker & Content")]
        [SerializeField] private CharacterData speaker;
        [TextArea(3, 5)]
        [SerializeField] private string lineText;
        [SerializeField] private AudioClip voiceClip;
        [SerializeField] private float voiceDelay = 0f;
        
        [Header("Navigation")]
        [SerializeField] private List<DialogueChoice> choices = new();
        [SerializeField] private DialogueNode nextNodeIfAuto;
        
        [Header("Commands")]
        [SerializeField] private List<string> startCommands = new();
        [SerializeField] private List<string> endCommands = new();
        
        [Header("Node Settings")]
        [SerializeField] private bool isEndNode = false;
        [SerializeField] private float displayDuration = 0f;
        
        public string NodeId => nodeId;
        public CharacterData Speaker => speaker;
        public string LineText => lineText;
        public AudioClip VoiceClip => voiceClip;
        public float VoiceDelay => voiceDelay;
        public List<DialogueChoice> Choices => choices;
        public DialogueNode NextNodeIfAuto => nextNodeIfAuto;
        public List<string> StartCommands => startCommands;
        public List<string> EndCommands => endCommands;
        public bool IsEndNode => isEndNode;
        public float DisplayDuration => displayDuration;
    }
}

