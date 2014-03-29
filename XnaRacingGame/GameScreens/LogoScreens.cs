// Project: RacingGame, File: LogoScreens.cs
// Namespace: RacingGame.GameScreens, Class: LogoScreens
// Path: C:\code\RacingGame\GameScreens, Author: Abi
// Code lines: 76, Size of file: 2,16 KB
// Creation date: 23.10.2006 17:50
// Last modified: 23.10.2006 18:28
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.GameScreens
{
	/// <summary>
	/// Logo screens
	/// </summary>
	class LogoScreens : IGameScreen
	{
		#region Constants
		/// <summary>
		/// Show all logos in a timeframe of 10 seconds total.
		/// </summary>
		const int ShowLogo1Time = 6000,
			ShowLogo2Time = 6000,
			ShowLogo3Time = 6000;
		/// <summary>
		/// Time we fade stuff in and out. Between the logos we overblend them.
		/// </summary>
		const int FadeInOutTime = 1750;
		#endregion

		#region Render
		/// <summary>
		/// Render
		/// </summary>
		public bool Render()
		{
			// This starts both menu and in game post screen shader!
			BaseGame.UI.PostScreenMenuShader.Start();

			// Render game background
			BaseGame.UI.RenderGameBackground();
			BaseGame.UI.RenderMenuTrackBackground();

			// Fill complete background to black (85%)
			BaseGame.UI.RenderBlackBar(0, 640);

			// Show logos
			float alpha = 1.0f;
			if (BaseGame.TotalTimeMilliseconds < ShowLogo1Time)
			{
				if (BaseGame.TotalTimeMilliseconds < FadeInOutTime)
					alpha = BaseGame.TotalTimeMilliseconds / (float)FadeInOutTime;
				else if (BaseGame.TotalTimeMilliseconds > ShowLogo1Time - FadeInOutTime)
					alpha = (ShowLogo1Time - BaseGame.TotalTimeMilliseconds) /
						(float)FadeInOutTime;

				BaseGame.UI.SplashLogo(0).RenderOnScreen(
					BaseGame.CalcRectangleCenteredWithGivenHeight(
					1024 / 2, 640 / 2, 256, BaseGame.UI.SplashLogo(0).GfxRectangle),
					BaseGame.UI.SplashLogo(0).GfxRectangle,
					ColorHelper.ApplyAlphaToColor(Color.White, alpha));
			} // if (BaseGame.TotalTimeMilliseconds)

			int logo2StartTime = ShowLogo1Time;
			if (BaseGame.TotalTimeMilliseconds - logo2StartTime > 0 &&
				BaseGame.TotalTimeMilliseconds - logo2StartTime < ShowLogo2Time)
			{
				if (BaseGame.TotalTimeMilliseconds - logo2StartTime < FadeInOutTime)
					alpha = (BaseGame.TotalTimeMilliseconds - logo2StartTime) /
						(float)FadeInOutTime;
				else if (BaseGame.TotalTimeMilliseconds - logo2StartTime >
					ShowLogo2Time - FadeInOutTime)
					alpha = (ShowLogo2Time - (BaseGame.TotalTimeMilliseconds - logo2StartTime)) /
						(float)FadeInOutTime;

				BaseGame.UI.SplashLogo(1).RenderOnScreen(
					BaseGame.CalcRectangleCenteredWithGivenHeight(
					1024 / 2, 640 / 2, 256, BaseGame.UI.SplashLogo(1).GfxRectangle),
					BaseGame.UI.SplashLogo(1).GfxRectangle,
					ColorHelper.ApplyAlphaToColor(Color.White, alpha));
			} // if (BaseGame.TotalTimeMilliseconds)

			int logo3StartTime = ShowLogo1Time + ShowLogo2Time;
			if (BaseGame.TotalTimeMilliseconds - logo3StartTime > 0 &&
				BaseGame.TotalTimeMilliseconds - logo3StartTime < ShowLogo2Time)
			{
				if (BaseGame.TotalTimeMilliseconds - logo3StartTime < FadeInOutTime)
					alpha = (BaseGame.TotalTimeMilliseconds - logo3StartTime) /
						(float)FadeInOutTime;
				else if (BaseGame.TotalTimeMilliseconds - logo3StartTime >
					ShowLogo3Time - FadeInOutTime)
					alpha = (ShowLogo3Time - (BaseGame.TotalTimeMilliseconds - logo3StartTime)) /
						(float)FadeInOutTime;

				BaseGame.UI.SplashLogo(2).RenderOnScreen(
					BaseGame.CalcRectangleCenteredWithGivenHeight(
					1024 / 2, 640 / 2, 256, BaseGame.UI.SplashLogo(2).GfxRectangle),
					BaseGame.UI.SplashLogo(2).GfxRectangle,
					ColorHelper.ApplyAlphaToColor(Color.White, alpha));
			} // if (BaseGame.TotalTimeMilliseconds)

			// Waited long enough? Then quit too.
			if (BaseGame.TotalTimeMilliseconds - logo3StartTime > ShowLogo2Time)
				return true;

			// Clicking or pressing start will go to the next screen
			return Input.MouseLeftButtonJustPressed ||
				Input.KeyboardSpaceJustPressed ||
				Input.KeyboardEscapeJustPressed ||
				Input.GamePadStartPressed;
		} // Render()
		#endregion
	} // class LogoScreens
} // namespace RacingGame.GameScreens
