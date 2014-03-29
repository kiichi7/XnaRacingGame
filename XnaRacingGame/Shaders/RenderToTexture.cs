// Author: abi
// Project: XnaRacingGame
// Path: C:\code\Xna\XnaRacingGame\Shaders
// Creation date: 10.02.2008 13:07
// Last modified: 11.02.2008 00:24

// Project: RacingGame, File: RenderToTexture.cs
// Namespace: RacingGame.Shaders, Class: RenderToTexture
// Path: C:\code\RacingGame\Shaders, Author: Abi
// Code lines: 463, Size of file: 12,89 KB
// Creation date: 12.09.2006 07:20
// Last modified: 22.10.2006 18:52
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model = RacingGame.Graphics.Model;
using Texture = RacingGame.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
#endregion

namespace RacingGame.Shaders
{
	/// <summary>
	/// Render to texture helper class based on the Texture class.
	/// This class allows to render stuff onto textures, if thats not
	/// supported, it will just not work and report an engine log message.
	/// This class is required for most PostScreenShaders.
	/// </summary>
	public class RenderToTexture : Texture
	{
		#region Variables
		/// <summary>
		/// Our render target we are going to render to. Much easier than in MDX
		/// where you have to use Surfaces, etc. Also supports the Xbox360 model
		/// of resolving the render target texture before we can use it, otherwise
		/// the RenderToTexture class would not work on the Xbox360.
		/// </summary>
		RenderTarget2D renderTarget = null;

		/// <summary>
		/// Z buffer surface for shadow mapping render targets that do not
		/// fit in our resolution. Usually unused!
		/// </summary>
		DepthStencilBuffer zBufferSurface = null;
		/// <summary>
		/// ZBuffer surface
		/// </summary>
		/// <returns>Surface</returns>
		public DepthStencilBuffer ZBufferSurface
		{
			get
			{
				return zBufferSurface;
			} // get
		} // ZBufferSurface

		/// <summary>
		/// Posible size types for creating a RenderToTexture object.
		/// </summary>
		public enum SizeType
		{
			/// <summary>
			/// Uses the full screen size for this texture
			/// </summary>
			FullScreen,
			/// <summary>
			/// Use full screen and force z buffer (sometimes needed for correct
			/// rendering).
			/// </summary>
			FullScreenWithZBuffer,
			/// <summary>
			/// Uses half the full screen size, e.g. 800x600 becomes 400x300
			/// </summary>
			HalfScreen,
			/// <summary>
			/// Use half screen and force z buffer (sometimes needed for correct
			/// rendering).
			/// </summary>
			HalfScreenWithZBuffer,
			/// <summary>
			/// Uses a quarter of the full screen size, e.g. 800x600 becomes
			/// 200x150
			/// </summary>
			QuarterScreen,
			/// <summary>
			/// Shadow map texture, usually 1024x1024, but can also be better
			/// like 2048x2048 or 4096x4096.
			/// </summary>
			ShadowMap,
		} // enum SizeTypes

		/// <summary>
		/// Size type
		/// </summary>
		private SizeType sizeType;

		/// <summary>
		/// Calc size
		/// </summary>
		private void CalcSize()
		{
			switch (sizeType)
			{
				case SizeType.FullScreen:
				case SizeType.FullScreenWithZBuffer:
					texWidth = BaseGame.Width;
					texHeight = BaseGame.Height;
					break;
				case SizeType.HalfScreen:
				case SizeType.HalfScreenWithZBuffer:
					texWidth = BaseGame.Width / 2;
					texHeight = BaseGame.Height / 2;
					break;
				case SizeType.QuarterScreen:
					texWidth = BaseGame.Width / 4;
					texHeight = BaseGame.Height / 4;
					break;
				case SizeType.ShadowMap:
#if XBOX360
					// TODO: try to implement vsm technique!
					if (BaseGame.HighDetail)
					{
						texWidth = 2048;// 512;// 1024;//512
						texHeight = 2048;// 512;// 1024;//512
					} // else
					else
					{
						texWidth = 1024;
						texHeight = 1024;
					} // else
#else
					// Use 2048x2048 if we allow ps30
					if (BaseGame.CanUsePS30 &&
						BaseGame.HighDetail)
					{
						texWidth = 2048;
						texHeight = 2048;
					} // if (BaseGame.CanUsePS30)
					else
					{
						// 512x512 is an option too, but it is very blocky!
						if (BaseGame.CanUsePS20 == false &&
							BaseGame.HighDetail == false)
						{
							texWidth = 512;
							texHeight = 512;
						} // if
						else
						{
							texWidth = 1024;
							texHeight = 1024;
						} // else
					} // else
#endif
					break;
			} // switch
			CalcHalfPixelSize();
		} // CalcSize()

