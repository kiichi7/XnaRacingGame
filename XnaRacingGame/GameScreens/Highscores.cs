// Project: RacingGame, File: Highscores.cs
// Namespace: RacingGame.GameScreens, Class: Highscores
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 447, Size of file: 13,31 KB
// Creation date: 12.09.2006 07:22
// Last modified: 22.10.2006 18:41
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RacingGame.Sounds;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Highscores
	/// </summary>
	/// <returns>IGame screen</returns>
	class Highscores : IGameScreen
	{
		#region Highscore helper class
		/// <summary>
		/// Highscore helper class
		/// </summary>
		private struct HighscoreInLevel
		{
			#region Variables
			/// <summary>
			/// Player name
			/// </summary>
			public string name;
			/// <summary>
			/// Highscore points 
			/// </summary>
			public int timeMilliseconds;
			#endregion

			#region Constructor
			/// <summary>
			/// Create highscore
			/// </summary>
			/// <param name="setName">Set name</param>
			/// <param name="setTimeMs">Set time ms</param>
			public HighscoreInLevel(string setName, int setTimeMs)
			{
				name = setName;
				timeMilliseconds = setTimeMs;
			} // HighscoreInLevel(setName, setTimeMs)
			#endregion

			#region ToString
			/// <summary>
			/// To string
			/// </summary>
			/// <returns>String</returns>
			public override string ToString()
			{
				return name + ":" + timeMilliseconds;
			} // ToString()
			#endregion
		} // struct HighscoreInLevel

		/// <summary>
		/// Number of highscores displayed in this screen.
		/// </summary>
		private const int NumOfHighscores = 10,
			NumOfHighscoreLevels = 3;

		/// <summary>
		/// List of remembered highscores.
		/// </summary>
		private static HighscoreInLevel[,] highscores = null;

		/// <summary>
		/// Write highscores to string. Used to save to highscores settings.
		/// </summary>
		private static void WriteHighscoresToSettings()
		{
			string saveString = "";
			for (int level = 0; level < NumOfHighscoreLevels; level++)
				for (int num = 0; num < NumOfHighscores; num++)
        {
					saveString += (saveString.Length == 0 ? "" : ", ") +
						highscores[level, num];
        } // for for (num)

			GameSettings.Default.Highscores = saveString;
		} // WriteHighscoresToSettings()

		/// <summary>
		/// Read highscores from settings
		/// </summary>
		/// <returns>True if reading succeeded, false otherwise.</returns>
		private static bool ReadHighscoresFromSettings()
		{
			if (String.IsNullOrEmpty(GameSettings.Default.Highscores))
				return false;

			string highscoreString = GameSettings.Default.Highscores;
			string[] allHighscores = highscoreString.Split(new char[] { ',' });
			for (int level = 0; level < NumOfHighscoreLevels; level++)
				for (int num = 0; num < NumOfHighscores &&
					level*NumOfHighscores + num < allHighscores.Length; num++)
				{
					string[] oneHighscore =
						allHighscores[level*NumOfHighscores+num].
						Split(new char[] { ':' });
					highscores[level, num] = new HighscoreInLevel(
						oneHighscore[0], Convert.ToInt32(oneHighscore[1]));
				} // for for (num)

			return true;
		} // ReadHighscoresFromSettings()
		#endregion

		#region Static constructor
		/// <summary>
		/// Create Highscores class, will basically try to load highscore list,
		/// if that fails we generate a standard highscore list!
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
			Justification="Just a pain in the ass this warning, unsolvable without "+
			"changing hundreds lines of code ... also makes no sense, is not a "+
			"performance factor! This static code is only called once and its fast.")]
		static Highscores()
		{
			// Init highscores
			highscores =
				new HighscoreInLevel[NumOfHighscoreLevels, NumOfHighscores];

			// Get player name from windows computer/user name if not set yet.
			if (String.IsNullOrEmpty(GameSettings.Default.PlayerName))
			{
				// And save it back
				GameSettings.Default.PlayerName = WindowsHelper.GetDefaultPlayerName();
			} // if (String.IsNullOrEmpty)

			if (ReadHighscoresFromSettings() == false)
			{
				// Generate default lists
				for (int level = 0; level < NumOfHighscoreLevels; level++)
				{
					if (11 - level < 10)
						highscores[level, 11 - level] =
							new HighscoreInLevel("Newbie", 130000 * (level + 1));
					if (10 - level < 10)
						highscores[level, 10 - level] =
							new HighscoreInLevel("Tom", 125000 * (level + 1));
					highscores[level, 9 - level] =
						new HighscoreInLevel("Kai", 120000 * (level + 1));
					highscores[level, 8 - level] =
						new HighscoreInLevel("Viper", 115000 * (level + 1));
					highscores[level, 7 - level] =
						new HighscoreInLevel("Netfreak", 110000 * (level + 1));
					highscores[level, 6 - level] =
						new HighscoreInLevel("Darky", 105000 * (level + 1));
					highscores[level, 5 - level] =
						new HighscoreInLevel("Waii", 100000 * (level + 1));
					highscores[level, 4 - level] =
						new HighscoreInLevel("Judge", 95000 * (level + 1));
					highscores[level, 3 - level] =
						new HighscoreInLevel("exDreamBoy", 90000 * (level + 1));
					highscores[level, 2 - level] =
						new HighscoreInLevel("Master_L", 85000 * (level + 1));
					if (1 - level >= 0)
						highscores[level, 1 - level] =
							new HighscoreInLevel("Freshman", 80000 * (level + 1));
					if (0 - level >= 0)
						highscores[level, 0 - level] =
							new HighscoreInLevel("abi", 75000 * (level + 1));
				} // for (level)

				WriteHighscoresToSettings();
			} // if (ReadHighscoresFromSettings)
		} // Highscores()
		#endregion

		#region Get top lap time
		/// <summary>
		/// Get top lap time
		/// </summary>
		/// <param name="level">Level</param>
		/// <returns>Best lap time</returns>
		public static float GetTopLapTime(int level)
		{
			return (float)highscores[level, 0].timeMilliseconds / 1000.0f;
		} // GetTopLapTime(level)
		#endregion

		#region Get top 5 rank lap times
		/// <summary>
		/// Get top 5 rank lap times
		/// </summary>
		/// <param name="level">Current level</param>
		/// <returns>Array of top 5 times</returns>
		public static int[] GetTop5LapTimes(int level)
		{
			return new int[]
				{
					highscores[level, 0].timeMilliseconds,
					highscores[level, 1].timeMilliseconds,
					highscores[level, 2].timeMilliseconds,
					highscores[level, 3].timeMilliseconds,
					highscores[level, 4].timeMilliseconds,
				};
		} // GetTop5LapTimes()
		#endregion

		#region Get rank from current score
		/// <summary>
		/// Get rank from current time.
		/// Used in game to determinate rank while flying around ^^
		/// </summary>
		/// <param name="level">Level</param>
		/// <param name="timeMilisec">Time ms</param>
		/// <returns>Int</returns>
		public static int GetRankFromCurrentTime(int level, int timeMilliseconds)
		{
			// Time must be at least 1 second
			if (timeMilliseconds < 1000)
				// Invalid time, return rank 11 (out of highscore)
				return NumOfHighscores;

			// Just compare with all highscores and return the rank we have reached.
			for (int num = 0; num < NumOfHighscores; num++)
			{
				if (timeMilliseconds <= highscores[level, num].timeMilliseconds)
					return num;
			} // for (num)

			// No Rank found, use rank 11
			return NumOfHighscores;
		} // GetRankFromCurrentTime(level, timeMilliseconds)
		#endregion

		#region Submit highscore after game
		/// <summary>
		/// Submit highscore. Done after each game is over (won or lost).
		/// New highscore will be added to the highscore screen.
		/// In the future: Also send highscores to the online server.
		/// </summary>
		/// <param name="score">Score</param>
		/// <param name="levelName">Level name</param>
		public static void SubmitHighscore(int level, int timeMilliseconds)
		{
			// Search which highscore rank we can replace
			for (int num = 0; num < NumOfHighscores; num++)
			{
				if (timeMilliseconds <= highscores[level, num].timeMilliseconds)
				{
					// Move all highscores up
					for (int moveUpNum = NumOfHighscores - 1; moveUpNum > num;
						moveUpNum--)
					{
						highscores[level, moveUpNum] = highscores[level, moveUpNum - 1];
					} // for (moveUpNum)

					// Add this highscore into the local highscore table
					highscores[level, num].name = GameSettings.Default.PlayerName;
					highscores[level, num].timeMilliseconds = timeMilliseconds;

					// And save that
					Highscores.WriteHighscoresToSettings();

					break;
				} // if (timeMilisec)
			} // for (num)

			// Else no highscore was reached, we can't replace any rank.
		} // SubmitHighscore(level, timeMilisec)
		#endregion

		#region Render
		int selectedLevel = 2;
		/// <summary>
		/// Render game screen. Called each frame.
		/// </summary>
		/// <returns>Bool</returns>
		public bool Render()
		{
			// This starts both menu and in game post screen shader!
			BaseGame.UI.PostScreenMenuShader.Start();

			// Render background
			BaseGame.UI.RenderMenuBackground();
			BaseGame.UI.RenderBlackBar(160, 498-160);

			// Highscores header
			BaseGame.UI.Logos.RenderOnScreenRelative1600(
#if XBOX360
				10 + 36, 18 + 26, UIRenderer.HeaderHighscoresGfxRect);
#else
				10, 18, UIRenderer.HeaderHighscoresGfxRect);
