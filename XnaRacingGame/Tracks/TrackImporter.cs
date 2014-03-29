// This class can be completely removed from the game, it is only used
// to import the track and level data from 3ds max to our own binary format.
#if DEBUG

// Project: RacingGame, File: TrackImporter.cs
// Namespace: RacingGame.Tracks, Class: ColladaTrack
// Path: C:\code\RacingGame\Tracks, Author: Abi
// Code lines: 715, Size of file: 21,95 KB
// Creation date: 04.11.2006 05:14
// Last modified: 04.11.2006 23:45
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RacingGame.Helpers;
#endregion

namespace RacingGame.Tracks
{
	/// <summary>
	/// Track importer
	/// </summary>
	class TrackImporter
	{
		/// <summary>
		/// Collada loader base class. Used to imports Collada data from a
		/// 3d program like 3D Studio Max. We can import models (mesh data),
		/// splines for our track creation or the whole level data as exported
		/// by Does currently only support 1 mesh and uses
		/// TangentVertex for the vertices. Rendering is always done with
		/// the standard NormalMapping shader.
		/// </summary>
		class ColladaLoader
		{
			#region Constants
			/// <summary>
			/// Scaling factor for converting from Max to our World.
			/// We should use the same scaling, but if something changes,
			/// just adjust this factor.
			/// </summary>
			protected const float MaxScalingFactor = 1.0f;//0.5f;//1.0f;

			/// <summary>
			/// Default Extension for Collada files exported with 3ds max.
			/// </summary>
			protected const string Extension = "DAE";
			#endregion

			#region Variables
			/// <summary>
			/// Filename for this collada file.
			/// </summary>
			protected string filename = "";

			/// <summary>
			/// Collada file as a xml node for loading.
			/// </summary>
			protected XmlNode colladaFile = null;
			#endregion

			#region Properties
			/// <summary>
			/// Filename
			/// </summary>
			/// <returns>String</returns>
			public string Filename
			{
				get
				{
					return filename;
				} // get
			} // Filename
			#endregion

			#region Constructor
			/// <summary>
			/// Create and load collada mesh
			/// </summary>
			/// <param name="setFilename">Set filename</param>
			public ColladaLoader(string setFilename)
			{
				Load(setFilename);
			} // LoadColladaMesh(meshFilepath)

			/// <summary>
			/// Create collada loader, empty constructor for derived classes,
			/// use Load to do the loading!
			/// </summary>
			protected ColladaLoader()
			{
			} // ColladaLoader()

			#region Load
			/// <summary>
			/// Load
			/// </summary>
			/// <param name="setFilename">Set filename</param>
			protected virtual void Load(string setFilename)
			{
				//Log.Write("ColladaLoader");
				// Build filename and add extension if it is missing
				filename = setFilename;
				if (StringHelper.GetExtension(filename).Length == 0)
					filename += "." + Extension;

				if (File.Exists(filename) == false)
					throw new FileNotFoundException(
						"Collada file not found, unable to load Collada object: "+
						filename);
				
				// Load file
				colladaFile = XmlHelper.LoadXmlFromFile(filename);
			} // Load(setFilename)
			#endregion
			#endregion
		} // class ColladaLoader
		
		/// <summary>
		/// Little helper class to load combinations of objects, which simplify
		/// our level creation.
		/// </summary>
		/// <returns>Collada loader</returns>
		class ColladaCombiModels : ColladaLoader
		{
			#region Variables
			/// <summary>
			/// List of combi objects used in this file.
			/// </summary>
			private List<TrackCombiModels.CombiObject> objects =
				new List<TrackCombiModels.CombiObject>();

			/// <summary>
			/// Name of this combi model (extracted from filename).
			/// </summary>
			private string name = "";

			/// <summary>
			/// Size of this combi model.
			/// </summary>
			private float size = 10;
			#endregion

			#region Properties
			/// <summary>
			/// Objects
			/// </summary>
			/// <returns>List</returns>
			public List<TrackCombiModels.CombiObject> Objects
			{
				get
				{
					return objects;
				} // get
			} // Objects

			/// <summary>
			/// Name
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
			public float Size
			{
				get
				{
					// Return size 10 for palms, stones and ruins, rest gets size = 50
					return size;
				} // get
			} // Size
			#endregion

