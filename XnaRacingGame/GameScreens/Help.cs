// Project: RacingGame, File: Help.cs
// Namespace: RacingGame.GameScreens, Class: Help
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 77, Size of file: 1,73 KB
// Creation date: 12.09.2006 07:22
// Last modified: 22.10.2006 18:42
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Properties;
using Microsoft.Xna.Framework;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Help
	/// </summary>
	/// <returns>IGame screen</returns>
	class Help : IGameScreen
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
			//unused, done in the help texture: BaseGame.UI.RenderBlackBar(120, 400);

			// Help header
			BaseGame.UI.Logos.RenderOnScreenRelative1600(
#if XBOX360
				10 + 36, 18 + 26, UIRenderer.HeaderHelpGfxRect);
#else
				10, 18, UIRenderer.HeaderHelpGfxRect);
#endif

			// Help
#if XBOX360
			BaseGame.UI.HelpScreen.RenderOnScreen(
				BaseGame.CalcRectangleKeep4To3(
				25, 130, BaseGame.UI.HelpScreen.GfxRectangle.Width-50,
				BaseGame.UI.HelpScreen.GfxRectangle.Height-12),
				BaseGame.UI.HelpScreen.GfxRectangle);
#else
			BaseGame.UI.HelpScreen.RenderOnScreenRelative4To3(
				0, 125, BaseGame.UI.HelpScreen.GfxRectangle);
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
		/// Test help
		/// </summary>
		static public void TestHelp()
		{
			Help helpScreen = null;
			TestGame.Start(
				delegate
				{
					helpScreen = new Help();
					RacingGameManager.AddGameScreen(helpScreen);
				},
				delegate
				{
					helpScreen.Render();
				});
		} // TestHelp()
#endif
		#endregion
	} // class Help
} // namespace RacingGame.GameScreens
