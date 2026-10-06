using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class WebGLAutoPlayer : MonoBehaviour
{
    [SerializeField] private string videoFileName = "intro.mp4";

    private VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // Set source to URL
        videoPlayer.source = VideoSource.Url;

        // Path.Combine handles local editor and WebGL web root properly
        string fullPath = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = fullPath;

        // Mute all audio tracks completely to satisfy browser autoplay policies
        for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
        {
            videoPlayer.SetDirectAudioMute(i, true);
        }

        // Subscribe to preparation events
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived += OnError;

        // Start preparing the video stream
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer source)
    {
        // Plays immediately without waiting for a click or keypress
        source.Play();
    }

    private void OnError(VideoPlayer source, string message)
    {
        Debug.LogError($"[WebGL Video Error] {message}");
    }
}