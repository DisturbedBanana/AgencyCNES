using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PasswordManager : MonoBehaviour
{
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
    List<PASSWORDKEYS> _currentPassword = new List<PASSWORDKEYS>();

    [SerializeField] Light _light;

    public void AddKey(PASSWORDKEYS key)
    {
        _currentPassword.Add(key);
        
        if (_currentPassword.Count == _correctPassword.Count)
        {
            for (int i = 0; i < _correctPassword.Count; i++)
            {
                if (_correctPassword[i] != _currentPassword[i])
                {
                    _currentPassword.Clear();
                    //Wrong password
                    break;
                }
            }

            //Correct Password
        }
    }

    public void WrongPasswordVisualFeedback()
    {
        StartCoroutine(FlashingLightCoroutine());
    }

    private IEnumerator FlashingLightCoroutine(bool success = false)
    {
        _light.color = new Color(0, 0, 0);
        float t = 0;
        
        for (float i = 0; i < 255; i++)
        {
            t = i / 255;
            _light.color = Color.Lerp(_light.color, Color.red, t);
            yield return new WaitForSeconds(0.01f);
        }

        t = 0;
        yield return new WaitForSeconds(2);

        for (float i = 0; i < 255; i++)
        {
            t = i / 255;
            _light.color = Color.Lerp(_light.color, Color.white, t);
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }
}
