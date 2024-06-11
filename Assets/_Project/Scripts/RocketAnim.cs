using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class RocketAnim : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Animator>().enabled = false ;
    }

    [Button("Open")]
    public void TestOpenDoors()
    {
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play("OpenDoorsAnim");
    }

    [Button("Close")]
    public void TestCloseDoors()
    {
        GetComponent<Animator>().Play("CloseDoorsAnim");
    }
}
