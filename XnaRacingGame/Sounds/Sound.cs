// Project: RacingGame, File: Sound.cs
// Namespace: RacingGame.Sounds, Class: Sound
// Path: C:\code\RacingGame\Sounds, Author: Abi
// Code lines: 729, Size of file: 21,38 KB
// Creation date: 23.10.2006 17:21
// Last modified: 23.10.2006 23:28
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
#endregion

namespace RacingGame.Sounds
{
	/// <summary>
	/// Sound
	/// </summary>
	class Sound
	{
		#region Variables
		/// <summary>
		/// Sound stuff for XAct
		/// </summary>
		static AudioEngine audioEngine;
		/// <summary>
		/// Wave bank
		/// </summary>
		static WaveBank waveBank;
		/// <summary>
		/// Sound bank
		/// </summary>
		static SoundBank soundBank;
		/// <summary>
		/// Gears category to change volume and pitching of gear sounds.
		/// </summary>
		static AudioCategory gearsCategory;
		#endregion

		#region Enums
		/// <summary>
		/// Sounds we use in this game. This are all the sounds and even the
		/// music, only the gear sounds are handled seperately below.
		/// </summary>
		/// <returns>Enum</returns>
		public enum Sounds
		{
			// Menu Sounds
			ButtonClick,
			ScreenClick,
			ScreenBack,
			Highlight,
			// Game Sounds
			Startsign,
			BrakeCurveMajor,
			BrakeCurveMinor,
			BrakeMajor,
			BrakeMinor,
			CarCrashMinor,
			CarCrashTotal,
			// Additional Game Sounds (gear sounds are extra)
			CheckpointBetter,
			CheckpointWorse,
			Victory,
			CarLose,
			// Music
			MenuMusic,
			GameMusic,
		} // enum Sounds
		#endregion

		#region Constructor
		/// <summary>
		/// Private constructor to prevent instantiation.
		/// </summary>
		private Sound()
		{
		} // Sound()

