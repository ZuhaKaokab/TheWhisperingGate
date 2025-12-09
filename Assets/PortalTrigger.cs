using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; // Main Menu scene ka exact name

    private void OnTriggerEnter(Collider other)
    {
        // Check karo ke player portal me enter kar raha hai
        if (other.CompareTag("Player"))
        {
            // Load Main Menu scene
            SceneManager.LoadScene(0);
        }
    }
}