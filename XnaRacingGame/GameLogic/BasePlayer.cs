// Project: RacingGame, File: BasePlayer.cs
// Namespace: RacingGame.GameLogic, Class: BasePlayer
// Path: C:\code\RacingGame\GameLogic, Author: Abi
// Code lines: 188, Size of file: 5,42 KB
// Creation date: 03.10.2006 18:15
// Last modified: 07.11.2006 02:46
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameScreens;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Landscapes;
using RacingGame.Properties;
using RacingGame.Sounds;
using RacingGame.Tracks;
#endregion

namespace RacingGame.GameLogic
{
	/// <summary>
	/// Base player helper class, holds all the current game values:
	/// Game time, game over, level number, victory.
	/// More stuff that has impact to the CarController or ChaseCamera classes
	/// should be included here, else add them directly to the Player class,
	/// which handles all the game logic.
	/// For example adding items or powerups should be done in this class
	/// if they affect the speed or physics of our car.
	/// For multiplayer purposes this class should be extended to assign
	/// a player id to each player and link the network stuff up here.
	/// </summary>
	public class BasePlayer
	{
		#region Global game parameters (game time, game over, etc.)
		/// <summary>
		/// Current game time in ms. Used for time display in game. Also used to
		/// update the sun position and for the highscores.
		/// Will be stopped if we die or if we are still zooming in.
		/// </summary>
		protected float currentGameTimeMilliseconds = 0;

		/// <summary>
		/// Current lap. Increases and when we reach 3, the game is won.
		/// </summary>
		protected int lap = 0;

		/// <summary>
		/// Current lap
		/// </summary>
		public int CurrentLap
		{
			get
			{
				return lap;
			} // get
		} // CurrentLap

		/// <summary>
		/// Remember best lap time, unused until we complete the first lap.
		/// Then it is set every lap, always using the best and fastest lap time.
		/// </summary>
		private float bestLapTimeMilliseconds = 0;

		/// <summary>
		/// Best lap time we have archived in this game
		/// </summary>
		public float BestTimeMilliseconds
		{
			get
			{
				return bestLapTimeMilliseconds;
			} // get
		} // BestTimeMilliseconds

		/// <summary>
		/// Start new lap, will reset all lap variables and the game time.
		/// If all laps are done the game is over.
		/// </summary>
		protected void StartNewLap()
		{
			lap++;

			RacingGameManager.Landscape.StartNewLap();

			// Got new best time?
			if (bestLapTimeMilliseconds == 0 ||
				currentGameTimeMilliseconds < bestLapTimeMilliseconds)
				bestLapTimeMilliseconds = currentGameTimeMilliseconds;

			// Start at 0:00.00 again
			currentGameTimeMilliseconds = zoomInTime;
		} // StartNewLap()

		/// <summary>
		/// Game time ms, will return negative values if currently zooming in!
		/// </summary>
		/// <returns>Int</returns>
		public float GameTimeMilliseconds
		{
			get
			{
				return currentGameTimeMilliseconds - zoomInTime;
			} // get
		} // GameTimeMilliseconds

		/// <summary>
		/// How long do we zoom in: 3 seconds (counting down).
		/// </summary>
		public const int StartGameZoomTimeMilliseconds = 3000;//2000;//2500;//3000;

		/// <summary>
		/// Zoom in time
		/// </summary>
		private float zoomInTime = StartGameZoomTimeMilliseconds;

		/// <summary>
		/// Zoom in time
		/// </summary>
		/// <returns>Float</returns>
		protected float ZoomInTime
		{
			get
			{
				return zoomInTime;
			} // get
			set
			{
				zoomInTime = value;
			} // set
		} // ZoomInTime

		/// <summary>
		/// Won or lost?
		/// </summary>
		protected bool victory = false;

		/// <summary>
		/// Level num, set when starting game!
		/// </summary>
		protected int levelNum = 0;

