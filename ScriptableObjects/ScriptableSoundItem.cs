using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract class used to represent data
/// for a sound element
/// </summary>
[Serializable]
public abstract class ScriptableSoundItem : ScriptableObject
{
    /// <summary>
    /// This is the value used to distinguish this sound from others.
    /// It should be distinct from all other SoundItems in the AudioManager
    /// </summary>
    public string Name { get { return name; } }
    [Tooltip("If a sound belonging to a collection does not specify its mixer group " +
        "but its collection does, the sound will default to the collection's mixer " +
        "group; otherwise, collection mixer groups are overwritten")]
    [SerializeField] protected AudioMixerGroup _mixerGroup = null;
    [Tooltip("If a Sound belongs to a collection, its volume is multiplied by the volume of the collection")]
    [Range(0f, 1f)]
    [SerializeField] protected float _volume = 0.5f;

    /// <summary>
    /// Setter for this item's mixer group
    /// </summary>
    /// <param name="group"></param>
    public void SetMixerGroup(AudioMixerGroup group)
    {
        _mixerGroup = group;
    }
    /// <summary>
    /// Returns true if this item has a designated mixer group
    /// </summary>
    /// <returns></returns>
    public bool HasMixerGroup
    {
        get
        {
            return _mixerGroup != null;
        }
    }

    /// <summary>
    /// The default volume which this item plays at
    /// </summary>
    public float StandardVolume => _volume;
    /// <summary>
    /// The mixer group which this sound item plays through
    /// </summary>
    public AudioMixerGroup MixerGroup => _mixerGroup;

    public override string ToString()
    {
        return Name;
    }

    /// <summary>
    /// Base decleration for accepting a visitor
    /// </summary>
    /// <param name="visitor"></param>
    public abstract void Accept(ScriptableSoundItemVisitor visitor);
}