		/// <summary>
		/// Does this texture use some high percision format?
		/// Better than 8 bit color?
		/// </summary>
		private bool usesHighPercisionFormat = false;
		#endregion

		#region Properties
		/// <summary>
		/// Render target
		/// </summary>
		/// <returns>Render target 2D</returns>
		public RenderTarget2D RenderTarget
		{
			get
			{
				return renderTarget;
			} // get
		} // RenderTarget

		/// <summary>
		/// Override how to get XnaTexture, we have to resolve the render target
		/// for supporting the Xbox, which requires calling Resolve first!
		/// After that you can call this property to get the current texture.
		/// </summary>
		/// <returns>XnaTexture</returns>
		public override XnaTexture XnaTexture
		{
			get
			{
				if (alreadyResolved)
					internalXnaTexture = renderTarget.GetTexture();
				return internalXnaTexture;
			} // get
		} // XnaTexture

		/// <summary>
		/// Does this texture use some high percision format?
		/// Better than 8 bit color?
		/// </summary>
		public bool UsesHighPercisionFormat
		{
			get
			{
				return usesHighPercisionFormat;
			} // get
		} // UsesHighPercisionFormat

		/// <summary>
		/// Is render target valid? Will be false if loading failed.
		/// </summary>
		public override bool IsValid
		{
			get
			{
				return loaded &&
					renderTarget != null;
			} // get
		} // IsValid

		/// <summary>
		/// Back buffer depth format
		/// </summary>
		static DepthFormat backBufferDepthFormat = DepthFormat.Depth32;
		/// <summary>
		/// Back buffer depth format
		/// </summary>
		/// <returns>Surface format</returns>
		public static DepthFormat BackBufferDepthFormat
		{
			get
			{
				return backBufferDepthFormat;
			} // get
		} // BackBufferDepthFormat

		/// <summary>
		/// Remember multi sample type in Initialize for later use
		/// in RenderToTexture!
		/// </summary>
		static MultiSampleType remMultiSampleType = MultiSampleType.None;
		/// <summary>
		/// MultiSampleType
		/// </summary>
		public static MultiSampleType MultiSampleType
		{
			get
			{
				return remMultiSampleType;
			} // get
		} // MultiSampleType

		/// <summary>
		/// Remember multi sample quality.
		/// </summary>
		static int remMultiSampleQuality = 0;
		/// <summary>
		/// Multi sample quality
		/// </summary>
		public static int MultiSampleQuality
		{
			get
			{
				return remMultiSampleQuality;
			} // get
		} // MultiSampleQuality

		/// <summary>
		/// Remember depth stencil buffer in case we have to reset it!
		/// </summary>
		internal static DepthStencilBuffer remDepthBuffer = null;

		/// <summary>
		/// Initialize depth buffer format and multisampling
		/// </summary>
		/// <param name="setPreferredDepthStencilFormat">Set preferred depth
		/// stencil format</param>
		public static void InitializeDepthBufferFormatAndMultisampling(
			DepthFormat setPreferredDepthStencilFormat)
		{
			backBufferDepthFormat = setPreferredDepthStencilFormat;
			remMultiSampleType =
				BaseGame.Device.PresentationParameters.MultiSampleType;
			if (remMultiSampleType == MultiSampleType.NonMaskable)
				remMultiSampleType = MultiSampleType.None;
			remMultiSampleQuality =
				BaseGame.Device.PresentationParameters.MultiSampleQuality;
			remDepthBuffer = BaseGame.Device.DepthStencilBuffer;
		} // InitializeDepthBufferFormatAndMultisampling(setPreferredDepthStenci)
		#endregion

