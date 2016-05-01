using AdvancedInspector;
using System.Text;
using UnityEngine;
public class DataManager : MonoBehaviour {

    private static DataManager instance;

    public static DataManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (DataManager.Instance != null && DataManager.instance != this)
        {
            Destroy(DataManager.Instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public const string MUSIC_VOLUME = "MusicVolume";
    public const string SFX_VOLUME = "SFXVolume";
    public const string MUSIC_MUTE = "MusicMute";
    public const string SFX_MUTE = "SFXMute";
    public const string RESOLUTION = "Resolution";
    public const string FULL_SCREEN = "FullScreen";
    public const string QUALITY_SETTINGS = "QualitySettings";

    public const string NETWORK_ADDRESS = "Cache-NetworkAddress";
    public const string NETWORK_PORT = "Cache-NetworkPort";

    #region Audio

    [Inspect, RangeValue(0, 1), ReadOnly("IsReadOnly")]
    public float MusicVolume
    {
        get
        {
            return PlayerPrefsPlus.GetFloat(MUSIC_VOLUME, 0.5f);
        }
        set
        {
            PlayerPrefsPlus.SetFloat(MUSIC_VOLUME, value);
        }
    }

    [Inspect, RangeValue(0, 1), ReadOnly("IsReadOnly")]
    public float SFXVolume
    {
        get
        {
            return PlayerPrefsPlus.GetFloat(SFX_VOLUME, 1f);
        }
        set
        {
            PlayerPrefsPlus.SetFloat(SFX_VOLUME, value);
        }
    }


    [Inspect, ReadOnly("IsReadOnly")]
    public bool MusicMute
    {
        get
        {
            return PlayerPrefsPlus.GetBool(MUSIC_MUTE, false);
            //return IntToBool(PlayerPrefs.GetInt("MusicMute",0));
        }
        set
        {
            PlayerPrefsPlus.SetBool(MUSIC_MUTE, value);
            //PlayerPrefs.SetInt("MusicMute", BoolToInt(value));
            //PlayerPrefs.Save();
        }
    }

    private bool sfxMute = false;

    [Inspect, ReadOnly("IsReadOnly")]
    public bool SFXMute
    {
        get
        {
            return PlayerPrefsPlus.GetBool(SFX_MUTE, false);
            //return IntToBool(PlayerPrefs.GetInt("SFXMute",0));
        }
        set
        {
            PlayerPrefsPlus.SetBool(SFX_MUTE, value);
            //PlayerPrefsPlus.SetInt("SFXMute", BoolToInt(value));
        }
    }

    #endregion

    #region Network

    public void SaveNetworkCache(string address, int port)
    {
        PlayerPrefsPlus.SetString(NETWORK_ADDRESS, address);
        PlayerPrefsPlus.SetInt(NETWORK_PORT, port);
    }

    public void LoadNetworkCache(out string address, out int port)
    {
        address = PlayerPrefsPlus.GetString(NETWORK_ADDRESS, string.Empty);
        port = PlayerPrefsPlus.GetInt(NETWORK_PORT, 0);
    }

    #endregion

    private bool IsReadOnly()
    {
        return Application.isPlaying;
    }

}
