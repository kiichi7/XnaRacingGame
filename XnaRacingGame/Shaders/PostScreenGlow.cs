// Project: RacingGame, File: PostScreenGlow.cs
// Namespace: RacingGame.Shaders, Class: PostScreenGlow
// Path: C:\code\RacingGame\Shaders, Author: Abi
// Code lines: 409, Size of file: 12,60 KB
// Creation date: 12.09.2006 07:20
// Last modified: 13.10.2006 11:24
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using System;
using System.Collections;
using System.Text;
using System.IO;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Texture = RacingGame.Graphics.Texture;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
#endregion

namespace RacingGame.Shaders
{
	/// <summary>
	/// Post screen glow shader based on PostScreenGlow.fx.
	/// Derive from PostScreenMenu, this way we can save duplicating
	/// the effect parameters and use the same RenderToTextures.
	/// </summary>
	public class PostScreenGlow : PostScreenMenu
	{
		#region Variables
		/// <summary>
		/// The shader effect filename for this shader.
		/// </summary>
		private const string Filename = "PostScreenGlow.fx";

		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		private EffectParameter radialSceneMap,
			radialBlurScaleFactor,
			screenBorderFadeoutMap;

		/// <summary>
		/// Links to the passTextures, easier to write code this way.
		/// This are just reference copies.
		/// </summary>
		private RenderToTexture radialSceneMapTexture;

		/// <summary>
		/// Helper texture for the screen border (darken the borders).
		/// </summary>
		private Texture screenBorderFadeoutMapTexture = null;
		#endregion

		#region Properties
		/// <summary>
		/// Last used radial blur scale factor
		/// </summary>
		private float lastUsedRadialBlurScaleFactor = -0.0001f;
		/// <summary>
		/// Radial blur scale factor
		/// </summary>
		public float RadialBlurScaleFactor
		{
			get
			{
				return lastUsedRadialBlurScaleFactor;
			} // get
			set
			{
				if (radialBlurScaleFactor != null &&
					lastUsedRadialBlurScaleFactor != value)
				{
					lastUsedRadialBlurScaleFactor = value;
					radialBlurScaleFactor.SetValue(value);
				} // if (effect.D3DEffect)
			} // set
		} // RadialBlurScaleFactor
		#endregion

		#region Constructor
		/// <summary>
		/// Create post screen glow
		/// </summary>
		public PostScreenGlow()
			: base(Filename)
		{
			// Final map for glow, used to perform radial blur next step
			radialSceneMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.FullScreen);
		} // PostScreenGlow()
		#endregion

		#region Get parameters
		/// <summary>
		/// Reload
		/// </summary>
		protected override void GetParameters()
		{
			// Can't get parameters if loading failed!
			if (effect == null)
				return;

			windowSize = effect.Parameters["windowSize"];
			sceneMap = effect.Parameters["sceneMap"];

			// We need both windowSize and sceneMap.
			if (windowSize == null ||
				sceneMap == null)
				throw new NotSupportedException("windowSize and sceneMap must be " +
					"valid in PostScreenShader=" + Filename);

			// Init additional stuff
			downsampleMap = effect.Parameters["downsampleMap"];
			blurMap1 = effect.Parameters["blurMap1"];
			blurMap2 = effect.Parameters["blurMap2"];
			radialSceneMap = effect.Parameters["radialSceneMap"];

			// Load screen border texture
			screenBorderFadeoutMap = effect.Parameters["screenBorderFadeoutMap"];
			screenBorderFadeoutMapTexture = new Texture("ScreenBorderFadeout.dds");
			// Set texture
			screenBorderFadeoutMap.SetValue(
				screenBorderFadeoutMapTexture.XnaTexture);

			radialBlurScaleFactor = effect.Parameters["radialBlurScaleFactor"];
		} // GetParameters()
		#endregion

