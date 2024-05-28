using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValveManager : MonoBehaviour
{
    public static ValveManager instance;

    [SerializeField] private ValvePuzzlePart[] _valvePuzzleParts;
    bool isSolved = true;
    

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
                isSolved = false;
                break;
            }
        }

        if (isSolved)
        {
            Debug.Log("Valve puzzle solved!");
        }
    }
}
