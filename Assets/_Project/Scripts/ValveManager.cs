using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ValveManager : NetworkBehaviour
{
    public static ValveManager instance;

    [SerializeField] private ValvePuzzlePart[] _valvePuzzleParts;
    bool _isSolved = false;
    
    public bool IsSolved { get { return _isSolved; } set { _isSolved = value; } }

    [Header("Events")]
    public UnityEvent OnComplete;
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
        OnComplete?.Invoke();
        Debug.LogError("Valve puzzle solved!");
        GameState.Instance.ChangeState(GameState.GAMESTATES.FUSES);
    }
}
