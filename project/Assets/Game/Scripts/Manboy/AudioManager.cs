using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;
using AdvancedInspector;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get { return AudioManager.instance; }
    }

    [Inspect(0),Group("Music")]
    public List<AudioClip> musicList = new List<AudioClip>();
    [Inspect(1), Group("Music")]
    public void ResetMusicList()
    {
        musicList = new List<AudioClip>();
        musicData = new Dictionary<string, AudioClip>();
    }
    [Inspect(2), Group("Music")]
    public AudioSource musicSource;
    [Inspect(3), Group("Music")]
    public bool autoStartMusic;

    [Inspect(0), Group("SFX")]
    public List<SFX> sfxList = new List<SFX>();
    [Inspect(1), Group("SFX")]
    public void ResetSFXList()
    {
        sfxList = new List<SFX>();
        sfxData = new Dictionary<string, AudioClip[]>();
    }
    [Inspect(2), Group("SFX")]
    public AudioSource sfxSource;

    private Dictionary<string, AudioClip> musicData = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip[]> sfxData = new Dictionary<string, AudioClip[]>();

    private float musicVolume = 0.5f;
    private Tweener musicTweener;
    private float musicVolumeTarget;

    private IEnumerator musicCoroutine;

    [Inspect, Group("Music"),RangeValue(0, 1)]
    public float MusicVolume
    {
        get 
        {
            return musicVolume; 
        }
        set
        {
            musicVolume = value;
            DataManager.Instance.MusicVolume = value;

            if(DataManager.Instance.MusicMute)
            {
                return;
            }

            if (musicTweener != null)
            {
                if (musicTweener.IsActive())
                {
                    if (musicVolumeTarget > 0)
                    {
                        float remaining = musicTweener.Duration() - musicTweener.Elapsed();
                        musicTweener.Kill();
                        musicTweener = musicSource.DOFade(value, remaining).SetAutoKill(true).OnKill(() => musicTweener = null);
                    }
                }
                else
                {
                    musicSource.volume = musicVolume;
                }
            }
            else
            {
                musicSource.volume = musicVolume;
            }
        }
    }

    private float sfxVolume = 1f;

    [Inspect, Group("SFX"), RangeValue(0, 1)]
    public float SFXVolume
    {
        get { return sfxVolume; }
        set
        {
            sfxSource.volume = sfxVolume = value;
            DataManager.Instance.SFXVolume = value;
        }
    }

    private bool ready = false;

    public bool Ready
    {
        get
        {
            return ready;
        }
    }

   void Awake()
    {

        if (AudioManager.Instance != null)
        {
            //AudioManager.Instance.StopMusic(1f, true);
            //Destroy(AudioManager.Instance.gameObject);
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if(musicSource == null || sfxSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            musicSource = sources[0];
            sfxSource = sources[1];
        }

        for (int m = 0; m < musicList.Count; m++)
        {
            musicData.Add(musicList[m].name, musicList[m]);
        }

        for (int s = 0; s < sfxList.Count; s++)
        {
            sfxData.Add(sfxList[s].group, sfxList[s].clips);
        }

        //MusicVolume = DataManager.Instance.MusicVolume;
        //SfxVolume = DataManager.Instance.SFXVolume;

        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return DataManager.Instance != null;

        if (DataManager.Instance.MusicMute)
        {
            musicSource.volume = 0;
            musicVolume = DataManager.Instance.MusicVolume;
        }
        else
        {
            musicVolume = musicSource.volume = DataManager.Instance.MusicVolume;
        }

        if (DataManager.Instance.SFXMute)
        {
            sfxSource.volume = 0;
            sfxVolume = DataManager.Instance.SFXVolume;
        }
        else
        {
            sfxVolume = sfxSource.volume = DataManager.Instance.SFXVolume;
        }

        ready = true;

        if(autoStartMusic)
        {
            PlayMusic(0);
        }
    }

    public void PlayMusic(int trackID)
    {
        AudioClip clip = musicList[trackID];

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicTweener.Kill();
        }

        if (DataManager.Instance.MusicMute)
        {
            musicSource.clip = clip;
            musicSource.Play();
            musicSource.volume = 0;
        }
        else
        {
            musicCoroutine = CrossFade(clip);
            StartCoroutine(musicCoroutine);
        }
        //StartCoroutine(Co_CrossFade(clip));
    }

    public void PlayMusic(string trackName)
    {
        if (!musicData.ContainsKey(trackName))
        {
            Debug.LogError("Cannot find sound clip");
            return;
        }

        AudioClip clip = musicData[trackName];

        if(DataManager.Instance.MusicMute)
        {
            musicSource.clip = clip;
            musicSource.Play();
            musicSource.volume = 0;
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicTweener.Kill();
        }

        musicCoroutine = CrossFade(clip);
        StartCoroutine(musicCoroutine);
    }

    public void StopMusic(float duration = 0.5f, bool autoDestroy = false)
    {
        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicTweener.Kill();
        }

        musicCoroutine = CrossFade(null, duration, autoDestroy);
        StartCoroutine(musicCoroutine);
    }

    public void MuteMusic(bool state, float duration = 1f)
    {
        if(state)
        {
            musicTweener = musicSource.DOFade(0, duration).SetAutoKill(true).OnKill(() => musicTweener = null);
        }
        else
        {
            if(musicSource.volume != musicVolume)
            {
                musicTweener = musicSource.DOFade(musicVolume, duration).SetAutoKill(true).OnKill(() => musicTweener = null);
            }
        }

        DataManager.Instance.MusicMute = state;
    }

    public void MuteSFX(bool state)
    {
        /*if(state)
        {
            sfxSource.volume = 0;
        }
        else
        {
            sfxSource.volume = sfxVolume;
        }*/

        DataManager.Instance.SFXMute = state;
    }

    private IEnumerator CrossFade(AudioClip toClip, float duration = 1f, bool autoDestroy = false)
    {
        if (!musicSource.isPlaying)
        {
            musicSource.volume = 0;
        }

        if (musicSource.clip != null)
        {
            musicVolumeTarget = 0;
            //musicTweener = musicSource.DOFade(musicVolumeTarget, duration).SetAutoKill(false);
            musicTweener = musicSource.DOFade(musicVolumeTarget, duration).SetAutoKill(true).OnKill(() => musicTweener = null);
            yield return musicTweener.WaitForCompletion();
            //yield return musicSource.DOFade(musicVolumeTarget, 5).WaitForCompletion();
        }

        if (toClip == null)
        {
            musicSource.Stop();
            if (autoDestroy)
            {
                Destroy(gameObject);
            }
            yield break;
        }

        musicSource.clip = toClip;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
        
        musicVolumeTarget = musicVolume;
        //musicTweener.SetAutoKill(true).OnKill(() => musicTweener = null);
        musicTweener = musicSource.DOFade(musicVolumeTarget, duration).SetAutoKill(true).OnKill(() => musicTweener = null);
    }

    public void PlaySFX(string sfxGroup)
    {
        if (DataManager.Instance.SFXMute == true)
        {
            return;
        }

        sfxSource.PlayOneShot(sfxData[sfxGroup][UnityEngine.Random.Range(0, sfxData[sfxGroup].Length)]);
    }

    public void PlaySFX(string sfxGroup, int clipID)
    {
        if (DataManager.Instance.SFXMute == true)
        {
            return;
        }
        sfxSource.PlayOneShot(sfxData[sfxGroup][clipID]);
    }

    public void PlaySFX(string sfxGroup, string clipName)
    {
        if (DataManager.Instance.SFXMute == true)
        {
            return;
        }
        AudioClip clip = Array.Find(sfxData[sfxGroup], element => element.name == clipName);
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXDelayed(float delay, string sfxGroup)
    {
        if (DataManager.Instance.SFXMute)
        {
            return;
        }
        StartCoroutine(Co_PlaySFXDelayed(delay, sfxGroup));
    }

    public void PlaySFXDelayed(float delay, string sfxGroup, int clipID)
    {
        if (DataManager.Instance.SFXMute)
        {
            return;
        }
        StartCoroutine(Co_PlaySFXDelayed(delay, sfxGroup, clipID));
    }

    public void PlaySFXDelayed(float delay, string sfxGroup, string clipName)
    {
        if (DataManager.Instance.SFXMute)
        {
            return;
        }
        StartCoroutine(Co_PlaySFXDelayed(delay, sfxGroup, clipName));
    }

    private IEnumerator Co_PlaySFXDelayed(float delay, string sfxGroup)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(sfxGroup);
    }

    private IEnumerator Co_PlaySFXDelayed(float delay, string sfxGroup, int clipID)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(sfxGroup, clipID);
    }

    private IEnumerator Co_PlaySFXDelayed(float delay, string sfxGroup, string clipName)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(sfxGroup, clipName);
    }
}

[System.Serializable]
public struct SFX
{
    public string group;
    public AudioClip[] clips;

    public SFX(string group, AudioClip[] clips)
    {
        this.group = group;
        this.clips = clips;
    }
}