		/// <summary>
		/// Game over?
		/// </summary>
		protected bool isGameOver = false;

		/// <summary>
		/// Is game over? True if both all lifes are used and no fuel is left.
		/// </summary>
		/// <returns>Bool</returns>
		public bool GameOver
		{
			get
			{
				return isGameOver;
			} // get
		} // GameOver

		/// <summary>
		/// Did the player win the game? Makes only sense if GameOver is true!
		/// </summary>
		public bool WonGame
		{
			get
			{
				return victory;
			} // get
		} // WonGame

		/// <summary>
		/// Remember if we already uploaded our highscore for this game.
		/// Don't do this twice (e.g. when pressing esc).
		/// </summary>
		private bool alreadyUploadedHighscore = false;

		/// <summary>
		/// Set game over and upload highscore
		/// </summary>
		public void SetGameOverAndUploadHighscore()
		{
			// Set gameOver to true to mark this game as ended.
			isGameOver = true;

			// Upload highscore
			if (alreadyUploadedHighscore == false)
			{
				alreadyUploadedHighscore = true;
				Highscores.SubmitHighscore(levelNum,
					(int)currentGameTimeMilliseconds);
			} // if (alreadyUploadedHighscore)
		} // SetGameOverAndUploadHighscore()

		/// <summary>
		/// Helper to determinate if user can control the car.
		/// If game just started we still zoom into the chase camera.
		/// </summary>
		/// <returns>Bool</returns>
		public bool CanControlCar
		{
			get
			{
				return zoomInTime <= 0 &&
					GameOver == false;
			} // get
		} // CanControlCar
		#endregion

		#region Reset everything for starting a new game
		/// <summary>
		/// Reset all player entries for restarting a game.
		/// In derived classes reset all the variables we need to reset for
		/// a new game there (e.g. car speed in CarController or
		/// cameraWobbel in ChaseCamera).
		/// </summary>
		public virtual void Reset()
		{
			levelNum = TrackSelection.SelectedTrackNumber;
			isGameOver = false;
			alreadyUploadedHighscore = false;
			currentGameTimeMilliseconds = 0;
			bestLapTimeMilliseconds = 0;
			lap = 0;
			zoomInTime = StartGameZoomTimeMilliseconds;
		} // Reset()

		/// <summary>
		/// Clear variables for game over
		/// </summary>
		public virtual void ClearVariablesForGameOver()
		{
		} // ClearVariablesForGameOver()
		#endregion

		#region Handle game logic
		/// <summary>
		/// Update game logic, called every frame. In Rocket Commander we did
		/// all the game logic in one big method inside the player class, but it
		/// was hard to add new game logic and many small things were also in
		/// the GameAsteroidManager. For this game we split everything up into
		/// much more classes and every class handles only its own variables.
		/// For example this class just handles the game time and zoom in time,
		/// for the car speed and physics just go into the CarController class.
		/// </summary>
		public virtual void Update()
		{
			// Handle zoomInTime at the beginning of a game
			if (zoomInTime > 0)
			{
				// Handle start traffic light object (red, yellow, green!)
				RacingGameManager.Landscape.ReplaceStartLightObject(
					//zoomInTime > 1000 ? 2 : 1);
					2-(int)((zoomInTime+1000)/1000));
				zoomInTime -= BaseGame.ElapsedTimeThisFrameInMilliseconds;
				if (zoomInTime < 0)
				{
					zoomInTime = 0;
					RacingGameManager.Landscape.ReplaceStartLightObject(2);
				} // if
			} // if (zoomInTime)

			// Don't handle any more game logic if game is over or still zooming in.
			if (CanControlCar == false)
				return;

			// Increase game time
			currentGameTimeMilliseconds +=
				BaseGame.ElapsedTimeThisFrameInMilliseconds;
		} // Update()
		#endregion
	} // class BasePlayer
} // namespace RacingGame.GameLogic
