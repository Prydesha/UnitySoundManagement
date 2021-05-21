using System.Collections.Generic;

/// <summary>
/// Visitor which visits a list of SoundItems and 
/// collects only the Sounds of the list
/// </summary>
public class InGameSoundListFinder : InGameSoundItemVisitor
{
    private List<InGameSound> _foundSounds = new List<InGameSound>();

    public List<InGameSound> FoundSounds => _foundSounds;

    public override void VisitSound(InGameSound sound)
    {
        _foundSounds.Add(sound);
    }

    public override void VisitSoundCollection(InGameSoundCollection coll)
    {
        foreach (InGameSoundItem item in coll.sounds)
        {
            item.Accept(this);
        }
    }
}
