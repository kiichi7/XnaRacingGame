// Project: RacingGame, File: ShadowMapShader.cs
// Namespace: RacingGame.Shaders, Class: ShadowMapShader
// Path: C:\code\RacingGame\Shaders, Author: Abi
// Code lines: 655, Size of file: 19,49 KB
// Creation date: 07.10.2006 10:23
// Last modified: 14.10.2006 10:10
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.GameScreens;
using Model = RacingGame.Graphics.Model;
using Texture = RacingGame.Graphics.Texture;
#endregion

namespace RacingGame.Shaders
{
	/// <summary>
	/// Shadow map shader
	/// </summary>
	public class ShadowMapShader : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// Shadow mapping shader filename
		/// </summary>
		const string ShaderFilename = "ShadowMap.fx";

		/// <summary>
		/// Shadow map texture we render to.
		/// </summary>
		internal RenderToTexture
			shadowMapTexture = null;

		/// <summary>
		/// Restrict near and far plane for much better depth resolution!
		/// </summary>
		internal float
			shadowNearPlane = 1.0f,//1.0f,//2.0f,
			shadowFarPlane = 1.0f*28;//42.0f;//32.0f;//16.0f;//6.0f;

		/// <summary>
		/// Virtual point light parameters for directional shadow map lighting.
		/// Used to create a point light position for the directional light.
		/// </summary>
		internal float
			virtualLightDistance = 24,//36.0f,//26.0f,//8.0f,// 5.0f,
			virtualVisibleRange = 23.5f;//35.0f;//25.0f;//5.0f;//12.0f;// 5.0f;

		/// <summary>
		/// Shadow distance
		/// </summary>
		/// <returns>Float</returns>
		public float ShadowDistance
		{
			get
			{
				return virtualLightDistance;
			} // get
		} // ShadowDistance

		private Vector3 shadowLightPos = Vector3.Zero;

		/// <summary>
		/// Shadow light position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 ShadowLightPos
		{
			get
			{
				return shadowLightPos;
			} // get
		} // ShadowLightPos

		/// <summary>
		/// Texel width and height and offset for texScaleBiasMatrix,
		/// this way we can directly access the middle of each texel.
		/// </summary>
		float
			texelWidth = 1.0f / 1024.0f,
			texelHeight = 1.0f / 1024.0f,
			texOffsetX = 0.5f,
			texOffsetY = 0.5f;

		/// <summary>
		/// Compare depth bias
		/// </summary>
		internal float compareDepthBias = 0.00025f;

		/// <summary>
		/// Tex extra scale
		/// </summary>
		/// <returns>1.0f</returns>
		internal float texExtraScale = 1.0f;//1.0015f;//1.0075f;

		/// <summary>
		/// Shadow map depth bias value
		/// </summary>
		/// <returns>+</returns>
		internal float shadowMapDepthBiasValue = 0.00025f;

		/// <summary>
		/// The matrix to convert proj screen coordinates in the -1..1 range
		/// to the shadow depth map texture coordinates.
		/// </summary>
		Matrix texScaleBiasMatrix;

		/// <summary>
		/// Used matrices for the light casting the shadows.
		/// </summary>
		internal Matrix lightProjectionMatrix, lightViewMatrix;

		/// <summary>
		/// Additional effect handles
		/// </summary>
		private EffectParameter
			shadowTexTransform,
			worldViewProjLight,
			nearPlane,
			farPlane,
			depthBias,
			shadowMapDepthBias,
			shadowMap,
			shadowMapTexelSize,
			shadowDistanceFadeoutTexture;

		/// <summary>
		/// Shadow map blur post screen shader, used in RenderShadows
		/// to blur the shadow results.
		/// </summary>
		internal ShadowMapBlur shadowMapBlur = null;
		#endregion