		/// <summary>
		/// Create sound.
		/// 
		/// Note: Ignore FxCop warning because the XNA framework
		/// does not tell us which exceptions could be thrown and this
		/// constantly happens in unit tests that sound files are missing
		/// or loading fails for some reason.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification="We don't want the app to not work just because no "+
			"sound device was found or something is wrong with the sound project. "+
			"We can log this error and savely ignore it, the rest of the game "+
			"will still run the same. If the user wants to fix sound, reinstall " +
			"game and make sure he has a working sound device.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Performance",
			"CA1810:InitializeReferenceTypeStaticFieldsInline",
			Justification = "Just a pain in the ass this warning, unsolvable " +
			"without changing hundreds lines of code ... also makes no sense, is " +
			"not a performance factor! This static code is only called once and "+
			"its fast.")]
		static Sound()
		{
			try
			{
				string dir = Directories.SoundsDirectory;
				audioEngine = new AudioEngine(
					Path.Combine(dir, "RacingGameManager.xgs"));
				waveBank = new WaveBank(audioEngine,
					Path.Combine(dir, "Wave Bank.xwb"));

				// Dummy wavebank call to get rid of the warning that waveBank is
				// never used (well it is used, but only inside of XNA).
				if (waveBank != null)
					soundBank = new SoundBank(audioEngine,
						Path.Combine(dir, "Sound Bank.xsb"));

				// Get the gears category to change volume and pitching of gear sounds.
				gearsCategory = audioEngine.GetCategory("Gears");
			} // try
			catch (Exception ex)
			{
				// Audio creation crashes in early xna versions, log it and ignore it!
				Log.Write("Failed to create sound class: " + ex.ToString());
			} // catch
		} // Sound()
		#endregion

		#region Play
		/// <summary>
		/// Play
		/// </summary>
		/// <param name="soundName">Sound name</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification="Can fail, but should not mess up our calling code!")]
		public static void Play(string soundName)
		{
			if (soundBank == null)
				return;

			try
			{
				soundBank.PlayCue(soundName);
				//for this we have to remember cue and store it:
				//old: soundBank.GetCue(soundName).Play();
			} // try
			catch (Exception ex)
			{
				Log.Write("Playing sound " + soundName + " failed: " + ex.ToString());
			} // catch
		} // Play(soundName)

		/// <summary>
		/// Play
		/// </summary>
		/// <param name="sound">Sound</param>
		public static void Play(Sounds sound)
		{
			Play(sound.ToString());
		} // Play(sound)
		#endregion

		#region StopMusic
		/// <summary>
		/// Stop music
		/// </summary>
		public static void StopMusic()
		{
			if (soundBank == null)
				return;

			// Use a little trick, start new music, but use the cue. This will
			// replace the old music, then stop the music and everything is stopped!
			Cue musicCue = soundBank.GetCue("MenuMusic");
			musicCue.Play();
			// Wait for a short while to let Xact kick in ^^
			Thread.Sleep(10);
			musicCue.Stop(AudioStopOptions.Immediate);
		} // StopMusic()
		#endregion

		#region Play brake sound
		/// <summary>
		/// Prevent playing brake sounds on top of each other with help of this
		/// variable.
		/// </summary>
		static float brakeSoundStillPlayingMs = 1000;
		/// <summary>
		/// Play brake sound
		/// </summary>
		/// <param name="speed">Speed</param>
		/// <param name="speedChange">Speed change</param>
		/// <param name="rotationChange">Rotation change</param>
		public static void PlayBrakeSound(
			float speed, float speedChange, float rotationChange)
		{
			// Only start new brake sound if not currently playing one
			if (brakeSoundStillPlayingMs <= 0 &&
				// Don't play anything like this sounds in the menu mode!
				RacingGameManager.InMenu == false)
			{
				// Currently in heavy rotation?
				bool inRotation =  (rotationChange >
					0.25f * Player.MaxRotationPerSec * BaseGame.MoveFactorPerSecond);

				Sounds soundBrakeType = inRotation ?
					Sounds.BrakeCurveMinor : Sounds.BrakeMinor;
				if (speed > 1.5f &&
					Math.Abs(speedChange) > 5 * BaseGame.MoveFactorPerSecond)
				{
					soundBrakeType = inRotation ?
						Sounds.BrakeCurveMajor : Sounds.BrakeMajor;
				} // if (speed)

				// Play sound
				Play(soundBrakeType);

				// Wait until sound is done (and a little bit longer)
				switch (soundBrakeType)
				{
					case Sounds.BrakeMinor:
						brakeSoundStillPlayingMs = 750;
						break;
					case Sounds.BrakeMajor:
						brakeSoundStillPlayingMs = 2500;
						break;
					case Sounds.BrakeCurveMinor:
						brakeSoundStillPlayingMs = 1250;
						break;
					case Sounds.BrakeCurveMajor:
						brakeSoundStillPlayingMs = 3500;
						break;
				} // switch
			} // if (brakeSoundStillPlayingMs)
		} // PlayBrakeSound(speed, speedChange, rotationChange)
		#endregion

		#region PlayCrashSound
		/// <summary>
		/// Prevent playing brake sounds on top of each other with help of this
		/// variable.
		/// </summary>
		static float crashSoundStillPlayingMs = 2000;

		/// <summary>
		/// Play crash sound
		/// </summary>
		/// <param name="totalCrash">Total crash</param>
		public static void PlayCrashSound(bool totalCrash)
		{
			// Only start new brake sound if not currently playing one
			if (crashSoundStillPlayingMs <= 0 &&
				// Don't play anything like this sounds in the menu mode!
				RacingGameManager.InMenu == false)
			{
				Sound.Play(totalCrash ? Sounds.CarCrashTotal : Sounds.CarCrashMinor);
				// Wait a while for the next crash sound
				crashSoundStillPlayingMs = totalCrash ? 3456 : 2345;
			} // if (crashSoundStillPlayingMs)
		} // PlayCrashSound(totalCrash)
		#endregion

		#region Gear Sounds
		#region Gear Constants
		/// <summary>
		/// Number of gears we got in this game.
		/// </summary>
		const int NumberOfGears = 5;

		/// <summary>
		/// All gear change sounds are 1200 ms long.
		/// </summary>
		const int GearChangeSoundLengthInMs = 1200;
		
		/// <summary>
		/// Constants for the gear sounds
		/// </summary>
		const float stayingVol = 0.5f;//0.25f;//0.1f;
		/// <summary>
		/// Volumes for each gear, currently all set to 100% because we
		/// adjusted the gear volumes in Xact already.
		/// </summary>
		static readonly float[] vol =
			new float[NumberOfGears] { 1, 1, 1, 1, 1 };
		/// <summary>
		/// Minimum pitch for each gear, this is the sound pitch you will hear
		/// when the gear is at the very start. It goes up to maxPitch and
		/// then the next gear is initiated.
		/// </summary>
		static readonly float[] minPitch =
			//very heavy:
			//new float[NumberOfGears] { -0.75f, -0.75f, -0.69f, -0.50f, -0.41f };
			//not so strong:
			new float[NumberOfGears] { -0.375f, -0.375f, -0.345f, -0.25f, -0.205f };

		/// <summary>
		/// Max pitch for each gear.
		/// </summary>
		static readonly float[] maxPitch =
			//very heavy:
			//new float[NumberOfGears] { 0.68f, 0.54f, 0.54f, 0.49f, 0.20f };
			//not so strong:
			//new float[NumberOfGears] { 0.34f, 0.27f, 0.27f, 0.245f, 0.10f };
			//even weaker, sounds better:
			new float[NumberOfGears] { 0.24f, 0.17f, 0.17f, 0.145f, 0.10f };
		#endregion

		#region Gear Variables
		/// <summary>
		/// Current gear we are playing, this might not be the gear that we
		/// actually have calculated from the speed. It will be the same most
		/// of the time, but this might lag behind because the gear change
		/// sound takes a while (1.2sec).
		/// 
		/// Gear the car has in the game, while this is closely linked to the
		/// speed of the car, this does NOT mean we will hear the gear right away.
		/// First of all we are going to initiate a gear change sound if the gear
		/// is not currently playing and then we advance to the next gear.
		/// Droping down a gear is done very quickly after reaching the minimum
		/// pitch of the current gear.
		/// For display this gear value should be used. Speed should be accurate,
		/// but gears are sound dependant (at least in this game) ^^
		/// </summary>
		static int currentGear = 0;//-1;

