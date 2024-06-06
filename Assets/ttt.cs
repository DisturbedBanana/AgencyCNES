using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.XR.Content.Interaction;

public class ttt : MonoBehaviour
{
    public GameState.GAMESTATES stateToForce;

    [Button]
    public void RegardeCaMarche()
    {
        GetComponent<XRGripButton>().onPress.Invoke();
    }

    [Button]
    public void ForceGameState()
    {
        GameState.Instance.StateForce(stateToForce);
    }
}
