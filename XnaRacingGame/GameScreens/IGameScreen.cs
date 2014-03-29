// Project: RacingGame, File: IGameScreen.cs
// Namespace: RacingGame.GameScreens, Class: 
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 29, Size of file: 892 Bytes
// Creation date: 12.09.2006 07:22
// Last modified: 24.09.2006 07:11
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Game screen helper interface for all game screens of our game.
	/// Helps us to put them all into one list and manage them in our RaceGame.
	/// </summary>
	public interface IGameScreen
	{
		/// <summary>
		/// Run game screen. Called each frame. Returns true if we want to exit it.
		/// </summary>
		bool Render();
	} // interface IGameScreen
} // namespace RacingGame.GameScreens
