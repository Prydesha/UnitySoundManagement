
/// <summary>
/// Visitor class which is used to find a specific sound collection
/// </summary>
public class InGameSoundCollectionFinder : InGameSoundItemVisitor
{
    // the SoundItem this visitor is currently
    // visiting
    private InGameSoundCollection _coll = null;
    public InGameSoundCollection Collection { get { return _coll; } }
    // the name of the SoundItem we are searching for
    private string _dName;
    // whether the finder should look for a child with the desired name
    private bool _lookForChild = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="lookForChild">if true, looks for the collection that has a child with the name 
    /// parameter, rather than the collection with the desired name</param>
    public InGameSoundCollectionFinder(string name, bool lookForChild = false)
    {
        _dName = name;
        _lookForChild = lookForChild;
    }

    public override void VisitSoundCollection(InGameSoundCollection sc)
    {
        _coll = sc;
        if (!HasDesiredCollection())
        {
            // check any collection in our sounds
            foreach (InGameSoundItem item in sc.sounds)
            {
                item.Accept(this);
                if (HasDesiredCollection())
                {
                    break;
                }
            }
        }
    }

    public bool HasDesiredCollection()
    {
        if (_coll == null) { return false; }
        if (_lookForChild)
        {
            foreach (InGameSoundItem item in _coll.sounds)
            {
                if (item.Name == _dName)
                {
                    return true;
                }
            }
            return false;
        }
        return _coll.Name == _dName;
    }

}
