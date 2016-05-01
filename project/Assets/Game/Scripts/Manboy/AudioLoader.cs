using UnityEngine;
using System.Collections;
using CodeBureau;
using System.Collections.Generic;
using AdvancedInspector;

[RequireComponent(typeof(AudioManager))]
public class AudioLoader : MonoBehaviour {

    public AudioManager audioManager;

    public string mainUrl;
    public string sfxSuffix;
    public string musicSuffix;

    public List<AudioFile> music;
    public List<SFXGroup> sfx;

    public void Load()
    {
        StartCoroutine(Co_Load());
    }

    private IEnumerator Co_Load()
    {
        string musicUrl = mainUrl + "/" + musicSuffix + "/";
        foreach (AudioFile audioFile in music)
        {
            string url = musicUrl + audioFile.audioName + StringEnum.GetStringValue(audioFile.extension);
            WWW www = new WWW(url);
            yield return www;

            audioManager.musicList.Add(www.audioClip);
        }

        yield return null;

        string sfxUrl = mainUrl + "/" + sfxSuffix + "/";
        foreach (SFXGroup sfxGroup in sfx)
        {
            int length = sfxGroup.audioFiles.Length;
            AudioClip[] clips = new AudioClip[length];
            for (int i = 0; i < length; i++)
            {
                AudioFile audioFile = sfxGroup.audioFiles[i];

                string url = sfxUrl + audioFile.audioName + StringEnum.GetStringValue(audioFile.extension);
                WWW www = new WWW(url);
                yield return www;

                clips[i] = www.audioClip;
            }

            audioManager.sfxList.Add(new SFX(sfxGroup.group, clips));
        }

        Debug.Log("Load Finished");
    }

    [Inspect]
    public void ImportFromAudioManager()
    {
        music = new List<AudioFile>();
        sfx = new List<SFXGroup>();

        foreach (AudioClip clip in audioManager.musicList)
        {
            AudioFile audioFile = new AudioFile(clip.name, AudioExtension.WAV);
            audioFile.audioName = clip.name;
            music.Add(audioFile);
        }

        foreach (SFX s in audioManager.sfxList)
        {
            sfx.Add(new SFXGroup(s.group, s.clips));
        }
    }
}

[System.Serializable]
public struct AudioFile
{
    public string audioName;
    public AudioExtension extension;

    public AudioFile(string audioName, AudioExtension extension)
    {
        this.audioName = audioName;
        this.extension = extension;
    }
}

[System.Serializable]
public struct SFXGroup
{
    public string group;
    public AudioFile[] audioFiles;

    public SFXGroup(string group, AudioClip[] clips)
    {
        this.group = group;
        this.audioFiles = new AudioFile[clips.Length];

        for (int i = 0; i < clips.Length; i++)
        {
            audioFiles[i] = new AudioFile(clips[i].name, AudioExtension.WAV);
        }
    }
}

public enum AudioExtension
{
    [StringValue(".wav")]
    WAV,
    [StringValue(".mp3")]
    MP3,
    [StringValue(".ogg")]
    OGG
}
