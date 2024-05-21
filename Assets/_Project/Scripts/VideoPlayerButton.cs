using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerButton : MonoBehaviour
{

    [SerializeField] GameObject _videoObject;
    VideoPlayer _video;

    private void Awake()
    {
        _video = _videoObject.GetComponent<VideoPlayer>();
        _video.Stop();
        _videoObject.SetActive(false);
    }

    public void PlayVideo()
    {
        if (GameState.instance.StateIndex == 1)
        {
            _videoObject.SetActive(true);
            _video.Play();
            GameState.instance.NextState();
        }
    }
}
