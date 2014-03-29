// Project: RacingGame, File: TextureFontBigNumbers.cs
// Namespace: RacingGame.Graphics, Class: TextureFontBigNumbers
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 213, Size of file: 5,93 KB
// Creation date: 12.10.2006 11:46
// Last modified: 12.10.2006 12:42
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Helpers;
using RacingGame.GameLogic;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// TextureFontBigNumbers
	/// </summary>
	public sealed class TextureFontBigNumbers
	{
		#region Constants
		/// <summary>
		/// Big numbers in the Ingame.png graphic
		/// </summary>
		private static readonly Rectangle[] BigNumberRects =
			{
				// 0
				new Rectangle(2, 342, 80, 133),
				// 1
				new Rectangle(84, 342, 80, 133),
				// 2
				new Rectangle(167, 342, 80, 133),
				// 3
				new Rectangle(247, 342, 78, 133),
				// 4
				new Rectangle(330, 342, 80, 133),
				// 5
				new Rectangle(411, 342, 80, 133),
				// 6
				new Rectangle(495, 342, 80, 133),
				// 7
				new Rectangle(578, 342, 80, 133),
				// 8
				new Rectangle(659, 342, 80, 133),
				// 9
				new Rectangle(749, 342, 80, 133),
			};
		#endregion

		#region Constructor
		/// <summary>
		/// Private constructor to prevent instantiation.
		/// </summary>
		private TextureFontBigNumbers()
		{
		} // TextureFontBigNumbers()
		#endregion

		#region Write number
		/// <summary>
		/// Write digit
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="digit">Digit</param>
		/// <returns>Int</returns>
		private static int WriteDigit(int x, int y, int digit)
		{
			if (digit < 0)
				return 0;

			float resScalingX = (float)BaseGame.Width / 1600.0f;//1024.0f;
			float resScalingY = (float)BaseGame.Height / 1200.0f;//768.0f;

			Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
			BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
				(int)Math.Round(rect.Width * resScalingX),
				(int)Math.Round(rect.Height * resScalingY)), rect);

			return (int)Math.Round(rect.Width * resScalingX);
		} // WriteDigit(x, y, digit)

		/// <summary>
		/// Write digit
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="height">Height</param>
		/// <param name="digit">Digit</param>
		/// <returns>Int</returns>
		private static int WriteDigit(int x, int y, int height, int digit)
		{
			if (digit < 0)
				return 0;

			float resScalingX = (float)BaseGame.Width / 1600.0f;//1024.0f;
			float resScalingY = (float)BaseGame.Height / 1200.0f;//768.0f;
			float scaleFactor = height / (float)BigNumberRects[0].Height;

			Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
			BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
				(int)Math.Round(rect.Width * resScalingX * scaleFactor),
				(int)Math.Round(rect.Height * resScalingY * scaleFactor)), rect);

			return (int)Math.Round(rect.Width * resScalingX * scaleFactor);
		} // WriteDigit(x, y, digit)

		/// <summary>
		/// Write digit
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="digit">Digit</param>
		/// <param name="alpha">Alpha</param>
		/// <returns>Int</returns>
		private static int WriteDigit(int x, int y, int digit, float alpha)
		{
			float resScalingX = (float)BaseGame.Width / 1600.0f;//024.0f;
			float resScalingY = (float)BaseGame.Height / 1200.0f;//768.0f;

			Rectangle rect = BigNumberRects[digit % BigNumberRects.Length];
			BaseGame.UI.Ingame.RenderOnScreen(new Rectangle(x, y,
				(int)Math.Round(rect.Width * resScalingX),
				(int)Math.Round(rect.Height * resScalingY)), rect,
				ColorHelper.ApplyAlphaToColor(Color.White, alpha));

			return (int)Math.Round(rect.Width * resScalingX);
		} // WriteDigit(x, y, digit)

		/// <summary>
		/// Write number
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="number">Number</param>
		/// <returns>Int</returns>
		public static int WriteNumber(int x, int y, int number)
		{
			// Convert to string
			string numberText = number.ToString();
			int width = 0;

			// And now process every letter
			//foreach (char numberChar in numberText.ToCharArray())
			char[] chars = numberText.ToCharArray();
			for (int num = 0; num < chars.Length; num++)
			{
				width += WriteDigit(x + width, y, (int)chars[num] - (int)'0');
			} // foreach (numberChar)

			return width;
		} // WriteNumber(x, y, number)

		/// <summary>
		/// Write number
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="number">Number</param>
		/// <param name="alpha">Alpha</param>
		/// <returns>Int</returns>
		public static int WriteNumber(int x, int y, int number, float alpha)
		{
			// Convert to string
			string numberText = number.ToString();
			int width = 0;

			// And now process every letter
			//foreach (char numberChar in numberText.ToCharArray())
			char[] chars = numberText.ToCharArray();
			for (int num = 0; num < chars.Length; num++)
			{
				width += WriteDigit(
					x + width, y, (int)chars[num] - (int)'0', alpha);
			} // foreach (numberChar)

			return width;
		} // WriteNumber(x, y, number)
		
		/// <summary>
		/// Write number
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="height">Height</param>
		/// <param name="number">Number</param>
		/// <returns>Int</returns>
		public static int WriteNumber(int x, int y, int height, int number)
		{
			// Convert to string
			string numberText = number.ToString();
			int width = 0;

			// And now process every letter
			//foreach (char numberChar in numberText.ToCharArray())
			char[] chars = numberText.ToCharArray();
			for (int num = 0; num < chars.Length; num++)
			{
				width += WriteDigit(
					x + width, y, height, (int)chars[num] - (int)'0');
			} // foreach (numberChar)

			return width;
		} // WriteNumber(x, y, height)

		/// <summary>
		/// Write number centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="number">Number</param>
		public static void WriteNumberCentered(int x, int y, int number)
		{
			WriteNumber(
				(int)(x - (number.ToString().Length * BigNumberRects[0].Width / 2) *
				((float)BaseGame.Width / 1600.0f)),
				y, number);
		} // WriteBigNumberCentered(x, y, number)

		/// <summary>
		/// Write number centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="number">Number</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteNumberCentered(int x, int y, int number, float alpha)
		{
			WriteNumber(
				(int)(x - (number.ToString().Length * BigNumberRects[0].Width / 2) *
				((float)BaseGame.Width / 1600.0f)),
				y, number, alpha);
		} // WriteNumberCentered(x, y, number)
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test write numbers
		/// </summary>
		//[Test]
		public static void TestWriteNumbers()
		{
			TestGame.Start("TestWriteNumbers",
				delegate
				{
				},
				delegate
				{
					BaseGame.AlphaBlending = true;
					TextureFontBigNumbers.WriteNumber(100, 100, 123);
					TextureFontBigNumbers.WriteNumber(100, 250, 507893, 0.5f);
					TextureFontBigNumbers.WriteNumber(50, 500, 20, 893);
					TextureFontBigNumbers.WriteNumber(150, 500, 40, 34);
					TextureFontBigNumbers.WriteNumber(250, 500, 60, 59);
					TextureFontBigNumbers.WriteNumber(350, 500, 80, 84);
					TextureFontBigNumbers.WriteNumberCentered(500, 200, 121);
					TextureFontBigNumbers.WriteNumberCentered(500, 450, 231, 0.5f);
				});
		} // TestWriteNumbers()

		public static void TestFontFadeupEffect()
		{
			TestGame.Start("TestFontFadeupEffect",
				null,
				delegate
				{
					TextureFont.WriteText(30, 30,
						"Press A, B, X (gamepad or keyboard) to add the time effects.");

					if (Input.GamePadAJustPressed ||
						Input.KeyboardKeyJustPressed(Keys.A) ||
						Input.KeyboardSpaceJustPressed)
						BaseGame.UI.AddTimeFadeupEffect(3539, UIRenderer.TimeFadeupMode.Plus);
					else if (Input.GamePadBJustPressed ||
						Input.KeyboardKeyJustPressed(Keys.B))
						BaseGame.UI.AddTimeFadeupEffect(69, UIRenderer.TimeFadeupMode.Minus);
					else if (Input.GamePadXJustPressed ||
						Input.KeyboardKeyJustPressed(Keys.X))
						BaseGame.UI.AddTimeFadeupEffect(13539, UIRenderer.TimeFadeupMode.Normal);
				});
		} // TestFontFadeupEffect()
#endif
		#endregion
	} // class TextureFontBigNumbers
} // namespace RacingGame.Graphics
