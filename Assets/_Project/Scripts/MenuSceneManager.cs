using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class MenuSceneManager : MonoBehaviour
{
    [SerializeField] List<string> _scenesList = new List<string>();

    [Button]
    private void PopulateSceneList()
    {
        {
            _scenesList.Clear();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = scenePath.Substring(scenePath.LastIndexOf('/') + 1, scenePath.LastIndexOf('.') - scenePath.LastIndexOf('/') - 1);
                _scenesList.Add(sceneName);
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_scenesList.Contains(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
