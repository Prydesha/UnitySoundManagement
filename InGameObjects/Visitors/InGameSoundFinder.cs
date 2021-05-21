using UnityEngine;

/// <summary>
/// Visitor used to find a specific sound object
/// </summary>
public class InGameSoundFinder : InGameSoundItemVisitor
{
    // the SoundItem this visitor is currently
    // visiting
    private InGameSound _sound;
    public InGameSound Sound { get { return _sound; } }
    // the name of the SoundItem we are searching for
    private string _name;
    // the source which the sound item must have
    private AudioSource _source = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">The name of the desired sound</param>
    /// <param name="source">Optional. The source of the desired sound. 
    /// If not set, the desired sound is assumed to be 
    /// from a source under the audio manager</param>
    public InGameSoundFinder(string name, AudioSource source = null)
    {
        _name = name;
        _source = source;
    }

    /// <summary>
    /// "Visits" a Sound, setting the current visiting 
    /// sound to the parameter sound
    /// </summary>
    /// <param name="sound"></param>
    public override void VisitSound(InGameSound sound)
    {
        _sound = sound;
    }

    public override void VisitSoundCollection(InGameSoundCollection coll)
    {
        foreach (InGameSoundItem item in coll.sounds)
        {
            item.Accept(this);
            if (HasDesiredSound())
            {
                break;
            }
        }
    }

    /// <summary>
    /// Returns true if the current sound acquired by this Finder
    /// is the one which it has set out to find
    /// </summary>
    /// <returns></returns>
    public bool HasDesiredSound()
    {
        if (_source == null)
        {
            if (!_sound.isManagerChild)
            {
                return false;
            }
        }
        else if (_source != _sound.Source)
        {
            return false;
        }
        return _sound.Name == _name;
    }

}
