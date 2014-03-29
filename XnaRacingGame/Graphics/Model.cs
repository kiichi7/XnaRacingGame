// Project: RacingGame, File: Model.cs
// Namespace: RacingGame.Graphics, Class: Model
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 1307, Size of file: 43,46 KB
// Creation date: 07.09.2006 05:56
// Last modified: 05.11.2006 00:29
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Tracks;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// Model class for loading and displaying x files including all materials
	/// and textures. Provides load and render functions to render static
	/// non animated models (mostly 1 mesh), for animated models we need a more
	/// advanced class, which is not required for this game yet.
	/// </summary>
	public class Model : IDisposable
	{
		#region Variables
		/// <summary>
		/// Name of this model, also used to load it from the content system.
		/// </summary>
		string name = "";

		/// <summary>
		/// Underlying xna model object. Loaded with the content system.
		/// </summary>
		XnaModel xnaModel = null;

		/*not longer required
		/// <summary>
		/// Scaling factor from 3ds max to our engine (1 unit = 1 meter)
		/// </summary>
		const float MaxModelScaling = 1.0f;
		 */

		/// <summary>
		/// Default object matrix to fix models from 3ds max to our engine!
		/// </summary>
		static readonly Matrix objectMatrix =
			//right handed models: Matrix.CreateRotationX(MathHelper.Pi);// *
			//Matrix.CreateScale(MaxModelScaling);
			// left handed models (else everything is mirrored with x files)
			Matrix.CreateRotationX(MathHelper.Pi / 2.0f);

		/// <summary>
		/// Transform matrices for this model, used in all Render methods,
		/// build once in the constructor, it never changes and none of our
		/// models are animated.
		/// </summary>
		Matrix[] transforms = null;
		/// <summary>
		/// Scaling for this object, used for distance comparisons.
		/// </summary>
		float realScaling = 1.0f, scaling = 1.0f;

		/// <summary>
		/// Does this model has alpha textures? Then render with alpha blending
		/// turned on. This is usually false and rendering is faster without
		/// alpha blending. Also used to skip shadow receiving, which looks
		/// strange on palms.
		/// </summary>
		bool hasAlpha = false;

		/// <summary>
		/// Is this the car model? Set in constructor and used in the render
		/// methods, this way we can compare much faster when rendering!
		/// </summary>
		bool isCar = false;

		/// <summary>
		/// If we want to animated some mesh in the model, just set the
		/// modelmesh here. Used for the windmill, which is rotated in
		/// Render!
		/// </summary>
		ModelMesh animatedMesh = null;

		/// <summary>
		/// Cached effect parameters to improve performance.
		/// For around 100 000 objects we save 1 second per effect parameters
		/// call. We only save the world matrix, worldViewProj matrix,
		/// viewInverse matrix and the lightDir vector effect parameters.
		/// Update: We also save diffuseTexture, ambientColor and diffuseColor now
		/// </summary>
		List<EffectParameter> cachedEffectParameters =
			new List<EffectParameter>();

		/// <summary>
		/// Another helper to check if the effect technique is
		/// "ReflectionSpecular". Checking this each frame takes a lot of time,
		/// this helper does the check only once in the constuctor.
		/// Used in RenderCar!
		/// </summary>
		List<bool> cachedIsReflectionSpecularTechnique =
			new List<bool>();

		/// <summary>
		/// Renderable meshes dictionary. Used to render every RenderableMesh
		/// in our render method.
		/// </summary>
		Dictionary<ModelMeshPart, MeshRenderManager.RenderableMesh>
			renderableMeshes =
			new Dictionary<ModelMeshPart, MeshRenderManager.RenderableMesh>();
		#endregion

		#region Properties
		/// <summary>
		/// Name for this model, this is the content name.
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return name;
			} // get
		} // Name

		/// <summary>
		/// Size
		/// </summary>
		/// <returns>Float</returns>
		public float Size
		{
			get
			{
				return realScaling;
			} // get
		} // Size

		/// <summary>
		/// Number of mesh parts
		/// </summary>
		/// <returns>Int</returns>
		public int NumOfMeshParts
		{
			get
			{
				int ret = 0;
				for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
					ret += xnaModel.Meshes[meshNum].MeshParts.Count;
				return ret;
			} // get
		} // NumOfMeshParts
		#endregion

		#region Constructor
		/// <summary>
		/// Create model
		/// </summary>
		/// <param name="setModelName">Set model name</param>
		public Model(string setModelName)
		{
			name = setModelName;

			xnaModel = BaseGame.Content.Load<XnaModel>(
				@"Content\" + name);

			// Get matrix transformations of the model
			// Has to be done only once because we don't use animations in our game.
			if (xnaModel != null)
			{
				transforms = new Matrix[xnaModel.Bones.Count];
				xnaModel.CopyAbsoluteBoneTransformsTo(transforms);

				// Calculate scaling for this object, used for distance comparisons.
				if (xnaModel.Meshes.Count > 0)
					realScaling = scaling =
						xnaModel.Meshes[0].BoundingSphere.Radius *
						transforms[0].Right.Length();

				// For palms, laterns, holders and column holders reduce scaling
				// to reduce the number of objects we have to render.
				if (StringHelper.Compare(name,
					new string[] { "AlphaPalm", "AlphaPalm2", "AlphaPalm3",
						//don't, looks better with more: "GuardRailHolder",
						"RoadColumnSegment" }))
					scaling *= 0.75f;//0.66f;

				// Hotels and windmills should always be visible (they are big)
				if (StringHelper.Compare(name,
					new string[] { "Hotel01", "Hotel02", "Casino01", "Windmill" }))
					scaling *= 5.0f;
				else
					// Don't use more than 3m for scaling and checking smaller objects
					if (scaling > 3)
						scaling = 3;
			} // if (xnaModel)

			hasAlpha = name.StartsWith("Alpha");
			// Is this a sign or banner? Then make sure ambient is pretty high!
			bool isSign = name.StartsWith("Sign") ||
				name.StartsWith("Banner") ||
				// Also include windmills, they are too dark
				name.StartsWith("Windmill");
				//name.StartsWith("StartLight");

			isCar = name == "Car";
			
			// Go through all meshes in the model
			for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
			{
				ModelMesh mesh = xnaModel.Meshes[meshNum];
				int meshPartNum = 0;
				string meshName = mesh.Name;

				// Remember this mesh for animations done in Render!
				if (name == "Windmill" &&
					meshName.StartsWith("Windmill_Wings"))
					animatedMesh = mesh;

				// And for each effect this mesh uses (usually just 1, multimaterials
				// are nice in 3ds max, but not efficiently for rendering stuff).
				for (int effectNum = 0; effectNum < mesh.Effects.Count; effectNum++)
				{
					Effect effect = mesh.Effects[effectNum];
					// Store our 4 effect parameters
					cachedEffectParameters.Add(effect.Parameters["diffuseTexture"]);
					cachedEffectParameters.Add(effect.Parameters["ambientColor"]);
					cachedEffectParameters.Add(effect.Parameters["diffuseColor"]);
					cachedEffectParameters.Add(effect.Parameters["world"]);
					cachedEffectParameters.Add(effect.Parameters["viewProj"]);
					cachedEffectParameters.Add(effect.Parameters["viewInverse"]);
					cachedEffectParameters.Add(effect.Parameters["lightDir"]);

					// Store if this is a "ReflectionSpecular" technique
					cachedIsReflectionSpecularTechnique.Add(
						effect.CurrentTechnique.Name.Contains("ReflectionSpecular"));

					// Increase ambient value to 0.5, 0.5, 0.5 for signs and banners!
					if (isSign)
						effect.Parameters["ambientColor"].SetValue(
							new Color(128, 128, 128).ToVector4());

					// Get technique from meshName
					int techniqueIndex = -1;
					if (meshName.Length > meshPartNum)
					{
						string techniqueNumberString = meshName.Substring(
							meshName.Length-(1+meshPartNum), 1);
#if !XBOX360
						// Faster and does not throw an exception!
						int.TryParse(techniqueNumberString, out techniqueIndex);
#else
						try
						{
							techniqueIndex = Convert.ToInt32(techniqueNumberString);
						} // try
						catch { } // ignore if that failed
#endif
					} // if (meshName.Length)
					
					// No technique found or invalid?
					if (techniqueIndex < 0 ||
						techniqueIndex >= effect.Techniques.Count)
					{
						// Try to use last technique
						techniqueIndex = effect.Techniques.Count-1;
						// If this is NormalMapping, use DiffuseSpecular20 instead
						// of the last technique (which is SpecularWithReflection20)
						if (effect.Techniques[techniqueIndex].Name.Contains(
							"SpecularWithReflection"))
							techniqueIndex -= 2;
						// Update: We have now 2 more techniques (ReflectionSpecular)
						if (effect.Techniques[techniqueIndex].Name.Contains(
							"ReflectionSpecular"))
							techniqueIndex -= 4;
					} // if (techniqueIndex)

					// If the technique ends with 20, but we can't do ps20,
					// use the technique before that (which doesn't use ps20)
					if (BaseGame.CanUsePS20 == false &&
						effect.Techniques[techniqueIndex].Name.EndsWith("20"))
						techniqueIndex--;
			
					// Set current technique for rendering below
					effect.CurrentTechnique = effect.Techniques[techniqueIndex];

					// Next mesh part
					meshPartNum++;
				} // foreach (effect)

				// Add all mesh parts!
				for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
				{
					ModelMeshPart part = mesh.MeshParts[partNum];
					// The model mesh part is not really used, we just extract the
					// index and vertex buffers and all the render data.
					// Material settings are build from the effect settings.
					// Also add this to our own dictionary for rendering.
					renderableMeshes.Add(part, BaseGame.MeshRenderManager.Add(
						mesh.VertexBuffer, mesh.IndexBuffer, part, part.Effect));
				} // for (partNum)
			} // foreach (mesh)

#if DEBUG
			// Check if there are no meshes to render
			if (xnaModel.Meshes.Count == 0)
				throw new ArgumentException("Invalid model "+name+
					". It does not contain any meshes");
#endif
		} // Model(setModelName)
		#endregion

		#region Dispose
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
				// Just set everything to null so we stop using this!
				name = "";
				xnaModel = null;
				transforms = null;
				animatedMesh = null;
			} // if
		} // Dispose(disposing)
		#endregion

		#region Render
		/// <summary>
		/// Default view distance optimizer is at 250m, then skip stuff.
		/// This will be reduced as our framerate runs low to improve performance
		/// on low end systems.
		/// </summary>
		static int maxViewDistance = 200;//250;

		/// <summary>
		/// Maximum view distance
		/// </summary>
		/// <returns>Int</returns>
		public static int MaxViewDistance
		{
			get
			{
				return maxViewDistance;
			} // get
			set
			{
				// Only set if we reduce, don't increase again if it is running
				// a little faster for a short time.
				if (value < maxViewDistance)
					maxViewDistance = value;
			} // set
		} // MaxViewDistance

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void Render(Matrix renderMatrix)
		{
			// Optimization to skip smaller objects, which are very far away!
			// Display 1 meter big objects only if in a distance of 250 meters!
			// Scaling is guessed by the length of the first vector in our matrix,
			// because we always use the same scaling for x, y, z this should be
			// correct!
			float maxDistance = maxViewDistance * scaling;
			float distanceSquared = Vector3.DistanceSquared(
				BaseGame.CameraPos, renderMatrix.Translation);
			if (distanceSquared >	maxDistance * maxDistance)
				// Don't render, too far away!
				return;

			// Check out if object is behind us or not visible, then we can skip
			// rendering. This is the GREATEST performance gain in the whole game!
			// Object must be away at least 20 units!
			if (distanceSquared > 20 * 20 &&
				// And the object size must be small
				distanceSquared > (10 * scaling) * (10 * scaling))
			{
				Vector3 objectDirection =
					Vector3.Normalize(BaseGame.CameraPos - renderMatrix.Translation);

				// Half field of view should be fov / 2, but because of
				// the aspect ratio (1.33) and an additional offset we need
				// to include to see objects at the borders.
				float objAngle = Vector3Helper.GetAngleBetweenVectors(
					BaseGame.CameraRotation, objectDirection);
				if (objAngle > BaseGame.ViewableFieldOfView)
					// Skip.
					return;
			} // if (distanceSquared)

			// Multiply object matrix by render matrix, result is used multiple
			// times here.
			renderMatrix = objectMatrix * renderMatrix;
			
			// Go through all meshes in the model
			for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
			{
				ModelMesh mesh = xnaModel.Meshes[meshNum];

				// Assign world matrix
				Matrix worldMatrix =
					transforms[mesh.ParentBone.Index] *
					renderMatrix;

				// Got animation?
				if (animatedMesh == mesh)
				{
					worldMatrix =
						Matrix.CreateRotationZ(
						// Use pseudo number for this object for different rotations
						renderMatrix.Translation.Length() * 3 +
						renderMatrix.Determinant() * 5 +
						(1.0f+((int)(renderMatrix.M42 * 33.3f)%100)*0.00123f)*
						BaseGame.TotalTime / 0.654f) *
						transforms[mesh.ParentBone.Index] *
						renderMatrix;
				} // if (animatedMesh)

				// Just add this world matrix to our render matrices for each part.
				for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
				{
					// Find mesh part in the renderableMeshes dictionary and add the
					// new render matrix to be picked up in the mesh rendering later.
					renderableMeshes[mesh.MeshParts[partNum]].renderMatrices.Add(
						worldMatrix);
				} // for (partNum)
			} // foreach (mesh)
		} // Render(renderMatrix)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderPos">Render position</param>
		public void Render(Vector3 renderPos)
		{
			Render(Matrix.CreateTranslation(renderPos));
		} // Render(renderPos)
		#endregion

		#region Render car
		/// <summary>
		/// Render car model with this seperate method because we
		/// render it in 2 steps, first the solid stuff, then the alpha glass.
		/// We also rotate the wheels around :)
		/// </summary>
		/// <param name="carNumber">Car type number (0, 1 or 2) for the car
		/// texture</param>
		/// <param name="carColor">Car color we are currently using.
		/// For car 0 and 1 this means we will also use another shader
		/// to change the color hue values for the whole car!</param>
		/// <param name="shadowCarMode">In the shadow car mode we render
		/// everything (including wheels and glass) with a special ShadowCar
		/// shader, that is very transparent. Used for the shadow car when
		/// playing that shows how we drove the last time.</param>
		/// <param name="renderMatrix">Render matrix for the car</param>
		public void RenderCar(int carNumber, Color carColor, bool shadowCarMode,
			Matrix renderMatrix)
		{
			// Multiply object matrix by render matrix, result is used multiple
			// times here.
			renderMatrix = objectMatrix * renderMatrix;

			// Do we just want to render the shadow car? Then do this in a
			// simpified way here instead of messing with the already complicated
			// code below.
			if (shadowCarMode)
			{
				// Start shadow car shader
				ShaderEffect simpleShader = ShaderEffect.simple;
				simpleShader.Render(
					"ShadowCar",
					delegate
					{
						int wheelNumber = 0;
						// And just render all meshes with it!
						for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
						{
							ModelMesh mesh = xnaModel.Meshes[meshNum];

							Matrix meshMatrix = transforms[mesh.ParentBone.Index];
							
							// Only the wheels have 2 mesh parts (gummi and chrome)
							if (mesh.MeshParts.Count == 2)
							{
								wheelNumber++;
								meshMatrix =
									Matrix.CreateRotationX(
									// Rotate left 2 wheels forward, the other 2 backward!
									(wheelNumber == 2 || wheelNumber == 4 ? 1 : -1) *
									RacingGameManager.Player.CarWheelPos) *
									meshMatrix;
							} // if (mesh.MeshParts.Count)
							
							// Assign world matrix
							BaseGame.WorldMatrix =
								meshMatrix *
								renderMatrix;

							// Set all matrices
							simpleShader.SetParameters();
							simpleShader.Update();

							// And render (must be done without mesh.Draw, which would just
							// use the original shaders for the model)
							for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
							{
								ModelMeshPart part = mesh.MeshParts[partNum];
								// Make sure vertex declaration is correct
								BaseGame.Device.VertexDeclaration = part.VertexDeclaration;
								// Set vertex buffer and index buffer
								BaseGame.Device.Vertices[0].SetSource(
									mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
								BaseGame.Device.Indices = mesh.IndexBuffer;

								// And render all primitives
								BaseGame.Device.DrawIndexedPrimitives(
									PrimitiveType.TriangleList,
									part.BaseVertex, 0, part.NumVertices,
									part.StartIndex, part.PrimitiveCount);
							} // for
						} // for
					});

				// And get outta here
				return;
			} // if

			// Usually use default color values
			Color ambientColor = Material.DefaultAmbientColor;
			Color diffuseColor = Material.DefaultDiffuseColor;

			// But for car 3 (carNumber == 2) use simple recoloring.
			if (carNumber == 2 ||
				// If we can't do PS 2.0, still use the normal recoloring techinque!
				BaseGame.CanUsePS20 == false)
			{
				ambientColor =
					ColorHelper.MultiplyColors(
					//Material.DefaultAmbientColor,
					// Use gray color instead of dark ambient because shadows
					// will darken down the non lighted side of our car anyway!
					new Color(80, 80, 80),
					carColor);
				diffuseColor =
					// Darken down a little
					ColorHelper.MultiplyColors(
					new Color(200, 200, 200),
					carColor);
			} // if
			else
			{
				// Select the VertexOutput_SpecularWithReflectionForCar20 instead.
				// Done below in the render loop (non alpha stuff).
			} // else

			EffectTechnique remCurrentTechnique = null;
			for (int alphaPass = 0; alphaPass < 2; alphaPass++)
			{
				int wheelNumber = 0;
				
				int effectParameterIndex = 0;
				int effectTechniqueIndex = 0;

				for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
				{
					ModelMesh mesh = xnaModel.Meshes[meshNum];
					bool dontRender = false;

					for (int effectNum = 0; effectNum < mesh.Effects.Count; effectNum++)
					{
						Effect effect = mesh.Effects[effectNum];
						if (effectNum == 0)
							remCurrentTechnique = effect.CurrentTechnique;

						// Find out if this is ReflectionSimpleGlass.fx,
						// NormalMapping.fx will also use reflection, but the techniques
						// are named in another way (SpecularWithReflection, etc.)
						if (cachedIsReflectionSpecularTechnique[effectTechniqueIndex++])
						{
							if (alphaPass == 0)
							{
								dontRender = true;
								effectParameterIndex += 7;
								break;
							} // if (alphaPass)

							// Skip the first 3 effect parameters
							effectParameterIndex += 3;
						} // if (effect.CurrentTechnique.Name.Contains)
						else
						{
							if (alphaPass == 1)
							{
								dontRender = true;
								effectParameterIndex += 7;
								break;
							} // if (alphaPass)

							// To improve performance we only have to set this when it changes!
							// Doesn't do much, because this eats only 10% performance,
							// 5-10% are the matrices below and most of the performance is
							// just rendering the car with Draw!

							// Overwrite car diffuse textures depending on the car number
							// we want to render.
							cachedEffectParameters[effectParameterIndex++].SetValue(
								RacingGameManager.CarTexture(carNumber).XnaTexture);

							// Also set color
							cachedEffectParameters[effectParameterIndex++].SetValue(
								ambientColor.ToVector4());
							cachedEffectParameters[effectParameterIndex++].SetValue(
								diffuseColor.ToVector4());

							// Change shader to VertexOutput_SpecularWithReflectionForCar20
							// if we changed the color.
							if (carNumber < 2 &&
								// Only PS 2.0 is supported
								BaseGame.CanUsePS20 &&
								// And we only have to do this if the color was changed
								RacingGameManager.currentCarColor != 0 &&
								effectNum == 0)
							{
								effect.CurrentTechnique =
									effect.Techniques["SpecularWithReflectionForCar20"];
								// And set carHueColorChange
								effect.Parameters["carHueColorChange"].SetValue(
									// Special case for color 1, make more red than pink
									RacingGameManager.currentCarColor == 1 ? 0.06f :
									RacingGameManager.currentCarColor * 0.1f);
							} // if
						} // else

						Matrix meshMatrix = transforms[mesh.ParentBone.Index];

						// Only the wheels have 2 mesh parts (gummi and chrome)
						if (mesh.MeshParts.Count == 2)
						{
							wheelNumber++;
							meshMatrix =
								Matrix.CreateRotationX(
								// Rotate left 2 wheels forward, the other 2 backward!
								(wheelNumber == 2 || wheelNumber == 4 ? 1 : -1) *
								RacingGameManager.Player.CarWheelPos) *
								meshMatrix;
						} // if (mesh.MeshParts.Count)

						// Assign world matrix
						BaseGame.WorldMatrix =
							meshMatrix *
							renderMatrix;

						// Set matrices
						//effect.Parameters["world"].SetValue(
						cachedEffectParameters[effectParameterIndex++].SetValue(
							BaseGame.WorldMatrix);

						// These values should only be set once every frame (see above)!
						// to improve performance again, also we should access them
						// with EffectParameter and not via name!
						// But since we got only 1 car it doesn't matter so much ..
						cachedEffectParameters[effectParameterIndex++].SetValue(
							BaseGame.ViewProjectionMatrix);
						cachedEffectParameters[effectParameterIndex++].SetValue(
							BaseGame.InverseViewMatrix);
						// Set light direction
						cachedEffectParameters[effectParameterIndex++].SetValue(
							BaseGame.LightDirection);
					} // foreach (effect)

					// Render
					if (dontRender == false)
						mesh.Draw();

					// Change shader back to default render technique.
					if (carNumber < 2 &&
						// Only PS 2.0 is supported
						BaseGame.CanUsePS20 &&
						// And we only have to do this if the color was changed
						RacingGameManager.currentCarColor != 0 &&
						remCurrentTechnique != null)
					{
						 mesh.Effects[0].CurrentTechnique = remCurrentTechnique;
					} // if
				} // foreach (mesh)
			} // for (alphaPass)
		} // RenderCar(renderMatrix)
		#endregion

		#region Generate shadow
		/// <summary>
		/// Generate shadow for this model in the generate shadow pass
		/// of our shadow mapping shader. All objects rendered here will
		/// cast shadows to our scene (if they are in range of the light)
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void GenerateShadow(Matrix renderMatrix)
		{
			// Find out how far the object is away from the shadow,
			// we can ignore it if it is outside of the shadow generation range.
			// Everything smaller than 0.5 meter can be ignored. 
			float maxDistance =
				//nice, but not good for shadow mapping, have to use fixed value!
				//0.5f * ShaderEffect.shadowMapping.ShadowDistance * scaling;
				//1.25f * ShaderEffect.shadowMapping.ShadowDistance;
				scaling/2.5f + 1.015f * ShaderEffect.shadowMapping.ShadowDistance;
			if (Vector3.DistanceSquared(
				ShaderEffect.shadowMapping.ShadowLightPos, renderMatrix.Translation) >
				maxDistance * maxDistance)
				// Don't render, too far away!
				return;

			// Multiply object matrix by render matrix.
			renderMatrix = objectMatrix * renderMatrix;
			
			int wheelNumber = 0;
			for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
			{
				ModelMesh mesh = xnaModel.Meshes[meshNum];
				// Use the ShadowMapShader helper method to set the world matrices
				ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
					transforms[mesh.ParentBone.Index] *
					renderMatrix);

				// Got animation?
				if (animatedMesh == mesh)
				{
					ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
						Matrix.CreateRotationZ(
						// Use pseudo number for this object for different rotations
						renderMatrix.Translation.Length() * 3 +
						renderMatrix.Determinant() * 5 +
						(1.0f+((int)(renderMatrix.M42 * 33.3f)%100)*0.00123f)*
						BaseGame.TotalTime / 0.654f) *
						transforms[mesh.ParentBone.Index] *
						renderMatrix);
				} // if (animatedMesh)

				// Skip glass for car model
				if (isCar)
				{
					if (mesh.Name == "glass")
						continue;
					
					// Only the wheels have 2 mesh parts (gummi and chrome)
					if (mesh.MeshParts.Count == 2)
					{
						//ignore
						//continue;
						wheelNumber++;
						ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
							Matrix.CreateRotationX(
							// Rotate left 2 wheels forward, the other 2 backward!
							(wheelNumber == 2 || wheelNumber == 4 ? 1 : -1) *
							RacingGameManager.Player.CarWheelPos) *
							transforms[mesh.ParentBone.Index] *
							renderMatrix);
					} // if (mesh.MeshParts.Count)
				} // if (isCar)

				for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
				{
					ModelMeshPart part = mesh.MeshParts[partNum];
					// Render just the vertices, do not use the shaders of our model.
					// This is the same code as ModelMeshPart.Draw() uses, but
					// this method is internal and can't be used by us :(
					BaseGame.Device.VertexDeclaration = part.VertexDeclaration;
					BaseGame.Device.Vertices[0].SetSource(
						mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
					BaseGame.Device.Indices = mesh.IndexBuffer;
					BaseGame.Device.DrawIndexedPrimitives(
						PrimitiveType.TriangleList,
						part.BaseVertex, 0,
						part.NumVertices, part.StartIndex, part.PrimitiveCount);
				} // foreach (part)
			} // foreach (mesh)
		} // GenerateShadow(renderMatrix)
		#endregion

		#region Use shadow
		/// <summary>
		/// Use shadow for our scene. We render all objects that should receive
		/// shadows here. Called from the ShadowMappingShader.UseShadow method.
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void UseShadow(Matrix renderMatrix)
		{
			// If this is an object that uses alpha textures, never receive
			// shadows, this only causes troubes and needs a much more complex
			// shader. This affects usually only palms anyway, which look better
			// without shadowing on.
			if (hasAlpha)
				return;

			// Find out how far the object is away from the shadow,
			// we can ignore it if it is outside of the shadow generation range.
			// Everything smaller than 0.25 meter can be ignored.
			// Note: For receiving we usually use more objects than for generating
			// shadows.
			float maxDistance =
				//nice, but not good for shadow mapping, have to use fixed value!
				//0.25f * ShaderEffect.shadowMapping.ShadowDistance * scaling;
				1.015f * ShaderEffect.shadowMapping.ShadowDistance;
			if (Vector3.DistanceSquared(
				ShaderEffect.shadowMapping.ShadowLightPos, renderMatrix.Translation) >
				maxDistance * maxDistance)
				// Don't render, too far away!
				return;

			// Multiply object matrix by render matrix.
			renderMatrix = objectMatrix * renderMatrix;
			
			for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
			{
				ModelMesh mesh = xnaModel.Meshes[meshNum];
				// Use the ShadowMapShader helper method to set the world matrices
				ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
					transforms[mesh.ParentBone.Index] *
					renderMatrix);

				// Got animation?
				if (animatedMesh == mesh)
				{
					ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
						Matrix.CreateRotationZ(
						// Use pseudo number for this object for different rotations
						renderMatrix.Translation.Length() * 3 +
						renderMatrix.Determinant() * 5 +
						(1.0f+((int)(renderMatrix.M42 * 33.3f)%100)*0.00123f)*
						BaseGame.TotalTime / 0.654f) *
						transforms[mesh.ParentBone.Index] *
						renderMatrix);
				} // if (animatedMesh)

				// Skip glass for car model
				if (isCar)
				{
					if (mesh.Name == "glass")
						continue;
				} // if (isCar)

				for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
				{
					ModelMeshPart part = mesh.MeshParts[partNum];
					// Render just the vertices, do not use the shaders of our model.
					// This is the same code as ModelMeshPart.Draw() uses, but
					// this method is internal and can't be used by us :(
					BaseGame.Device.VertexDeclaration = part.VertexDeclaration;
					BaseGame.Device.Vertices[0].SetSource(
						mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
					BaseGame.Device.Indices = mesh.IndexBuffer;
					BaseGame.Device.DrawIndexedPrimitives(
						PrimitiveType.TriangleList,
						part.BaseVertex, 0,
						part.NumVertices, part.StartIndex, part.PrimitiveCount);
				} // foreach (part)
			} // foreach (mesh)
		} // UseShadow(renderMatrix)
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestSingleModel
		/// <summary>
		/// Test single model
		/// </summary>
		static public void TestSingleModel()
		{
			Model testModel = null;
			TestGame.Start("TestSingleModel",
				delegate
				{
					testModel = new Model("Hotel01");//"OilTanks");
				},
				delegate
				{
					//BaseGame.Device.RenderState.DepthBufferEnable = true;
					testModel.Render(Matrix.Identity);
				});
		} // TestSingleModel()
		#endregion

		#region TestCarModel
		/// <summary>
		/// Test car model
		/// </summary>
		static public void TestCarModel()
		{
			bool shadowCarMode = false;
			int carType = 0;
			Model testModel = null;

			TestGame.Start("TestCarModel",
				delegate
				{
					testModel = new Model("Car");
				},
				delegate
				{
					BaseGame.UI.RenderGameBackground();

					RacingGameManager.Player.SetCarWheelPos(
						BaseGame.TotalTime * 12.0f);
					testModel.RenderCar(carType,
						RacingGameManager.CarColor,
						shadowCarMode,
						Matrix.Identity);

					TextureFont.WriteText(20, 30,
						"Press Space to toggle shadow car mode: " +
						shadowCarMode);
					TextureFont.WriteText(20, 60,
						"Press Left/Right to toggle car type: " +
						carType);
					TextureFont.WriteText(20, 90,
						"Press Up/Down to toggle car color: " +
						RacingGameManager.currentCarColor);
					if (Input.KeyboardSpaceJustPressed ||
						Input.GamePadAJustPressed)
						shadowCarMode = !shadowCarMode;
					if (Input.KeyboardLeftJustPressed ||
						Input.GamePadLeftJustPressed)
						carType = (carType + 2) % RacingGameManager.NumberOfCarTextureTypes;
					if (Input.KeyboardRightJustPressed ||
						Input.GamePadRightJustPressed)
						carType = (carType + 1) % RacingGameManager.NumberOfCarTextureTypes;
					if (Input.KeyboardUpJustPressed ||
						Input.GamePadUpJustPressed)
						RacingGameManager.currentCarColor =
							(RacingGameManager.currentCarColor + 1) %
							RacingGameManager.NumberOfCarColors;
					if (Input.KeyboardDownJustPressed ||
						Input.GamePadDownJustPressed)
						RacingGameManager.currentCarColor =
							(RacingGameManager.currentCarColor +
							RacingGameManager.NumberOfCarColors - 1) %
							RacingGameManager.NumberOfCarColors;
				});
		} // TestCarModel()
		#endregion

		#region TestWindmillAnimation
		/// <summary>
		/// Test windmill animation, we do a custom animation since XNA
		/// does not support animated files yet.
		/// </summary>
		static public void TestWindmillAnimation()
		{
			Model testModel = null;
			TestGame.Start("TestWindmillAnimation",
				delegate
				{
					testModel = new Model("Windmill");
				},
				delegate
				{
					BaseGame.UI.RenderGameBackground();

					BaseGame.Device.RenderState.DepthBufferEnable = true;
					testModel.Render(Matrix.Identity);
					testModel.Render(new Vector3(40, 15, 0));

					BaseGame.UI.PostScreenGlowShader.Show();
				});
		} // TestWindmillAnimation()
		#endregion

		#region TestRenderModel
		/// <summary>
		/// Test render model
		/// </summary>
		static public void TestRenderModel()
		{
			List<Model> testModels = new List<Model>();
			int modelNum = 0;

			TestGame.Start(
				delegate
				{
					testModels.Add(new Model("Blockade"));
					testModels.Add(new Model("Blockade2"));
					testModels.Add(new Model("Hydrant"));
					testModels.Add(new Model("Hotel01"));
					testModels.Add(new Model("Hotel02"));
					testModels.Add(new Model("Casino01"));
					testModels.Add(new Model("Kaktus"));
					testModels.Add(new Model("Kaktus2"));
					testModels.Add(new Model("KaktusBenny"));
					testModels.Add(new Model("KaktusSeg"));
					testModels.Add(new Model("AlphaDeadTree"));
					testModels.Add(new Model("AlphaPalm"));
					testModels.Add(new Model("AlphaPalm2"));
					testModels.Add(new Model("AlphaPalm3"));
					testModels.Add(new Model("AlphaPalmSmall"));
					testModels.Add(new Model("Laterne"));
					testModels.Add(new Model("Laterne2Sides"));
					testModels.Add(new Model("Trashcan"));
					testModels.Add(new Model("Roadsign"));
					testModels.Add(new Model("Roadsign2"));
					testModels.Add(new Model("Goal"));
					testModels.Add(new Model("Building"));
					testModels.Add(new Model("Building2"));
					testModels.Add(new Model("Building3"));
					testModels.Add(new Model("Building4"));
					testModels.Add(new Model("Building5"));
					testModels.Add(new Model("OilPump"));
					testModels.Add(new Model("OilTanks"));
					testModels.Add(new Model("RoadColumnSegment"));
					testModels.Add(new Model("Windmill"));
					testModels.Add(new Model("Ruin"));
					testModels.Add(new Model("RuinHouse"));
					testModels.Add(new Model("SandCastle"));
					testModels.Add(new Model("Banner"));
					testModels.Add(new Model("Banner2"));
					testModels.Add(new Model("Banner3"));
					testModels.Add(new Model("Banner4"));
					testModels.Add(new Model("Banner5"));
					testModels.Add(new Model("Banner6"));
					testModels.Add(new Model("Sign"));
					testModels.Add(new Model("Sign2"));
					testModels.Add(new Model("SignWarning"));
					testModels.Add(new Model("SignCurveLeft"));
					testModels.Add(new Model("SignCurveRight"));
					testModels.Add(new Model("SharpRock"));
					testModels.Add(new Model("SharpRock2"));
					testModels.Add(new Model("Stone4"));
					testModels.Add(new Model("Stone5"));
					testModels.Add(new Model("AlphaTrain"));
					testModels.Add(new Model("GuardRailHolder"));
					testModels.Add(new Model("CarSelectionPlate"));
					testModels.Add(new Model("Car"));
				},
				delegate
				{
					TrackLine.ShowGroundGrid();
					if (testModels[modelNum].name == "Car")
						testModels[modelNum].RenderCar(
							0, Color.White, false, Matrix.Identity);
					else
						testModels[modelNum].Render(Vector3.Zero);
					//TextureFont.WriteText(100, 100, "Cam pos: " + BaseGame.CameraPos);
					TextureFont.WriteText(2, 30, "Model name: " +
						testModels[modelNum].name);
					TextureFont.WriteText(2, 60, "1 grid line = 1m");
					TextureFont.WriteText(2, 90,
						"Press Space or A to toggle to next model");

					if (Input.KeyboardSpaceJustPressed ||
						Input.GamePadAJustPressed)
						modelNum = (modelNum + 1) % testModels.Count;
				});
		} // TestRenderModel()
		#endregion

		#region TestManyModelsPerformance
		/// <summary>
		/// Test many models performance
		/// </summary>
		public static void TestManyModelsPerformance()
		{
			List<Model> testModels = new List<Model>();
			const int GridWidth = 86,
				GridHeight = 86;
			int[,] palmTypeGrids = new int[GridWidth, GridHeight];

			TestGame.Start("TestManyModelsPerformance",
				delegate
				{
					/*tst1
					testModels.Add(new Model("AlphaPalm"));
					testModels.Add(new Model("AlphaPalm2"));
					testModels.Add(new Model("AlphaPalm3"));
					testModels.Add(new Model("AlphaPalmSmall"));
					 */
					//testModels.Add(new Model("RuinHouse"));
					testModels.Add(new Model("AlphaPalm"));

					for (int x = 0; x < GridWidth; x++)
						for (int y = 0; y < GridHeight; y++)
						{
							palmTypeGrids[x, y] =
								RandomHelper.GetRandomInt(testModels.Count);
						} // for for (int)
					RacingGameManager.Player.SetCarPosition(Vector3.Zero,
						new Vector3(0, 1, 0), new Vector3(0, 0, 1));

					/*tst
					// Render all models
					for (int x = 0; x < GridWidth; x++)
						for (int y = 0; y < GridHeight; y++)
						{
							testModels[palmTypeGrids[x,y]].Render(
								Matrix.CreateTranslation(new Vector3(
								-5*GridWidth/2+5*x,
								-5*GridHeight/2+5*y, 0)));
						} // for for (int)
					 */
				},
				delegate
				{
					BaseGame.UI.RenderGameBackground();

					//*tst
					// Render all models
					for (int x = 0; x < GridWidth; x++)
						for (int y = 0; y < GridHeight; y++)
						{
							testModels[palmTypeGrids[x,y]].Render(
								Matrix.CreateTranslation(new Vector3(
								-5*GridWidth/2+5*x,
								-5*GridHeight/2+5*y, 0)));
						} // for for (int)
					//*/

					TextureFont.WriteText(2, 30,
						"Rendering "+(GridWidth*GridHeight)+" models with "+
						testModels[0].NumOfMeshParts*(GridWidth*GridHeight)+
						" meshes.");

					//unused: BaseGame.UI.PostScreenGlowShader.Show();
				});
		} // TestManyModelsPerformance()
		#endregion
#endif
		#endregion
	} // class Model
} // namespace RacingGame.Graphics
