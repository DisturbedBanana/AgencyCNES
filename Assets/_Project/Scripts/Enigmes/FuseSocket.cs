using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class FuseSocket : MonoBehaviour
{
    [SerializeField] int _ID;

    [Button]
    public void AttachFuse()
    {
        if (!isCorrectGameState())
            return;

        FuseManager.Instance.ActivateFuseLightRpc(_ID);
    }

    [Button]
    public void DetachFuse()
    {
        if (!isCorrectGameState())
            return;

        FuseManager.Instance.DeactivateFuseLightRpc(_ID);
    }

    private bool isCorrectGameState()
    {
        return GameState.Instance.CurrentGameState == GameState.GAMESTATES.FUSES;
    }
}