		#region Constructors
		/// <summary>
		/// Id for each created RenderToTexture for the generated filename.
		/// </summary>
		private static int RenderToTextureGlobalInstanceId = 0;
		/// <summary>
		/// Creates an offscreen texture with the specified size which
		/// can be used for render to texture.
		/// </summary>
		public RenderToTexture(SizeType setSizeType)
		{
			sizeType = setSizeType;
			CalcSize();

			texFilename = "RenderToTexture instance " +
				RenderToTextureGlobalInstanceId++;

			Create();

			RenderToTexture.AddRemRenderToTexture(this);
		} // RenderToTexture(setSizeType)
		#endregion

		#region Handle device reset
		/// <summary>
		/// Handle the DeviceReset event, we have to re-create all our render
		/// targets.
		/// </summary>
		private void DeviceReset()
		{
			// Just recreate it.
			Create();
			alreadyResolved = false;
		} // HandleDeviceReset()

		/// <summary>
		/// Handle the DeviceReset event, we have to re-create all our render
		/// targets.
		/// </summary>
		public static void HandleDeviceReset()
		{
			foreach (RenderToTexture renderToTexture in remRenderToTextures)
				renderToTexture.DeviceReset();
		} // HandleDeviceReset()
		#endregion

		#region Create
		/// <summary>
		/// Check if we can use a specific surface format for render targets.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		private static bool CheckRenderTargetFormat(SurfaceFormat format)
		{
			return BaseGame.Device.CreationParameters.Adapter.CheckDeviceFormat(
				BaseGame.Device.CreationParameters.DeviceType,
				BaseGame.Device.DisplayMode.Format,
				TextureUsage.None,//.ResolveTarget,//.WriteOnly,
				QueryUsages.None,//.RenderTarget,
				ResourceType.RenderTarget,//.Texture2D,
				format);
		} // CheckRenderTargetFormat(format)

		/// <summary>
		/// Create
		/// </summary>
		private void Create()
		{
			SurfaceFormat format = SurfaceFormat.Color;
			// Try to use R32F format for shadow mapping if possible (ps20),
			// else just use A8R8G8B8 format for shadow mapping and
			// for normal RenderToTextures too.
			if (sizeType == SizeType.ShadowMap && BaseGame.CanUsePS20)
			{
				// Can do R32F format?
				if (CheckRenderTargetFormat(SurfaceFormat.Single))
					format = SurfaceFormat.Single;
				// Else try R16F format, thats still much better than A8R8G8B8
				else if (CheckRenderTargetFormat(SurfaceFormat.HalfSingle))
					format = SurfaceFormat.HalfSingle;
				// And check a couple more formats (mainly for the Xbox360 support)
				else if (CheckRenderTargetFormat(SurfaceFormat.HalfVector2))
					format = SurfaceFormat.HalfVector2;
				else if (CheckRenderTargetFormat(SurfaceFormat.Luminance16))
					format = SurfaceFormat.Luminance16;
				// Else nothing found, well, then just use the 8 bit Color format.

#if XBOX360
				// Try to force Surface format on the Xbox360, CheckRenderTargetFormat
				// does not work on the Xbox at all!
				format = SurfaceFormat.Single;
#endif
			} // if (sizeType)

			try
			{
				// Create render target of specified size.
				renderTarget = new RenderTarget2D(BaseGame.Device,
					texWidth, texHeight, 1, format,
					MultiSampleType, MultiSampleQuality);
				if (format != SurfaceFormat.Color)
					usesHighPercisionFormat = true;

				// Create z buffer surface for shadow map render targets
				// if they don't fit in our current resolution.
				if (sizeType == SizeType.FullScreenWithZBuffer ||
					sizeType == SizeType.HalfScreenWithZBuffer ||
					sizeType == SizeType.ShadowMap &&
					(texWidth > BaseGame.Width ||
					texHeight > BaseGame.Height))
				{
					zBufferSurface = new DepthStencilBuffer(BaseGame.Device,
						texWidth, texHeight,
						// Lets use the same stuff as the back buffer.
						BackBufferDepthFormat,
						// Don't use multisampling, render target does not support that.
						MultiSampleType, MultiSampleQuality);
				} // if (sizeType)

				loaded = true;
			} // try
			catch (Exception ex)
			{
				// Everything failed, make this unuseable.
				Log.Write("Creating RenderToTexture failed: " + ex.ToString());
				renderTarget = null;
				internalXnaTexture = null;
				loaded = false;
			} // catch
		} // Create()
		#endregion

