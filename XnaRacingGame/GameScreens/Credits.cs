// Project: RacingGame, File: Credits.cs
// Namespace: RacingGame.GameScreens, Class: Credits
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 80, Size of file: 1,84 KB
// Creation date: 12.09.2006 07:22
// Last modified: 22.10.2006 18:41
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Properties;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Credits
	/// </summary>
	/// <returns>IGame screen</returns>
	class Credits : IGameScreen
	{
		#region Render
		/// <summary>
		/// Render game screen. Called each frame.
		/// </summary>
		public bool Render()
		{
			// This starts both menu and in game post screen shader!
			BaseGame.UI.PostScreenMenuShader.Start();

			// Render background and black bar
			BaseGame.UI.RenderMenuBackground();
			//BaseGame.UI.RenderBlackBar(240, 260);

			// Credits header
			BaseGame.UI.Logos.RenderOnScreenRelative1600(
#if XBOX360
				10 + 36, 18 + 26, UIRenderer.HeaderCreditsGfxRect);
#else
				10, 18, UIRenderer.HeaderCreditsGfxRect);
#endif

			// Credits
#if XBOX360
			BaseGame.UI.CreditsScreen.RenderOnScreen(
				BaseGame.CalcRectangleKeep4To3(
				25, 130, BaseGame.UI.CreditsScreen.GfxRectangle.Width - 50,
				BaseGame.UI.CreditsScreen.GfxRectangle.Height - 12),
				BaseGame.UI.CreditsScreen.GfxRectangle);
#else
			BaseGame.UI.CreditsScreen.RenderOnScreenRelative4To3(
				0, 125, BaseGame.UI.CreditsScreen.GfxRectangle);
#endif

			BaseGame.UI.RenderBottomButtons(true);

			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBJustPressed ||
				Input.GamePadBackJustPressed ||
				Input.MouseLeftButtonJustPressed)
				return true;

			return false;
		} // Render()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test credits
		/// </summary>
		static public void TestCredits()
		{
			Credits creditsScreen = null;
			TestGame.Start(
				delegate
				{
					creditsScreen = new Credits();
					RacingGameManager.AddGameScreen(creditsScreen);
				},
				delegate
				{
					creditsScreen.Render();
				});
		} // TestCredits()
#endif
		#endregion
	} // class Credits
} // namespace RacingGame.GameScreens
