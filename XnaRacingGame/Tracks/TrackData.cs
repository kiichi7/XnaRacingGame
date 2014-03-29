// Project: RacingGame, File: TrackData.cs
// Namespace: RacingGame.Tracks, Class: TrackData
// Path: C:\code\RacingGame\Tracks, Author: Abi
// Code lines: 363, Size of file: 9,24 KB
// Creation date: 04.11.2006 17:20
// Last modified: 05.11.2006 00:00
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
	/// Track data, imported from 3ds max data. See TrackImporter class.
	/// </summary>
	public class TrackData
	{
		#region Constants
		/// <summary>
		/// Directory where all the track data files are stored.
		/// </summary>
		public const string Directory = "Content";
		/// <summary>
		/// Extension for the track data files.
		/// </summary>
		public const string Extension = "Track";
		#endregion

		#region Variables
		/// <summary>
		/// Track points
		/// </summary>
		private List<Vector3> trackPoints = new List<Vector3>();

		#region WidthHelper class
		/// <summary>
		/// Width helper
		/// </summary>
		[Serializable]
		public class WidthHelper
		{
			/// <summary>
			/// Position
			/// </summary>
			public Vector3 pos;
			/// <summary>
			/// Scale
			/// </summary>
			public float scale;

			/// <summary>
			/// Create width helper
			/// </summary>
			public WidthHelper()
			{
			} // WidthHelper()

			/// <summary>
			/// Create width helper
			/// </summary>
			/// <param name="setPos">Set position</param>
			/// <param name="setScale">Set scale</param>
			public WidthHelper(Vector3 setPos, float setScale)
			{
				pos = setPos;
				scale = setScale;
			} // WidthHelper(setPos, setScale)
		} // class WidthHelper
		#endregion

		/// <summary>
		/// Width helper position
		/// </summary>
		private List<WidthHelper> widthHelpers = new List<WidthHelper>();

		#region RoadHelper class
		/// <summary>
		/// Road helper
		/// </summary>
		[Serializable]
		public class RoadHelper
		{
			/// <summary>
			/// Helper type
			/// </summary>
			public enum HelperType
			{
				Tunnel,
				Palms,
				Laterns,
				Reset,
			} // enum HelperType

			/// <summary>
			/// Type
			/// </summary>
			public HelperType type;
			/// <summary>
			/// Position
			/// </summary>
			public Vector3 pos;

			/// <summary>
			/// Create road helper
			/// </summary>
			public RoadHelper()
			{
			} // RoadHelper()

			/// <summary>
			/// Create road helper
			/// </summary>
			/// <param name="setType">Set type</param>
			/// <param name="setPos">Set position</param>
			public RoadHelper(HelperType setType, Vector3 setPos)
			{
				type = setType;
				pos = setPos;
			} // RoadHelper(setType, setPos)
		} // class RoadHelper
		#endregion

		/// <summary>
		/// Tunnel helper position
		/// </summary>
		private List<RoadHelper> roadHelpers = new List<RoadHelper>();

		#region NeutralObject class
		/// <summary>
		/// Neutral object
		/// </summary>
		[Serializable]
		public class NeutralObject
		{
			/// <summary>
			/// Model name
			/// </summary>
			public string modelName;
			/*not required, just use the matrix
			/// <summary>
			/// Position
			/// </summary>
			public Vector3 pos;
			 */
			/// <summary>
			/// Matrix
			/// </summary>
			public Matrix matrix;

			/// <summary>
			/// Create neutral object
			/// </summary>
			public NeutralObject()
			{
			} // NeutralObject()

			/// <summary>
			/// Create neutral object
			/// </summary>
			/// <param name="setModelName">Set model name</param>
			/// <param name="setMatrix">Set matrix</param>
			public NeutralObject(string setModelName, Matrix setMatrix)
			{
				modelName = setModelName;
				matrix = setMatrix;
			} // NeutralObject(setModelName, setPos, setMatrix)
		} // class NeutralObject
		#endregion

		/// <summary>
		/// List of neutral objects used in this level
		/// </summary>
		private List<NeutralObject> objects = new List<NeutralObject>();
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
		public List<WidthHelper> WidthHelpers
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
		public List<RoadHelper> RoadHelpers
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
		public List<NeutralObject> NeutralsObjects
		{
			get
			{
				return objects;
			} // get
		} // NeutralsObjects
		#endregion

		#region Constructor
		/// <summary>
		/// Create track data, empty constructor, required for Serialization.
		/// </summary>
		public TrackData()
		{
		} // TrackData()

		/// <summary>
		/// Create track data (used only in TrackImporter).
		/// </summary>
		/// <param name="setFilename">Set filename</param>
		/// <param name="setTrackPoints">Set track points</param>
		/// <param name="setWidthHelpers">Set width helpers</param>
		/// <param name="setRoadHelpers">Set road helpers</param>
		/// <param name="setObjects">Set objects</param>
		public TrackData(List<Vector3> setTrackPoints,
			List<WidthHelper> setWidthHelpers,
			List<RoadHelper> setRoadHelpers,
			List<NeutralObject> setObjects)
		{
			trackPoints = setTrackPoints;
			widthHelpers = setWidthHelpers;
			roadHelpers = setRoadHelpers;
			objects = setObjects;
		} // TrackData(setFilename, setTrackPoints, setWidthHelpers)

		/// <summary>
		/// Load track data
		/// </summary>
		/// <param name="setFilename">Set filename</param>
		public static TrackData Load(string setFilename)
		{
			// Load track data
			StreamReader file = new StreamReader(FileHelper.LoadGameContentFile(
				Directory+"\\"+setFilename+"."+Extension));

			// Load everything into this class with help of the XmlSerializer.
			TrackData loadedTrack = (TrackData)
				new XmlSerializer(typeof(TrackData)).
				Deserialize(file.BaseStream);

			// Close the file
			file.Close();

			// Return loaded file
			return loadedTrack;
		} // TrackData(setFilename)
		#endregion

		#region Unit Testing
#if DEBUG
		/*include NUnit.Framework for this
		/// <summary>
		/// Test collada track
		/// </summary>
		//[Test]
		static public void TestColladaTrack()
		{
			TrackData track = new TrackData("TrackWithHelpers");
			Assert.AreEqual("TrackWithHelpers", track.Filename);
			Assert.AreEqual(4, track.trackPoints.Count);
			Assert.AreEqual(3, track.widthHelpers.Count);
			Assert.AreEqual(0, track.tunnelHelperPos.Count);

			// Load second track with XRef objects
			track = new TrackData("TestTrackWithHouse");
			Assert.AreEqual("TestTrackWithHouse", track.Filename);
			Assert.AreEqual(4, track.trackPoints.Count);
			Assert.AreEqual(2, track.widthHelpers.Count);
			Assert.AreEqual(2, track.tunnelHelperPos.Count);
			Assert.AreEqual(1, track.objects.Count);
			Assert.AreEqual("SimpleScene", track.objects[0].modelName);

			Vector3 pos = new Vector3(-202.917206f, 283.443451f, 0f);
			Assert.AreEqual(pos, track.objects[0].pos);

			Matrix mat = new Matrix(
				//0.667814f, -0.744328f, 0, -202.917206f,
				//0.744328f, 0.667814f, 0, 283.443451f,
				0, 1, 0, 0,
				-1, 0, 0, 0,
				0, 0, 1, 0,
				-202.917206f, 283.443451f, 0, 1);
				//0, -1, 0, -202.917206f,
				//1, 0, 0, 283.443451f,
				//0, 0, 1, 0,
				//0, 0, 0, 1);
			//Assert.AreEqual(mat, track.objects[0].matrix);
			Assert.AreEqual(mat.M12, track.objects[0].matrix.M12);
			Assert.AreEqual(mat.M21, track.objects[0].matrix.M21);
			Assert.AreEqual(mat.M33, track.objects[0].matrix.M33);
			Assert.AreEqual(mat.M44, track.objects[0].matrix.M44);

			/*not supported
			track = new TrackData("TestTrackWithHouseBakedMatrices");
			Assert.AreEqual("TestTrackWithHouseBakedMatrices",
				track.Filename);
			Assert.AreEqual(4, track.trackPoints.Count);
			Assert.AreEqual(3, track.widthHelperPos.Count);
			Assert.AreEqual(3, track.widthHelperScale.Count);
			Assert.AreEqual(0, track.tunnelHelperPos.Count);
			Assert.AreEqual(1, track.objects.Count);
			Assert.AreEqual("House", track.objects[0].modelName);
			Assert.AreEqual(Vector3.Zero, track.objects[0].pos);
			Assert.AreEqual(Matrix.Identity, track.objects[0].matrix);
			 *
		} // TestColladaTrack()
		*/
#endif
		#endregion
	} // class TrackData
} // namespace RacingGame.Tracks