			#region Constructor
			/// <summary>
			/// Create collada combi models
			/// </summary>
			/// <param name="setFilename">Set filename</param>
			public ColladaCombiModels(string setFilename)
				: base("Tracks\\"+setFilename)
			{
				// Get referenced models we got in this file.
				XmlNode visualScene =
					XmlHelper.GetChildNode(colladaFile, "visual_scene");

				XmlNode sceneNode = XmlHelper.GetChildNode(visualScene, "node");
				while (sceneNode != null)
				{
					string nodeId = XmlHelper.GetXmlAttribute(sceneNode, "id");

					// Must be a node
					if (nodeId.EndsWith("-node") == false)
					{
						sceneNode = sceneNode.NextSibling;
						continue;
					} // if (nodeId.EndsWith)

					// Find all helpers
					// Ignore unnamed models, we can't do anything with them.
					if (nodeId.StartsWith("Line") == false &&
						nodeId.StartsWith("Point") == false &&
						nodeId.StartsWith("Dummy") == false &&
						nodeId.StartsWith("Landscape") == false)
					{
						//for XRef objects, but we use XRef scenes now, XRef objects are
						//just too buggy in max8:
						XmlNode translateNode = XmlHelper.GetChildNode(
							sceneNode, "translate");
						XmlNode scaleNode = XmlHelper.GetChildNode(
							sceneNode, "scale");
						XmlNode rotateNode = XmlHelper.GetChildNode(
							sceneNode, "rotate");
						// No translate node found? Then skip!
						if (translateNode != null)
						{
							// Find out name of model
							string[] idSplitted = nodeId.Split(new char[] { '_', '-' });
							string modelName = idSplitted[0];

							// Get translate values
							float[] translateValues = StringHelper.ConvertStringToFloatArray(
								translateNode.FirstChild.Value);
							Vector3 pos = MaxScalingFactor * new Vector3(
								translateValues[0], translateValues[1], translateValues[2]);

							// Also get scaling if possible
							float scale = 1.0f;
							if (scaleNode != null)
							{
								float[] scaleValues = StringHelper.ConvertStringToFloatArray(
									scaleNode.FirstChild.Value);
								scale = scaleValues[0];
							} // if (scaleNode)
							Vector3 rotationAxis = new Vector3(0, 0, 1);
							float rotation = 0.0f;
							if (rotateNode != null)
							{
								float[] rotateValues = StringHelper.ConvertStringToFloatArray(
									rotateNode.FirstChild.Value);
								rotationAxis = new Vector3(
									rotateValues[0], rotateValues[1], rotateValues[2]);
								rotation = rotateValues[3];
							} // if (rotateNode)

							/*tst
							Log.Write("modelName="+modelName+
								", pos="+pos+
								", scale="+scale+
								", rotationAxis="+rotationAxis+
								", rotation="+rotation);
							 */

							// Now add neutral object
							objects.Add(new TrackCombiModels.CombiObject(
								modelName,
								Matrix.CreateFromAxisAngle(rotationAxis,
								MathHelper.ToRadians(rotation)) *
								Matrix.CreateScale(scale * MaxScalingFactor) *
								Matrix.CreateTranslation(pos)));
						} // if (translateNode)
					} // else

					sceneNode = sceneNode.NextSibling;
				} // while (sceneNode)

				name = StringHelper.ExtractFilename(filename, true);

				// Return size 10 for palms, stones and ruins, rest gets size = 50
				size = StringHelper.Compare(Name,
					new string[] { "CombiPalms", "CombiPalms2", "CombiRuins",
						"CombiRuins2", "CombiStones", "CombiStones2" }) ?
						10 : 50;
			} // ColladaCombiModels(setFilename)
			#endregion
		} // class ColladaCombiModels

		/// <summary>
		/// Collada track
		/// </summary>
		class ColladaTrack : ColladaLoader
		{
			#region Variables
			/// <summary>
			/// Track points
			/// </summary>
			private List<Vector3> trackPoints = new List<Vector3>();

			/// <summary>
			/// Width helper position
			/// </summary>
			private List<TrackData.WidthHelper> widthHelpers =
				new List<TrackData.WidthHelper>();

			/// <summary>
			/// Tunnel helper position
			/// </summary>
			private List<TrackData.RoadHelper> roadHelpers =
				new List<TrackData.RoadHelper>();

			/// <summary>
			/// List of neutral objects used in this level
			/// </summary>
			private List<TrackData.NeutralObject> objects =
				new List<TrackData.NeutralObject>();
			#endregion

