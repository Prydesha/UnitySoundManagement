using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents an instance of a SoundItem ScriptableObject
/// in game
/// </summary>
public abstract class InGameSoundItem
{
    protected ScriptableSoundItem _data;

    // the source which this sound will play through
    private AudioSource _source;
    public AudioSource Source => _source;
    /// <summary>
    /// True if the source of this sound is a child of the
    /// audio manager (a 2D audio source)
    /// </summary>
    public bool isManagerChild { get; private set; }

    /// <summary>
    /// Sets the source of this Sound
    /// </summary>
    /// <param name="source"></param>
    /// <param name="managerChild">True if the provided source is 
    /// a child of the audio manager</param>
    /// <param name="initializedSource">Optional parameter, if set to false, play on awakes will not
    /// be triggered by this set</param>
    public virtual void SetSource(AudioSource source, bool managerChild, bool initializedSource = true)
    {
        _source = source;
        Source.gameObject.name = _data.Name;
        isManagerChild = managerChild;
    }

    /// <summary>
    /// Base decleration for playing an item at a specific volume level
    /// </summary>
    /// <param name="volume">if left unspecified, 
    /// defaults to inspector volume</param>
    /// <returns>true if the sound was played</returns>
    public virtual bool Play(float volume = -1f, bool overridePitch = false, float pitch = 1f)
    {
        return false;
    }

    // the collection this item is in (if applicable)
    protected InGameSoundCollection _container = null;

    /// <summary>
    /// Setter for this item's collection
    /// </summary>
    /// <param name="soundCollection"></param>
    public void SetCollection(InGameSoundCollection soundCollection)
    {
        _container = soundCollection;
    }

    /// <summary>
    /// string identifier for this sound item
    /// </summary>
    /// <returns></returns>
    public string Name => _data.Name;

    public float Volume => _data.StandardVolume;

    /// <summary>
    /// Base decleration for accepting a visitor
    /// </summary>
    /// <param name="visitor"></param>
    public abstract void Accept(InGameSoundItemVisitor visitor);
}

