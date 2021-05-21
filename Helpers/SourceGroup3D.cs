using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a group of 3D audio sources
/// </summary>
public class SourceGroup3D : MonoBehaviour 
{

    /* PUBLIC MEMBERS */
    [SerializeField] List<AudioSource> _sources3D = new List<AudioSource>();
    [Tooltip("If true, disables all sources on startup")]
    [SerializeField] bool _disabledOnStart = false;
    [SerializeField] string _defaultSoundName = "";

    /* PRIVATE MEMBERS */

    List<AudioManagerExtension> _extensions = new List<AudioManagerExtension>();

    Dictionary<AudioSource, float> _nativeVolume = new Dictionary<AudioSource, float>();

    /* PROPERTIES */
    
    /// <summary>
    /// True if the sources of this group are enabled or not
    /// </summary>
    public bool Active { get; private set; }

    /* METHODS */

    #region Unity Functions

    private void Start()
    {
        // initiailize sources
        for(int i = _sources3D.Count - 1; i >= 0; i--)
        {
            if (_sources3D[i] == null)
            {
                _sources3D.RemoveAt(i);
            }
            else
            {
                // find out if the source has an AMExtension linked to it
                var ame = _sources3D[i].GetComponent<AudioManagerExtension>();
                if (ame != null && ame.overrideSource != null)
                {
                    _extensions.Add(ame);
                }
                else if (_defaultSoundName != "")
                {
                    AudioManager.Play(_defaultSoundName, overrideSource: _sources3D[i]);
                    _nativeVolume[_sources3D[i]] = _sources3D[i].volume;
                }
            }
        }

        // make sure we don't prematurely initialize
        if (_extensions.Count > 0)
        {
            StartCoroutine(WaitForInitializedExtensions(!_disabledOnStart));
        }
        else
        {
            SetGroupState(!_disabledOnStart);
        }
    }

    /// <summary>
    /// sets the initial group state after all connected AMExtension instances
    /// have finished initializing
    /// </summary>
    /// <param name="finalSetValue"> the initial enabled value for
    /// each source in this group</param>
    /// <returns></returns>
    IEnumerator WaitForInitializedExtensions(bool finalSetValue)
    {
        bool allAreDone = false;
        while (!allAreDone)
        {
            yield return null;
            allAreDone = true;
            foreach(var ame in _extensions)
            {
                if (!ame.Initialized)
                {
                    allAreDone = false;
                }
            }
        }
        foreach (var ame in _extensions)
        {
            _nativeVolume[ame.overrideSource] = ame.overrideSource.volume;
        }
        SetGroupState(finalSetValue);
    }

    #endregion

    /// <summary>
    /// "Enables" or "disables" the sources of this group
    /// </summary>
    /// <param name="value"></param>
    /// <remarks> does not actual enable or disable, simple changes volume so all sources
    /// can stay in sync</remarks>
    public void SetGroupState(bool value)
    {
        Active = value;
        foreach (AudioSource s in _sources3D)
        {
            if (value)
            {
                if (_nativeVolume.ContainsKey(s))
                {
                    s.volume = _nativeVolume[s];
                }
                else
                {
                    s.volume = 1f;
                }
            }
            else
            {
                s.volume = 0f;
            }
        }
    }

}
