#if DEBUG
// Project: RacingGame, File: ShaderTests.cs
// Namespace: RacingGame.Shaders, Class: ShaderTests
// Path: C:\code\RacingGame\Shaders, Author: Abi
// Code lines: 95, Size of file: 2,42 KB
// Creation date: 20.10.2006 18:14
// Last modified: 20.10.2006 18:20
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.Graphics;
#endregion

namespace RacingGame.Shaders
{
	/// <summary>
	/// Shader tests. Extra class to make sure we can call shaders
	/// here without instantiating ShaderEffect first because
	/// the graphic engine is not initialized yet.
	/// </summary>
	class ShaderTests
	{
		#region Unit Testing
#if DEBUG
		#region Test simple diffuse shader
		/// <summary>
		/// Test simple diffuse shader
		/// </summary>
		public static void TestSimpleDiffuseShader()
		{
			PlaneRenderer testPlane = null;

			TestGame.Start("TestSimpleDiffuseShader",
				delegate
				{
					testPlane = new PlaneRenderer(
						Vector3.Zero, new Plane(new Vector3(0, 0, 1), 0),
						new Material("CityGround", "CityGroundNormal"), 25.0f);
				},
				delegate
				{
					testPlane.Render(ShaderEffect.simple, "Diffuse20");
				});
		} // TestSimpleDiffuseShader()
		#endregion

		#region Test simple SpecularPerPixel shader
		/// <summary>
		/// Test simple SpecularPerPixel shader
		/// </summary>
		public static void TestSimpleSpecularPerPixelShader()
		{
			PlaneRenderer testPlane = null;

			TestGame.Start("TestSimpleSpecularPerPixelShader",
				delegate
				{
					testPlane = new PlaneRenderer(
						Vector3.Zero, new Plane(new Vector3(0, 0, 1), 0),
						new Material("CityGround", "CityGroundNormal"), 25.0f);
				},
				delegate
				{
					testPlane.Render(ShaderEffect.simple, "SpecularPerPixel20");
				});
		} // TestSimpleSpecularPerPixelShader()
		#endregion

		#region Test NormalMapping shader
		/// <summary>
		/// Test NormalMapping shader
		/// </summary>
		public static void TestNormalMappingShader()
		{
			PlaneRenderer testPlane = null;

			TestGame.Start("TestNormalMappingShader",
				delegate
				{
					testPlane = new PlaneRenderer(
						Vector3.Zero, new Plane(new Vector3(0, 0, 1), 0),
						new Material("CityGround", "CityGroundNormal"), 25.0f);
				},
				delegate
				{
					testPlane.Render(ShaderEffect.normalMapping, "DiffuseSpecular20");
				});
		} // TestNormalMappingShader()
		#endregion
#endif
		#endregion
	} // class ShaderTests
} // namespace RacingGame.Shaders
#endif