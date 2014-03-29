// Project: RacingGame, File: Texture.cs
// Namespace: RacingGame.Graphics, Class: Texture
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 614, Size of file: 17,23 KB
// Creation date: 07.09.2006 05:56
// Last modified: 27.10.2006 02:42
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using RacingGame;
using RacingGame.GameLogic;
using RacingGame.Helpers;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// Texture class helping you with using DirectX Textures and handling
	/// possible errors that can happen while loading (stupid DirectX
	/// error messages telling you absolutly nothing, so a lot of pre-checks
	/// help you determinate the error before even calling DirectX methods).
	/// </summary>
	public class Texture : IDisposable
	{
		#region Variables
		/// <summary>
		/// If you want to add a default extension if none is given, set it here.
		/// If this is empty, no default extension will be applied.
		/// </summary>
		protected const string AddExtensionIfNoneGiven = "dds";

		/// <summary>
		/// Texture filename
		/// </summary>
		protected string texFilename = "";
		/// <summary>
		/// Get filename of texture.
		/// </summary>
		public string Filename
		{
			get
			{
				return texFilename;
			} // get
		} // Filename

		/// <summary>
		/// Size of texture
		/// </summary>
		protected int texWidth, texHeight;

		/// <summary>
		/// Width of texture
		/// </summary>
		public int Width
		{
			get
			{
				return texWidth;
			} // get
		} // Width
		/// <summary>
		/// Height of texture
		/// </summary>
		public int Height
		{
			get
			{
				return texHeight;
			} // get
		} // Height

		/// <summary>
		/// Gfx rectangle
		/// </summary>
		/// <returns>Rectangle</returns>
		public Rectangle GfxRectangle
		{
			get
			{
				return new Rectangle(0, 0, texWidth, texHeight);
			} // get
		} // GfxRectangle

		/// <summary>
		/// Size of half a pixel, will be calculated when size is set.
		/// </summary>
		private Vector2 precaledHalfPixelSize = Vector2.Zero;//.Empty;
		/// <summary>
		/// Get the size of half a pixel, used to correct texture
		/// coordinates when rendering on screen, see Texture.RenderOnScreen.
		/// </summary>
		public Vector2 HalfPixelSize
		{
			get
			{
				return precaledHalfPixelSize;
			} // get
		} // HalfPixelSize

		/// <summary>
		/// Calc half pixel size
		/// </summary>
		protected void CalcHalfPixelSize()
		{
			precaledHalfPixelSize = new Vector2(
				(1.0f / (float)texWidth) / 2.0f,
				(1.0f / (float)texHeight) / 2.0f);
		} // CalcHalfPixelSize()

		/// <summary>
		/// Graphic Texture
		/// </summary>
		protected Microsoft.Xna.Framework.Graphics.Texture2D internalXnaTexture = null;
		/// <summary>
		/// Xna texture
		/// </summary>
		public virtual Microsoft.Xna.Framework.Graphics.Texture2D XnaTexture
		{
			get
			{
				return internalXnaTexture;
			} // get
		} // XnaTexture

		/// <summary>
		/// Loading succeeded?
		/// </summary>
		protected bool loaded = true;
		/// <summary>
		/// Error?
		/// </summary>
		protected string error = "";

		/// <summary>
		/// Is texture valid? Will be false if loading failed.
		/// </summary>
		public virtual bool IsValid
		{
			get
			{
				return loaded &&
					internalXnaTexture != null;
			} // get
		} // IsValid

		/// <summary>
		/// Has alpha?
		/// </summary>
		protected bool hasAlpha = false;
		/// <summary>
		/// Has texture alpha information?
		/// </summary>
		public bool HasAlphaPixels
		{
			get
			{
				return hasAlpha;
			} // get
		} // HasAlphaPixels
		#endregion

		#region Constructor
		/// <summary>
		/// Create texture from given filename.
		/// </summary>
		/// <param name="setFilename">Set filename, must be relative and be a
		/// valid file in the textures directory.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
			Justification="No XNA documentation about the exceptions that Load "+
			"could throw, but we want support for loading by filenames on the " +
			"Windows platform, which is handled in the catch block!")]
		public Texture(string setFilename)
		{
			if (String.IsNullOrEmpty(setFilename))
				throw new ArgumentNullException("setFilename",
					"Unable to create texture without valid filename.");

			// Set content name (cut off extension!)
			texFilename = StringHelper.ExtractFilename(setFilename, true);
			string fullFilename =
				Path.Combine(Directories.ContentDirectory, texFilename);

			// Try to load texture
			try
			{
				// Try loading as 2d texture
				internalXnaTexture = BaseGame.Content.Load<Texture2D>(fullFilename);

				// Get info from the texture directly.
				texWidth = internalXnaTexture.Width;
				texHeight = internalXnaTexture.Height;

				// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
				// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
				hasAlpha =
					internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
					internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
					internalXnaTexture.Format.ToString().StartsWith("A");

				loaded = true;

				CalcHalfPixelSize();
			} // try
			catch (Exception ex)
			{
#if XBOX360
				// Failed to load
				loaded = false;
				// Create dummy empty texture (to prevent any errors in the game)
				internalXnaTexture = new Texture2D(BaseGame.Device,
					16, 16, 0, TextureUsage.None, SurfaceFormat.Dxt1);
				Log.Write("Failed to load texture " + fullFilename +
					", will use empty texture! Error: " + ex.ToString());
#else
				// Try again with filename (try to use existing extension)
				texFilename = StringHelper.ExtractFilename(setFilename, false);

				// Add default extension if none is given
				if (AddExtensionIfNoneGiven.Length > 0 &&
					StringHelper.GetExtension(texFilename).Length == 0)
					texFilename = texFilename + "." + AddExtensionIfNoneGiven;
				// Build full filename
				fullFilename =
					Directories.TexturesDirectory + "\\" + texFilename;

				// Check if file exists, else we can't continue loading!
				if (File.Exists(fullFilename))
				{
					// Finally load texture
					internalXnaTexture = Texture2D.FromFile(
						BaseGame.Device, fullFilename);
					
					// Get info from the texture directly.
					texWidth = internalXnaTexture.Width;
					texHeight = internalXnaTexture.Height;

					// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
					// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
					hasAlpha =
						internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
						internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
						internalXnaTexture.Format.ToString().StartsWith("A");

					loaded = true;

					CalcHalfPixelSize();
				} // if (File.Exists)
				else
				{
					// Failed to load
					loaded = false;
					// Create dummy empty texture (to prevent any errors in the game)
					internalXnaTexture = new Texture2D(BaseGame.Device,
						16, 16, 0, TextureUsage.None, SurfaceFormat.Dxt1);
					Log.Write("Failed to load texture " + fullFilename +
						", will use empty texture! Error: " + ex.ToString());
				} // else
#endif
			} // catch (ex)
		} // Texture(setFilename)

		/// <summary>
		/// Create texture, protected version for derived classes.
		/// </summary>
		protected Texture()
		{
		} // Texture()

		/// <summary>
		/// Create texture by just assigning a Texture2D.
		/// </summary>
		/// <param name="tex">Tex</param>
		public Texture(Texture2D tex)
		{
			if (tex == null)
				throw new ArgumentNullException("tex");

			internalXnaTexture = tex;
			
			// Get info from the texture directly.
			texWidth = internalXnaTexture.Width;
			texHeight = internalXnaTexture.Height;

			// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
			// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
			hasAlpha =
				internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
				internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
				internalXnaTexture.Format.ToString().StartsWith("A");

			loaded = true;

			CalcHalfPixelSize();
		} // Texture(tex)
		#endregion

		#region Disposing
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		} // Dispose()

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing">Disposing</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (internalXnaTexture != null)
					internalXnaTexture.Dispose();
				internalXnaTexture = null;
			} // if

			loaded = false;
		} // Dispose(disposing)
		#endregion

		#region Select texture as texture stage
		/*not that easy without fixed function
		/// <summary>
		/// Select this texture as texture stage 0
		/// </summary>
		public virtual void Select()
		{
			if (xnaTexture != null)
				RCGame.device.sette.DirectXDevice.SetTexture(0, xnaTexture);
		} // Select()
		 */
		#endregion

		#region Render on screen
		/// <summary>
		/// Render texture at rect directly on screen using pixelRect.
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect);
		} // RenderOnScreen(rect, pixelRect)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelX">Pixel x</param>
		/// <param name="pixelY">Pixel y</param>
		/// <param name="pixelWidth">Pixel width</param>
		/// <param name="pixelHeight">Pixel height</param>
		public void RenderOnScreen(Rectangle rect,
			int pixelX, int pixelY, int pixelWidth, int pixelHeight)
		{
			SpriteHelper.AddSpriteToRender(this, rect,
				new Rectangle(pixelX, pixelY, pixelWidth, pixelHeight));
		} // RenderOnScreen(rect, pixelX, pixelY)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="pos">Position</param>
		public void RenderOnScreen(Point pos)
		{
			SpriteHelper.AddSpriteToRender(this,
				new Rectangle(pos.X, pos.Y, texWidth, texHeight),
				new Rectangle(0, 0, texWidth, texHeight));
		} // RenderOnScreen(pos)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="renderRect">Render rectangle</param>
		public void RenderOnScreen(Rectangle renderRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				renderRect, GfxRectangle);
		} // RenderOnScreen(renderRect)

		/// <summary>
		/// Render on screen relative for 1024x640 (16:9) graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative16To9(int relX, int relY,
			Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangle(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative(relX, relY, pixelRect)

		/// <summary>
		/// Render on screen relative 1024x786 (4:3)
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative4To3(int relX, int relY,
			Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangleKeep4To3(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative4To3(relX, relY, pixelRect)

		/// <summary>
		/// Render on screen relative for 1600px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative1600(
			int relX, int relY, Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangle1600(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative1600(relX, relY, pixelRect)

		/*unused
		/// <summary>
		/// Render on screen relative  for 1920px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative1920(
			int relX, int relY, Rectangle pixelRect)
		{
			BaseGame.UI.AddSpriteToRender(this,
				BaseGame.CalcRectangle1920(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative1920(relX, relY, pixelRect)
		 */

		/// <summary>
		/// Render texture at rect directly on screen using texture cordinates.
		/// This method allows to render with specific color and alpha values.
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="color">Color</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
			Color color)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color);
		} // RenderOnScreen(rect, pixelRect, color)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="color">Color</param>
		/// <param name="blendMode">Blend mode</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
			Color color, SpriteBlendMode blendMode)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color, blendMode);
		} // RenderOnScreen(rect, pixelRect, color)
		#endregion

		#region Rendering on screen with rotation
		/// <summary>
		/// Render on screen with rotation
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="rotation">Rotation</param>
		public void RenderOnScreenWithRotation(
			Rectangle rect, Rectangle pixelRect,
			float rotation, Vector2 rotationPoint)
		{
			//Rectangle rect =
			//	new Rectangle(pos.X, pos.Y, pixelRect.Width, pixelRect.Height);
			SpriteHelper.AddSpriteToRender(this,
				rect, pixelRect, rotation,
				//new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
				rotationPoint);
		} // RenderOnScreenWithRotation(pos, pixelRect, rotation)
		#endregion

		#region To string
		/// <summary>
		/// To string
		/// </summary>
		public override string ToString()
		{
			return "Texture(filename=" + texFilename +
				", width=" + texWidth +
				", height=" + texHeight +
				", xnaTexture=" + (internalXnaTexture != null ? "valid" : "null") +
				", hasAlpha=" + hasAlpha + ")";
		} // ToString()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test textures
		/// </summary>
		//[Test]
		public static void TestTextures()
		{
			Texture testTexture = null;
			TestGame.Start("TestTextures",
				delegate
				{
					//testTexture = new Texture("MouseCursor.dds");
					testTexture = new Texture("Sun");
				},
				delegate
				{
					testTexture.RenderOnScreen(
						new Rectangle(100, 100, 256, 256),
						testTexture.GfxRectangle,
						Color.White, SpriteBlendMode.None);

					// Use alpha blending
					testTexture.RenderOnScreen(
						new Rectangle(Input.MousePos.X, Input.MousePos.Y, 512, 512),
						testTexture.GfxRectangle,
						new Color(64, 64, 64, 64), SpriteBlendMode.Additive);
				});
		} // TestTextures()

		/*
		/// <summary>
		/// Test fullscreen texture
		/// </summary>
		//[Test]
		public static void TestFullscreenTextures()
		{
			Texture testTexture = null, testTexture2 = null;
			RaceGame.StartTest("TestFullscreenTextures",
				delegate
				{
					testTexture = new Texture("MainMenu.dds");
					testTexture2 = new Texture("MouseCursor.dds");
				},
				delegate
				{
					// Fill background with crazy color
					RaceGame.DirectXDevice.Clear(ClearFlags.Target,
						Color.Aquamarine, 1, 0);

					// Draw texture on top of it without any alpha!
					RaceGame.DirectXDevice.RenderState.AlphaBlendEnable = false;
					testTexture.RenderOnScreen(
						GraphicForm.ResolutionRect,
						new Rectangle(0, 0, 1024, 768));
					testTexture2.RenderOnScreen(new Rectangle(0, 0,
						testTexture2.Width, testTexture2.Height));
				});
		} // TestTextures()
		 */

		/// <summary>
		/// Test rendering rotated texture
		/// </summary>
		//[Test]
		public static void TestRenderingRotatedTexture()
		{
			Texture testTexture = null;
			TestGame.Start("TestRenderingRotatedTexture",
				delegate
				{
					testTexture = new Texture("landscape");
				},
				delegate
				{
					testTexture.RenderOnScreen(
						new Rectangle(0, 0, 100, 100),
						new Rectangle(0, 0, 100, 100));

					testTexture.RenderOnScreenWithRotation(
						new Rectangle(100, 100, 150, 150),
						new Rectangle(100, 100, 150, 150),
						Input.MousePos.X / 100.0f,
						new Vector2(50, 50));
				});
		} // TestTextures()
#endif
		#endregion
	} // class Texture
} // namespace RacingGame.Graphics
