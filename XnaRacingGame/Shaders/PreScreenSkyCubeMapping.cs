// Project: RacingGame, File: PreScreenSkyCubeMapping.cs
// Namespace: RacingGame.Shaders, Class: PreScreenSkyCubeMapping
// Path: C:\code\RacingGame\Shaders, Author: Abi
// Code lines: 217, Size of file: 5,51 KB
// Creation date: 08.09.2006 06:06
// Last modified: 05.11.2006 00:52
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using RacingGame.Graphics;
using RacingGame.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RacingGame.GameScreens;
#endregion

namespace RacingGame.Shaders
{
	/// <summary>
	/// Pre screen sky cube mapping, if shaders are not supported a special
	/// fallback to fixed function pipeline rendering is provided also!
	/// </summary>
	public class PreScreenSkyCubeMapping : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// Shader effect filename.
		/// </summary>
		const string Filename = "PreScreenSkyCubeMapping.fx";

		/// <summary>
		/// Sky cube map texture filename.
		/// </summary>
		const string SkyCubeMapFilename = //"Textures\\SkyCubeMap.dds";
			//"SkyCubeMap"; // content name
			// Use same content name as used by models:
			"SkyCubeMap%0"; // content name

		/// <summary>
		/// Default sky color
		/// </summary>
		static readonly Color DefaultSkyColor = new Color(232, 232, 232);

		/// <summary>
		/// The Cube Map texture for the sky!
		/// </summary>
		private TextureCube skyCubeMapTexture = null;

		/// <summary>
		/// Sky cube map texture
		/// </summary>
		/// <returns>Texture cube</returns>
		public TextureCube SkyCubeMapTexture
		{
			get
			{
				return skyCubeMapTexture;
			} // get
		} // SkyCubeMapTexture
		#endregion

		#region Constructor
		/// <summary>
		/// Create pre screen sky cube mapping
		/// </summary>
		public PreScreenSkyCubeMapping() : base(Filename)
		{
			// All loading is done in the ShaderEffect class.
		} // PreScreenSkyCubeMapping()
		#endregion

		#region Get parameters
		/// <summary>
		/// Reload
		/// </summary>
		protected override void GetParameters()
		{
			base.GetParameters();

			// Load and set cube map texture
			skyCubeMapTexture = BaseGame.Content.Load<TextureCube>(
				Path.Combine(Directories.ContentDirectory, SkyCubeMapFilename));
			diffuseTexture.SetValue(skyCubeMapTexture);

			// Set sky color to nearly white and scale to 1
			AmbientColor = DefaultSkyColor;
			Scale = 1.0f;
		} // GetParameters()
		#endregion

		#region Render sky
		/// <summary>
		/// Render sky with help of shader.
		/// </summary>
		public void RenderSky(float setWrappingScale, Color setSkyColor)
		{
			// Can't render with shader if shader is not valid!
			if (this.Valid == false)
				return;

			// Don't use or write to the z buffer
			BaseGame.Device.RenderState.DepthBufferEnable = false;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;

			// Also don't use any kind of blending.
			BaseGame.Device.RenderState.AlphaBlendEnable = false;

			Scale = setWrappingScale;
			AmbientColor = setSkyColor;
			// Rotate view matrix by level number, this way we look to a different
			// direction depending on which level we are in.
			Matrix invViewMatrix = BaseGame.InverseViewMatrix *
				Matrix.CreateRotationZ(-TrackSelection.SelectedTrackNumber *
				(float)Math.PI / 2.0f);
			InverseViewMatrix = invViewMatrix;

			// Start shader
			try
			{
				// Remember old state because we will use clamp texturing here
				effect.Begin(SaveStateMode.SaveState);

				// Render with specific pass
				//foreach (EffectPass pass in effect.Techniques[0].Passes)
				for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
				{
					EffectPass pass = effect.CurrentTechnique.Passes[num];

					// Render each pass
					pass.Begin();
					VBScreenHelper.Render();
					pass.End();
				} // foreach (pass)
			} // try
			finally
			{
				// End shader
				effect.End();
			} // finally

			// Enable z buffer again
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
		} // RenderSky(setWrappingScale, setSkyColor)

		/// <summary>
		/// Render sky
		/// </summary>
		public void RenderSky()
		{
			RenderSky(lastUsedScale, lastUsedAmbientColor);
		} // RenderSky()
		#endregion

		#region Unit testing
#if DEBUG
		/// <summary>
		/// Test sky cube mapping
		/// </summary>
		//[Test]
		public static void TestSkyCubeMapping()
		{
			PreScreenSkyCubeMapping skyCube = null;

			TestGame.Start("TestSkyCubeMapping",
				delegate
				{
					skyCube = new PreScreenSkyCubeMapping();
				},
				delegate
				{
					skyCube.RenderSky();
				});
		} // TestSkyCubeMapping()
#endif
		#endregion
	} // class PreScreenSkyCubeMapping
} // namespace RacingGame.Shaders
