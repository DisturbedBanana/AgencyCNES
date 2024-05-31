using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ttt : MonoBehaviour
{
    [Button]
    public void RegardeCaMarche()
    {
        GetComponent<GameState>().NextStateForce();
    }
}
