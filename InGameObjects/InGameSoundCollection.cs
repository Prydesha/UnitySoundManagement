using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents an instance of a SoundCollection
/// ScriptableObject in game
/// </summary>
public class InGameSoundCollection : InGameSoundItem
{
    // array of sounds in this collection.
    // these are only the immediate children of this item
    public readonly List<InGameSoundItem> sounds;

    /// <summary>
    /// Initialize this sound collection.
    /// This occurs during the first frame of our game.
    /// </summary>
    public InGameSoundCollection(ScriptableSoundCollection collectionData, List<InGameSoundItem> collection)
    {
        sounds = collection;
        _data = collectionData;
    }

    /// <summary>
    /// Plays a random sound from this collection
    /// </summary>
    /// <returns>true if the sound was played</returns>
    public bool PlayRandom()
    {
        InGameSoundItem snd = sounds[UnityEngine.Random.Range(0, sounds.Count)];
        if (snd == null)
        {
            // This should not happen, but just in case:
            Debug.LogWarning("Shawn, your PlayRandom function in SoundCollection is flawed");
            return false;
        }
        return snd.Play();
    }
    public override bool Play(float volume = -1f, bool overridePitch = false, float pitch = 1f)
    {
        return PlayRandom();
    }

    /// <summary>
    /// Accepts a SoundFinder visitor, then sends it to each 
    /// sounditem in this collection
    /// </summary>
    /// <param name="visitor"></param>
    public override void Accept(InGameSoundItemVisitor visitor)
    {
        visitor.VisitSoundCollection(this);
    }
}
