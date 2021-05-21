using UnityEngine;

/// <summary>
/// This class is used for scenes which don't have a script
/// to ask the Audio Manager to play something. It fills 
/// that role in their stead. This solves the problem of an 
/// assigned audiomanager being overwritten by a new audio
/// manager. Used particularly for unity buttons/events
/// </summary>
[System.Serializable]
public class AudioManagerExtension : MonoBehaviour
{
    public bool playOnAwake = false;
    [Tooltip("sound name to play on awake")]
    public string toPlayOnAwake = "";
    [Tooltip("If specified, overrides the default 2d audio source for the desired sound with the specified 3d audio source")]
    public AudioSource overrideSource = null;
    
    [Header("Fading")]
    [Tooltip("Used for any Fade calls on this extension. Determines how long in seconds the fade should last")]
    public float fadeTime = 1f;
    [Tooltip("Used for any Fade calls on this extension. Determines whether fade calls should fade in or out")]
    public bool fadeIn = true;

    public bool Initialized { get; private set; }

    private void Awake()
    {
        Initialized = false;
    }

    void Start()
    {
        if (playOnAwake)
        {
            PlayFromAM(toPlayOnAwake);
        }
        Initialized = true;
    }
    /// <summary>
    /// Asks the audio manager to play a sound
    /// with the given name
    /// </summary>
    /// <param name="name"></param>
    public void PlayFromAM(string name)
    {
        AudioManager.Play(name, overrideSource: overrideSource);
    }
    /// <summary>
    /// Asks the audio manager to stop playing a sound
    /// with the given name
    /// </summary>
    /// <param name="name"></param>
    public void StopFromAM(string name)
    {
        AudioManager.Stop(name, overrideSource);
    }

    /// <summary>
    /// Asks the audio manager to fade a sound
    /// with the given name. Other fading variables are extension specific
    /// </summary>
    /// <param name="name"></param>
    public void FadeFromAM(string name)
    {
	    AudioManager.Fade(name, !fadeIn, fadeTime);
    }
}
