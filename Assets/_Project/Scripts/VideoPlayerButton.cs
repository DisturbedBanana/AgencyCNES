using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode;

public class VideoPlayerButton : NetworkBehaviour
{
    [SerializeField] GameState _gs;
    
    private DateTime _pressedTime;
    private DateTime _stockedTime;

    bool _canLaunch = true;
    
    #region PROPERTIES
    public bool CanLaunch 
    {
        get { return _canLaunch; }
        set { _canLaunch = value; }
    }

    public DateTime Stockedtime
    {
        get { return _stockedTime; }
        set { _stockedTime = value; }
    }
    #endregion

    public void ButtonPress() 
    {
        if (_canLaunch)
        {
            GameState.instance.CheckLaunchButtonTimingRpc(DateTime.Now, NetworkManager.Singleton.LocalClientId);
            Debug.LogError("Clicked button" + NetworkManager.Singleton.LocalClientId);
        }
    }
}