#endif

			// Track selection
			int xPos = BaseGame.XToRes(512-160*3/2 + 25);
			int yPos = BaseGame.YToRes(182);
			int lineHeight = BaseGame.YToRes(27);

			// Beginner track
			bool inBox = Input.MouseInBox(new Rectangle(
				xPos, yPos, BaseGame.XToRes(125), lineHeight));
			TextureFont.WriteText(xPos, yPos, "Beginner",
				selectedLevel == 0 ? Color.Yellow :
				inBox ? Color.White : Color.LightGray);
			if (inBox && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				selectedLevel = 0;
			} // if (inBox)
			xPos += BaseGame.XToRes(160 + 8);

			// Advanced track
			inBox = Input.MouseInBox(new Rectangle(
				xPos, yPos, BaseGame.XToRes(125), lineHeight));
			TextureFont.WriteText(xPos, yPos, "Advanced",
				selectedLevel == 1 ? Color.Yellow :
				inBox ? Color.White : Color.LightGray);
			if (inBox && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				selectedLevel = 1;
			} // if (inBox)
			xPos += BaseGame.XToRes(160 + 30 - 8);

			// Expert track
			inBox = Input.MouseInBox(new Rectangle(
				xPos, yPos, BaseGame.XToRes(125), lineHeight));
			TextureFont.WriteText(xPos, yPos, "Expert",
				selectedLevel == 2 ? Color.Yellow :
				inBox ? Color.White : Color.LightGray);
			if (inBox && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				selectedLevel = 2;
			} // if (inBox)

			// Also handle xbox controller input
			if (Input.GamePadLeftJustPressed ||
				Input.KeyboardLeftJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				selectedLevel = (selectedLevel + 2) % 3;
			} // if (Input.GamePadLeftJustPressed)
			else if (Input.GamePadRightJustPressed ||
				Input.KeyboardRightJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				selectedLevel = (selectedLevel + 1) % 3;
			} // else if

			int xPos1 = BaseGame.XToRes(300);
			int xPos2 = BaseGame.XToRes(350);
			int xPos3 = BaseGame.XToRes(640);

			// Draw seperation line
			yPos = BaseGame.YToRes(208);
			BaseGame.DrawLine(
				new Point(xPos1, yPos),
				new Point(xPos3+TextureFont.GetTextWidth("5:67:89"), yPos),
				new Color(192, 192, 192, 128));
			// And another one, looks better with 2 pixel height
			BaseGame.DrawLine(
				new Point(xPos1, yPos+1),
				new Point(xPos3 + TextureFont.GetTextWidth("5:67:89"), yPos + 1),
				new Color(192, 192, 192, 128));

			yPos = BaseGame.YToRes(220);

			// Go through all highscores
			for (int num = 0; num < NumOfHighscores; num++)
			{
				Rectangle lineRect = new Rectangle(
					0, yPos, BaseGame.Width, lineHeight);
				Color col = Input.MouseInBox(lineRect) ?
					Color.White : new Color(200, 200, 200);
				TextureFont.WriteText(xPos1, yPos,
					(1 + num) + ".", col);
				TextureFont.WriteText(xPos2, yPos,
					highscores[selectedLevel, num].name, col);

				TextureFont.WriteGameTime(xPos3, yPos,
					highscores[selectedLevel, num].timeMilliseconds,
					Color.Yellow);

				yPos += lineHeight;
			} // for (num)

			BaseGame.UI.RenderBottomButtons(true);

			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBJustPressed ||
				Input.GamePadBackJustPressed ||
				Input.MouseLeftButtonJustPressed &&
				// Don't allow clicking on the controls to quit
				Input.MousePos.Y > yPos)
				return true;

			return false;
		} // Render()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test Highscores
		/// </summary>
		static public void TestHighscores()
		{
			Highscores highscoresScreen = null;
			TestGame.Start(
				delegate
				{
					highscoresScreen = new Highscores();
					RacingGameManager.AddGameScreen(highscoresScreen);
				},
				delegate
				{
					highscoresScreen.Render();
				});
		} // TestHighscores()
#endif
		#endregion
	} // class Highscores
} // namespace RacingGame.GameScreens