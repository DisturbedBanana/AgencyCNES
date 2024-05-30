using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ValveManager : NetworkBehaviour
{
    public static ValveManager instance;

    [SerializeField] private ValvePuzzlePart[] _valvePuzzleParts;
    bool _isSolved = false;
    
    public bool IsSolved { get { return _isSolved; } set { _isSolved = value; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void CheckValves()
    {
        foreach (ValvePuzzlePart valvePuzzlePart in _valvePuzzleParts)
        {
            if (!valvePuzzlePart.IsSolved)
            {
                _isSolved = false;
                return;
            }
        }

        _isSolved = true;
        GameState.instance.ChangeState(GameState.GAMESTATES.LAUNCH);
        Debug.LogError("Valve puzzle solved!");
    }
}
