using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PasswordManager;

public class PasswordButton : MonoBehaviour
{
    [SerializeField] PasswordManager _manager;
    [SerializeField] PASSWORDKEYS _key;

    public void OnClick()
    {
        _manager.AddKey(_key);
    }
}
