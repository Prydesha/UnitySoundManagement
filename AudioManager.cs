using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class represents our game's Audio Management System interface.
/// It is the root at which all other components are able to execute operations
/// on a certain sound
/// </summary>
public class AudioManager : MonoBehaviour
{
    
    [Tooltip("List of sound items used in this game\n" +
             "(each item can be a collection of sounds\n" +
             "or an independent sound)")]
    // This array is the list of data for our sounds
    [SerializeField] private List<ScriptableSoundItem> _sounds = new List<ScriptableSoundItem>();
    [Tooltip("The mixer group which controls the master volume of our game")]
    [SerializeField] private MixerInfo _masterMixerGroup = null;
    [Tooltip("The list of non-master audio mixer groups which should be visible to " +
        "scripts outside of the audio management system")]
    [SerializeField] private List<MixerInfo> _standardMixerGroups = new List<MixerInfo>();

    /// <summary>
    /// This array is the collection of instances of sounds in the game
    /// </summary>
    private List<InGameSoundItem> _activeSounds = new List<InGameSoundItem>();

    // the current audio manager
    private static AudioManager Instance;

    #region Public gets/sets

    /// <summary>
    /// The mixer group which controls the master volume of our game
    /// </summary>
    public static MixerInfo MasterMixer
    {
        get
        {
            if (Instance)
            {
                return Instance._masterMixerGroup;
            }
            return null;
        }
        set
        {
            if (Instance)
            {
                Instance._masterMixerGroup = value;
            }
        }
    }
    /// <summary>
    /// The list of non-master audio mixer groups
    /// </summary>
    public static List<MixerInfo> StandardMixerGroups
    {
        get
        {
            if (Instance)
            {
                return Instance._standardMixerGroups;
            }
            return new List<MixerInfo>();
        }
        set
        {
            if (Instance)
            {
                Instance._standardMixerGroups = value;
            }
        }
    }

    #endregion

