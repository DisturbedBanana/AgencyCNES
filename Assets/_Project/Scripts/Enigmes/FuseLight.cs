using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseLight : MonoBehaviour
{
    [SerializeField] AvailableColors _colorToChangeTo;

    private Material _targetMaterial;
    private Material _oldMaterial;
    MeshRenderer _mr;

    public enum AvailableColors
    {
        Green,
        Yellow,
        Red
    }

    /*Material Leak DO NOT UNCOMMENT*/
    //private void OnValidate()
    //{
    //    _mr = GetComponent<MeshRenderer>();

    //    switch (_colorToChangeTo)
    //    {
    //        case AvailableColors.Green:
    //            _mr.material.color = Color.green;
    //            break;
    //        case AvailableColors.Yellow:
    //            _mr.material.color = Color.yellow;
    //            break;
    //        case AvailableColors.Red:
    //            _mr.material.color = Color.red;
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public AvailableColors FuseLightColor
    {
        get { return _colorToChangeTo; }
    }

    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        _mr.material.color = Color.white;
    }

    private void Start()
    {
        switch (_colorToChangeTo)
        {
            case AvailableColors.Green:
                _targetMaterial = FuseManager.Instance.GreenMat;
                break;
            case AvailableColors.Yellow:
                _targetMaterial = FuseManager.Instance.YellowMat;
                break;
            case AvailableColors.Red:
                _targetMaterial = FuseManager.Instance.RedMat;
                break;
            default:
                break;
        }
    }

    public void ActivateLight()
    {
        _oldMaterial = _mr.material;
        _mr.material = _targetMaterial;
    }

    public void DeactivateLight()
    {
        _mr.material = _oldMaterial;
    }
}
