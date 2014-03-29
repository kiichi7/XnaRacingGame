// Project: RacingGame, File: Player.cs
// Namespace: RacingGame.GameLogic, Class: Player
// Path: C:\code\RacingGame\GameLogic, Author: Abi
// Code lines: 224, Size of file: 6,71 KB
// Creation date: 12.09.2006 07:22
// Last modified: 20.10.2006 16:07
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.GameLogic
{
	/// <summary>
	/// Player helper class, holds all the current game properties:
	/// Fuel, Health, Speed, Lifes and Score.
	/// Note: This class is just used in RacingGame and we only have
	/// 1 instance of it for the current player for the current game.
	/// If we want to have more than 1 player (e.g. in multiplayer mode)
	/// you should add a multiplayer class and have all player instances there.
	/// </summary>
	public class Player : ChaseCamera
	{
		#region Variables
		/// <summary>
		/// Remember all lap times for the victory screen.
		/// </summary>
		private List<float> lapTimes = new List<float>();

		/// <summary>
		/// Add lap time
		/// </summary>
		/// <param name="setLapTime">Lap time</param>
		public void AddLapTime(float setLapTime)
		{
			lapTimes.Add(setLapTime);
		} // AddLapTime(setLapTime)

		/// <summary>
		/// If the car is in the air and does not reach the ground again
		/// for more than 5 seconds, its game over!
		/// </summary>
		//obs: private float inAirTimeoutMilliseconds = 0.0f;
		#endregion

		#region Constructor
		/// <summary>
		/// Create chase camera
		/// </summary>
		/// <param name="setCarPosition">Set car position</param>
		/// <param name="setCameraPos">Set camera pos</param>
		public Player(Vector3 setCarPosition)
			: base(setCarPosition)
		{
		} // Player(setCarPosition)
		#endregion

		#region Reset
		/// <summary>
		/// Reset player values.
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			lapTimes.Clear();
		} // Reset()
		#endregion

		#region Handle game logic
		/// <summary>
		/// Update game logic, called every frame.
		/// </summary>
		public override void Update()
		{
			// Don't handle any more game logic if game is over.
			if (RacingGameManager.InGame &&
				ZoomInTime <= 0)
			{
				// Game over? Then show end screen!
				if (isGameOver)
				{
					// Just rotate around, don't use camera class!
					cameraPos = CarPosition + new Vector3(0, -5, +20) +
						Vector3.TransformNormal(new Vector3(30, 0, 0),
						Matrix.CreateRotationZ(BaseGame.TotalTimeMilliseconds / 2593.0f));
					BaseGame.ViewMatrix = Matrix.CreateLookAt(
						cameraPos, CarPosition, CarUpVector);
					int rank = Highscores.GetRankFromCurrentTime(
						this.levelNum, (int)this.BestTimeMilliseconds);
					this.currentGameTimeMilliseconds = this.BestTimeMilliseconds;
					// Always show we reached lap 3
					this.lap = 2;

					if (victory)
					{
						// Display Victory message
						TextureFont.WriteTextCentered(
							BaseGame.Width / 2, BaseGame.Height / 7,
							"Victory! You won.",
							Color.LightGreen, 1.25f);

						// Show one of the trophies
						BaseGame.UI.GetTrophyTexture(
							// Select right one
							rank == 0 ? UIRenderer.TrophyType.Gold :
							rank == 1 ? UIRenderer.TrophyType.Silver :
							UIRenderer.TrophyType.Bronse).
							RenderOnScreen(new Rectangle(
							BaseGame.Width / 2 - BaseGame.Width / 8,
							BaseGame.Height / 2 - BaseGame.YToRes(10),
							BaseGame.Width / 4, BaseGame.Height * 2 / 5 ));
					} // if
					else
					{
						// Display game over message
						TextureFont.WriteTextCentered(
							BaseGame.Width / 2, BaseGame.Height / 7,
							"Game Over! You lost.",
							Color.Red, 1.25f);
					} // else
					for (int num = 0; num < lapTimes.Count; num++)
						TextureFont.WriteTextCentered(
							BaseGame.Width / 2,
							BaseGame.Height / 7 + BaseGame.YToRes(35) * (1 + num),
							"Lap " + (num + 1) + " Time: " +
							(((int)lapTimes[num]) / 60).ToString("00") + ":" +
							(((int)lapTimes[num]) % 60).ToString("00") + "." +
							(((int)(lapTimes[num] * 100)) % 100).ToString("00"),
							Color.White, 1.25f);
					TextureFont.WriteTextCentered(
						BaseGame.Width / 2,
						BaseGame.Height / 7 + BaseGame.YToRes(35) * (1 + lapTimes.Count),
						"Rank: " + (1+rank),
						Color.White, 1.25f);

					// Don't continue processing game logic
					return;
				} // if

				/*unused, see CarPhysics.cs:ApplyGravity
				// Check if car is in the air,
				// used to check if the player died.
				if (this.isCarOnGround == false)
					inAirTimeoutMilliseconds +=
						BaseGame.ElapsedTimeThisFrameInMilliseconds;
				else
				 *
					// Back on ground, reset
					inAirTimeoutMilliseconds = 0;
				*/
				// Flying is not not supported anymore
				// Game not over yet, check if we lost or won.
				// Check if we have fallen from the track
				float trackDistance = Vector3.Distance(CarPosition, groundPlanePos);
				if (trackDistance > 50)// ||
					//inAirTimeoutMilliseconds > 5.0f)
				{
					// Reset player variables (stop car, etc.)
					ClearVariablesForGameOver();

					// And indicate that game is over and we lost!
					isGameOver = true;
					victory = false;
					Sound.Play(Sound.Sounds.CarLose);
					
					// Also stop engine sound
					Sound.StopGearSound();
				} // if
				//*/

				// Finished all laps? Then we won!
				if (CurrentLap >= 3)
				{
					// Reset player variables (stop car, etc.)
					ClearVariablesForGameOver();

					// Then game is over and we won!
					isGameOver = true;
					victory = true;
					Sound.Play(Sound.Sounds.Victory);

					// Also stop engine sound
					Sound.StopGearSound();
				} // if
			} // if

			base.Update();
		} // Update()
		#endregion
	} // class Player
} // namespace RacingGame.GameLogic
