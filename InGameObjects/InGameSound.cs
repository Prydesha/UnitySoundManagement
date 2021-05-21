using UnityEngine;
using System.Collections;

/// <summary>
/// Represents an instance of a Sound ScriptableObject
/// in game
/// </summary>
public class InGameSound : InGameSoundItem
{
    #region Member Variables & Properties
    // the data which specifies how we play our sound
    // as well as what sound we play
    private ScriptableSound _soundData;
    private ScriptableSound SoundData
    {
        get { return _soundData; }
        set
        {
            _soundData = value;
            _data = value;
        }
    }

    // the manager this item belongs to
    protected AudioManager _manager = null;

    #region Runtime Fading Vars
    // Is this sound currently fading out?
    public bool fadingOut => currentFadeOut != null;
    /// <summary>
    /// Coroutine set and used by the audio manager to determine if
    /// this sound is currently fading out
    /// </summary>
    public Coroutine currentFadeOut { get; set; }
    // is this sound currently fading in?
    public bool fadingIn => currentFadeIn != null;
    /// <summary>
    /// Coroutine set and used by the audio manager to determine if
    /// this sound is currently fading in
    /// </summary>
    public Coroutine currentFadeIn { get; set; }
    #endregion

    /// <summary>
    /// Volume (if applicable, multiplied by collection volume)
    /// </summary>
    public float SimulatedVolume
    {
        get
        {
            if (_container != null)
            {
                return SoundData.StandardVolume * _container.Volume;
            }
            return SoundData.StandardVolume;
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize this Sound.
    /// This occurs during the first frame of our game.
    /// </summary>
    public InGameSound(AudioManager audioManager, ScriptableSound sound)
    {
        _manager = audioManager;
        SoundData = sound;
    }
    public override void SetSource(AudioSource source, bool managerChild, bool initializedSource = true)
    {
        base.SetSource(source, managerChild);
        InitializeSource(initializedSource);
    }
    /// <summary>
    /// Sets all of the attributes on this Sound's source
    /// (if it is present)
    /// </summary>
    /// <param name="initializedSource">Optional parameter, if set to false, play on awakes will not
    /// be triggered by this set</param>
    private void InitializeSource(bool initializedSource = true)
    {
        if (Source && SoundData)
        {
            Source.clip = SoundData.Clip;
            Source.volume = SimulatedVolume;
            Source.pitch = SoundData.Pitch;
            Source.loop = SoundData.Loop;
            Source.playOnAwake = SoundData.PlayOnAwake;
            Source.outputAudioMixerGroup = SoundData.MixerGroup;
            // ensure we are properly set to 2D or 3D audio
            Source.spatialBlend = isManagerChild ? 0.0f : 1.0f;
            
            if (SoundData.PlayOnAwake && initializedSource)
            {
                Source.Play();
            }
        }
    }
    #endregion

    #region Visitors
    /// <summary>
    /// Accepts a SoundFinder
    /// </summary>
    /// <param name="visitor"></param>
    public override void Accept(InGameSoundItemVisitor visitor)
    {
        visitor.VisitSound(this);
    }
    #endregion

    /// <summary>
    /// Attempt to play the sound clip associated 
    /// with this sound (One shot, with set volume)
    /// </summary>
    /// <param name="volume">if left unspecified, 
    /// defaults to inspector volume</param>
    /// <returns>true if the sound was played</returns>
    public override bool Play(float volume = -1f, bool overridePitch = false, float pitch = 1f)
    {
        if (volume == -1f)
        {
            volume = SimulatedVolume;
        }
        if (overridePitch)
        {
            Source.pitch = pitch;
        }
        else
        {
            Source.pitch = _soundData.Pitch;
        }
        Source.volume = volume;
        return TruePlay();
    }
    /// <summary>
    /// Actually play this object's sound.
    /// Does not set volume 
    /// </summary>
    /// <param name="pitchIsPersonalized">Optional parameter, true if we want to override a random pitch</param>
    private bool TruePlay(bool pitchIsPersonalized = false)
    {
        // choose a random pitch if desired
        if (SoundData.UsesRandomPitches && !pitchIsPersonalized)
        {
            Source.pitch = Random.Range(SoundData.RandomPitchRange.x,
                SoundData.RandomPitchRange.y);
        }
        bool soundPlayed = false;
        // make sure we aren't rapid firing SFX
        if (Source.isPlaying)
        {
            if (Source.time >= SoundData.StackDelay)
            {
                Source.Play();
                soundPlayed = true;
            }
        }
        else
        {
            Source.Play();
            soundPlayed = true;
        }
        return soundPlayed;
    }
    /// <summary>
    /// Stops playing audio clip
    /// </summary>
    public void Stop()
    {
        Source.Stop();
        Source.volume = SimulatedVolume;
        Source.pitch = SoundData.Pitch;
    }

    #region Fading
    /// <summary>
    /// Fade in this sound's volume
    /// </summary>
    /// <param name="volumeFadeDuration">How long we want to fade the volume</param>
    /// <returns></returns>
    public IEnumerator FadeIn(float volumeFadeDuration = 1f)
    {
        return FadeIn(true, false, volumeFadeDuration);
    }
    /// <summary>
    /// An advanced fading function, capable of fading in this sound's volume
    /// and pitch over individual amounts of time
    /// </summary>
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
    /// <returns></returns>
    public IEnumerator FadeIn(bool fadeVolume, bool fadePitch,
        float volumeDuration = 1f, float pitchDuration = 1f,
        float initialPitch = -3f, float finalPitch = 1f,
        bool startOriginalPitch = false, bool endOriginalPitch = true)
    {
        #region Error Checks and Initializers
        // if we're already fading, stop the new fade, let the old one finish
        if (fadingIn)
        {
            yield break;
        }
        // If we don't want any kind of fade, this should just be a simple
        // play. Break the coroutine
        if (!fadeVolume && !fadePitch)
        {
            Play();
            yield break;
        }

        if (volumeDuration < 0f) { volumeDuration = 0.1f; }
        if (pitchDuration < 0f) { pitchDuration = 0.1f; }

        if (startOriginalPitch) { initialPitch = Source.pitch; }
        else if (initialPitch < -3f) { initialPitch = -3f; }
        else if (initialPitch > 3f) { initialPitch = 3f; }

        if (endOriginalPitch) { finalPitch = Source.pitch; }
        if (finalPitch < -3f) { finalPitch = Source.pitch; }
        if (finalPitch > 3f) { finalPitch = Source.pitch; }
        #endregion

        bool volumeFaded = !fadeVolume;
        bool pitchFaded = !fadePitch;

        float originalVolume = SimulatedVolume;

        if (fadeVolume)
        {
            Source.volume = 0;
        }
        if (fadePitch)
        {
            Source.pitch = initialPitch;
        }
        TruePlay(fadePitch);

        float timeSinceStart = 0.0f;
        while (!volumeFaded || !pitchFaded)
        {
            timeSinceStart += Time.deltaTime;
            // lerp the volume (if desired)
            if (fadeVolume)
            {
                float volPercentComplete = timeSinceStart / volumeDuration;
                Source.volume = Mathf.Lerp(0.0f, originalVolume, volPercentComplete);
                if (volPercentComplete >= 1)
                {
                    volumeFaded = true;
                }
            }
            // lerp the pitch (if desired)
            if (fadePitch)
            {
                float pitchPercentComplete = timeSinceStart / pitchDuration;
                Source.pitch = Mathf.Lerp(initialPitch, finalPitch, pitchPercentComplete);
                if (pitchPercentComplete >= 1)
                {
                    pitchFaded = true;
                }
            }

            yield return null;
        }
        currentFadeIn = null;
    }

    /// <summary>
    /// Fade out this sound's volume
    /// </summary>
    /// <param name="volumeFadeDuration">How long we want to fade the volume</param>
    /// <returns></returns>
    public IEnumerator FadeOut(float volumeFadeDuration = 1f)
    {
        return FadeOut(true, false, volumeFadeDuration);
    }
    /// <summary>
    /// An advanced fading function, capable of fading out this sound's volume
    /// and pitch over individual amounts of time
    /// </summary>
    /// <param name="fadeVolume">True if we want to fade the volume</param>
    /// <param name="fadePitch">True if we want to fade the pitch</param>
    /// <param name="volumeDuration">How long we want to fade the volume</param>
    /// <param name="pitchDuration">How long we want to fade the pitch</param>
    /// <param name="finalPitch">Pitch to end the fade at</param>
    /// <param name="endOriginalPitch">Do we want to end the fade at 
    /// our original pitch?</param>
    /// <returns></returns>
    public IEnumerator FadeOut(bool fadeVolume, bool fadePitch,
        float volumeDuration = 1f, float pitchDuration = 1f,
        float finalPitch = 1f, bool endOriginalPitch = true)
    {
        #region Error Checks and Initializers
        // if we're already fading, stop the new fade, let the old one finish
        if (fadingOut)
        {
            yield break;
        }
        // If we don't want any kind of fade, this should just be a simple
        // stop. Break the coroutine
        if (!fadeVolume && !fadePitch)
        {
            Stop();
            yield break;
        }

        if (volumeDuration < 0f) { volumeDuration = 0.1f; }
        if (pitchDuration < 0f) { pitchDuration = 0.1f; }

        if (endOriginalPitch) { finalPitch = Source.pitch; }
        if (finalPitch < -3f) { finalPitch = Source.pitch; }
        if (finalPitch > 3f) { finalPitch = Source.pitch; }
        #endregion

        bool volumeFaded = !fadeVolume;
        bool pitchFaded = !fadePitch;

        float originalVolume = SimulatedVolume;
        float initialPitch = Source.pitch;

        float timeSinceStart = 0.0f;
        while (!volumeFaded || !pitchFaded)
        {
            timeSinceStart += Time.deltaTime;
            // lerp the volume (if desired)
            if (fadeVolume)
            {
                float volPercentComplete = timeSinceStart / volumeDuration;
                Source.volume = Mathf.Lerp(originalVolume, 0.0f, volPercentComplete);
                if (volPercentComplete >= 1)
                {
                    volumeFaded = true;
                }
            }
            // lerp the pitch (if desired)
            if (fadePitch)
            {
                float pitchPercentComplete = timeSinceStart / pitchDuration;
                Source.pitch = Mathf.Lerp(initialPitch, finalPitch, pitchPercentComplete);
                if (pitchPercentComplete >= 1)
                {
                    pitchFaded = true;
                }
            }

            yield return null;
        }
        currentFadeOut = null;
        Stop();
    }
    #endregion
}

