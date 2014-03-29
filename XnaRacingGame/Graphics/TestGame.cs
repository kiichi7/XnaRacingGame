#if DEBUG
// Project: RacingGame, File: TestGame.cs
// Namespace: RacingGame.Graphics, Class: TestGame
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 389, Size of file: 11,89 KB
// Creation date: 07.09.2006 05:56
// Last modified: 13.10.2006 00:27
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using RacingGame.Shaders;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// Test game
	/// </summary>
	public partial class TestGame : RacingGameManager//BaseGame
	{
		#region Variables
		/// <summary>
		/// Init code
		/// </summary>
		protected RenderHandler initCode, updateCode, renderCode;
		#endregion

		#region Constructor
		/// <summary>
		/// Create test game
		/// </summary>
		/// <param name="setWindowsTitle">Set windows title</param>
		/// <param name="windowWidth">Window width</param>
		/// <param name="windowHeight">Window height</param>
		/// <param name="setInitCode">Set init code</param>
		/// <param name="setUpdateCode">Set update code</param>
		/// <param name="setRenderCode">Set render code</param>
		protected TestGame(string setWindowsTitle,
			int windowWidth, int windowHeight,
			RenderHandler setInitCode,
			RenderHandler setUpdateCode,
			RenderHandler setRenderCode)
			: base(setWindowsTitle)
		{
			/*not required
			// Make sure we are in the game directory, unit tests might
			// be executed from another directory and then the content files
			// will not be found!
			Directory.SetCurrentDirectory(Directories.GameBaseDirectory);
			 */
#if !XBOX360
			if (windowWidth > 0 &&
				windowHeight > 0)
			{
				// Update width and height
				this.Window.BeginScreenDeviceChange(false);
				this.Window.EndScreenDeviceChange(
					this.Window.ScreenDeviceName, windowWidth, windowHeight);
				// Make sure our device is changed too (else the resolution
				// is not set for our 2d rendering).
				this.graphicsManager.PreferredBackBufferWidth = windowWidth;
				this.graphicsManager.PreferredBackBufferHeight = windowHeight;
				this.graphicsManager.ApplyChanges();
			} // if (windowWidth)
#endif

			//unused, done in constructor:
			//if (String.IsNullOrEmpty(setWindowsTitle) == false)
			//	this.Window.Title = setWindowsTitle;

			//don't show, we got a custom mouse cursor now:
			//this.IsMouseVisible = true;
#if !XBOX360
#if DEBUG
			WindowsHelper.ForceForegroundWindow(this.Window.Handle.ToInt32());
#endif
#endif
			initCode = setInitCode;
			updateCode = setUpdateCode;
			renderCode = setRenderCode;
		} // TestGame(setWindowsTitle, setInitCode, setUpdateCode)

		/// <summary>
		/// Initialize
		/// </summary>
		protected override void Initialize()
		{
			// Initialize might will set our car pos to the start pos
			base.Initialize();

			// Make sure we reset our camera to the origin for testing
			// If we use the landscape, we set it to the start pos again anyway!
			RacingGameManager.Player.SetCarPosition(Vector3.Zero,
				new Vector3(0, 1, 0), new Vector3(0, 0, 1));
			RacingGameManager.Player.FreeCamera = true;

			if (initCode != null)
				initCode();
		} // Initialize()

		/// <summary>
		/// Create test game
		/// </summary>
		/// <param name="setWindowsTitle">Set windows title</param>
		/// <param name="setInitCode">Set init code</param>
		/// <param name="setUpdateCode">Set update code</param>
		/// <param name="setRenderCode">Set render code</param>
		protected TestGame(string setWindowsTitle,
			RenderHandler setInitCode,
			RenderHandler setUpdateCode,
			RenderHandler setRenderCode)
			: this(setWindowsTitle, -1, -1,
			setInitCode, setUpdateCode, setRenderCode)
		{
		} // TestGame(setWindowsTitle, windowWidth, windowHeight)
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed)
				this.Exit();

			// Execute game logic
			if (updateCode != null)
				updateCode();
		} // Update()
		#endregion

		#region Render
		/// <summary>
		/// Render
		/// </summary>
		protected override void Render()
		{
			// Don't handle RacingGameManager.Render, which handles the game screens.
			// For unit tests we want to handle everything ourself.

			BaseGame.Device.RenderState.DepthBufferEnable = true;

			// Drawing code
			if (renderCode != null)
				renderCode();
			/*suxx, obfuscates the location of the exception
			try
			{
				// Drawing code
				if (renderCode != null)
					renderCode();
			} // try
			catch (Exception ex)
			{
				Log.Write("Fatal exception happend while rendering unit test: " + ex);
				throw ex;
			} // catch
			 */
		} // Render()
		#endregion

		#region Start test
		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="windowWidth">Window width</param>
		/// <param name="windowHeight">Window height</param>
		/// <param name="initCode">Init code</param>
		/// <param name="updateCode">Update code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			int windowWidth, int windowHeight,
			RenderHandler initCode,
			RenderHandler updateCode,
			RenderHandler renderCode)
		{
			using (TestGame game = new TestGame(
				testName, windowWidth, windowHeight,
				initCode, updateCode, renderCode))
			{
				game.Run();
			} // using (game)
		} // Start(testName, initCode, updateCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="initCode">Init code</param>
		/// <param name="updateCode">Update code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			RenderHandler initCode,
			RenderHandler updateCode,
			RenderHandler renderCode)
		{
			using (TestGame game = new TestGame(
				testName, initCode, updateCode, renderCode))
			{
				game.Run();
			} // using (game)
		} // Start(testName, initCode, updateCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="initCode">Init code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			RenderHandler initCode, RenderHandler renderCode)
		{
			Start(testName, initCode, null, renderCode);
		} // Start(testName, initCode, renderCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="windowWidth">Window width</param>
		/// <param name="windowHeight">Window height</param>
		/// <param name="initCode">Init code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			int windowWidth, int windowHeight,
			RenderHandler initCode, RenderHandler renderCode)
		{
			Start(testName, windowWidth, windowHeight, initCode, null, renderCode);
		} // Start(testName, windowWidth, windowHeight)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="initCode">Init code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(
			RenderHandler initCode, RenderHandler renderCode)
		{
			Start("UnitTest", initCode, null, renderCode);
		} // Start(initCode, renderCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="renderCode">Render code</param>
		public static void Start(RenderHandler renderCode)
		{
			Start(null, renderCode);
		} // Start(renderCode)
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestEmptyGame
		/// <summary>
		/// Test empty game
		/// </summary>
		//[Test]
		public static void TestEmptyGame()
		{
			TestGame.Start(null);
		} // TestEmptyGame()
		#endregion

		#region TestCreateYourOwnGameTutorial
		/// <summary>
		/// Test create your own game tutorial
		/// </summary>
		//[Test]
		public static void TestCreateYourOwnGameTutorial()
		{
			Texture tex = null;
			Point pos = new Point(0, 0);
			SpriteBatch batch = null;
			// Store some info about the sprite's motion
			int m_dSpriteHorizSpeed = 10;
			int m_dSpriteVertSpeed = 10;

			TestGame.Start("TestCreateYourOwnGameTutorial",
				delegate // init
				{
					tex = new Texture("background.png");
					batch = new SpriteBatch(TestGame.Device);
				},
				delegate // update
				{
					// move the sprite by speed
					pos.X += m_dSpriteHorizSpeed;
					pos.Y += m_dSpriteVertSpeed;

					int MaxX = TestGame.Width - tex.Width;
					int MinX = 0;
					int MaxY = TestGame.Height - tex.Height;
					int MinY = 0;

					//check for bounce
					if (pos.X > MaxX)
					{
						m_dSpriteHorizSpeed *= -1;
						pos.X = MaxX;
					}
					else if (pos.X < MinX)
					{
						m_dSpriteHorizSpeed *= -1;
						pos.X = MinX;
					}

					if (pos.Y > MaxY)
					{
						m_dSpriteVertSpeed *= -1;
						pos.Y = MaxY;
					}
					else if (pos.Y < MinY)
					{
						m_dSpriteVertSpeed *= -1;
						pos.Y = MinY;
					}
				},
				delegate // render
				{
					batch.Begin();
					batch.Draw(tex.XnaTexture, new Vector2(pos.X, pos.Y), Color.White);
					batch.End();
				});
		} // TestCreateYourOwnGameTutorial()
		#endregion
#endif
		#endregion
	} // class TestGame
} // namespace RacingGame.Graphics
#endif