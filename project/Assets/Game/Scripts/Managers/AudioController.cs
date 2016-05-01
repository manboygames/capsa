using UnityEngine;
using System.Collections.Generic;
using System;
using AdvancedInspector;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AudioController : MonoBehaviour {

    private static AudioController instance;

    [Serializable]
    public class SceneMusicDictionary : UDictionary<string, string> { }

    public SceneMusicDictionary sceneMusics = new SceneMusicDictionary();

    public AudioManager audioManager;
    public Toggle toggleMusic;
    public Toggle toggleSound;

    private DataManager dataManager;

    public static AudioController Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if(AudioController.Instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        if(DataManager.Instance == null)
        {
            yield return null;
        }

        dataManager = DataManager.Instance;

        toggleSound.isOn = (!dataManager.SFXMute);
        toggleMusic.isOn = (!dataManager.MusicMute);

        toggleSound.onValueChanged.AddListener(OnSoundToggle);
        toggleMusic.onValueChanged.AddListener(OnMusicToggle);
    }

    void OnLevelWasLoaded(int level)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        string track = "";

        if(!sceneMusics.TryGetValue(sceneName, out track))
        {
            audioManager.StopMusic();
            return;
        }

        //audioManager.StopMusic();
        audioManager.PlayMusic(track);
    }

    private void OnSoundToggle(bool isOn)
    {
        //dataManager.SFXMute = !isOn;
        audioManager.MuteSFX(!isOn);
    }

    private void OnMusicToggle(bool isOn)
    {
        //dataManager.MusicMute = !isOn;
        audioManager.MuteMusic(!isOn);
    }

    void OnDestroy()
    {
        toggleSound.onValueChanged.RemoveListener(OnSoundToggle);
        toggleMusic.onValueChanged.RemoveListener(OnMusicToggle);
    }

}