		#region Clear
		/// <summary>
		/// Clear render target (call SetRenderTarget first)
		/// </summary>
		public void Clear(Color clearColor)
		{
			if (loaded == false ||
				renderTarget == null)
				return;

			BaseGame.Device.Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer,
				clearColor, 1.0f, 0);
		} // Clear(clearColor)
		#endregion

		#region Resolve
		/// <summary>
		/// Make sure we don't call XnaTexture before resolving for the first time!
		/// </summary>
		bool alreadyResolved = false;
		/// <summary>
		/// Resolve render target. For windows developers this method may seem
		/// strange, why not just use the rendertarget's texture? Well, this is
		/// just for the Xbox360 support. The Xbox requires that you call Resolve
		/// first before using the rendertarget texture. The reason for that is
		/// copying the data over from the EPRAM to the video memory, for more
		/// details read the XNA docs.
		/// Note: This method will only work if the render target was set before
		/// with SetRenderTarget, else an exception will be thrown to ensure
		/// correct calling order.
		/// </summary>
		public void Resolve(bool fullResetToBackBuffer)
		{
			// Make sure this render target is currently set!
			if (RenderToTexture.CurrentRenderTarget != renderTarget)
				throw new Exception(
					"You can't call Resolve without first setting the render target!");

			alreadyResolved = true;
			//does not exist anymore: BaseGame.Device.res.ResolveRenderTarget(0);

			// Instead just restore the back buffer (will automatically resolve)
			ResetRenderTarget(fullResetToBackBuffer);
		} // Resolve()
		#endregion

		#region Set and reset render targets
		/// <summary>
		/// Remember scene render target. This is very important because
		/// for our post screen shaders we have to render our whole scene
		/// to this render target. But in the process we will use many other
		/// shaders and they might set their own render targets and then
		/// reset it, but we need to have this scene still to be set.
		/// Don't reset to the back buffer (with SetRenderTarget(0, null), this
		/// would stop rendering to our scene render target and the post screen
		/// shader will not be able to process our screen.
		/// The whole reason for this is that we can't use StrechRectangle
		/// like in Rocket Commander because XNA does not provide that function
		/// (the reason for that is cross platform compatibility with the XBox360).
		/// Instead we could use ResolveBackBuffer, but that method is VERY SLOW.
		/// Our framerate would drop from 600 fps down to 20, not good.
		/// However, multisampling will not work, so we will disable it anyway!
		/// </summary>
		static RenderTarget2D remSceneRenderTarget = null;
		/// <summary>
		/// Remember the last render target we set, this way we can check
		/// if the rendertarget was set before calling resolve!
		/// </summary>
		static RenderTarget2D lastSetRenderTarget = null;

		/// <summary>
		/// Remember render to texture instances to allow recreating them all
		/// when DeviceReset is called.
		/// </summary>
		static List<RenderToTexture> remRenderToTextures =
			new List<RenderToTexture>();

		/// <summary>
		/// Add render to texture instance to allow recreating them all
		/// when DeviceReset is called with help of the remRenderToTextures list. 
		/// </summary>
		public static void AddRemRenderToTexture(RenderToTexture renderToTexture)
		{
			remRenderToTextures.Add(renderToTexture);
		} // AddRemRenderToTexture(renderToTexture)

		/// <summary>
		/// Current render target we have set, null if it is just the back buffer.
		/// </summary>
		public static RenderTarget2D CurrentRenderTarget
		{
			get
			{
				return lastSetRenderTarget;
			} // get
		} // CurrentRenderTarget

		/// <summary>
		/// Set render target to this texture to render stuff on it.
		/// </summary>
		public bool SetRenderTarget()
		{
			if (loaded == false ||
				renderTarget == null)
				return false;

			SetRenderTarget(renderTarget, false);
			if (zBufferSurface != null)
				BaseGame.Device.DepthStencilBuffer = zBufferSurface;
			return true;
		} // SetRenderTarget()