			#region Properties
			/// <summary>
			/// Track points
			/// </summary>
			/// <returns>List</returns>
			public List<Vector3> TrackPoints
			{
				get
				{
					return trackPoints;
				} // get
			} // TrackPoints

			/// <summary>
			/// Width helpers
			/// </summary>
			/// <returns>List</returns>
			public List<TrackData.WidthHelper> WidthHelpers
			{
				get
				{
					return widthHelpers;
				} // get
			} // WidthHelpers

			/// <summary>
			/// Tunnel helper position
			/// </summary>
			/// <returns>List</returns>
			public List<TrackData.RoadHelper> RoadHelpers
			{
				get
				{
					return roadHelpers;
				} // get
			} // TunnelHelperPos

			/// <summary>
			/// Neutrals objects
			/// </summary>
			/// <returns>List</returns>
			public List<TrackData.NeutralObject> NeutralsObjects
			{
				get
				{
					return objects;
				} // get
			} // NeutralsObjects
			#endregion

			#region Constructor
			/// <summary>
			/// Create collada track
			/// </summary>
			/// <param name="setFilename">Set filename</param>
			public ColladaTrack(string setFilename)
				: base(setFilename)
			{
				//Log.Write("ColladaTrack colladaFile=" + colladaFile);
				#region Get track data
				// Get spline first (only use one, we don't care about any other lines)
				XmlNode geometry =
					XmlHelper.GetChildNode(colladaFile, "geometry");
				XmlNode visualScene =
					XmlHelper.GetChildNode(colladaFile, "visual_scene");

				string splineId = XmlHelper.GetXmlAttribute(geometry, "id");
				// Make sure this is a spline, everything else is not supported.
				if (splineId.EndsWith("-spline") == false)
					throw new Exception("The ColladaTrack file " + Filename +
						" does not have a spline geometry in it. Unable to load track!");

				// Get spline points
				XmlNode pointsArray = XmlHelper.GetChildNode(geometry, "float_array");
				// Convert the points to a float array
				float[] pointsValues =
					StringHelper.ConvertStringToFloatArray(pointsArray.FirstChild.Value);

				// Skip first and third of each input point (tangent data from max)
				trackPoints.Clear();
				int pointNum = 0;
				while (pointNum < pointsValues.Length)
				{
					// Skip first point
					pointNum += 3;
					// Take second one
					trackPoints.Add(MaxScalingFactor * new Vector3(
						pointsValues[pointNum++],
						pointsValues[pointNum++],
						pointsValues[pointNum++]));
					// And skip thrid
					pointNum += 3;
				} // while (pointNum)

				// Check if we can find translation or scaling values for our spline
				XmlNode splineInstance = XmlHelper.GetChildNode(
					visualScene, "url", "#" + splineId);
				XmlNode splineMatrixNode = XmlHelper.GetChildNode(
					splineInstance.ParentNode, "matrix");
				if (splineMatrixNode != null)
					throw new Exception("The ColladaTrack file " + Filename +
						" should not use baked matrices. Please export again " +
						"without baking matrices. Unable to load track!");

				XmlNode splineTranslateNode = XmlHelper.GetChildNode(
					splineInstance.ParentNode, "translate");
				XmlNode splineScaleNode = XmlHelper.GetChildNode(
					splineInstance.ParentNode, "scale");
				Vector3 splineTranslate = Vector3.Zero;
				if (splineTranslateNode != null)
				{
					float[] translateValues = StringHelper.ConvertStringToFloatArray(
						splineTranslateNode.FirstChild.Value);
					splineTranslate = MaxScalingFactor * new Vector3(
						translateValues[0], translateValues[1], translateValues[2]);
				} // if (splineTranslateNode)
				Vector3 splineScale = new Vector3(1, 1, 1);
				if (splineScaleNode != null)
				{
					float[] scaleValues = StringHelper.ConvertStringToFloatArray(
						splineScaleNode.FirstChild.Value);
					splineScale = new Vector3(
						scaleValues[0], scaleValues[1], scaleValues[2]);
				} // if (splineTranslateNode)

				// Convert all points with our translation and scaling values
				for (int num = 0; num < trackPoints.Count; num++)
				{
					trackPoints[num] = Vector3.Transform(trackPoints[num],
						Matrix.CreateScale(splineScale) *
						Matrix.CreateTranslation(splineTranslate));
				} // for (num)

				//Log.Write("TrackPoints=" + StringHelper.WriteArrayData(trackPoints));
				#endregion

				// Update 2006-10-09: new helpers logic added!
				// RoadWidth for road widths
				// Reset, Palms, Laterns, Tunnel, Collisions are for road stuff
				// All other are the scene, try to find name (see models list)

				#region Get helpers
				XmlNode sceneNode = XmlHelper.GetChildNode(visualScene, "node");
				while (sceneNode != null)
				{
					string nodeId = XmlHelper.GetXmlAttribute(sceneNode, "id");

					// Must be a node
					if (nodeId.EndsWith("-node") == false)
					{
						sceneNode = sceneNode.NextSibling;
						continue;
					} // if (nodeId.EndsWith)
					
					// Find all helpers
					if (nodeId.StartsWith("RoadWidth"))
					{
						XmlNode translateNode = XmlHelper.GetChildNode(
							sceneNode, "translate");
						if (translateNode != null)
						{
							float[] translateValues = StringHelper.ConvertStringToFloatArray(
								translateNode.FirstChild.Value);
							Vector3 widthHelperPos = MaxScalingFactor * new Vector3(
								translateValues[0], translateValues[1], translateValues[2]);

							// Also get scaling if possible
							XmlNode scaleNode = translateNode.NextSibling;
							float widthScale = 1.0f;
							if (scaleNode != null &&
								scaleNode.Name == "scale")
							{
								float[] scaleValues = StringHelper.ConvertStringToFloatArray(
									scaleNode.FirstChild.Value);
								widthScale = scaleValues[0];
							} // if (scaleNode)

							widthHelpers.Add(
								new TrackData.WidthHelper(widthHelperPos, widthScale));
						} // if (translateNode)
					} // if (nodeId.StartsWith)
					else if (nodeId.StartsWith("Tunnel") ||
						nodeId.StartsWith("Palms") ||
						nodeId.StartsWith("Latern") ||
						nodeId.StartsWith("Reset") ||
						// Point was used for tunnels in the old version
						nodeId.StartsWith("Point"))
					{
						XmlNode translateNode = XmlHelper.GetChildNode(
							sceneNode, "translate");
						if (translateNode != null)
						{
							float[] translateValues = StringHelper.ConvertStringToFloatArray(
								translateNode.FirstChild.Value);
							TrackData.RoadHelper.HelperType helperType =
								nodeId.StartsWith("Tunnel") ?
								TrackData.RoadHelper.HelperType.Tunnel :
								nodeId.StartsWith("Palms") ?
								TrackData.RoadHelper.HelperType.Palms :
								nodeId.StartsWith("Latern") ?
								TrackData.RoadHelper.HelperType.Laterns :
								TrackData.RoadHelper.HelperType.Reset;
							roadHelpers.Add(new TrackData.RoadHelper(helperType,
								MaxScalingFactor * new Vector3(
								translateValues[0], translateValues[1], translateValues[2])));
						} // if (translateNode)
					} // else if
					else if (nodeId.StartsWith("Line") == false &&
						nodeId.StartsWith("Point") == false &&
						// Ignore unnamed models, we can't do anything with them.
						nodeId.StartsWith("Dummy") == false &&
						nodeId.StartsWith("Landscape") == false &&
						nodeId.StartsWith("Particle") == false)
					{
						#region Get neutral objects
						//for XRef objects, but we use XRef scenes now, XRef objects are
						//just too buggy in max8:
						//XmlNode instanceNode = XmlHelper.GetChildNode(
						//	sceneNode, "instance_node");
						XmlNode translateNode = XmlHelper.GetChildNode(
							sceneNode, "translate");
						XmlNode scaleNode = XmlHelper.GetChildNode(
							sceneNode, "scale");
						XmlNode rotateNode = XmlHelper.GetChildNode(
							sceneNode, "rotate");
						// No translate node found? Then skip!
						if (translateNode != null)
						{
							// Find out name of model
							string[] idSplitted = nodeId.Split(new char[] { '_', '-' });
							string modelName = idSplitted[0];

							// Get translate values
							float[] translateValues = StringHelper.ConvertStringToFloatArray(
								translateNode.FirstChild.Value);
							Vector3 pos = MaxScalingFactor * new Vector3(
								translateValues[0], translateValues[1], translateValues[2]);

							// Also get scaling if possible
							float scale = 1.0f;
							if (scaleNode != null)
							{
								float[] scaleValues = StringHelper.ConvertStringToFloatArray(
									scaleNode.FirstChild.Value);
								scale = scaleValues[0];
							} // if (scaleNode)
							Vector3 rotationAxis = new Vector3(0, 0, 1);
							float rotation = 0.0f;
							if (rotateNode != null)
							{
								float[] rotateValues = StringHelper.ConvertStringToFloatArray(
									rotateNode.FirstChild.Value);
								rotationAxis = new Vector3(
									rotateValues[0], rotateValues[1], rotateValues[2]);
								rotation = rotateValues[3];
							} // if (rotateNode)

							/*tst
							Log.Write("modelName="+modelName+
								", pos="+pos+
								", scale="+scale+
								", rotationAxis="+rotationAxis+
								", rotation="+rotation);
							 */

							// Now add neutral object
							objects.Add(new TrackData.NeutralObject(
								modelName,
								//not required: pos,
								Matrix.CreateFromAxisAngle(rotationAxis,
								MathHelper.ToRadians(rotation)) *
								Matrix.CreateScale(scale * MaxScalingFactor) *
								Matrix.CreateTranslation(pos)));
						} // if (translateNode)
						#endregion
					} // else

					sceneNode = sceneNode.NextSibling;
				} // while (sceneNode)
				#endregion
			} // ColladaTrack(setFilename)
			#endregion
		} // class ColladaTrack

