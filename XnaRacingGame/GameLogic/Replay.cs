// Author: abi
// Project: Xbox 360 Copy of RacingGame
// Path: C:\code\Xna\RacingGame\GameLogic
// Creation date: 03.02.2008 18:36
// Last modified: 09.02.2008 17:27

// Project: RacingGame, File: Replay.cs
// Namespace: RacingGame.GameLogic, Class: Replay
// Path: C:\code\RacingGame\GameLogic, Author: Abi
// Code lines: 1501, Size of file: 49,58 KB
// Creation date: 19.10.2006 22:03
// Last modified: 07.11.2006 03:04
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using RacingGame.Helpers;
using System.IO;
using RacingGame.GameScreens;
using RacingGame.Tracks;
#endregion

namespace RacingGame.GameLogic
{
	/// <summary>
	/// This class stores a replay for a track, which means we can
	/// see how the player drove one lap of the track. It will be
	/// replace once we drive faster than the current record.
	/// By default we start with the default replay, which just shows
	/// the computer driving at a low speed.
	/// </summary>
	public class Replay
	{
		#region Constants
		/// <summary>
		/// Track matrix intervals between the trackMatrixValues.
		/// </summary>
		public const float TrackMatrixIntervals = 0.2f;

		/// <summary>
		/// Replay filenames for each of the tracks
		/// </summary>
		static readonly string[] ReplayFilenames = new string[]
			{
				"TrackBeginner.Replay",
				"TrackAdvanced.Replay",
				"TrackExpert.Replay",
			};
		#endregion

		#region Variables
		/// <summary>
		/// Track number for this replay: 0 (beginner), 1 (advanced) or 2 (expert)
		/// </summary>
		int trackNum = 0;

		/// <summary>
		/// Lap time for this replay in seconds.
		/// </summary>
		float lapTime = 0.0f;

		/// <summary>
		/// Track matrix values, each value contains the position and
		/// rotatation of the car at that specific time.
		/// Between this values the car matrix will get interpolated.
		/// </summary>
		List<Matrix> trackMatrixValues = new List<Matrix>();

		/// <summary>
		/// Times for each checkpoint for comparing if we have done
		/// better or worse than the best time.
		/// </summary>
		List<float> checkpointTimes = new List<float>();
		#endregion

		#region Properties
		/// <summary>
		/// Track number
		/// </summary>
		public int TrackNumber
		{
			get
			{
				return trackNum;
			} // get
		} // get

		/// <summary>
		/// Lap time
		/// </summary>
		public float LapTime
		{
			get
			{
				return lapTime;
			} // get
		} // LapTime
		
		/// <summary>
		/// Number of track matrices currently in trackMatrixValues list.
		/// </summary>
		public int NumberOfTrackMatrices
		{
			get
			{
				return trackMatrixValues.Count;
			} // get
		} // NumberOfTrackMatrices

		/// <summary>
		/// Track number
		/// </summary>
		public List<float> CheckpointTimes
		{
			get
			{
				return checkpointTimes;
			} // get
		} // CheckpointTimes

		/// <summary>
		/// Get car matrix at time (interpolated)
		/// </summary>
		/// <param name="trackTime">Track time</param>
		/// <returns>Matrix</returns>
		public Matrix GetCarMatrixAtTime(float trackTime)
		{
			// No values available? We need at least 2 matrices!
			if (trackMatrixValues.Count < 2)
				return Matrix.Identity;

			// Not started yet? Then just return start matrix
			if (trackTime <= 0.0f)
				return trackMatrixValues[0];

			// Get track num and percent of the current interval.
			int trackNum = (int)(trackTime / TrackMatrixIntervals);
			float trackIntervalPercent =
				(trackTime - trackNum * TrackMatrixIntervals) /
				TrackMatrixIntervals;
			if (trackNum < 0)
				trackNum = 0;
			
			// At end? Then wait at start, do not interpolate anymore!
			if (trackNum > trackMatrixValues.Count - 2)
				return trackMatrixValues[0];

			// Interpolate and return
			return Matrix.Lerp(
				trackMatrixValues[trackNum],
				trackMatrixValues[trackNum + 1],
				trackIntervalPercent);

			//Log.Write("trackTime="+trackTime+
			//	", trackNum=" + trackNum +
			//	", ret=" + ret);
			//return ret;
		} // GetCarMatrixAtTime(trackTime)
		#endregion