		#region Show
		/// <summary>
		/// Execute shaders and show result on screen, Start(..) must have been
		/// called before and the scene should be rendered to sceneMapTexture.
		/// </summary>
		public override void Show()
		{
			// Only apply post screen glow if texture is valid and effect is valid 
			if (sceneMapTexture == null ||
				effect == null ||
				started == false)
				return;

			started = false;

			// Resolve sceneMapTexture render target for Xbox360 support
			sceneMapTexture.Resolve(true);

			// Don't use or write to the z buffer
			BaseGame.Device.RenderState.DepthBufferEnable = false;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;
			// Also don't use any kind of blending.
			//Update: allow writing to alpha!
			BaseGame.Device.RenderState.AlphaBlendEnable = false;

			if (windowSize != null)
				windowSize.SetValue(
					new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
			if (sceneMap != null)
				sceneMap.SetValue(sceneMapTexture.XnaTexture);

			RadialBlurScaleFactor = //-0.0025f //heavy: -0.0085f;//medium: -0.005f;
				// Warning: To big values will make the motion blur look to
				// stepy (we see each step and thats not good). -0.02 should be max.
				-(0.0025f + RacingGameManager.Player.Speed * 0.005f /
				Player.DefaultMaxSpeed);

			effect.CurrentTechnique = effect.Techniques[
				BaseGame.CanUsePS20 ? "ScreenGlow20" : "ScreenGlow"];
			
			// We must have exactly 5 passes!
			if (effect.CurrentTechnique.Passes.Count != 5)
				throw new InvalidOperationException(
					"This shader should have exactly 5 passes!");

			try
			{
				effect.Begin();
				for (int pass = 0; pass < effect.CurrentTechnique.Passes.Count;
					pass++)
				{
					if (pass == 0)
						radialSceneMapTexture.SetRenderTarget();
					else if (pass == 1)
						downsampleMapTexture.SetRenderTarget();
					else if (pass == 2)
						blurMap1Texture.SetRenderTarget();
					else if (pass == 3)
						blurMap2Texture.SetRenderTarget();
					else
						// Do a full reset back to the back buffer
						RenderToTexture.ResetRenderTarget(true);

					EffectPass effectPass = effect.CurrentTechnique.Passes[pass];
					effectPass.Begin();
					// For first effect we use radial blur, draw it with a grid
					// to get cooler results (more blur at borders than in middle).
					if (pass == 0)
						VBScreenHelper.Render10x10Grid();
					else
						VBScreenHelper.Render();
					effectPass.End();

					if (pass == 0)
					{
						radialSceneMapTexture.Resolve(false);
						if (radialSceneMap != null)
							radialSceneMap.SetValue(radialSceneMapTexture.XnaTexture);
						effect.CommitChanges();
					} // if
					else if (pass == 1)
					{
						downsampleMapTexture.Resolve(false);
						if (downsampleMap != null)
							downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
						effect.CommitChanges();
					} // if
					else if (pass == 2)
					{
						blurMap1Texture.Resolve(false);
						if (blurMap1 != null)
							blurMap1.SetValue(blurMap1Texture.XnaTexture);
						effect.CommitChanges();
					} // else if
					else if (pass == 3)
					{
						blurMap2Texture.Resolve(false);
						if (blurMap2 != null)
							blurMap2.SetValue(blurMap2Texture.XnaTexture);
						effect.CommitChanges();
					} // else if
				} // for (pass, <, ++)
			} // try
			finally
			{
				effect.End();

				// Restore z buffer state
				BaseGame.Device.RenderState.DepthBufferEnable = true;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			} // finally
		} // Show()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test post screen glow
		/// </summary>
		//[Test]
		public static void TestPostScreenGlow()
		{
			//Model testModel = null;
			//PostScreenGlow glowShader = null;

			TestGame.Start("TestPostScreenGlow",
				delegate
				{
					//testModel = new Model("Asteroid2.x");
					//glowShader = new PostScreenGlow();
				},
				delegate
				{
					BaseGame.UI.PostScreenGlowShader.Start();

					//Thread.Sleep(10);
					BaseGame.UI.RenderGameBackground();
					//testModel.Render(Vector3.Empty);
					BaseGame.DrawLine(
						Vector3.Zero, new Vector3(100, 100, 100), Color.Red);
					BaseGame.FlushLineManager3D();
					
					//*
					if (Input.Keyboard.IsKeyDown(Keys.LeftControl) ||
						Input.GamePadXPressed)
						BaseGame.UI.PostScreenMenuShader.Show();
					else if (Input.Keyboard.IsKeyDown(Keys.LeftAlt) == false &&
						Input.GamePadAPressed == false)
						BaseGame.UI.PostScreenGlowShader.Show();
					else
					{
						// Resolve first
						if (PostScreenGlow.Started)
							sceneMapTexture.Resolve(true);
						started = false;

						// Reset background buffer
						RenderToTexture.ResetRenderTarget(true);
						// Just show scene map
						//BaseGame.UI.PostScreenGlowShader.
						sceneMapTexture.RenderOnScreen(BaseGame.ResolutionRect);
					} // else

					TextureFont.WriteText(2, 30,
						"Press left control or X to show menu post screen shader instead.");
					TextureFont.WriteText(2, 60,
						"Press left alt or A to just show the unchanged screen.");
					TextureFont.WriteText(2, 90,
						"Press space or B to see all menu post screen render passes.");

					if (Input.Keyboard.IsKeyDown(Keys.Space) ||// == false)
						Input.GamePadBPressed)
					{
						PostScreenGlow psg = BaseGame.UI.PostScreenGlowShader;
						sceneMapTexture.RenderOnScreen(
							new Rectangle(10, 10, 256, 256));
						psg.radialSceneMapTexture.RenderOnScreen(
							new Rectangle(10+256+10, 10, 256, 256));
						downsampleMapTexture.RenderOnScreen(
							new Rectangle(10+256+10+256+10, 10, 256, 256));
						blurMap1Texture.RenderOnScreen(
							new Rectangle(10, 10+256+10, 256, 256));
						blurMap2Texture.RenderOnScreen(
							new Rectangle(10+256+10, 10+256+10, 256, 256));
						psg.screenBorderFadeoutMapTexture.RenderOnScreen(
							new Rectangle(10+256+10+256+10, 10+256+10, 256, 256));
					} // if (Input.Keyboard.IsKeyDown)
				});
		} // TestPostScreenGlow()
#endif
		#endregion
	} // class PostScreenGlow
} // namespace RacingGame.Shaders