		#region GenerateCombiModelBinaryData
		/// <summary>
		/// Generate combi model binary data
		/// </summary>
		public static void GenerateCombiModelBinaryData()
		{
			// Load all combos
			ColladaCombiModels[] combos = new ColladaCombiModels[]
				{
					new ColladaCombiModels("CombiPalms"),
					new ColladaCombiModels("CombiPalms2"),
					new ColladaCombiModels("CombiRuins"),
					new ColladaCombiModels("CombiRuins2"),
					new ColladaCombiModels("CombiStones"),
					new ColladaCombiModels("CombiStones2"),
					new ColladaCombiModels("CombiOilTanks"),
					new ColladaCombiModels("CombiSandCastle"),
					new ColladaCombiModels("CombiBuildings"),
					new ColladaCombiModels("CombiHotels"),
				};
			// Save them all
			foreach (ColladaCombiModels combo in combos)
			{
				StreamWriter file = new StreamWriter(
					TrackCombiModels.Directory+"\\"+combo.Name+"."+
					TrackCombiModels.Extension);//,
					//FileMode.Create, FileAccess.Write);

				// Save everything in this class with help of the XmlSerializer.
				new XmlSerializer(typeof(List<TrackCombiModels.CombiObject>)).
					Serialize(file.BaseStream, combo.Objects);

				// Close the file
				file.Close();
			} // foreach (combo)
		} // GenerateCombiModelBinaryData()
		#endregion
		
