using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PasswordPuzzleManager;

public class PasswordButton : MonoBehaviour
{
    [SerializeField] PasswordPuzzleManager _manager;
    [SerializeField] PASSWORDKEYS _key;

    public void OnClick()
    {
        _manager.AddKey(_key);
    }
}