#if DEBUG
		/// <summary>
		/// Current gear of car
		/// </summary>
		static public int CurrentGear
		{
			get
			{
				return currentGear;
			} // get
		} // CurrentGear
#endif
		#endregion

		#region PlayGearSound
		static Cue currentGearCue = null;
		static Cue currentGearChangeCue = null;
		static float gearChangeSoundInitiatedMs = 0;
		/// <summary>
		/// Play gear sound
		/// </summary>
		/// <param name="soundName">Gear sound type</param>
		private static void PlayGearSound(string soundName)
		{
			if (soundName.Contains("To"))
			{
				// Gear sound is automatically replaced!
				currentGearChangeCue = soundBank.GetCue(soundName);
				currentGearChangeCue.Play();
				gearChangeSoundInitiatedMs = GearChangeSoundLengthInMs;
				currentGearCue = null;
			} // if (soundName.Contains)
			else
			{
				// Gear change sound is automatically replaced!
				currentGearCue = soundBank.GetCue(soundName);
				currentGearCue.Play();
				currentGearChangeCue = null;
			} // else
		} // PlayGearSound(soundName)
		#endregion

		#region Change gear volume and pitch
		/// <summary>
		/// Update gear volume and pitch
		/// </summary>
		/// <param name="gearSound">Gear sound</param>
		/// <param name="volume">Volume</param>
		/// <param name="pitch">pitch</param>
		private static void UpdateGearVolumeAndPitch(
			string gearSound, float volume,	float pitch)//freq)
		{
			// Gear changing in progress?
			if (gearChangeSoundInitiatedMs > 0)
			{
				gearChangeSoundInitiatedMs -=
					BaseGame.ElapsedTimeThisFrameInMilliseconds;
				// If gear change sound ends in this frame (max time - frameMs),
				// then start gear sound!
				if (gearChangeSoundInitiatedMs <= 0)
				{
					gearChangeSoundInitiatedMs = 0;
					PlayGearSound(gearSound);
					volume = lastGearVolume = 1.0f;
					pitch = lastGearPitch = -0.3f;//minPitch[newGear];
				} // if (gearChangeSoundInitiatedMs)
				/*unused:
				if (gearChangeSoundInitiatedMs > 1100)
					volume *= (gearChangeSoundInitiatedMs - 1100) / 100.0f;
				else if (gearChangeSoundInitiatedMs > 300)
				{
					volume = 0;
					if (currentGearCue != null &&
						currentGearCue.Name != gearSound)
						PlayGearSound(gearSound);
				} // else if
				else
					volume *= 1.0f - (gearChangeSoundInitiatedMs / 300.0f);
				 */
			} // if (gearChangeSoundInitiatedMs)

			// Set the global volume for this category
			gearsCategory.SetVolume(MathHelper.Clamp(volume, 0, 1));

//#if !XBOX360
			// Set pitch only if this is a gear sound
			if (currentGearCue != null)// &&
				//always: currentGearCue.Name.Contains("To") == false)
			{
				currentGearCue.SetVariable("Pitch",
//too strong:					100 * MathHelper.Clamp(pitch, -1, 1));
//too weak?					50 * MathHelper.Clamp(pitch, -1, 1));
					55 * MathHelper.Clamp(pitch, -1, 1));
					//100.0f * ((freq / 44100.0f) - 1.0f));
			} // if (currentGearCue)
//#endif
		} // UpdateGearVolumeAndPitch(volume, freq)
		#endregion

		#region Start gear sound
		/// <summary>
		/// Start gear sound
		/// </summary>
		public static void StartGearSound()
		{
			currentGear = 0;
			Sound.PlayGearSound("Gear1");
			Sound.UpdateGearVolumeAndPitch("Gear1", stayingVol, minPitch[0]);
		} // StartGearSound()
		#endregion

		#region Stop gear sound
		/// <summary>
		/// Start gear sound
		/// </summary>
		public static void StopGearSound()
		{
			// Stop everything
			currentGear = 0;
			if (currentGearChangeCue != null)
				currentGearChangeCue.Stop(AudioStopOptions.Immediate);
			currentGearChangeCue = null;
			if (currentGearCue != null)
				currentGearCue.Stop(AudioStopOptions.Immediate);
			currentGearCue = null;
		} // StartGearSound()
		#endregion

		#region Update gear sound
		static float lastGearVolume = stayingVol;
		static float lastGearPitch = 0;
		/// <summary>
		/// Update gear sound, must be called every frame to make sure we
		/// always have the most recent gear sounds. Especially the gear
		/// changing sounds depends on accuracity!
		/// </summary>
		public static void UpdateGearSound(float speed, float acceleration)
		{
			// Calculate new gear depending on the current speed
			int newGear = (int)(NumberOfGears * speed / Player.MaxPossibleSpeed);

			// Make sure newGear is between 0 and NumberOfGears
			if (newGear < 0)
				newGear = 0;
			if (newGear >= NumberOfGears)
				newGear = NumberOfGears-1;

			/*tst
			if (currentGear != newGear)
			{
				currentGear = newGear;
				PlayGearSound("Gear" + (currentGear + 1));
			} // if (currentGear)
			 */

			// We can only change gear if no other gear change sound is in progress
			if (gearChangeSoundInitiatedMs <= 0)
			{
				if (newGear > currentGear)
				{
					// Next gear
					Sound.PlayGearSound(
						"Gear" + (newGear) + "ToGear" + (newGear + 1));
					lastGearVolume = 1.0f;
					lastGearPitch = 0.0f;
				} // if (newGear)
				else if (newGear < currentGear)
				{
					// Previous gear, change immediately
					//Sound.PlayGearSound(
					//	"Gear" + (newGear + 2) + "ToGear" + (newGear + 1));
					Sound.PlayGearSound("Gear"+(newGear + 1));
					lastGearVolume = 1.0f;
					lastGearPitch = maxPitch[newGear];
				} // else if
				currentGear = newGear;
			} // if (gearChangeSoundInitiatedMs)

			// If negative, play gear1 sound and make sure we stay in gear1
			if (speed < 0)
				speed = MathHelper.Clamp(
					Math.Abs(speed), 0, Player.MaxPossibleSpeed / 5);
			float gearPercentage = (float)
				((int)((speed / Player.MaxPossibleSpeed) * 499) %
				(int)(500/NumberOfGears)) / 100.0f;
#if DEBUG
			remGearPercentage = gearPercentage;
#endif
			gearPercentage = MathHelper.Clamp(gearPercentage, 0, 1);

			float minVolume = currentGear > 0 ? vol[currentGear-1] : stayingVol;
			float maxVolume = vol[currentGear];
			float volume = MathHelper.Lerp(minVolume, maxVolume, gearPercentage);
			float pitch = MathHelper.Lerp(
				minPitch[currentGear], maxPitch[currentGear], gearPercentage);

			// If gear change sound is in progress, make sure pitch is untouched
			if (gearChangeSoundInitiatedMs > 0)
			{
				pitch = 0;
				//if (acceleration < 0.0f)
				//	lastGearPitch = 0;
			} // if
			
			// If accelerating use loud sounds.
			if (acceleration > 0.25f)
			{
				volume = 1.0f;
			} // if
			else
			{
				// If staying around or not accelerating, make a little quieter
				volume /= 1.75f;
				// If slowing down do not go above 0 for the pitch, sounds wrong!
				pitch = Math.Min(-0.025f, pitch / 1.25f);
				if (lastGearPitch > pitch)
					lastGearPitch = lastGearPitch * 0.9f + pitch * 0.1f;
			} // else
			
			// Slowly interpolate volume and pitch, abrupt changes don't sound cool.
			// Always start with min/max pitch if we are in a new gear (see above).
			// Changes between gears and if accelerating or not should also be smooth.
			lastGearVolume = MathHelper.Lerp(lastGearVolume, volume,
				5.0f*BaseGame.MoveFactorPerSecond);
			lastGearPitch = MathHelper.Lerp(lastGearPitch, pitch,
				5.0f*BaseGame.MoveFactorPerSecond);
			Sound.UpdateGearVolumeAndPitch(
				"Gear" + (currentGear + 1), lastGearVolume, lastGearPitch);
			
#if DEBUG
			//tst:
			remVolume = lastGearVolume;
			remPitch = lastGearPitch;
#endif
		} // UpdateGearSound()
