using System;
using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Class used to represent an array of sound objects.
/// Create in Inspector.
/// </summary>
[Serializable][CreateAssetMenu(fileName = "New Sound Collection", menuName = "Audio/Sound Collection")]
public class ScriptableSoundCollection : ScriptableSoundItem
{
    #region Collection Variables
    // array of sounds in this collection.
    public List<ScriptableSoundItem> sounds = new List<ScriptableSoundItem>();

    public override void Accept(ScriptableSoundItemVisitor visitor)
    {
        visitor.VisitSoundCollection(this);
    }

    // Directory where this collection stores its sound items
    //private string _soundFolder = null;
    #endregion

    #region Editor variables
    /*
    [Header("Sound Collection Editor")]
    // This is an optional parameter for adding a sound item.
    // If provided, a newly created sound item will be created
    // from this clip
    [SerializeField] private AudioClip clipToAdd;
    */
    #endregion

    #region Outdated AssetDatabase editing function
    /*
    /// <summary>
    /// Creates a Sound object, then adds it
    /// to this collection
    /// </summary>
    public void AddNewSound()
    {
        var newSound = CreateInstance<Sound>();
        // If we don't already have a soundFolder, create it
        if (_soundFolder == null || _soundFolder == "")
        {
            string path = AssetDatabase.GetAssetPath(this);
            int lastSlash = path.LastIndexOf('/');
            string parentPath = path.Substring(0, lastSlash);
            string folderGuid = AssetDatabase.CreateFolder(parentPath, _name + " Sounds");
            _soundFolder = AssetDatabase.GUIDToAssetPath(folderGuid);
        }
        // find a good asset name for this asset
        #region without clip
        if (clipToAdd == null)
        {
            int iteration = 0;
            while (true)
            {
                var results = AssetDatabase.FindAssets("Sound." + iteration + ".sound", new[] { _soundFolder });
                bool taken = false;
                foreach (string GUID in results)
                {
                    taken = true;
                }
                if (!taken)
                {
                    AssetDatabase.CreateAsset(newSound,
                        _soundFolder + "/"+ "Sound." + iteration + ".sound.asset");
                    break;
                }
                iteration++;
            }
        }
        #endregion
        #region with clip
        else
        {
            newSound.SetClip(clipToAdd);
            int iteration = 0;
            while (true)
            {
                string[] results;
                if (iteration == 0)
                {
                    results = AssetDatabase.FindAssets(newSound.Name + ".sound");
                }
                else
                {
                    results = AssetDatabase.FindAssets(newSound.Name + "." + iteration + ".sound");
                }
                bool taken = false;
                foreach (string GUID in results)
                {
                    taken = true;
                }
                if (!taken)
                {
                    if (iteration == 0)
                    {
                        AssetDatabase.CreateAsset(newSound,
                            _soundFolder + "/"+ newSound.Name + ".sound.asset");
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(newSound,
                            _soundFolder + "/" + newSound.Name + "." + iteration + ".sound.asset");
                    }
                    break;
                }
                iteration++;
            }
        }
        #endregion
        // update the collection
        sounds.Add(newSound);
        clipToAdd = null;
    }
    */
    #endregion
}