		#region GenerateTrackBinaryData
		/// <summary>
		/// Generate track binary data
		/// </summary>
		public static void GenerateTrackBinaryData()
		{
			// Load all tracks
			ColladaTrack[] tracks = new ColladaTrack[]
				{
					new ColladaTrack("Tracks\\TrackBeginner"),
					new ColladaTrack("Tracks\\TrackAdvanced"),
					new ColladaTrack("Tracks\\TrackExpert"),
				};
			// Save them all
			foreach (ColladaTrack track in tracks)
			{
				// Create file
				StreamWriter file = new StreamWriter(
					TrackData.Directory+"\\"+
					StringHelper.ExtractFilename(track.Filename, true)+"."+
					TrackData.Extension);

				// Construct TrackData from ColladaTrack
				TrackData trackData = new TrackData(
					track.TrackPoints,
					track.WidthHelpers,
					track.RoadHelpers,
					track.NeutralsObjects);

				// Save everything in this class with help of the XmlSerializer.
				new XmlSerializer(typeof(TrackData)).
					Serialize(file.BaseStream, trackData);
				// That's it, more data is not saved in a track file.

				// Close the file
				file.Close();
			} // foreach (combo)
		} // GenerateCombiModelBinaryData()
		#endregion
	} // class TrackImporter
} // namespace RacingGame.Tracks
#endif