#if DEBUG
		static float remGearPercentage = 0;
		static float remVolume = 0;
		static float remPitch = 0;
#endif
		#endregion
		#endregion

		#region Update
		/// <summary>
		/// Update, just calls audioEngine.Update!
		/// </summary>
		public static void Update()
		{
			if (brakeSoundStillPlayingMs > 0)
				brakeSoundStillPlayingMs -= BaseGame.ElapsedTimeThisFrameInMilliseconds;
			if (crashSoundStillPlayingMs > 0)
				crashSoundStillPlayingMs -= BaseGame.ElapsedTimeThisFrameInMilliseconds;
			
			if (audioEngine != null)
				audioEngine.Update();
		} // Update()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test play click sound
		/// </summary>
		//[Test]
		public static void TestPlayClickSound()
		{
			//int crazyCounter = 0;

			TestGame.Start(
				delegate
				{
					if (Input.MouseLeftButtonJustPressed ||
						Input.GamePadAJustPressed)
						Sound.Play(Sounds.ButtonClick);
					else if (Input.MouseRightButtonJustPressed ||
						Input.GamePadBJustPressed)
						Sound.Play(Sounds.Highlight);
					else if (Input.KeyboardKeyJustPressed(Keys.D1))
						Sound.Play(Sounds.GameMusic);
					else if (Input.KeyboardKeyJustPressed(Keys.D2))
						Sound.Play(Sounds.MenuMusic);
					else if (Input.KeyboardKeyJustPressed(Keys.D3))
						Sound.Play(Sounds.BrakeCurveMajor);
					else if (Input.KeyboardKeyJustPressed(Keys.D4))
						Sound.Play(Sounds.BrakeMinor);
					else if (Input.KeyboardKeyJustPressed(Keys.D5))
						Sound.Play(Sounds.CarCrashTotal);
					else if (Input.KeyboardKeyJustPressed(Keys.D6))
						Sound.Play(Sounds.Startsign);
						/*tst
					else if (Input.KeyboardKeyJustPressed(Keys.D6))
						Sound.Play(Sounds.Gear1);
					else if (Input.KeyboardKeyJustPressed(Keys.D7))
					{
						Sound.Play(Sounds.Gear1ToGear2);
						Thread.Sleep(500);
						Sound.Play(Sounds.Gear2);
					} // else if
					else if (Input.KeyboardKeyJustPressed(Keys.D8))
					{
						Sound.Play(Sounds.Gear2ToGear3);
						Thread.Sleep(500);
						Sound.Play(Sounds.Gear3);
					} // else if
					else if (Input.KeyboardKeyJustPressed(Keys.D9))
					{
						Sound.Play(Sounds.Gear3ToGear4);
						Thread.Sleep(500);
						Sound.Play(Sounds.Gear4);
					} // else if
					else if (Input.KeyboardKeyJustPressed(Keys.D0))
					{
						Sound.Play(Sounds.Gear4ToGear3);
						Thread.Sleep(10);
						Sound.Play(Sounds.Gear3);
					} // else if
						 */

					TextureFont.WriteText(2, 30,
						"Press 0-9 or A/B or left/right mouse buttons to play back "+
						"sounds!");
				});
		} // TestPlayClickSound()

		public static void TestGearSoundsWithUI()
		{
			float speed = 0.0f;
			float lastAcceleration = 0.0f;

			TestGame.Start(
				delegate
				{
					Sound.StartGearSound();
				},
				delegate
				{
					// Fix Camera Rotation (we don't want any rotations here)
					BaseGame.ViewMatrix = Matrix.CreateRotationX(-MathHelper.Pi/2);
					BaseGame.UI.RenderGameBackground();

					// Speed sound
					float thisAcceleration = 0;
					if (Input.KeyboardUpPressed ||
						Input.GamePadUpPressed)
					{
						speed += BaseGame.MoveFactorPerSecond * 4;// 2;
						thisAcceleration = 1;
						if (speed > Player.MaxPossibleSpeed)
							speed = Player.MaxPossibleSpeed;
					} // if (Input.KeyboardUpPressed)
					else if (Input.KeyboardDownPressed ||
						Input.GamePadDownPressed)
					{
						speed -= BaseGame.MoveFactorPerSecond * 4;// 2;
						thisAcceleration = -1;
					} // else if
					speed -= speed * 0.025f * BaseGame.MoveFactorPerSecond;
					Sound.UpdateGearSound(speed, thisAcceleration);
					
					float newAcceleration =
						0.5f * speed / CarPhysics.MaxPossibleSpeed +
						0.5f*thisAcceleration;
					lastAcceleration =
						MathHelper.Lerp(lastAcceleration, newAcceleration,
						0.5f * BaseGame.MoveFactorPerSecond);

					TextureFont.WriteText(25, 130,
						"Press up and down cursors to speed up and slow down");
					TextureFont.WriteText(25, 170,
						"Speed: "+(speed * Player.MeterPerSecToMph));
					TextureFont.WriteText(25, 210,
						"MaxSpeed: " +
						(Player.MaxPossibleSpeed * Player.MeterPerSecToMph));
					TextureFont.WriteText(25, 250,
						"Acceleration: "+lastAcceleration);
					TextureFont.WriteText(25, 290,
						"Gear percentage: "+remGearPercentage);
					TextureFont.WriteText(25, 330,
						"Volume: "+remVolume);
					TextureFont.WriteText(25, 370,
						"Pitch: "+remPitch);
					TextureFont.WriteText(25, 410,
						"gearChangeSoundInitiatedMs: "+gearChangeSoundInitiatedMs);
					TextureFont.WriteText(25, 450,
						"ElapsedTimeThisFrameInMilliseconds: "+
						BaseGame.ElapsedTimeThisFrameInMilliseconds);

					BaseGame.UI.RenderGameUI(
						0, 0, 1,
						speed * Player.MeterPerSecToMph,
						1+Sound.CurrentGear,//1+(int)(5*speed/Player.MaxSpeed),
						lastAcceleration,
						"Beginner",
						new int[] { 0, 0, 0, 0, 0 });
				});
		} // TestPlayClickSound()
#endif
		#endregion
	} // class Sound
} // RacingGameManager.Sounds
