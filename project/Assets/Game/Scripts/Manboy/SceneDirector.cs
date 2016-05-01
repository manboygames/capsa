using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using AdvancedInspector;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneDirector : MonoBehaviour {

    [Inspect(0),ReadOnly]
    public List<string> scenesList;

    [Inspect(0),Group("Load Scene"),Restrict("ValidSceneNames",RestrictDisplay.DropDown)]
    public string sceneToLoad;
    [Inspect(1), Group("Load Scene")]
    public bool async;
    [Inspect(2), Group("Load Scene")]
    public LoadSceneMode loadSceneMode;

    private IList ValidSceneNames()
    {
        return scenesList;
    }

#if UNITY_EDITOR

    [Inspect(2)]
    public void UpdateScenes()
    {
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

        scenesList.Clear();

        foreach (EditorBuildSettingsScene scene in buildScenes)
        {
            string sceneName = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
            sceneName = sceneName.Substring(0, sceneName.Length - 6);

            scenesList.Add(sceneName);
        }
    }
#endif

    private static SceneDirector instance;

    public static SceneDirector Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if(SceneDirector.Instance != null)
        {
            Destroy(SceneDirector.Instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene()
    {
        if(async)
        {
            SceneManager.LoadSceneAsync(sceneToLoad, loadSceneMode);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad, loadSceneMode);
        }
    }

    public void LoadScene(int sceneId)
    {
        if (async)
        {
            SceneManager.LoadSceneAsync(sceneId, loadSceneMode);
        }
        else
        {
            SceneManager.LoadScene(sceneId, loadSceneMode);
        }
    }

    public void LoadScene(string scene)
    {
        if (async)
        {
            SceneManager.LoadSceneAsync(scene, loadSceneMode);
        }
        else
        {
            SceneManager.LoadScene(scene, loadSceneMode);
        }
    }

    public void LoadScene(string scene, bool isAsync, LoadSceneMode mode)
    {
        if(isAsync)
        {
            SceneManager.LoadSceneAsync(scene, mode);
        }
        else
        {
            SceneManager.LoadScene(scene, mode);
        }
    }
}
