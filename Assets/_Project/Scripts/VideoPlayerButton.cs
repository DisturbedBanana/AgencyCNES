using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode;

public class VideoPlayerButton : NetworkBehaviour
{
    private DateTime _pressedTime;
    private DateTime _stockedTime;

    public DateTime Stockedtime
    {
        get { return _stockedTime; }
        set { _stockedTime = value; }
    }

    [SerializeField] VideoPlayerButton _otherButton;
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
        if (GameState.instance.CurrentGameState == GameState.GAMESTATES.LAUNCH)
        {
            _videoObject.SetActive(true);
            _video.Play();
        }
    }

    public void ButtonPress() 
    {
        _pressedTime = DateTime.Now;
        
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void NetworkButtonPressedRpc(DateTime date)
    {
        if (_otherButton.Stockedtime != null)
        {
            _otherButton.Stockedtime = date;
        }
        else
        {
            _otherButton.Stockedtime = date;
        }

        Stockedtime = date;
    }
}
