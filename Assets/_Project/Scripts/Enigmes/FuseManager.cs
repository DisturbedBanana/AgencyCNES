using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class FuseManager : NetworkBehaviour, IGameState
{
    public static FuseManager Instance;

    [Header("Color Materials")]
    [SerializeField] Material _greenMaterial;
    [SerializeField] Material _yellowMaterial;
    [SerializeField] Material _redMaterial;
    [Space()]
    [Header("Other Parameters")]
    [SerializeField] List<FuseLight> _fuseLightList = new List<FuseLight>();
    [SerializeField] List<GameObject> _lightsOnFuseBox = new List<GameObject>();
    private int _currentGreenFusesActivated;
    private int _currentConnectedFuses;
    private bool _areFusesSolved = false;

    [Header("Voices")]
    [Expandable]
    [SerializeField] private VoiceAI _voicesAI;
    private List<VoiceData> _voicesHint => _voicesAI.GetAllHintVoices();
    private NetworkVariable<int> _currentHintIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Events")]
    public UnityEvent OnStateStart;
    public UnityEvent OnStateComplete;

    #region PROPERTIES
    public Material GreenMat
    {
        get { return _greenMaterial; }
    }

    public Material YellowMat
    {
        get { return _yellowMaterial; }
    }

    public Material RedMat
    {
        get { return _redMaterial; }
    }
    #endregion

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        foreach (GameObject item in _lightsOnFuseBox)
        {
            item.SetActive(false);
        }
    }
    public void StartState()
    {
        OnStateStart?.Invoke();
        SoundManager.Instance.PlayVoices(gameObject, _voicesAI.GetAllStartVoices());
        StartCoroutine(StartHintCountdown());
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].ActivateLight();
        Debug.LogError("Activated light number: " + ID);
        _currentConnectedFuses++;
        UpdateFuseBoxLights(true);

        if (_fuseLightList[ID].FuseLightColor != FuseLight.AvailableColors.Green)
            return;

        _currentGreenFusesActivated++;
    }

    [Rpc(SendTo.Everyone)]
    public void DeactivateFuseLightRpc(int ID)
    {
        _fuseLightList[ID].DeactivateLight();
        UpdateFuseBoxLights(false);
        _currentConnectedFuses--;

        if (_fuseLightList[ID].FuseLightColor != FuseLight.AvailableColors.Green)
            return;

        if(_currentGreenFusesActivated > 0)
        {
            _currentGreenFusesActivated--;
        }
    }

    private void UpdateFuseBoxLights(bool shouldAddLight)
    {
        if (shouldAddLight)
            _lightsOnFuseBox[_currentConnectedFuses - 1].SetActive(true);
        else
            _lightsOnFuseBox[_currentConnectedFuses - 1].SetActive(false);
    }

    public void ValidatePuzzle()
    {
        if (!AreFusesSolved())
            return;

        OnStateCompleteClientRpc();

        GameState.Instance.ChangeState(GameState.GAMESTATES.SEPARATION);
    }

    private bool AreFusesSolved()
    {
        return _currentGreenFusesActivated >= 4;
    }

    [Rpc(SendTo.Everyone)]
    public void OnStateCompleteClientRpc()
    {
        OnStateComplete?.Invoke();
        StopCoroutine(StartHintCountdown());
    }
    public IEnumerator StartHintCountdown()
    {
        if (_voicesHint.Count == 0)
            yield break;

        for (int i = 0; i < _voicesHint[_currentHintIndex.Value].numberOfRepeat + 1; i++)
        {
            float waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
            int waitingHintIndex = _currentHintIndex.Value;

            while (waitBeforeHint > 0)
            {
                if (waitingHintIndex != _currentHintIndex.Value)
                {
                    if (_currentHintIndex.Value > _voicesHint.Count - 1)
                    {
                        yield break;
                    }
                    waitingHintIndex = _currentHintIndex.Value;
                    waitBeforeHint = _voicesHint[_currentHintIndex.Value].delayedTime;
                }

                waitBeforeHint -= Time.deltaTime;
                yield return null;
            }
            SoundManager.Instance.PlaySound(gameObject, _voicesHint[_currentHintIndex.Value].audio);
        }


        if (_currentHintIndex.Value < _voicesHint.Count - 1)
        {
            ChangeHintIndexServerRpc(_currentHintIndex.Value + 1);
            StartCoroutine(StartHintCountdown());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHintIndexServerRpc(int value)
    {
        _currentHintIndex.Value = value;
    }
}
