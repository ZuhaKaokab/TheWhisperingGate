using UnityEngine;

namespace WhisperingGate.Dialogue
{
    /// <summary>
    /// ScriptableObject asset representing a character in the dialogue system.
    /// Contains character metadata: ID, display name, portrait, theme audio, and description.
    /// </summary>
    [CreateAssetMenu(menuName = "Whispering Gate/Character Data", fileName = "Character_")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private AudioClip characterTheme;
        [TextArea(2, 4)]
        [SerializeField] private string characterDescription;
        
        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public Sprite PortraitSprite => portraitSprite;
        public AudioClip CharacterTheme => characterTheme;
        public string Description => characterDescription;
    }
}

