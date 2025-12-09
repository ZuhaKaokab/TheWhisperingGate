using UnityEngine;
using UnityEngine.Video;

public class VideoHideExample : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoScreen; // Raw Image ya Quad jahan video render ho raha hai

    void Start()
    {
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Video khatam hone pe screen hide kar do
        videoScreen.SetActive(false);
    }
}