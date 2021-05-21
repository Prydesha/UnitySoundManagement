Last Update: 4/03/2021
Author: Shawn Michael Pryde

Summary: This "package" is designed to easily implement audio 
into Unity projects using c#. It provides methods for audio 
organization, playing & fading audio, as well as implementing
3D audio sources


Important Notes and Tutorials:
 - To create a new sound:
	1.	In the project hierarchy, navigate to the 
		folder in which you want the sound data to be stored.
	2.	Right click the folder and select Create > Audio > Sound
	3.	Rename the sound to be something sensible, and assign
		its member variables as you see fit.
		
		Note: it is not advisable to change the name of a Sound after
		you have already added implementations for it in your project

 - To create a collection of sounds:
	1.	In the project hierarchy, navigate to the 
		folder in which you want the sound data to be stored.
	2.	Right click the folder and select Create > Audio > Sound Collection
	3.	Add any pre-existing sounds you like to the collection's "sounds" list
	4.	I recommend putting all of a collection's sounds within a folder 
		very close to the collection item. Example project hierarchy:
		
		AudioFolder / 
			[SoundCollectionName]
			[SoundCollectionName]Items /
				sound1
				sound2
				...
		
		Note: A collection of sounds is particularly useful for being able to
		play a random sound from the collection at runtime.

 - To make your sounds accessible in the current scene:
	1.	Create a new empty GameObject
	2.	Add the Audio Manager component to it
	3.	Add any sounds or collections you want to use in your 
		game to the audio manager's "sounds" list

 - To play a sound from a script:
	It's easy! You don't even need a reference to the Audio
	Manager object. Simply use the static functions present 
	in the AudioManager class. Examples:
		AudioManager.Play("HurtSFX");
		AudioManager.PlayRandomFromCollection("ExplosionSoundCollection");
		AudioManager.Fade("BackgroundMusic");
		AudioManager.FadeOutAdvanced("BackgroundMusic", true, true, 2f, 1.5f, 0.5f, false);

	Note: when asking the manager to play a specific item, 
	the name parameter of your function call must match the
	name of the sound or collection in your project's
	hierarchy


Here is a list of scripts included in the
AudioManagement Package that can be used in scenes:

 - AudioManager:
	This is the base script which does all of the work. In order 
	for a sound to play, it must go through the scene's audio manager.
	I recommend putting one of these scripts on a prefab (with no 
	other components) and keeping a copy of that prefab in all of your scenes.
	
 - AudioManagerExtension:
	This class is used for scenes which don't have a script
	to ask the Audio Manager to play something. It fills 
	that role in their stead. This solves the problem of an 
	assigned AudioManager being overwritten by a new audio
	manager during scene transitions. Use this for unity events
    and buttons.
    
 - SourceGroup3D:
    You can use this script to represent a collection of 3D audio 
    sources which are connected to each other in scene. The AudioManager
    defaults all of its audio to be 2D unless an overriding 3DAudioSource
    is provided to one of its "Play" calls. This is particularly useful
    for a collection of diagetic sounds which need to be synchronized 
    between each other.


