using UnityEngine;
using UnityEngine.Playables; // Timeline ke liye zaroori hai
using UnityEngine.SceneManagement;

public class TimelineSceneChanger : MonoBehaviour
{
    public PlayableDirector director; // Timeline wala object yahan drag karen
    public string nextSceneName = "GameplayScene2";

    void Update()
    {
        // Agar timeline chal rahi thi aur ab ruk gayi hai
        if (director.state != PlayState.Playing)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}