		#region Constructor
		/// <summary>
		/// Create new replay for a specific track number.
		/// </summary>
		/// <param name="setTrackNum">Set track number</param>
		public Replay(int setTrackNum, bool createNew, Track track)
		{
			trackNum = setTrackNum;

			// If not creating new, load it from file!
			if (createNew == false)
			{
				// Load if possible
				if (File.Exists(Path.Combine(
					Directories.ContentDirectory, ReplayFilenames[trackNum])))
				{
					FileStream stream = FileHelper.LoadGameContentFile(
						"Content\\"+ReplayFilenames[trackNum]);
					BinaryReader reader = new BinaryReader(stream);

					// Load total lap time
					lapTime = reader.ReadSingle();

					// Load matrix values
					int numOfMatrixValues = reader.ReadInt32();
					for (int num = 0; num < numOfMatrixValues; num++)
						trackMatrixValues.Add(
							FileHelper.ReadMatrix(reader));

					// Load checkpoint times
					int numOfCheckpointTimes = reader.ReadInt32();
					for (int num = 0; num < numOfCheckpointTimes; num++)
						checkpointTimes.Add(reader.ReadSingle());

					// Loading complete, close.
					stream.Close();
				} // if
				else
				{
					// Create new default replay for this track!
					// Get all track positions based on the current top highscore time!
					lapTime = Highscores.GetTopLapTime(trackNum);
					int numOfMatrixValues = 1 + (int)(lapTime / TrackMatrixIntervals);
					
					float lastTrackPos = 0.0f;
					int oldTrackSegmentNumber = 0;

					// Go twice as long and abort when we reach the finish line!
					for (int num = 0; num < numOfMatrixValues*2; num++)
					{
						// See Landscape.TestRenderLandscape for more details.
						float carTrackPos = 0.00001f +
							((float)num / (float)(numOfMatrixValues - 1));// *
							//not needed, scaled in GetTrackPositionMatrix: track.Length;
						float difference = carTrackPos - lastTrackPos;
						carTrackPos = lastTrackPos + difference * 0.1f;
						lastTrackPos = carTrackPos;

						float roadWidth, nextRoadWidth;
						Matrix carMatrix =
							track.GetTrackPositionMatrix(carTrackPos,
							out roadWidth, out nextRoadWidth);

						//Log.Write("create num=" + num +
						//	"\ncarTrackPos=" + carTrackPos +
						//	"\ncarMatrix=" + carMatrix);

						// Interpolate carPos a little
						//not good:
						//carPos = Vector3.SmoothStep(carPos, carMatrix.Translation, 0.25f);
						//carMatrix.Translation = carPos;

						// Store
						trackMatrixValues.Add(carMatrix);

						// Also check if we passed a checkpoint
						int trackSegmentNumber = (int)(carTrackPos * track.NumberOfSegments);
						// Segment changed
						if (trackSegmentNumber != oldTrackSegmentNumber)
						{
							// Check if we passed a checkpoint.
							for (int checkpointNum = 0; checkpointNum < track.
								CheckpointSegmentPositions.Count; checkpointNum++)
							{
								// We have to check if we are between the old and the current
								// track segement numbers, we might skip one or two in 200ms.
								if (track.CheckpointSegmentPositions[checkpointNum] >
									oldTrackSegmentNumber &&
									track.CheckpointSegmentPositions[checkpointNum] <=
									trackSegmentNumber)
								{
									// We passed that checkpoint, add the simulated time
									checkpointTimes.Add(
										lapTime * (float)num / (float)(numOfMatrixValues - 1));
									break;
								} // if
							} // for
						} // if
						oldTrackSegmentNumber = trackSegmentNumber;

						// Reached finish?
						if (carTrackPos >= 1.0f)
							// Then abort, do not add more.
							break;
					} // for (all matrix values)

					// Add the final checkpoint for the laptime
					checkpointTimes.Add(lapTime);
				} // else (not loading)
			} // if (createNew == false)
		} // Replay(setTrackNum)
		#endregion

		#region Save
		/// <summary>
		/// Save this replay, will be saved to
		/// TrackBeginner.replay, TrackAdvanced.replay or TrackExpert.replay.
		/// </summary>
		public void Save(float setLapTime)
		{
			try
			{
				FileStream stream = FileHelper.CreateGameContentFile(
					"Content\\" + ReplayFilenames[trackNum], true);
				BinaryWriter writer = new BinaryWriter(stream);

				lapTime = setLapTime;
				writer.Write(lapTime);

				// Save track matrix values
				writer.Write(trackMatrixValues.Count);
				for (int num = 0; num < trackMatrixValues.Count; num++)
					FileHelper.WriteMatrix(writer, trackMatrixValues[num]);

				// Save checkpoint times
				writer.Write(checkpointTimes.Count);
				for (int num = 0; num < checkpointTimes.Count; num++)
					writer.Write(checkpointTimes[num]);

				/*tst
				Log.Write("lapTime=" + lapTime +
					", trackMatrixValues.Count=" + trackMatrixValues.Count +
					", checkpointTimes.Count=" + checkpointTimes.Count);
				for (int num = 0; num < checkpointTimes.Count; num++)
					Log.Write("checkpointTimes[" + num + "]=" + checkpointTimes[num]);
				 */

				// That's it, save and close.
				stream.Close();
			} // try
			catch 
			{
				// Ignore, might fail on the Xbox 360
			} // catch
		} // Save()
		#endregion

		#region Add car matrix
		/// <summary>
		/// Add car matrix to trackMatrixValues.
		/// </summary>
		/// <param name="addMatrix">Add matrix</param>
		public void AddCarMatrix(Matrix addMatrix)
		{
			/*tst:
			Log.Write(
				"add matrix .. time=" + RacingGame.Graphics.BaseGame.TotalTime +
				", matrix=" + addMatrix +
				", game time ms=" + RacingGameManager.Player.GameTimeMilliseconds +
				", game time blub=" + (RacingGameManager.Player.GameTimeMilliseconds *
				Replay.TrackMatrixIntervals / 1000.0f) +
				", trackMatrixValues.Count=" + trackMatrixValues.Count);
			//*/
			trackMatrixValues.Add(addMatrix);
		} // AddCarMatrix(addMatrix)
		#endregion
	} // class Replay
} // namespace RacingGame.GameLogic
