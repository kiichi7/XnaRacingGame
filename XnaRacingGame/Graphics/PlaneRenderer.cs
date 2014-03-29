// Project: RacingGame, File: PlaneRenderer.cs
// Namespace: RacingGame.Graphics, Class: PlaneRenderer
// Path: C:\code\RacingGame\Graphics, Author: Abi
// Code lines: 181, Size of file: 5,24 KB
// Creation date: 15.10.2006 13:17
// Last modified: 20.10.2006 18:19
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Shaders;
using RacingGame.Tracks;
#endregion

namespace RacingGame.Graphics
{
	/// <summary>
	/// Helper class to render a simple plane, used in some unit tests,
	/// especially for testing the physics engine.
	/// </summary>
	class PlaneRenderer
	{
		#region Variables
		Vector3 pos;
		Plane plane;
		Material material;
		float size;
		const float Tiling = 20.0f;//10.0f;
		#endregion

		#region Constructor
		/// <summary>
		/// Create plane renderer
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setPlane">Set plane</param>
		/// <param name="setMaterial">Set material</param>
		/// <param name="setSize">Set size</param>
		public PlaneRenderer(Vector3 setPos,
			Plane setPlane, Material setMaterial, float setSize)
		{
			pos = setPos;
			plane = setPlane;
			material = setMaterial;
			size = setSize;
		} // PlaneRenderer(setPos, setPlane, setMaterial)
		#endregion

		#region Render
		/// <summary>
		/// Draw plane vertices
		/// </summary>
		private void DrawPlaneVertices()
		{
			// Calculate right and dir vectors for constructing the plane.
			// The following code might look strange, but we have to make sure
			// that we always get correct up, right and dir vectors. Cross products
			// can return (0, 0, 0) if the vectors are parallel!
			Vector3 up = plane.Normal;
			if (up.Length() == 0)
				up = new Vector3(0, 0, 1);
			Vector3 helperVec = Vector3.Cross(up, new Vector3(1, 0, 0));
			if (helperVec.Length() == 0)
				helperVec = new Vector3(0, 1, 0);
			Vector3 right = Vector3.Cross(helperVec, up);
			Vector3 dir = Vector3.Cross(up, right);
			float dist = plane.D;

			TangentVertex[] vertices = new TangentVertex[]
			{
				// Make plane VERY big and tile texture every 10 meters
				new TangentVertex(
					(-right-dir)*size+up*dist, -size/Tiling, -size/Tiling, up, right),
				new TangentVertex(
					(-right+dir)*size+up*dist, -size/Tiling, +size/Tiling, up, right),
				new TangentVertex(
					(right-dir)*size+up*dist, +size/Tiling, -size/Tiling, up, right),
				new TangentVertex(
					(right+dir)*size+up*dist, +size/Tiling, +size/Tiling, up, right),
			};

			// Draw the plane (just 2 simple triangles)
			BaseGame.Device.DrawUserPrimitives(
				PrimitiveType.TriangleStrip, vertices, 0, 2);
		} // DrawPlaneVertices()

		/// <summary>
		/// Just renders the plane with the given material.
		/// </summary>
		public void Render()
		{
			BaseGame.WorldMatrix = Matrix.CreateTranslation(pos);
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			ShaderEffect.normalMapping.Render(
				material,
				"DiffuseSpecular20",
				new BaseGame.RenderHandler(DrawPlaneVertices));
			BaseGame.WorldMatrix = Matrix.Identity;
		} // Render()
		
#if DEBUG
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="shader">Shader</param>
		/// <param name="shaderTechnique">Shader technique</param>
		public void Render(ShaderEffect shader, string shaderTechnique)
		{
			BaseGame.WorldMatrix = Matrix.CreateTranslation(pos);
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			shader.Render(
				material,
				shaderTechnique,
				new BaseGame.RenderHandler(DrawPlaneVertices));
			BaseGame.WorldMatrix = Matrix.Identity;
		} // Render(shader, shaderTechnique)
#endif
		#endregion

#if DEBUG
		#region Use shadow
		/// <summary>
		/// Use shadow on the plane, useful for our unit tests. The plane does not
		/// throw shadows, so we don't need a GenerateShadow method.
		/// </summary>
		public void UseShadow()
		{
			ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
				Matrix.CreateTranslation(pos));
			DrawPlaneVertices();
		} // UseShadow()
		#endregion
#endif

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test rendering plane xy
		/// </summary>
		static public void TestRenderingPlaneXY()
		{
			PlaneRenderer plane = null;
			TestGame.Start(
				delegate
				{
					plane = new PlaneRenderer(
						Vector3.Zero,
						new Plane(new Vector3(0, 0, 1), 0),
						//try1: new Material("RoadCement", "RoadCementNormal"));
						new Material("CityGround", "CityGroundNormal"),
						500.0f);
				},
				delegate
				{
					//TrackLine.ShowGroundGrid();
					plane.Render();
				});
		} // TestRenderingPlaneXY()
		
		/// <summary>
		/// Test rendering plane xy
		/// </summary>
		static public void TestRenderingPlaneWithVector111()
		{
			PlaneRenderer plane = null;
			TestGame.Start(
				delegate
				{
					plane = new PlaneRenderer(
						Vector3.Zero,
						new Plane(new Vector3(1, 1, 1), 1),
						//try1: new Material("RoadCement", "RoadCementNormal"));
						new Material("CityGround", "CityGroundNormal"),
						500.0f);
				},
				delegate
				{
					TrackLine.ShowGroundGrid();
					plane.Render();
				});
		} // TestRenderingPlaneXY()
#endif
		#endregion
	} // class PlaneRenderer
} // namespace RacingGame.Graphics