		#region Calc shadow map bias matrix
		/// <summary>
		/// Calculate the texScaleBiasMatrix for converting proj screen
		/// coordinates in the -1..1 range to the shadow depth map
		/// texture coordinates.
		/// </summary>
		internal void CalcShadowMapBiasMatrix()
		{
			texelWidth = 1.0f / (float)shadowMapTexture.Width;
			texelHeight = 1.0f / (float)shadowMapTexture.Height;
			texOffsetX = 0.5f + (0.5f / (float)shadowMapTexture.Width);
			texOffsetY = 0.5f + (0.5f / (float)shadowMapTexture.Height);

			texScaleBiasMatrix = new Matrix(
				0.5f * texExtraScale, 0.0f, 0.0f, 0.0f,
				0.0f, -0.5f * texExtraScale, 0.0f, 0.0f,
				0.0f, 0.0f, texExtraScale, 0.0f,
				texOffsetX, texOffsetY, 0.0f, 1.0f);
		} // CalcShadowMapBiasMatrix()
		#endregion

		#region Constructors
		/// <summary>
		/// Shadow map shader
		/// </summary>
		public ShadowMapShader()
			: base(ShaderFilename)
		{
			if (BaseGame.CanUsePS20)
				// We use R32F, etc. and have a lot of precision
				compareDepthBias = 0.0001f;
			else
				// A8R8G8B8 isn't very precise, increase comparing depth bias
				compareDepthBias = 0.0075f;// 0.025f;

			// Ok, time to create the shadow map render target
			shadowMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.ShadowMap);

			CalcShadowMapBiasMatrix();

