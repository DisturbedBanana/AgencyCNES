using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ttt : MonoBehaviour
{
    [SerializeField, EnumFlags] private GameState.GAMESTATES stateToGo;


    [Button]
    public void GoToState()
    {
        GetComponent<GameState>().GoToState(stateToGo);
    }

    [Button]
    public void RegardeCaMarche()
    {
        GetComponent<GameState>().NextStateForce();
    }
}