		/// <summary>
		/// Set render target
		/// </summary>
		/// <param name="isSceneRenderTarget">Is scene render target</param>
		internal static void SetRenderTarget(RenderTarget2D renderTarget,
			bool isSceneRenderTarget)
		{
			BaseGame.Device.SetRenderTarget(0, renderTarget);
			if (isSceneRenderTarget)
				remSceneRenderTarget = renderTarget;
			lastSetRenderTarget = renderTarget;
		} // SetRenderTarget(renderTarget, isSceneRenderTarget)

		/// <summary>
		/// Reset render target
		/// </summary>
		/// <param name="fullResetToBackBuffer">Full reset to back buffer</param>
		internal static void ResetRenderTarget(bool fullResetToBackBuffer)
		{
			if (remSceneRenderTarget == null ||
				fullResetToBackBuffer)
			{
				remSceneRenderTarget = null;
				lastSetRenderTarget = null;
				BaseGame.Device.SetRenderTarget(0, null);
				BaseGame.Device.DepthStencilBuffer = remDepthBuffer;
			} // if (remSceneRenderTarget)
			else
			{
				BaseGame.Device.SetRenderTarget(0, remSceneRenderTarget);
				lastSetRenderTarget = remSceneRenderTarget;
			} // else
		} // ResetRenderTarget(fullResetToBackBuffer)
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test create render to texture
		/// </summary>
		static public void TestCreateRenderToTexture()
		{
			Model testPlate = null;
			RenderToTexture renderToTexture = null;

			TestGame.Start(
				delegate
				{
					testPlate = new Model("CarSelectionPlate");
					renderToTexture = new RenderToTexture(
						//SizeType.ShadowMap1024);
						//.QuarterScreen);
						SizeType.FullScreen);
						//SizeType.HalfScreen);
				},
				delegate
				{
					bool renderToTextureWay =
						Input.Keyboard.IsKeyUp(Keys.Space) &&
						Input.GamePadAPressed == false;
					if (renderToTextureWay)
					{
						// Set render target to our texture
						renderToTexture.SetRenderTarget();

						// Clear background
						renderToTexture.Clear(Color.Blue);

						// Draw background lines
						BaseGame.DrawLine(new Point(0, 200), new Point(200, 0), Color.Blue);
						BaseGame.DrawLine(new Point(0, 0), new Point(400, 400), Color.Red);
						BaseGame.FlushLineManager2D();

						// And draw object
						testPlate.Render(Matrix.CreateScale(1.5f));
						// And flush render manager to draw all objects
						BaseGame.MeshRenderManager.Render();

						// Resolve
						renderToTexture.Resolve(true);

						// Reset background buffer
						//obs: RenderToTexture.ResetRenderTarget(true);
						
						// Show render target in a rectangle on our screen
						renderToTexture.RenderOnScreen(
							//tst:
							new Rectangle(100, 100, 256, 256));
							//BaseGame.ResolutionRect);
					} // if (renderToTextureWay)
					else
					{
						// Copy backbuffer way, render stuff normally first
						// Clear background
						BaseGame.Device.Clear(Color.Blue);

						// Draw background lines
						BaseGame.DrawLine(new Point(0, 200), new Point(200, 0), Color.Blue);
						BaseGame.DrawLine(new Point(0, 0), new Point(400, 400), Color.Red);
						BaseGame.FlushLineManager2D();

						// And draw object
						testPlate.Render(Matrix.CreateScale(1.5f));
					} // else

					TextureFont.WriteText(2, 30,
						"renderToTexture.Width=" + renderToTexture.Width);
					TextureFont.WriteText(2, 60,
						"renderToTexture.Height=" + renderToTexture.Height);
					TextureFont.WriteText(2, 90,
						"renderToTexture.Valid=" + renderToTexture.IsValid);
					TextureFont.WriteText(2, 120,
						"renderToTexture.XnaTexture=" + renderToTexture.XnaTexture);
					TextureFont.WriteText(2, 150,
						"renderToTexture.ZBufferSurface=" + renderToTexture.ZBufferSurface);
					TextureFont.WriteText(2, 180,
						"renderToTexture.Filename=" + renderToTexture.Filename);
				});
		} // TestCreateRenderToTexture()
#endif
		#endregion
	} // class RenderToTexture
} // namespace RacingGame.Shaders
