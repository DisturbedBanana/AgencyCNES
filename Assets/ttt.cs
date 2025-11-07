using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ttt : MonoBehaviour
{
    [SerializeField] private GameState.GAMESTATES stateToGo;


    [Button]
    public void GoToState()
    {
        GameState.Instance.NextStateForce(stateToGo);
    }

    [Button]
    public void RegardeCaMarche()
    {
        GetComponent<GameState>().NextStateForce();
    }
}
