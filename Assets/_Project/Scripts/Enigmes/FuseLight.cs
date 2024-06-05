using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseLight : MonoBehaviour
{
    [SerializeField] Color _lightColor;
    private Color _oldColor;
    MeshRenderer _mr;

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
    }

    public void ActivateLight()
    {
        _oldColor = _mr.material.color;
        _mr.material.color = _lightColor;
    }

    public void DeactivateLight()
    {
        _mr.material.color = _oldColor;
    }
}