			shadowMapBlur = new ShadowMapBlur();
		} // ShadowMapShader(tryToUsePS20, setBackFaceCheck, setShadowMapSize)
		#endregion

		#region Get parameters
		/// <summary>
		/// Get parameters
		/// </summary>
		protected override void GetParameters()
		{
			// Can't get parameters if loading failed!
			if (effect == null)
				return;

			base.GetParameters();

			// Get additional parameters
			shadowTexTransform = effect.Parameters["shadowTexTransform"];
			worldViewProjLight = effect.Parameters["worldViewProjLight"];
			nearPlane = effect.Parameters["nearPlane"];
			farPlane = effect.Parameters["farPlane"];
			depthBias = effect.Parameters["depthBias"];
			shadowMapDepthBias = effect.Parameters["shadowMapDepthBias"];
			shadowMap = effect.Parameters["shadowMap"];
			shadowMapTexelSize = effect.Parameters["shadowMapTexelSize"];
			shadowDistanceFadeoutTexture =
				effect.Parameters["shadowDistanceFadeoutTexture"];
			// Load shadowDistanceFadeoutTexture
			if (shadowDistanceFadeoutTexture != null)
				shadowDistanceFadeoutTexture.SetValue(
					new Texture("ShadowDistanceFadeoutMap").XnaTexture);
		} // GetParameters()
		#endregion

		#region Update parameters
		/// <summary>
		/// Update parameters
		/// </summary>
		public override void SetParameters(Material setMat)
		{
			// Can't set parameters if loading failed!
			if (effect == null)
				return;

			// If we are in menu mode, use different values for the shader
			// than in the game!
			if (RacingGameManager.InMenu)
			{
				shadowNearPlane = 1.0f;
				shadowFarPlane = 28;
				virtualLightDistance = 24;
				virtualVisibleRange = 23.5f;
				if (BaseGame.CanUsePS20)
				{
					compareDepthBias = 0.00075f;//0.00025f;
					shadowMapDepthBiasValue = 0.00075f;//0.00025f;
				} // if (BaseGame.CanUsePS20)
				else
				{
					// Ps 1.1 isn't as percise!
					compareDepthBias = 0.0025f;
					shadowMapDepthBiasValue = 0.002f;
				} // else
			} // if (RacingGameManager.InMenu)
			else
			{
				shadowNearPlane = 1.0f;
				shadowFarPlane = 6.25f * 28 * 1.25f;
				virtualLightDistance = 5.5f * 24 * 1.3f;
				virtualVisibleRange = 5.5f * 23.5f;
			
				if (BaseGame.CanUsePS20)
				{
					compareDepthBias = 0.00085f;//0.00025f;
					shadowMapDepthBiasValue = 0.00085f;// 0.00025f;
				} // if (BaseGame.CanUsePS20)
				else
				{
					// Ps 1.1 isn't as percise!
					shadowFarPlane = 4.75f*28;
					virtualLightDistance = 4.0f*24;
					virtualVisibleRange = 4.0f*23.5f;
					compareDepthBias = 0.0065f;//0.0045f;//0.0025f;
					shadowMapDepthBiasValue = 0.0035f;//0.0025f;
				} // else
			} // else
#if XBOX360
			// Make it a little more percise on the box.
			compareDepthBias = 0.00065f;//0.00025f;
			shadowMapDepthBiasValue = 0.00065f;// 0.00025f;
#endif

			base.SetParameters(setMat);

			// Set all extra parameters for this shader
			depthBias.SetValue(compareDepthBias);
			shadowMapDepthBias.SetValue(shadowMapDepthBiasValue);
			shadowMapTexelSize.SetValue(
				new Vector2(texelWidth, texelHeight));
			nearPlane.SetValue(shadowNearPlane);
			farPlane.SetValue(shadowFarPlane);
		} // UpdateShadowParameters()
		#endregion

		#region Create simple directional shadow mapping matrix
		/// <summary>
		/// Calc simple directional shadow mapping matrix
		/// </summary>
		private void CalcSimpleDirectionalShadowMappingMatrix()
		{
			// Put light for directional mode away from origin (create virutal point
			// light). But adjust field of view to see enough of the visible area.
			float virtualFieldOfView = (float)Math.Atan2(
				virtualVisibleRange, virtualLightDistance);

			// Set projection matrix for light
			lightProjectionMatrix = Matrix.CreatePerspective(
				// Don't use graphics fov and aspect ratio in directional lighting mode
				virtualFieldOfView,
				1.0f,
				shadowNearPlane,
				shadowFarPlane);

			// Calc light look pos, put it a little bit in front of our car
			Vector3 lightLookPos =
				RacingGameManager.InMenu ? RacingGameManager.Player.CarPosition :
				RacingGameManager.Player.CarPosition +
				RacingGameManager.Player.CarDirection * virtualVisibleRange / 6;

			// Well, this is how directional lights are done:
			lightViewMatrix = Matrix.CreateLookAt(
				// Use our current car position for our light look at origin!
				lightLookPos +
				BaseGame.LightDirection * virtualVisibleRange,//virtualLightDistance,
				lightLookPos,
				new Vector3(0, 0, 1));

			// Update light pos
			Matrix invView = Matrix.Invert(lightViewMatrix);
			shadowLightPos = new Vector3(invView.M41, invView.M42, invView.M43);
		} // CalcSimpleDirectionalShadowMappingMatrix()
		#endregion

		#region Generate shadow
		/// <summary>
		/// Update shadow world matrix.
		/// Calling this function is important to keep the shaders
		/// WorldMatrix and WorldViewProjMatrix up to date.
		/// </summary>
		/// <param name="setWorldMatrix">World matrix</param>
		internal void UpdateGenerateShadowWorldMatrix(Matrix setWorldMatrix)
		{
			Matrix world = setWorldMatrix;
			WorldMatrix = world;
			WorldViewProjMatrix =
				world * lightViewMatrix * lightProjectionMatrix;
			effect.CommitChanges();
		} // UpdateGenerateShadowWorldMatrix(setWorldMatrix)

		/// <summary>
		/// Generate shadow
		/// </summary>
		internal void GenerateShadows(BaseGame.RenderHandler renderObjects)
		{
			// Can't generate shadow if loading failed!
			if (effect == null)
				return;

			// This method sets all required shader variables.
			this.SetParameters(null);
			Matrix remViewMatrix = BaseGame.ViewMatrix;
			Matrix remProjMatrix = BaseGame.ProjectionMatrix;
			CalcSimpleDirectionalShadowMappingMatrix();
			
			// Time to generate the shadow texture
			DepthStencilBuffer remBackBufferSurface = null;
			// Start rendering onto the shadow map
			shadowMapTexture.SetRenderTarget();
			if (shadowMapTexture.ZBufferSurface != null)
			{
				remBackBufferSurface = BaseGame.Device.DepthStencilBuffer;
				BaseGame.Device.DepthStencilBuffer =
					shadowMapTexture.ZBufferSurface;
			} // if (shadowMapTexture.ZBufferSurface)

			// Make sure depth buffer is on
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			// Disable alpha
			BaseGame.Device.RenderState.AlphaBlendEnable = false;

			// Clear render target
			shadowMapTexture.Clear(Color.White);

			if (BaseGame.CanUsePS20)
				effect.CurrentTechnique = effect.Techniques["GenerateShadowMap20"];
			else
				effect.CurrentTechnique = effect.Techniques["GenerateShadowMap"];

			// Render shadows with help of the GenerateShadowMap shader
			RenderSinglePassShader(renderObjects);

			// Resolve the render target to get the texture (required for Xbox)
			shadowMapTexture.Resolve(false);

			// Set render target back to default
			//not required anymore: RenderToTexture.ResetRenderTarget(false);
			
			if (shadowMapTexture.ZBufferSurface != null)
				BaseGame.Device.DepthStencilBuffer = remBackBufferSurface;

			BaseGame.ViewMatrix = remViewMatrix;
			BaseGame.ProjectionMatrix = remProjMatrix;
		} // GenerateShadows()
		#endregion

		#region Use shadow
		/// <summary>
		/// Update calc shadow world matrix, has to be done for each object
		/// we want to render in CalcShadows.
		/// </summary>
		/// <param name="setWorldMatrix">Set world matrix</param>
		internal void UpdateCalcShadowWorldMatrix(Matrix setWorldMatrix)
		{
			this.WorldMatrix = setWorldMatrix;
			this.WorldViewProjMatrix =
				setWorldMatrix * BaseGame.ViewMatrix * BaseGame.ProjectionMatrix;

			// Compute the matrix to transform from view space to light proj:
			// inverse of view matrix * light view matrix * light proj matrix
			Matrix lightTransformMatrix =
				setWorldMatrix *
				lightViewMatrix *
				lightProjectionMatrix *
				texScaleBiasMatrix;
			shadowTexTransform.SetValue(lightTransformMatrix);

			Matrix worldViewProjLightMatrix =
				setWorldMatrix *
				lightViewMatrix *
				lightProjectionMatrix;
			worldViewProjLight.SetValue(worldViewProjLightMatrix);

			effect.CommitChanges();
		} // UpdateCalcShadowWorldMatrix(setWorldMatrix)

		/// <summary>
		/// Calc shadows with help of generated light depth map,
		/// all objects have to be rendered again for comparing.
		/// We could save a pass when directly rendering here, but this
		/// has 2 disadvantages: 1. we can't post screen blur the shadow
		/// and 2. we can't use any other shader, especially bump and specular
		/// mapping shaders don't have any instructions left with ps_1_1.
		/// This way everything is kept simple, we can do as complex shaders
		/// as we want, the shadow shaders work seperately.
		/// </summary>
		/// <param name="renderObjects">Render objects</param>
		public void RenderShadows(BaseGame.RenderHandler renderObjects)
		{
			// Can't calc shadows if loading failed!
			if (effect == null)
				return;

			// Make sure z buffer and writing z buffer is on
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

			// Render shadows into our shadowMapBlur render target
			shadowMapBlur.RenderShadows(
				delegate
				{
					if (BaseGame.CanUsePS20)
						effect.CurrentTechnique = effect.Techniques["UseShadowMap20"];
					else
						effect.CurrentTechnique = effect.Techniques["UseShadowMap"];

					// This method sets all required shader variables.
					this.SetParameters(null);

					// Use the shadow map texture here which was generated in
					// GenerateShadows().
					shadowMap.SetValue(shadowMapTexture.XnaTexture);

					// Render shadows with help of the UseShadowMap shader
					RenderSinglePassShader(renderObjects);
				});

			// Start rendering the shadow map blur (pass 1, which messes up our
			// background), pass 2 can be done below without any render targets.
			shadowMapBlur.RenderShadows();

			// Kill background z buffer (else glass will not be rendered correctly)
			RacingGameManager.Device.Clear(
				ClearOptions.DepthBuffer, Color.Black, 1, 0);
		} // RenderShadows(renderObjects)
		#endregion

		#region ShowShadows
		/// <summary>
		/// Show Shadows
		/// </summary>
		public void ShowShadows()
		{
			shadowMapBlur.ShowShadows();
		} // ShowShadows()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test shadow mapping
		/// </summary>
		public static void TestShadowMapping()
		{
			Model testPile = null;
			int carNumber = 0;

			TestGame.Start("TestShadowMapping",
				delegate
				{
					testPile = new Model("GuardRailHolder");
				},
				delegate
				{
					BaseGame.UI.PostScreenGlowShader.Start();

					if (Input.Keyboard.IsKeyUp(Keys.LeftAlt) &&
						Input.GamePadXPressed == false)
					{
						// Generate shadows
						ShaderEffect.shadowMapping.GenerateShadows(
							delegate
							{
								//testPlate.GenerateShadow(Matrix.CreateScale(1.5f));
								RacingGameManager.CarModel.GenerateShadow(Matrix.CreateRotationZ(0.85f));
								//testPile.GenerateShadow(Matrix.CreateScale(4));
							});

						// Render shadows
						ShaderEffect.shadowMapping.RenderShadows(
							delegate
							{
								RacingGameManager.CarSelectionPlate.UseShadow(Matrix.CreateScale(1.5f));
								//testPile.UseShadow(Matrix.CreateScale(4));
								RacingGameManager.CarModel.UseShadow(Matrix.CreateRotationZ(0.85f));
							});
					} // if

					RacingGameManager.ClearBackground();
					BaseGame.UI.RenderGameBackground();

					RacingGameManager.CarSelectionPlate.Render(Matrix.CreateScale(1.5f));
					RacingGameManager.CarModel.RenderCar(
						carNumber, Color.White, false,
						Matrix.CreateRotationZ(0.85f));

					// And flush render manager to draw all objects
					BaseGame.MeshRenderManager.Render();
					
					if (Input.Keyboard.IsKeyUp(Keys.LeftAlt) &&
						Input.GamePadXPressed == false)
					{
						ShaderEffect.shadowMapping.ShowShadows();
					} // if

					if (Input.KeyboardSpaceJustPressed ||
						Input.GamePadBJustPressed)
						carNumber = (carNumber + 1) % 3;

					if (Input.Keyboard.IsKeyDown(Keys.LeftShift) ||
						Input.GamePadAPressed)
					{
						//if (Input.KeyboardRightPressed)// == false)
						ShaderEffect.shadowMapping.shadowMapTexture.RenderOnScreen(
							new Rectangle(10, 10, 256, 256));
						//if (Input.KeyboardLeftPressed)// == false)
						ShaderEffect.shadowMapping.shadowMapBlur.SceneMapTexture.
							RenderOnScreen(
							new Rectangle(10 + 256 + 10, 10, 256, 256));
						if (ShaderEffect.shadowMapping.shadowMapBlur.BlurMapTexture.IsValid)
							ShaderEffect.shadowMapping.shadowMapBlur.BlurMapTexture.
								RenderOnScreen(
								new Rectangle(10 + 256 + 10+256+10, 10, 256, 256));
					} // if (Input.Keyboard.IsKeyDown)

					//tst:
					BaseGame.UI.PostScreenGlowShader.Show();

					int xPos = 2, yPos = 30;
#if XBOX360
					xPos += BaseGame.XToRes(26);
					yPos += BaseGame.YToRes(22);
#endif
					TextureFont.WriteText(xPos, yPos,
						"Press left Shift or A to show all shadow pass textures.");
					TextureFont.WriteText(xPos, yPos+30,
						"Press Space or B to toggles the car model.");
					TextureFont.WriteText(xPos, yPos+60,
						"Press Alt or X to skip shadow map rendering.");
				});
		} // TestShadowMapping()
#endif
		#endregion
	} // class ShadowMapShader
} // namespace Abi.Graphic.ShadowMapping
