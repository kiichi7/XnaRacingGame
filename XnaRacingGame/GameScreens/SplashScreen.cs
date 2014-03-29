// Project: RacingGame, File: SplashScreen.cs
// Namespace: RacingGame.GameScreens, Class: SplashScreen
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 35, Size of file: 816 Bytes
// Creation date: 24.09.2006 07:07
// Last modified: 24.09.2006 07:12
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
using RacingGame.GameLogic;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Splash screen
	/// </summary>
	class SplashScreen : IGameScreen
	{
		#region RenderSplashScreen
		/// <summary>
		/// Render splash screen
		/// </summary>
		public bool Render()
		{
			// This starts both menu and in game post screen shader!
			BaseGame.UI.PostScreenMenuShader.Start();

			// Render background and black bar
			BaseGame.UI.RenderMenuBackground();
			BaseGame.UI.RenderBlackBar(352, 61);

			// Show Press Start to continue.
			if ((int)(BaseGame.TotalTime / 0.375f) % 3 != 0)
				BaseGame.UI.Logos.RenderOnScreen(
					BaseGame.CalcRectangleCenteredWithGivenHeight(
					512, 352 + 61 / 2, 26, UIRenderer.PressStartGfxRect),
					UIRenderer.PressStartGfxRect);

			// Show logos
			BaseGame.UI.RenderLogos();

			// Clicking or pressing start will go to the menu
			return Input.MouseLeftButtonJustPressed ||
				Input.KeyboardSpaceJustPressed ||
				Input.KeyboardEscapeJustPressed ||
				Input.GamePadStartPressed;
		} // Render()
		#endregion
	} // class SplashScreen
} // namespace RacingGame.GameScreens