    #region Construction

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        Initialize();
    }
    /// <summary>
    /// Constructs an AudioSource for each item 
    /// in the manager's array of SoundItems.
    /// This occurs during the first frame of our game.
    /// </summary>
    private void Initialize()
    {
        //remove any null mixer groups
        MixerInfo[] tempMixers = _standardMixerGroups.ToArray();
        _standardMixerGroups.Clear();
        foreach (MixerInfo mi in tempMixers)
        {
            if (mi != null) { _standardMixerGroups.Add(mi); }
        }

        // initialize mixer groups
        if (_masterMixerGroup != null)
        {
            MasterMixer.Initialize();
        }
        foreach(MixerInfo mi in StandardMixerGroups)
        {
            mi.Initialize();
        }

        //remove any null sounds
        ScriptableSoundItem[] tempSounds = _sounds.ToArray();
        _sounds.Clear();
        foreach (ScriptableSoundItem mi in tempSounds)
        {
            if (mi != null) { _sounds.Add(mi); }
        }

        //initialize sound items
        foreach (ScriptableSoundItem snd in _sounds)
        {
            InGameSoundItem dump; //< AM does not need this
            foreach(var s in ConvertScriptable(snd, out dump))
            {
                s.SetSource(CreateAudioSource(), true);
                _activeSounds.Add(s);
            }
        }

        // get player prefs
        MasterMixer.MatchToPlayerPrefs();
        foreach (MixerInfo mi in StandardMixerGroups)
        {
            mi.MatchToPlayerPrefs();
        }
    }

    /// <summary>
    /// convert a scriptable object 
    /// sound item to a list of in game instances
    /// </summary>
    /// <param name="original"></param>
    /// <param name="convertedOriginal">the original converted to an ingame sound item</param>
    /// <returns>a list including the converted original sounditem as well as all of its 
    /// converted children (if applicable)</returns>
    private List<InGameSoundItem> ConvertScriptable(ScriptableSoundItem original, out InGameSoundItem convertedOriginal)
    {
        List<InGameSoundItem> newSounds = new List<InGameSoundItem>();
        if (original is ScriptableSound) // singular sound is a simple conversion
        {
            convertedOriginal = new InGameSound(this, original as ScriptableSound);
        }
        else if (original is ScriptableSoundCollection)
        {
            // collections require a list of each of their immediate children
            // but each of those children must also be converted
            List<InGameSoundItem> collectionLevelItems = new List<InGameSoundItem>();
            ScriptableSoundCollection ogCol = original as ScriptableSoundCollection;
            foreach(var scriptableSound in ogCol.sounds)
            {
                InGameSoundItem convertedChild;
                // If an item in this collection does not have 
                // a mixer group, set it to the collection's group
                if (!scriptableSound.HasMixerGroup)
                {
                    scriptableSound.SetMixerGroup(ogCol.MixerGroup);
                }
                // convert the item,
                // add all sub conversions to the greater list
                newSounds.AddRange(ConvertScriptable(scriptableSound, out convertedChild));
                // add only the item to the collection's list
                collectionLevelItems.Add(convertedChild);
            }
            var newSoundCOll = new InGameSoundCollection(ogCol, collectionLevelItems);
            convertedOriginal = newSoundCOll;
            // connect all immediate children to their parent collection
            foreach(var child in collectionLevelItems)
            {
                child.SetCollection(newSoundCOll);
            }
        }
        else { throw new System.NotImplementedException("SoundItem extension has not been implemented " +
            "in the audio manager's ConvertScriptable method"); }
        newSounds.Add(convertedOriginal);
        return newSounds;
    }

    /// <summary>
    /// Creates a new audio source on the audiomanager's 
    /// gameobject.
    /// </summary>
    /// <returns>The created AudioSource</returns>
    public AudioSource CreateAudioSource()
    {
        GameObject newSourceObj = new GameObject();
        newSourceObj.transform.parent = transform;
        return newSourceObj.AddComponent<AudioSource>();
    }

    #endregion

    #region Playing and Stopping Audio

    /// <summary>
    /// Play a sound from this audiomanager's array of sounds.
    /// </summary>
    /// <param name="name"> Name of the sound to play</param>
    /// <param name="volume"> Optionally set to override inspector volume</param>
    /// <param name="overrideSource">if set, the audio will play from the provided source rather than the default 2D source</param>
    /// <param name="overridePitch"> if set to true, plays using the provided "pitch" parameter</param>
    /// <param name="pitch">only applicable if overridePitch is set to true.</param>
    public static void Play(string name, float volume = -1f, AudioSource overrideSource = null, bool overridePitch = false, float pitch = 1f)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        // find the sound
        InGameSound snd = Instance.FindSound(name, overrideSource);

        // play it
        if (snd != null)
        {
            bool played = snd.Play(volume, overridePitch, pitch);
            if (played)
            {
                // we played a sound, if it was fading out before, 
                // it should no longer fade out
                if (snd.fadingOut)
                {
                    Instance.StopCoroutine(snd.currentFadeOut);
                    snd.currentFadeOut = null;
                }
            }
        }
    }
    /// <summary>
    /// Plays a random sound from the collection with
    /// the designated name
    /// </summary>
    /// <param name="name"></param>
    public static void PlayRandomFromCollection(string name)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        InGameSoundCollection coll = Instance.FindCollection(name);
        if (coll != null)
        {
            coll.PlayRandom();
        }
    }
    /// <summary>
    /// Stops playing an audio clip
    /// </summary>
    /// <param name="name">name of clip to stop playing</param>
    /// <param name="source3D">optional parameter to specify the source that
    /// should stop playing</param>
    public static void Stop(string name, AudioSource source3D = null)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        InGameSound snd = Instance.FindSound(name, source3D);
        if (snd != null)
        {
            snd.Stop();
            if (snd.currentFadeIn != null)
            {
                Instance.StopCoroutine(snd.currentFadeIn);
                snd.currentFadeIn = null;
            }
        }
    }
    /// <summary>
    /// Stop playing all audio clips
    /// </summary>
    public static void StopAllAudio()
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        foreach (InGameSoundItem snd in Instance.FindAllSounds())
        {
            Stop(snd.Name);
        }
    }
    #endregion

    #region Fading

    /// <summary>
    /// Starts the FadeIn/Out coroutine
    /// </summary>
    /// <param name="name">name of clip to fade</param>
    /// <param name="fadeOut">true if fading out</param>
    /// <param name="source3D">optional parameter to specify the source that
    /// should fade</param>
    public static void Fade(string name, bool fadeOut = true, float time = -1f, AudioSource source3D = null)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        InGameSound snd = Instance.FindSound(name, source3D);
        if (snd != null)
        {
            if (fadeOut && snd.currentFadeOut == null)
            {
                snd.currentFadeOut = Instance.StartCoroutine(snd.FadeOut(time));
            }
            else if (snd.currentFadeIn == null)
            {
                snd.currentFadeIn = Instance.StartCoroutine(snd.FadeIn(time));
            }
        }
    }

    /// <summary>
    /// Fade out using volume and/or pitch
    /// </summary>
    /// <param name="name">name of the sound to fade</param>
    /// <param name="fadeVolume">True if we want to fade the volume</param>
    /// <param name="fadePitch">True if we want to fade the pitch</param>
    /// <param name="volumeDuration">How long we want to fade the volume</param>
    /// <param name="pitchDuration">How long we want to fade the pitch</param>
    /// <param name="finalPitch">Pitch to end the fade at</param>
    /// <param name="endOriginalPitch">Do we want to end the fade at 
    /// our original pitch?</param>
    public static void FadeOutAdvanced(string name, bool fadeVolume, bool fadePitch,
        float volumeDuration = 1f, float pitchDuration = 1f,
        float finalPitch = 1f, bool endOriginalPitch = true, AudioSource source3D = null)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        InGameSound snd = Instance.FindSound(name, source3D);
        if (snd != null && snd.currentFadeOut == null)
        {
            snd.currentFadeOut = Instance.StartCoroutine(snd.FadeOut(fadeVolume, fadePitch, volumeDuration,
                pitchDuration, finalPitch, endOriginalPitch));
        }
    }

    /// <summary>
    /// Fade in using volume and/or pitch
    /// </summary>
    /// <param name="name">Name of the sound we want to fade in</param>
    /// <param name="fadeVolume">True if we want to fade the volume</param>
    /// <param name="fadePitch">True if we want to fade the pitch</param>
    /// <param name="volumeDuration">How long we want to fade the volume</param>
    /// <param name="pitchDuration">How long we want to fade the pitch</param>
    /// <param name="initialPitch">Pitch to start fading from</param>
    /// <param name="finalPitch">Pitch to end the fade at</param>
    /// <param name="startOriginalPitch">Do we want to start the fade at 
    /// our original pitch?</param>
    /// <param name="endOriginalPitch">Do we want to end the fade at 
    /// our original pitch?</param>
    public static void FadeInAdvanced(string name, bool fadeVolume, bool fadePitch,
        float volumeDuration = 1f, float pitchDuration = 1f,
        float initialPitch = -3f, float finalPitch = 1f,
        bool startOriginalPitch = false, bool endOriginalPitch = true,
        AudioSource source3D = null)
    {
        if (!Instance)
        {
            MissingInstanceBehavior();
            return;
        }
        // find the proper sound
        InGameSound snd = Instance.FindSound(name, source3D);
        // fade it
        if (snd != null && snd.currentFadeIn == null)
        {
            snd.currentFadeIn = Instance.StartCoroutine(snd.FadeIn(fadeVolume, fadePitch, volumeDuration, 
                pitchDuration, initialPitch, finalPitch, startOriginalPitch, 
                endOriginalPitch));
        }
    }
    #endregion

    
    #region Visitor Methods

    /// <summary>
    /// Finds a sound object with the designated name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="source3D">if specified, and a matching sound is not found, one is 
    /// created with this source and returned</param>
    /// <returns></returns>
    private InGameSound FindSound(string name, AudioSource source3D = null)
    {
        if (name == "")
        {
            return null;
        }
        // create a new visitor
        InGameSoundFinder visitor = new InGameSoundFinder(name, source3D);
        InGameSound snd = null;
        // send the visitor to each item
        foreach (InGameSoundItem item in _activeSounds)
        {
            item.Accept(visitor);
            if (visitor.HasDesiredSound())
            {
                // visitor has found the sound we are looking for
                snd = visitor.Sound;
                break;
            }
        }
        // edge case, sound DNE
        if (snd == null)
        {
            if (source3D != null)
            {
                // try to make a new sound with the 3d source
                var ss = FindSoundScriptable(name);
                if (ss != null)
                {
                    InGameSoundItem temp;
                    ConvertScriptable(ss, out temp);
                    snd = temp as InGameSound;
                    var coll = FindCollection(name, true);
                    if (coll != null)
                    {
                        snd.SetCollection(coll);
                    }
                    snd.SetSource(source3D, false);
                    _activeSounds.Add(snd);
                }
            }
            else
            {
                Debug.LogWarning("Sound not found: " + name);
            }
        }
        return snd;
    }

    /// <summary>
    /// Find a sound scriptable sound object with the specified name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private ScriptableSound FindSoundScriptable(string name)
    {
        // create a new visitor
        ScriptableSoundFinder visitor = new ScriptableSoundFinder(name);
        ScriptableSound found = null;
        // send the visitor to each item
        foreach (ScriptableSoundItem item in _sounds)
        {
            item.Accept(visitor);
            if (visitor.HasDesiredSound())
            {
                // visitor has found the sound we are looking for
                found = visitor.Sound;
                break;
            }
        }
        // edge case, sound DNE
        if (found == null)
        {
            Debug.LogWarning("Sound scriptable not assigned to manager: " + name);
        }
        return found;
    }

    /// <summary>
    /// Finds a collection object with the designated name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="withChild">if true, looks for the collection that has a child with the name 
    /// parameter, rather than the collection with the desired name</param>
    /// <returns></returns>
    private InGameSoundCollection FindCollection(string name, bool withChild = false)
    {
        // create a new visitor
        InGameSoundCollectionFinder visitor = new InGameSoundCollectionFinder(name, withChild);
        InGameSoundCollection sColl = null;
        // send the visitor to each item
        foreach (InGameSoundItem item in _activeSounds)
        {
            item.Accept(visitor);
            if (visitor.HasDesiredCollection())
            {
                // visitor has found the collection we are looking for
                sColl = visitor.Collection;
                break;
            }
        }
        // edge case, sound DNE
        if (sColl == null)
        {
            Debug.LogWarning("Collection not found: " + name);
        }

        return sColl;
    }

    /// <summary>
    /// Getter for all Sounds attached to this manager
    /// </summary>
    /// <returns></returns>
    private List<InGameSound> FindAllSounds()
    {
        InGameSoundListFinder visitor = new InGameSoundListFinder();
        foreach(InGameSoundItem snd in _activeSounds)
        {
            snd.Accept(visitor);
        }
        return visitor.FoundSounds;
    }

    #endregion

    /// <summary>
    /// This executes the operations desired for when an audiomanager
    /// is missing from the scene
    /// </summary>
    /// <param name="extraMessage"></param>
    private static void MissingInstanceBehavior(string extraMessage = "")
    {
        Debug.LogWarning("No AudioManager Instance found in the scene : " + extraMessage);
    }

    /// <summary>
    /// Stores the modification information
    /// for an audio mixer group
    /// </summary>
    [System.Serializable]
    public class MixerInfo
    {
        [SerializeField] private AudioMixerGroup MixerGroup = null;
        [Tooltip("String identifier for the exposed volume parameter of this mixer group")]
        [SerializeField] private string _volumeID = "";
        public string Name => MixerGroup.name;
        //public float MaxVolume { get; private set; }
        public const float MaxVolume = 20f;
        public const float MinVolume = -20f;
        private const float TrueMinimum = -80f;
        /// <summary>
        /// The current volume of this mixer
        /// </summary>
        /// <remarks>Volume values can never be lower than the 
        /// constant MinVolume value</remarks>
        /// <returns></returns>
        public float Volume
        {
            get
            {
                float curVol;
                MixerGroup.audioMixer.GetFloat(_volumeID, out curVol);
                return curVol;
            }
            set
            {
                value = Mathf.Clamp(value, MinVolume, MaxVolume);
                if (value == MinVolume)
                {
                    value = TrueMinimum;
                }
                MixerGroup.audioMixer.SetFloat(_volumeID, value);
                PlayerPrefs.SetFloat(_volumeID, value);
            }
        }

        /// <summary>
        /// Sets the volume of this mixer based on a 
        /// percentage value (0 - 1). This percentage value is adjusted
        /// to span the range of this mixer's minimum and maximum values
        /// </summary>
        /// <param name="percentage"></param>
        public void SetVolumeToPercentage(float percentage)
        {
            percentage = Mathf.Clamp01(percentage);
            float range = Mathf.Abs(MaxVolume) + Mathf.Abs(MinVolume);
            Volume = MinVolume + range * percentage;
        }

        public override string ToString()
        {
            if (MixerGroup)
            {
                return Name;
            }
            return base.ToString();
        }
        /// <summary>
        /// Only call this if you are the audio manager.
        /// This establishes the maximum volume of this mixer based on its
        /// initial value
        /// </summary>
        public void Initialize()
        {
            PlayerPrefs.SetFloat(_volumeID, MaxVolume);
        }
        /// <summary>
        /// Resets this mixer to it's default inspector value
        /// </summary>
        public void ResetToDefault()
        {
            MixerGroup.audioMixer.ClearFloat(_volumeID);
            PlayerPrefs.SetFloat(_volumeID, Volume);
        }
        /// <summary>
        /// Forces this mixer to match its volume setting to what is present 
        /// in playerprefs
        /// </summary>
        public void MatchToPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(_volumeID))
            {
                ResetToDefault();
            }
            //Volume = PlayerPrefs.GetFloat(_volumeID);
        }
    }
}
