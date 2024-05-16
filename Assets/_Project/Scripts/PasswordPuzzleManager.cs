using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PasswordPuzzleManager : MonoBehaviour
{
    bool _isCoroutineRunning = false;
    int _ignoreKeysAmount = 0;

    public enum PASSWORDKEYS
    {
        PASSWORDKEY1,
        PASSWORDKEY2,
        PASSWORDKEY3,
        PASSWORDKEY4,
        PASSWORDKEY5,
        PASSWORDKEY6,
        PASSWORDKEY7,
        PASSWORDKEY8,
        PASSWORDKEY9,
        PASSWORDKEY10,
        PASSWORDKEY11,
        PASSWORDKEY12
    }

    [SerializeField] List<PASSWORDKEYS> _correctPassword = new List<PASSWORDKEYS>();
    public List<PASSWORDKEYS> _currentPassword = new List<PASSWORDKEYS>();

    [SerializeField] Light _light;
    [SerializeField] GameObject _keyboardToActivate;
    [SerializeField] GameObject _keyboardToDeactivate;

    public void AddKey(PASSWORDKEYS key)
    {
        if (_isCoroutineRunning)
            return;


        if (_ignoreKeysAmount > 0)
        {
            _ignoreKeysAmount--;
            return;
        }
        
        _currentPassword.Add(key);
        Debug.Log(key);
        
        if (_currentPassword.Count == _correctPassword.Count)
        {
            for (int i = 0; i < _correctPassword.Count; i++)
            {
                if (_correctPassword[i] != _currentPassword[i])
                {
                    //Wrong password
                    _currentPassword.Clear();
                    StartCoroutine(FlashingLightCoroutine(false));
                    return;
                }
            }
            //Correct Password
            _currentPassword.Clear();
            StartCoroutine(FlashingLightCoroutine(true));
        }
    }

    public void WrongPasswordVisualFeedback(bool success)
    {
        StartCoroutine(FlashingLightCoroutine(success));
    }

    private IEnumerator FlashingLightCoroutine(bool success = false)
    {
        _isCoroutineRunning = true;
        Color targetColor;
        float t = 0;
        
        switch (success)
        {
            case true:
                targetColor = Color.green;
                break;
            case false:
                targetColor = Color.red;
                break;
        }

        
        for (float i = 0; i < 255; i++)
        {
            t = i / 255;
            _light.color = Color.Lerp(_light.color, targetColor, t);
            yield return new WaitForSeconds(0.02f);
        }

        t = 0;
        yield return new WaitForSeconds(0.5f);

        for (float i = 0; i < 255; i++)
        {
            t = i / 255;
            _light.color = Color.Lerp(_light.color, Color.white, t);
            yield return new WaitForSeconds(0.02f);
        }
        
        _isCoroutineRunning = false;
        yield return null;
    }

    public void CallKeyboardActivated(GameObject anchor)
    {
        anchor.SetActive(false);
        _keyboardToActivate.SetActive(true);
        _keyboardToDeactivate.GetComponentInChildren<MeshRenderer>().enabled = false;
    }
}
