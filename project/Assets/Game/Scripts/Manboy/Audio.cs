using UnityEngine;
using System.Collections;
using AdvancedInspector;

//[AdvancedInspector]
[System.Serializable]
public class Audio{

    public enum AudioType
    {
        NONE, MUSIC, SFX
    }

    public AudioType audioType;

    public enum MusicMethodType
    {
        CLIP_NAME, CLIP_ID
    }

    public enum SfxMethodType
    {
        RANDOM_GROUP, CLIP_NAME, CLIP_ID
    }

    [Inspect("IsMusic")]
    [Descriptor(Name="Method")]
    public MusicMethodType musicMethodType;

    [Inspect("IsSfx")]
    [Descriptor(Name = "Method")]
    public SfxMethodType sfxMethodType;

    [Inspect("IsNotNone")]
    public string group;

    [Inspect("IsFindByID")]
    public int clipIndex;

    [Inspect("IsFindByName")]
    public string clipName;

    private bool IsMusic()
    {
        return audioType == AudioType.MUSIC;
    }

    private bool IsSfx()
    {
        return audioType == AudioType.SFX;
    }

    private bool IsNotNone()
    {
        return audioType != AudioType.NONE;
    }

    private bool IsFindByID()
    {
        if(IsMusic())
        {
            return musicMethodType == MusicMethodType.CLIP_ID;
        }
        else if(IsSfx())
        {
            return sfxMethodType == SfxMethodType.CLIP_ID;
        }
        return false;
    }

    private bool IsFindByName()
    {
        if (IsMusic())
        {
            return musicMethodType == MusicMethodType.CLIP_NAME;
        }
        else if (IsSfx())
        {
            return sfxMethodType == SfxMethodType.CLIP_NAME;
        }
        return false;
    }

    public void Play()
    {
        if (audioType == AudioType.MUSIC)
        {
            switch (musicMethodType)
            {
                case MusicMethodType.CLIP_ID:
                    AudioManager.Instance.PlayMusic(clipIndex);
                    break;
                case MusicMethodType.CLIP_NAME:
                    AudioManager.Instance.PlayMusic(clipName);
                    break;
            }
        }
        else if (audioType == AudioType.SFX)
        {
            switch (sfxMethodType)
            {
                case SfxMethodType.RANDOM_GROUP:
                    AudioManager.Instance.PlaySFX(group);
                    break;

                case SfxMethodType.CLIP_ID:
                    AudioManager.Instance.PlaySFX(group, clipIndex);
                    break;

                case SfxMethodType.CLIP_NAME:
                    AudioManager.Instance.PlaySFX(group, clipName);
                    break;
            }
        }
    }

}
