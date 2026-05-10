using UnityEngine;

public class TriggerBabaJee : MonoBehaviour
{
    public BabaJeeAI babaScript; // Inspector mein Baba Jee ko yahan drag karein

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            babaScript.StartWalking();
            // Ek baar trigger hone ke baad collider band kar dein taake baar baar na ho
            gameObject.SetActive(false);
        }
    }
}