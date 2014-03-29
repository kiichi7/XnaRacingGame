// Project: RacingGame, File: CarPhysics.cs
// Namespace: RacingGame.GameLogic, Class: CarPhysics
// Path: C:\code\RacingGame\GameLogic, Author: Abi
// Code lines: 1501, Size of file: 49,58 KB
// Creation date: 19.10.2006 22:03
// Last modified: 07.11.2006 03:04
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RacingGame.GameLogic.Physics;
using RacingGame.Graphics;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Sounds;
using Model = RacingGame.Graphics.Model;
using RacingGame.Tracks;
#endregion

namespace RacingGame.GameLogic
{
	/// <summary>
	/// Car controller class for controlling the car we drive.
	/// This class is derived from the BasePlayer class, which stores all
	/// important game values for us (game time, game over, etc.).
	/// The ChaseCamera is then derived from this class and finally we got the
	/// Player class itself at the top, which is deriven from all these classes.
	/// This way we can easily access everything from the Player class.
	/// </summary>
	/// <returns>Base player</returns>
	public class CarPhysics : BasePlayer
	{
		#region Constants
		/// <summary>
		/// Car is 1000 kg
		/// </summary>
		public const float DefaultCarMass = 1000;//1000.0f;

		/// <summary>
		/// Gravity on earth is 9.81 m/s^2
		/// You could change this for different track behaviors
		/// </summary>
		const float Gravity = 9.81f;

		/// <summary>
		/// Max speed of our car is 275 mph.
		/// While we use mph for the display, we calculate internally with
		/// meters per sec since meter is the unit we use for everthing in the
		/// game. And it is a much nicer unit than miles or feet.
		/// </summary>
		public const float DefaultMaxSpeed =
			275.0f * MphToMeterPerSec,
			MaxPossibleSpeed =
			290.0f * MphToMeterPerSec;

		/// <summary>
		/// Max. acceleration in m/s^2 we can do per second.
		/// We have also to define the max and min overall accelleration we can
		/// do with our car (very car specfic, but for this game always the same
		/// to make it fair). Driving backwards is slower than driving forward.
		/// </summary>
		public const float DefaultMaxAccelerationPerSec = 2.85f,//5.0f,//120.0f,
			MaxAcceleration = 6.75f,//8.0f,//70.0f,//250.0f,
			MinAcceleration = -3.25f;//-6.0f;//-35.0f;//250.0f;//-40.0f;

		/// <summary>
		/// Friction we have on the road each second. If we are driving slow,
		/// this slows us down quickly. If we drive really fast, this does not
		/// matter so much anymore. The main slowdown will be the air friction.
		/// </summary>
		const float CarFrictionOnRoad = 15.523456789f;//15.0f;//2.5f;

		/// <summary>
		/// Air friction we get if we drive fast, this slows us down very fast
		/// if we drive fast. It makes it also much harder to drive faster if
		/// we drive already at a very fast speed. For slow speeds the air
		/// friction does not matter so much. This could be extended to include
		/// wind and then even at low speeds the air friction would slow us
		/// down or even influcene our movement. Maybe in a Game Mod sometime ...
		/// </summary>
		const float AirFrictionPerSpeed = 0.56f;//0.225f;//0.33f;//0.42f;

		/// <summary>
		/// Max air friction, this way we can have a good air friction for low
		/// speeds but we are not limited to 190-210mph, but can drive even faster.
		/// </summary>
		const float MaxAirFriction = AirFrictionPerSpeed * 200.0f;

		/// <summary>
		/// Break slowdown per second, 1.0 means we need 1 second to do a full
		/// break. Slowdown is also limited by max. 100 per sec!
		/// Note: This would not make sense in a real world simulation because
		/// stopping the car needs usually more time and is highly dependant
		/// on the speed resultin in bigger stopping distances.
		/// For this game it is easier and more fun to just brake always the same.
		/// </summary>
		const float BrakeSlowdown = 1.0f;

		/// <summary>
		/// Convert our meter per sec to mph for display.
		/// 1 mile = 1.609344 kilometers
		/// Each hour has 3600 seconds (60 min * 60 sec).
		/// 1 kilometer = 1000 meter.
		/// </summary>
		public const float MeterPerSecToMph =
			1.609344f * ((60.0f*60.0f)/1000.0f),
			MphToMeterPerSec = 1.0f / MeterPerSecToMph;

		/// <summary>
		/// Max rotation per second we use for our car.
		/// </summary>
		public const float MaxRotationPerSec = 1.25f;//0.95f;

		/// <summary>
		/// This vector will be added to the car position to let our camera
		/// look at the roof of our car and not at the street.
		/// </summary>
		protected static readonly Vector3 CarHeightVector =
			new Vector3(0, 0, 2.0f);
		#endregion

		#region Variables
		#region Car variables (based on the car model)
		/// <summary>
		/// Max speed of the car, set from the car type (see CarSelection screen).
		/// We start with the speed 0, then it is increased based on the
		/// current acceleration value to this maxSpeed value.
		/// </summary>
		protected static float maxSpeed = DefaultMaxSpeed * 1.05f;

		/// <summary>
		/// Mass of the car. Used for physics calculations.
		/// Set from the car type (see CarSelection screen).
		/// </summary>
		protected static float carMass = DefaultCarMass * 1.015f;

		/// <summary>
		/// Current acceleration of the car. Drive faster or break with up/down,
		/// left/right mouse or the gamepad (left/right triggers or right thumb).
		/// The acceleration influcences the current speed of the car.
		/// </summary>
		protected static float maxAccelerationPerSec =
			DefaultMaxAccelerationPerSec * 0.85f;

		/// <summary>
		/// Set car variables from car model. Called from CarSelection screen.
		/// See CarSelection class for all the car variables.
		/// </summary>
		/// <param name="setMaxCarSpeed">Set max car speed</param>
		/// <param name="setCarMass">Set car mass</param>
		/// <param name="setMaxAccelerationPerSec">Set max acceleration per second
		/// </param>
		public static void SetCarVariablesForCarType(float setMaxCarSpeed,
			float setCarMass, float setMaxAccelerationPerSec)
		{
			maxSpeed = setMaxCarSpeed;
			carMass = setCarMass;
			maxAccelerationPerSec = setMaxAccelerationPerSec;

			carPitchPhysics = new SpringPhysicsObject(
				carMass, 1.5f, 120, 0);
		} // CarSpeed
		#endregion

		/// <summary>
		/// Car position, updated each frame by our current carSpeed vector.
		/// </summary>
		protected Vector3 carPos;
		
		/// <summary>
		/// Direction the car is currently heading.
		/// </summary>
		Vector3 carDir;

		/// <summary>
		/// Speed of our car, just in the direction of our car.
		/// Sliding is a nice feature, but it overcomplicates too much and
		/// for this game sliding would be really bad and make it much harder
		/// to drive!
		/// </summary>
		float speed;

		/// <summary>
		/// Car up vector for orientation.
		/// </summary>
		Vector3 carUp;

		/// <summary>
		/// Car up vector
		/// </summary>
		/// <returns>Vector 3</returns>
		protected Vector3 CarUpVector
		{
			get
			{
				return carUp;
			} // get
		} // CarUpVector

		/// <summary>
		/// Force we currently apply on our car. Usually we determinate how
		/// much acceleration we get through the controls and multiplying that
		/// with the current carDir.
		/// But for gravity, staying on the ground, crashes, collisions impulses
		/// and all other forces this vector might be somewhat different.
		/// Used each frame to change carSpeed.
		/// </summary>
		Vector3 carForce;

		/// <summary>
		/// Car pitch physics helper for a simple spring effect for
		/// accelerating, decelerating and crashing.
		/// </summary>
		static SpringPhysicsObject carPitchPhysics = new SpringPhysicsObject(
			DefaultCarMass, 1.5f, 120, 0);//2, 135, 0);// 0.09995f, 10.1f, 0.0f);

		/// <summary>
		/// View distance, which we can change with page up/down and the mouse
		/// wheel, but it always moves back to 1. The real view distance is
		/// also changed depending on how fast we drive (see UpdateCar stuff below)
		/// </summary>
		float viewDistance = 1.0f;

		/// <summary>
		/// Wheel position, used for animating the wheels
		/// </summary>
		private float wheelPos = 0.0f;

		/// <summary>
		/// Wheel movement speed for the animation used in the model class.
		/// </summary>
		const float WheelMovementSpeed = 1.0f;

		/// <summary>
		/// Rotate car after collision.
		/// </summary>
		float rotateCarAfterCollision = 0;

		/// <summary>
		/// Is car on ground? Only allow rotation, apply ground friction,
		/// speed changing if we are on ground and adding brake tracks.
		/// </summary>
		protected bool isCarOnGround = false;

		/// <summary>
		/// Helper variables to keep track of our car position on the current
		/// track. Always start with 0 (start pos) and update each frame!
		/// We could also check for the track position each frame by going
		/// through all the track segments, but that would be very slow because
		/// we got a few thousand track segments. Instead we only have to check
		/// the previous and next track segments until we find the right location.
		/// Usually this means we don't have to change or just change the
		/// trackSegmentNumber by 1.
		/// </summary>
		int trackSegmentNumber = 0;
		/// <summary>
		/// Track segment percent, tells us where we are on the current segment.
		/// Always between 0 and 1, for more information
		/// <see>trackSegmentNumber</see>
		/// </summary>
		float trackSegmentPercent = 0;

		/// <summary>
		/// Car render matrix we calculate each frame.
		/// </summary>
		Matrix carRenderMatrix = Matrix.Identity;
		#endregion

		#region Properties
		/// <summary>
		/// Car position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 CarPosition
		{
			get
			{
				return carPos;
			} // get
		} // CarPosition

		/// <summary>
		/// Speed
		/// </summary>
		/// <returns>Float</returns>
		public float Speed
		{
			get
			{
				return speed;
			} // get
		} // Speed

		float lastAccelerationResult = 0.0f;
		int lastGear = 0;
		/// <summary>
		/// Acceleration
		/// </summary>
		/// <returns>Float</returns>
		public float Acceleration
		{
			get
			{
				// Find out how much force we apply to the current car direction.
				// Always interpolate our result.
				lastAccelerationResult +=
					Vector3.Dot(carForce, carDir) * 0.01f * BaseGame.MoveFactorPerSecond;
				if (lastAccelerationResult < -0.25f)
					lastAccelerationResult = -0.25f;
				if (lastAccelerationResult > 1)
					lastAccelerationResult = 1;

				// Drop to 0 for a short time if gear change happend
				int thisGear = 1 + (int)(5 * Speed / MaxPossibleSpeed);
				if (thisGear != lastGear)
				{
					lastAccelerationResult = 0;
					lastGear = thisGear;
				} // if (thisGear)

				return lastAccelerationResult;
			} // get
		} // Acceleration

		/// <summary>
		/// Look at position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 LookAtPos
		{
			get
			{
				return carPos + CarHeightVector;
			} // get
		} // LookAtPos

		/// <summary>
		/// Car direction
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 CarDirection
		{
			get
			{
				return carDir;
			} // get
		} // CarDirection

		/// <summary>
		/// Car wheel position
		/// </summary>
		/// <returns>Float</returns>
		public float CarWheelPos
		{
			get
			{
				return wheelPos;
			} // get
		} // CarWheelPos

#if DEBUG
		/// <summary>
		/// Some unit tests might want to set the car wheel positions.
		/// </summary>
		/// <param name="setWheelPos">Speed</param>
		public void SetCarWheelPos(float setWheelPos)
		{
			wheelPos = setWheelPos;
		} // SetCarWheelPos(setSpeed)
#endif

		/// <summary>
		/// Car right
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 CarRight
		{
			get
			{
				return Vector3.Cross(carDir, carUp);
			} // get
		} // CarRight

		/// <summary>
		/// Car render matrix, this is the final matrix for rendering our car,
		/// which is calculated in UpdateCarMatrixAndCamera, which is called
		/// by Update each frame.
		/// </summary>
		/// <returns>Matrix</returns>
		public Matrix CarRenderMatrix
		{
			get
			{
				return carRenderMatrix;
			} // get
		} // CarRenderMatrix
		#endregion

		#region Constructor
		/// <summary>
		/// Create car physics controller
		/// </summary>
		/// <param name="setCarPosition">Set car position</param>
		public CarPhysics(Vector3 setCarPosition)
		{
			SetCarPosition(setCarPosition,
				new Vector3(0, 1, 0), new Vector3(0, 0, 1));
		} // CarPhysics(setCarPosition)

		/// <summary>
		/// Create car physics controller
		/// </summary>
		/// <param name="setCarPosition">Set car position</param>
		/// <param name="setDirection">Set direction</param>
		/// <param name="setUp">Set up</param>
		public CarPhysics(Vector3 setCarPosition,
			Vector3 setDirection,
			Vector3 setUp)
		{
			SetCarPosition(setCarPosition, setDirection, setUp);
		} // CarPhysics(setCarPosition, setDirection, setUp)
		#endregion

		#region SetCarPosition
		/// <summary>
		/// Set car position
		/// </summary>
		/// <param name="setCarPosition">Set car position</param>
		/// <param name="setDirection">Set direction</param>
		/// <param name="setUp">Set up</param>
		public void SetCarPosition(
			Vector3 setNewCarPosition,
			Vector3 setDirection,
			Vector3 setUp)
		{
			// Add car height to make camera look at the roof and not at the street.
			carPos = setNewCarPosition;
			carDir = setDirection;
			carUp = setUp;
		} // SetCarPosition(setCarPosition, setDirection, setUp)
		#endregion

		#region Reset everything for starting a new game
		/// <summary>
		/// Reset all player entries for restarting a game, just resets the
		/// car speed here.
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			speed = 0;
			carForce = Vector3.Zero;
			trackSegmentNumber = 0;
			trackSegmentPercent = 0;
		} // Reset()

		/// <summary>
		/// Clear variables for game over
		/// </summary>
		public override void ClearVariablesForGameOver()
		{
			base.ClearVariablesForGameOver();
			speed = 0;
			carForce = Vector3.Zero;
			trackSegmentNumber = 0;
			trackSegmentPercent = 0;
		} // ClearVariablesForGameOver()
		#endregion

		#region Update
		float virtualRotationAmount = 0.0f;
		float rotationChange = 0.0f;
		/// <summary>
		/// Update game logic for our car.
		/// </summary>
		public override void Update()
		{
			base.Update();

			// Don't use the car position and car handling if in free camera mode.
			if (RacingGameManager.Player.FreeCamera)
				return;

			// Only allow control if zommed in, use carOnGround as helper
			if (ZoomInTime > 0)
				isCarOnGround = false;

			wheelPos += BaseGame.MoveFactorPerSecond * speed / WheelMovementSpeed;

			float moveFactor = BaseGame.MoveFactorPerSecond;
			// Make sure this is never below 0.001f and never above 0.5f
			// Else our formulars below might mess up or carSpeed and carForce!
			if (moveFactor < 0.001f)
				moveFactor = 0.001f;
			if (moveFactor > 0.5f)
				moveFactor = 0.5f;

			#region Handle rotations
			// First handle rotations (reduce last value)
			rotationChange *= 0.95f;//0.75f;

			// Left/right changes rotation
			if (Input.KeyboardLeftPressed ||
				Input.Keyboard.IsKeyDown(Keys.A))
				rotationChange += MaxRotationPerSec * moveFactor / 2.5f;// 2;
			else if (Input.KeyboardRightPressed ||
				Input.Keyboard.IsKeyDown(Keys.D) ||
				Input.Keyboard.IsKeyDown(Keys.E))
				rotationChange -= MaxRotationPerSec * moveFactor / 2.5f;//2;
			else
				rotationChange = 0;

			if (Input.MouseXMovement != 0)
				rotationChange -=
					(Input.MouseXMovement / 15.0f) *
					MaxRotationPerSec * moveFactor;
			if (Input.IsGamePadConnected)
			{
				// More dynamic force changing with gamepad (slow, faster, etc.)
				rotationChange -=
					Input.GamePad.ThumbSticks.Left.X *
					MaxRotationPerSec * moveFactor / 1.12345f;
				// Also allow pad to simulate same behaviour as on keyboard
				if (Input.GamePad.DPad.Left == ButtonState.Pressed)
					rotationChange +=
						MaxRotationPerSec * moveFactor / 1.5f;
				else if (Input.GamePad.DPad.Right == ButtonState.Pressed)
					rotationChange -=
						MaxRotationPerSec * moveFactor / 1.5f;
			} // if (Input.IsGamePadConnected)

			float maxRot = MaxRotationPerSec * moveFactor * 1.25f; // * 1;

			// Handle car rotation after collision
			if (rotateCarAfterCollision != 0)
			{
				if (rotateCarAfterCollision > maxRot)
				{
					rotationChange += maxRot;
					rotateCarAfterCollision -= maxRot;
				} // if (rotateCarAfterCollision)
				else if (rotateCarAfterCollision < -maxRot)
				{
					rotationChange -= maxRot;
					rotateCarAfterCollision += maxRot;
				} // else if
				else
				{
					rotationChange += rotateCarAfterCollision;
					rotateCarAfterCollision = 0;
				} // else
			} // if (rotateCarAfterCollision)
			else
			{
				// If we are staying or moving very slowly, limit rotation!
				if (speed < 10.0f)
					rotationChange *= 0.67f + 0.33f * speed / 10.0f;
				else
					rotationChange *= 1.0f + (speed - 10) / 100.0f;
			} // else

			// Limit rotation change to MaxRotationPerSec * 1.5 (usually for mouse)
			if (rotationChange > maxRot)
				rotationChange = maxRot;
			if (rotationChange < -maxRot)
				rotationChange = -maxRot;

			// Rotate dir around up vector
			// Interpolate rotatation amount.
			virtualRotationAmount += rotationChange;
			// Smooth over 200ms
			float interpolatedRotationChange =
				(rotationChange + virtualRotationAmount) *
				moveFactor / 0.225f;// / 0.200f;
			virtualRotationAmount -= interpolatedRotationChange;
			if (isCarOnGround)
				carDir = Vector3.TransformNormal(carDir,
					Matrix.CreateFromAxisAngle(carUp, interpolatedRotationChange));
			#endregion

			#region Handle view distance (page up/down and mouse wheel)
			if (Input.Keyboard.IsKeyDown(Keys.PageUp) ||
				Input.GamePad.Buttons.X == ButtonState.Pressed)
				viewDistance -= moveFactor * 2.0f;
			if (Input.Keyboard.IsKeyDown(Keys.PageDown) ||
				Input.GamePad.Buttons.Y == ButtonState.Pressed)
				viewDistance += moveFactor * 2.0f;
			if (Input.MouseWheelDelta != 0)
				viewDistance -= Input.MouseWheelDelta / 500.0f;

			// Limit to 0-4
			if (viewDistance < 0)
				viewDistance = 0;
			// Allow bigger distances when zooming in
			if (ZoomInTime == 0)
			{
				if (viewDistance > 3)
				viewDistance = 3;

				// Slowly reduce back to 1.0f
				float reduceViewDistance = moveFactor * 0.5f;
				if (viewDistance <= 1 - reduceViewDistance)
					viewDistance += reduceViewDistance;
				else if (viewDistance >= 1 + reduceViewDistance)
					viewDistance -= reduceViewDistance;
				else
					viewDistance = 1;
			} // if (zoomInTime)
			#endregion

			#region Handle speed
			// With keyboard, do heavy changes, but still smooth over 200ms
			// Up or left mouse button accelerates
			// Also support ASDW (querty) and AOEW (dvorak) shooter like controlling!
			float newAccelerationForce = 0.0f;
			if (Input.KeyboardUpPressed ||
				Input.Keyboard.IsKeyDown(Keys.W) ||
				Input.MouseLeftButtonPressed ||
				Input.GamePadAPressed)
				newAccelerationForce +=
					maxAccelerationPerSec;// * moveFactor;
			// Down or right mouse button decelerates
			else if (Input.KeyboardDownPressed ||
				Input.Keyboard.IsKeyDown(Keys.S) ||
				Input.Keyboard.IsKeyDown(Keys.O) ||
				Input.MouseRightButtonPressed)
				newAccelerationForce -=
					maxAccelerationPerSec;// * moveFactor;
			else if (Input.IsGamePadConnected)
			{
				// More dynamic force changing with gamepad (slow, faster, etc.)
				newAccelerationForce +=
					(Input.GamePad.Triggers.Right) *
					maxAccelerationPerSec;// *moveFactor;
				// Also allow pad to simulate same behaviour as on keyboard
				if (Input.GamePad.DPad.Up == ButtonState.Pressed)
					newAccelerationForce +=
						maxAccelerationPerSec;// * moveFactor;
				else if (Input.GamePad.DPad.Down == ButtonState.Pressed)
					newAccelerationForce -=
						maxAccelerationPerSec;// *moveFactor;
			} // else if

			// Limit acceleration (but drive as fast forwards as possible if we
			// are moving backwards)
			if (speed > 0 &&
				newAccelerationForce > MaxAcceleration)
				newAccelerationForce = MaxAcceleration;
			if (newAccelerationForce < MinAcceleration)
				newAccelerationForce = MinAcceleration;

			// If we are currently rotating, reduce force!
			//don't need this anymore, vectors do that for us!
			//newAccelerationForce *= 1.0f - Math.Abs(rotationChange * 2.5f);

			// Add acceleration force to total car force, but use the current carDir!
			if (isCarOnGround)
				carForce +=
					carDir * newAccelerationForce * (moveFactor * 85);// 70);//75);

			// Change speed with standard formula, use acceleration as our force
			float oldSpeed = speed;
			Vector3 speedChangeVector = carForce / carMass;
			// Only use the amount important for our current direction (slower rot)
			if (isCarOnGround &&
				speedChangeVector.Length() > 0)
			{
				float speedApplyFactor =
					Vector3.Dot(Vector3.Normalize(speedChangeVector), carDir);
				if (speedApplyFactor > 1)
					speedApplyFactor = 1;
				speed += speedChangeVector.Length() * speedApplyFactor;
			} // if (speedChangeVector.Length)

			// Apply friction. Basically we have 2 frictions that slow us down:
			// The friction from the contact of the wheels with the road (rolling
			// friction) and the air friction, which becomes bigger as we drive
			// faster. We need more force to overcome the resistances if we drive
			// faster. Our engine is strong enough to overcome the initial
			// car friction and air friction, but we want simulate that we need
			// more force to overcome the resistances at high speeds.
			// Usually this would require a more complex formula and the car
			// should need more fuel and force at high speeds, we just simulate that
			// by reducing the force depending on the frictions to get the same
			// effect while having our constant forces that are calculated above.

			// Max. air friction to MaxAirFiction, else driving very fast becomes
			// too hard.
			float airFriction = AirFrictionPerSpeed * Math.Abs(speed);
			if (airFriction > MaxAirFriction)
				airFriction = MaxAirFriction;
			// Don't use ground friction if we are not on the ground.
			float groundFriction = CarFrictionOnRoad;
			if (isCarOnGround == false)
				groundFriction = 0;

			carForce *= 1.0f - (//0.033f* //
				//0.025f * // 0.01f * //Math.Min(0.1f, Math.Max(0.01f, moveFactor)) *
				0.275f * 0.02125f *//BaseGame.MoveFactorPerSecond *
				0.2f * // 20% for force slowdown
				(groundFriction + airFriction));
			// Reduce the speed, but use very low values to make the game more fun!
			float noFrictionSpeed = speed;
			speed *= 1.0f - (0.01f * // moveFactor *
				//0.0033f * // 0.25% for speed slowdown (which is a lot for high speeds)
				0.1f * 0.02125f *//BaseGame.MoveFactorPerSecond *
				(groundFriction + airFriction));
			// Never change more than by 1
			if (speed < noFrictionSpeed - 1)
				speed = noFrictionSpeed - 1;

			if (isCarOnGround)
			{
				bool backPressed =
					Input.MouseRightButtonPressed ||
					Input.KeyboardDownPressed ||
					Input.GamePad.DPad.Down == ButtonState.Pressed;

				if (Input.Keyboard.IsKeyDown(Keys.Space) ||
					Input.MouseMiddleButtonPressed ||
					Input.GamePad.Buttons.LeftShoulder == ButtonState.Pressed ||
					Input.GamePad.Buttons.RightShoulder == ButtonState.Pressed ||
					Input.GamePad.Triggers.Left > 0.5f ||
					Input.GamePadBPressed ||
					// Also use back for this
					backPressed)
				{
					float slowdown =
						1.0f - moveFactor *
						// Use only half if we just decelerate
						(backPressed ? BrakeSlowdown / 2 : BrakeSlowdown) *
						// Don't brake so much if we are already driving backwards
						(speed < 0 ? 0.33f : 1.0f);
					speed *= Math.Max(0, slowdown);
					// Limit to max. 100 mph slowdown per sec
					if (speed > oldSpeed + 100 * moveFactor)
						speed = (oldSpeed + 100 * moveFactor);
					if (speed < oldSpeed - 100 * moveFactor)
						speed = (oldSpeed - 100 * moveFactor);

					// Remember that we slowed down for generating tracks.
					backPressed = true;
				} // if (Input.Keyboard.IsKeyDown)

				// Calculate pitch depending on the force
				float speedChange = speed - oldSpeed;

				// Add brake tracks.
				if (speed > 0.5f && speed < 7.5f && speedChange > 5.5f * moveFactor ||
					speed > 0.75f && speedChange < 10 * moveFactor && backPressed)
				{
					RacingGameManager.Landscape.AddBrakeTrack(
						carPos + carDir * 1.25f, carDir, CarRight);

					// And play sound for braking
					Sound.PlayBrakeSound(speed, speedChange, rotationChange);
				} // if (speed)

				// Limit speed change, never apply more than 5 per sec.
				if (speedChange < -8 * moveFactor)
					speedChange = -8 * moveFactor;
				if (speedChange > 8 * moveFactor)
					speedChange = 8 * moveFactor;
				carPitchPhysics.ChangePos(speedChange);
			} // if (isCarOnGround)

			// Limit speed
			if (speed > maxSpeed)
				speed = maxSpeed;
			if (speed < -maxSpeed)
				speed = -maxSpeed;

			// Apply speed and calculate new car position.
			carPos += speed * carDir * moveFactor * 1.75f;

			// Handle pitch spring
			carPitchPhysics.Simulate(moveFactor);
			#endregion

			#region Update track position and handle physics
			int oldTrackSegmentNumber = trackSegmentNumber;
			// Find out where we currently are on the track.
			RacingGameManager.Landscape.UpdateCarTrackPosition(
				carPos, //unused: carDir, carUp,
				ref trackSegmentNumber, ref trackSegmentPercent);
			// Was the track segment changed?
			if (trackSegmentNumber != oldTrackSegmentNumber &&
				// And we in game?
				RacingGameManager.InGame)
			{
				// Was this the start? Did we finish a lap?
				if (trackSegmentNumber == 0 &&
					// Ignore if we missed one checkpoint.
					RacingGameManager.Landscape.NewReplay.CheckpointTimes.Count >=
					RacingGameManager.Landscape.CheckpointSegmentPositions.Count-1)
				{
					// Show time we made for this lap
					BaseGame.UI.AddTimeFadeupEffect((int)GameTimeMilliseconds,
						UIRenderer.TimeFadeupMode.Normal);

					// We finished this lap, start next
					StartNewLap();
				} // if
				else
				{
					// Always only check for the next checkpoint
					int num =
						RacingGameManager.Landscape.NewReplay.CheckpointTimes.Count;
					if (num <
						RacingGameManager.Landscape.CheckpointSegmentPositions.Count &&
						RacingGameManager.Landscape.CheckpointSegmentPositions[num] >
						oldTrackSegmentNumber &&
						RacingGameManager.Landscape.CheckpointSegmentPositions[num] <=
						trackSegmentNumber)
					{
						// We passed that checkpoint, show time
						// Show improvements of time stored in best replay.
						int differenceMs =
							RacingGameManager.Landscape.CompareCheckpointTime(num);

						if (differenceMs < 0)
							Sound.Play(Sound.Sounds.CheckpointBetter);
						else
							Sound.Play(Sound.Sounds.CheckpointWorse);

						BaseGame.UI.AddTimeFadeupEffect(
							//normal: (int)GameTimeMilliseconds,
							Math.Abs(differenceMs),
							differenceMs < 0 ? UIRenderer.TimeFadeupMode.Minus :
							UIRenderer.TimeFadeupMode.Plus);

						// Add this checkpoint time to the current replay
						RacingGameManager.Landscape.NewReplay.CheckpointTimes.Add(
							RacingGameManager.Player.GameTimeMilliseconds / 1000.0f);
					} // if
				} // else
			} // if

			// And get the TrackMatrix and track values at this location.
			float roadWidth, nextRoadWidth;
			Matrix trackMatrix =
				RacingGameManager.Landscape.GetTrackPositionMatrix(
				//RacingGameManager.Player.CarTrackPosition,
				trackSegmentNumber, trackSegmentPercent,
				out roadWidth, out nextRoadWidth);

			// If we are coming from the TestCarPhysicsOnPlane unit test
			// don't use the track, just use the simple plane!
			if (BaseGame.WindowsTitle == "TestCarPhysicsOnPlane" ||
				BaseGame.WindowsTitle == "TestCarPhysicsOnPlaneWithGuardRails")
			{
				trackMatrix =
					Matrix.CreateRotationX(MathHelper.Pi / 2.0f) *
					Matrix.CreateRotationZ(MathHelper.Pi);
			} // if

			// Just set car up from trackMatrix, this should be done
			// better with a more accurate gravity model (see gravity calculation!)
			Vector3 remOldRightVec = CarRight;
			carUp = trackMatrix.Up;
			carDir = Vector3.Cross(carUp, remOldRightVec);

			// Set up the ground and guardrail boundings for the physics calculation.
			Vector3 trackPos = trackMatrix.Translation;
			RacingGameManager.Player.SetGroundPlaneAndGuardRails(
				trackPos, trackMatrix.Up,
				// Construct our guardrail positions for the collision testing
				trackPos - trackMatrix.Right *
				(roadWidth / 2 - GuardRail.InsideRoadDistance / 2),
				trackPos - trackMatrix.Right *
				(roadWidth / 2 - GuardRail.InsideRoadDistance / 2) +
				trackMatrix.Forward,
				trackPos + trackMatrix.Right *
				(nextRoadWidth / 2 - GuardRail.InsideRoadDistance / 2),
				trackPos + trackMatrix.Right *
				(nextRoadWidth / 2 - GuardRail.InsideRoadDistance / 2) +
				trackMatrix.Forward);
			carRenderMatrix = RacingGameManager.Player.UpdateCarMatrixAndCamera();

			// Finally check for collisions with the guard rails.
			// Also handle gravity.
			ApplyGravityAndCheckForCollisions();
			#endregion
		} // Update()
		#endregion

		#region CheckForCollisions
		/// <summary>
		/// Current gravity speed, increases as we fly around ^^
		/// </summary>
		//float gravitySpeed = 0.0f;
		/// <summary>
		/// Apply gravity to our car in case any of our wheels is in the air.
		/// Check for collisions, we only have the road and the guard rails
		/// as colision objects for this game. This way we can simplify
		/// the physics quite a lot. Usually it would be much better to have
		/// a fullblown physics engine, but thats a lot of work and goes beyond
		/// this starter kit game :)
		/// </summary>
		public void ApplyGravityAndCheckForCollisions()
		{
			// Don't do it in the menu
			if (RacingGameManager.InMenu)
				return;

			// Calc normals for the guard rail with help of the next guard rail
			// position and the ground normal.
			Vector3 guardrailLeftVec =
				Vector3.Normalize(nextGuardrailLeft - guardrailLeft);
			Vector3 guardrailRightVec =
				Vector3.Normalize(nextGuardrailRight - guardrailRight);
			Vector3 guardrailLeftNormal =
				Vector3.Cross(guardrailLeftVec, groundPlaneNormal);
			Vector3 guardrailRightNormal =
				Vector3.Cross(groundPlaneNormal, guardrailRightVec);
			float roadWidth = (guardrailLeft - guardrailRight).Length();

			// Calculate position we will have NEXT frame!
			float moveFactor = BaseGame.MoveFactorPerSecond;
			Vector3 pos = carPos;//old:  + speed * carDir * moveFactor;

			// Check all 4 corner points of our car.
			Vector3 carRight = Vector3.Cross(carDir, carUp);
			Vector3 carLeft = -carRight;
			// Car dimensions are 2.6m (width) x 5.6m (length) x 1.8m (height)
			// Note: This could be improved by using more points or using
			// the actual car geometry.
			// Note: We ignore the height, this way the collision is simpler.
			// We then check the height above the road to see if we are flying
			// above the guard rails out into the landscape.
			Vector3[] carCorners = new Vector3[]
			{
				// Top left
				pos + carDir * 5.6f/2.0f - carRight * 2.6f/2.0f,
				// Top right
				pos + carDir * 5.6f/2.0f + carRight * 2.6f/2.0f,
				// Bottom right
				pos - carDir * 5.6f/2.0f + carRight * 2.6f/2.0f,
				// Bottom left
				pos - carDir * 5.6f/2.0f - carRight * 2.6f/2.0f,
			};

			//float applyGravity = 0;

			// Check for each corner if we collide with the guard rail
			for (int num = 0; num < carCorners.Length; num++)
			{
				#region Apply gravity
				/*obs
				// Apply gravity if we are flying, do this for each wheel.
				if (carCorners[num].Z > groundPlanePos.Z)
					applyGravity += Gravity / 4;
				 */
				#endregion

				#region Hit guardrail
				// Hit any guardrail?
				float leftDist = Vector3Helper.DistanceToLine(
					carCorners[num], guardrailLeft, nextGuardrailLeft);
				float rightDist = Vector3Helper.DistanceToLine(
					carCorners[num], guardrailRight, nextGuardrailRight);

				// If we are closer than 0.1f, thats a collision!
				//TODO: ignore if we are too high (higher than guardrail).
				if (leftDist < 0.1f ||
					// Also include the case where we are father away from rightDist
					// than the road is width.
					rightDist > roadWidth)
				{
					// Force car back on the road, for that calculate impulse and
					// collision direction (same stuff as in Rocket Commander).
					Vector3 collisionDir =
						Vector3.Reflect(carDir, guardrailLeftNormal);
					//float impulse = speed * //CarMass *
					//	CollisionImpulseFactor;

					float collisionAngle =
						Vector3Helper.GetAngleBetweenVectors(
						carRight, guardrailLeftNormal);
					// Flip at 180 degrees (if driving in wrong direction)
					if (collisionAngle > MathHelper.Pi / 2)
						collisionAngle -= MathHelper.Pi;
					// Just correct rotation if 0-45 degrees (slowly)
					if (Math.Abs(collisionAngle) < MathHelper.Pi / 4.0f)
					{
						// Play crash sound
						Sound.PlayCrashSound(false);

						// For front wheels to full collision rotation, for back half!
						if (num < 2)
						{
							rotateCarAfterCollision = -collisionAngle / 1.5f;
							//slowdownCarAfterCollision = 0.6f;
							speed *= 0.93f;//0.85f;
							if (viewDistance > 0.75f)
								viewDistance -= 0.1f;//0.15f;
						} // if (num)
						else
						{
							rotateCarAfterCollision = -collisionAngle / 2.5f;
							//slowdownCarAfterCollision = 0.8f;
							speed *= 0.96f;//0.9f;
							if (viewDistance > 0.75f)
								viewDistance -= 0.05f;//0.1f;
						} // else
						ChaseCamera.SetCameraWobbel(0.00075f * speed);
					} // if (collisionAngle)

					// If 90-45 degrees (in either direction), make frontal crash
					// + stop car + wobble camera
					else if (Math.Abs(collisionAngle) < MathHelper.Pi * 3.0f / 4.0f)
					{
						// Also rotate car if less than 60 degrees
						if (Math.Abs(collisionAngle) < MathHelper.Pi / 3.0f)
							rotateCarAfterCollision = +collisionAngle / 3.0f;

						// Play crash sound
						Sound.PlayCrashSound(true);

						// Shake camera
						ChaseCamera.SetCameraWobbel(0.005f * speed);

						// Just stop car!
						speed = 0;
					} // if (collisionAngle)

					// For all collisions, kill the current car force
					carForce = Vector3.Zero;

					// Always make sure we are OUTSIDE of the collision range for
					// the next frame. But first find out how much we have to move.
					float speedDistanceToGuardrails =
						speed * Math.Abs(Vector3.Dot(carDir, guardrailLeftNormal));

					if (leftDist > 0)
					{
						float correctCarPosValue = (leftDist + 0.01f +//0.11f +
							0.1f * speedDistanceToGuardrails * moveFactor);
						carPos += correctCarPosValue * guardrailLeftNormal;
					} // if (rightDist)
				} // if (leftDist)

				if (rightDist < 0.1f ||
					// Also include the case where we are father away from rightDist
					// than the road is width.
					leftDist > roadWidth)
				{
					// Force car back on the road, for that calculate impulse and
					// collision direction (same stuff as in Rocket Commander).
					Vector3 collisionDir =
						Vector3.Reflect(carDir, guardrailRightNormal);

					float collisionAngle =
						Vector3Helper.GetAngleBetweenVectors(
						carLeft, guardrailRightNormal);
					// Flip at 180 degrees (if driving in wrong direction)
					if (collisionAngle > MathHelper.Pi / 2)
						collisionAngle -= MathHelper.Pi;
					// Just correct rotation if 0-45 degrees (slowly)
					if (Math.Abs(collisionAngle) < MathHelper.Pi / 4.0f)
					{
						// Play crash sound
						Sound.PlayCrashSound(false);

						// For front wheels to full collision rotation, for back half!
						if (num < 2)
						{
							rotateCarAfterCollision = +collisionAngle / 1.5f;
							//slowdownCarAfterCollision = 0.6f;
							speed *= 0.935f;//0.85f;
							if (viewDistance > 0.75f)
								viewDistance -= 0.1f;//0.15f;
						} // if (num)
						else
						{
							rotateCarAfterCollision = +collisionAngle / 2.5f;
							//slowdownCarAfterCollision = 0.8f;
							speed *= 0.96f;//0.9f;
							if (viewDistance > 0.75f)
								viewDistance -= 0.05f;//0.1f;
						} // else
						ChaseCamera.SetCameraWobbel(0.00075f * speed);
					} // if (collisionAngle)

					// If 90-45 degrees (in either direction), make frontal crash
					// + stop car + wobble camera
					else if (Math.Abs(collisionAngle) < MathHelper.Pi * 3.0f / 4.0f)
					{
						// Also rotate car if less than 60 degrees
						if (Math.Abs(collisionAngle) < MathHelper.Pi / 3.0f)
							rotateCarAfterCollision = +collisionAngle / 3.0f;

						// Play crash sound
						Sound.PlayCrashSound(true);

						// Shake camera
						ChaseCamera.SetCameraWobbel(0.005f * speed);

						// Just stop car!
						speed = 0;
					} // if (collisionAngle)

					// For all collisions, kill the current car force
					carForce = Vector3.Zero;

					// Always make sure we are OUTSIDE of the collision range for
					// the next frame. But first find out how much we have to move.
					float speedDistanceToGuardrails =
						speed * Math.Abs(Vector3.Dot(carDir, guardrailLeftNormal));

					if (rightDist > 0)
					{
						float correctCarPosValue = (rightDist + 0.01f +//0.11f +
							0.1f * speedDistanceToGuardrails * moveFactor);
						carPos += correctCarPosValue * guardrailRightNormal;
					} // if (rightDist)
				} // if (rightDist)
				#endregion
			} // for (num)

			ApplyGravity();
		} // CheckForCollisions(guardrailLeft, nextGuardrailLeft, guardrailRight)

		/// <summary>
		/// Apply gravity
		/// </summary>
		private void ApplyGravity()
		{
			float moveFactor = BaseGame.MoveFactorPerSecond;

			/*obs
			// Fix car on ground
			float distFromGround = Vector3Helper.SignedDistanceToPlane(
				carPos,
				// Substract a little to let car be more on ground and not fly around.
				groundPlanePos-groundPlaneNormal * new Vector3(0, 0, 0.15f),
				groundPlaneNormal);
			float origDist = distFromGround;
			*/

			// Always reset car on ground!
			float trackDistance = Vector3Helper.SignedDistanceToPlane(
				carPos,
				groundPlanePos - groundPlaneNormal * 0.025f, // 0.15f,
				groundPlaneNormal);
			carPos += groundPlaneNormal * trackDistance;

			/*
			isCarOnGround = distFromGround > -0.5f;//-0.1f;
			// Use very hard and instant gravity to fix if car is below ground!
			float maxGravity = Gravity * moveFactor;// *2;// ;// *2;
			// Use more smooth gravity for jumping
			// (Needs tweaking! see formula above)
			float minGravity = -Gravity * moveFactor;// / 2;// / 4;// / 2.5f;
			if (distFromGround > maxGravity)
			{
				distFromGround = maxGravity;
				gravitySpeed = 0;
			} // if (distFromGround)
			
			if (distFromGround < minGravity)
			{
				distFromGround = minGravity;
				gravitySpeed -= distFromGround;
			} // if (distFromGround)

			carPos.Z += distFromGround;
			 */

			//tst: always put the car directly on the road, this will fix any
			// flying and looping errors, but will not allow us to fly out of
			// the track (e.g. when driving too fast into a curve).
			isCarOnGround = true;
			//obs: gravitySpeed = 0;

			/*tst
			// Loopings are currently buggy, fix by putting car directly road!
			// Find out if this is a looping
			bool upsideDown = carUp.Z < +0.05f;
			bool movingUp = carDir.Z > 0.65f;
			bool movingDown = carDir.Z < -0.65f;
			if (upsideDown || movingUp || movingDown)
			{
				carPos.Z = (carPos.Z * 3.0f + groundPlanePos.Z) / 4.0f;
			} // if

			/*tst
			BaseGame.DrawLine(carPos,
				carPos + carDir * 5.0f, Color.Red);
			BaseGame.DrawLine(groundPlanePos,
				groundPlanePos + groundPlaneNormal * 5.0f, Color.Green);
			 */
		} // ApplyGravity()
		#endregion

		#region SetGuardRails
		/// <summary>
		/// Ground plane and guardrail positions.
		/// We update this every frame!
		/// </summary>
		protected Vector3 groundPlanePos, groundPlaneNormal,
			guardrailLeft, nextGuardrailLeft,
			guardrailRight, nextGuardrailRight;

		/// <summary>
		/// Set guard rails. We only calculate collisions to the current left
		/// and right guard rails, not with the complete level!
		/// </summary>
		/// <param name="setGroundPlanePos">Set ground plane position</param>
		/// <param name="setGroundPlaneNormal">Set ground plane normal</param>
		/// <param name="setGuardrailLeft">Set guardrail left</param>
		/// <param name="setNextGuardrailLeft">Set next guardrail left</param>
		/// <param name="setGuardrailRight">Set guardrail right</param>
		/// <param name="setNextGuardrailRight">Set next guardrail right</param>
		public void SetGroundPlaneAndGuardRails(
			Vector3 setGroundPlanePos, Vector3 setGroundPlaneNormal,
			Vector3 setGuardrailLeft, Vector3 setNextGuardrailLeft,
			Vector3 setGuardrailRight, Vector3 setNextGuardrailRight)
		{
			groundPlanePos = setGroundPlanePos;
			groundPlaneNormal = setGroundPlaneNormal;
			guardrailLeft = setGuardrailLeft;
			nextGuardrailLeft = setNextGuardrailLeft;
			guardrailRight = setGuardrailRight;
			nextGuardrailRight = setNextGuardrailRight;
		} // SetGroundPlaneAndGuardRails(setGroundPlanePos, setGroundPlaneNormal)
		#endregion

		#region UpdateCarMatrixAndCamera
		/// <summary>
		/// Update car matrix and camera
		/// </summary>
		public Matrix UpdateCarMatrixAndCamera()
		{
			// Get car matrix with help of the current car position, dir and up
			Matrix carMatrix = Matrix.Identity;
			carMatrix.Right = CarRight;
			carMatrix.Up = carUp;
			carMatrix.Forward = carDir;
			carMatrix.Translation = carPos;

			// Change distance based on our speed
			float chaseCamDistance =
				(4.25f + 9.75f * speed / maxSpeed) * viewDistance;
			if (RacingGameManager.InMenu == false &&
				ZoomInTime > 1500)
			{
				// Calculate zooming in camera position
				Vector3 camPos =
					carPos + CarHeightVector +
					carMatrix.Forward * (chaseCamDistance +
					((ZoomInTime-1500)/(float)StartGameZoomTimeMilliseconds) * 250.0f) -
					carMatrix.Up * (0.6f +
					((ZoomInTime-1500)/(float)StartGameZoomTimeMilliseconds) * 200.0f);

				// Make sure we don't interpolate at the first time
				if (ZoomInTime-BaseGame.ElapsedTimeThisFrameInMilliseconds >= 3000)
					RacingGameManager.Player.SetCameraPosition(camPos);
				else
					RacingGameManager.Player.InterpolateCameraPosition(camPos);
			} // if (RacingGameManager.InMenu)
			else if (RacingGameManager.Player.FreeCamera)
				RacingGameManager.Player.InterpolateCameraPosition(
					carPos + CarHeightVector +
					carMatrix.Forward * chaseCamDistance -
					carMatrix.Up * chaseCamDistance / (viewDistance+6.0f) -
					carMatrix.Up * 1.0f);
			else if (RacingGameManager.InMenu &&
				BaseGame.TotalTimeMilliseconds < 100)
				// No interpolation in menu, just set it (at least for the first ms)
				RacingGameManager.Player.SetCameraPosition(
					carPos + CarHeightVector +
					carMatrix.Forward * chaseCamDistance -
					//carMatrix.Up * chaseCamDistance / (viewDistance+6.0f) -
					carMatrix.Up * 0.6f);
			else
				RacingGameManager.Player.InterpolateCameraPosition(
					carPos + CarHeightVector +
					carMatrix.Forward * chaseCamDistance / 1.125f -
					//carMatrix.Up * chaseCamDistance / (viewDistance+6.0f) -
					carMatrix.Up * 0.8f);//0.6f);

			// Save this carMatrix into the current replay every time the
			// replay interval passes.
			if (RacingGameManager.Player.GameTimeMilliseconds >
				RacingGameManager.Landscape.NewReplay.NumberOfTrackMatrices *
				Replay.TrackMatrixIntervals * 1000.0f)
				//first try, sucks, too much entries for some reason:
				//BaseGame.EveryMillisecond((int)(Replay.TrackMatrixIntervals * 1000)))
				RacingGameManager.Landscape.NewReplay.AddCarMatrix(carMatrix);

			// For rendering rotate car to stay correctly on the road
			carMatrix =
				Matrix.CreateRotationX(MathHelper.Pi / 2.0f -
				carPitchPhysics.pos / 60) *// /75)* //192) * // /250)*
				Matrix.CreateRotationZ(MathHelper.Pi) *
				carMatrix;

			return carMatrix;
		} // UpdateCarMatrixAndCamera()
		#endregion

		#region Unit Testing
#if DEBUG
		#region DisplayPhysicsValuesAndHelp
		/// <summary>
		/// Display physics values and help
		/// </summary>
		public void DisplayPhysicsValuesAndHelp()
        {
            /*
			// Calc normals for the guard rail with help of the next guard rail
			// position and the ground normal.
			Vector3 guardrailLeftVec =
				Vector3.Normalize(nextGuardrailLeft - guardrailLeft);
			Vector3 guardrailRightVec =
				Vector3.Normalize(nextGuardrailRight - guardrailRight);
			Vector3 guardrailLeftNormal =
				Vector3.Cross(guardrailLeftVec, groundPlaneNormal);
			Vector3 guardrailRightNormal =
				Vector3.Cross(groundPlaneNormal, guardrailRightVec);

			// Display current ground and guardrail planes
			new PlaneRenderer(
				groundPlanePos, new Plane(groundPlaneNormal, 0),
				RacingGameManager.Landscape.CityMaterial, 10).Render();
			new PlaneRenderer(
				guardrailLeft, new Plane(guardrailLeftNormal, 0),
				RacingGameManager.Landscape.CityMaterial, 10).Render();
			new PlaneRenderer(
				guardrailRight, new Plane(guardrailRightNormal, 0),
				RacingGameManager.Landscape.CityMaterial, 10).Render();
             */

			// Display physics values and help
			TextureFont.WriteText(2, 30,
				"Cursor: Move and Rotate Car; Space: Brake; Mouse: View");
			TextureFont.WriteText(2, 60,
				"Car position: " + carPos);
			TextureFont.WriteText(2, 90,
				"Camera pos: " + RacingGameManager.CameraPos);
			TextureFont.WriteText(2, 120,
				"Speed: " + speed * MeterPerSecToMph);
			TextureFont.WriteText(2, 150,
				"Force: " + carForce);
			TextureFont.WriteText(2, 180,
				"Car dir: " + carDir);
			TextureFont.WriteText(2, 210,
				"Car up: " + carUp);
			TextureFont.WriteText(2, 240,
				"rotateCarAfterCollision: " + rotateCarAfterCollision);
			TextureFont.WriteText(2, 270,
				"isCarOnGround: " + isCarOnGround);
			TextureFont.WriteText(2, 300,
				"groundPlanePos: " + groundPlanePos);
			TextureFont.WriteText(2, 330,
				"guardrailLeft: " + guardrailLeft);
			TextureFont.WriteText(2, 360,
				"guardrailRight: " + guardrailRight);
			TextureFont.WriteText(2, 390,
				"viewDistance: " + viewDistance);
			/*tst
			TextureFont.WriteText(2, 420,
				"wannaCameraLookVector: " + RacingGameManager.Player.wannaCameraLookVector);
			TextureFont.WriteText(2, 450,
				"wannaCameraDistance: " + RacingGameManager.Player.wannaCameraDistance);
			 */
		} // DisplayPhysicsValuesAndHelp()
		#endregion

		#region Test car physics on plane
		/// <summary>
		/// Test car physics on a plane (xy) for testing the car controlling.
		/// </summary>
		static public void TestCarPhysicsOnPlane()
		{
			PlaneRenderer plane = null;

			TestGame.Start("TestCarPhysicsOnPlane",
				delegate
				{
					plane = new PlaneRenderer(Vector3.Zero,
						new Plane(new Vector3(0, 0, 1), 0),
						new Material("CityGround", "CityGroundNormal"), 500.0f);

					// Put car on the ground and use standard direction and up vectors
					RacingGameManager.Player.SetCarPosition(
						new Vector3(0, 0, 10),
						new Vector3(0, 1, 0),
						new Vector3(0, 0, 1));

					// Make sure we are not in free camera mode and can control the car
					RacingGameManager.Player.FreeCamera = false;
					RacingGameManager.Player.ZoomInTime = 0;
					RacingGameManager.Player.SetCameraPosition(new Vector3(0, -5, 8));
				},
				delegate
				{
					// Test slow computers by slowing down the framerate with Ctrl
					if (Input.Keyboard.IsKeyDown(Keys.LeftControl))
						Thread.Sleep(75);

					RacingGameManager.Player.SetGroundPlaneAndGuardRails(
						new Vector3(0, 0, 0), new Vector3(0, 0, 1),
						// Use our guardrails, always the same in this test!
						new Vector3(-10, 0, 0), new Vector3(-10, 10, 0),
						new Vector3(+10, 0, 0), new Vector3(+10, 10, 0));
					Matrix carMatrix = RacingGameManager.Player.UpdateCarMatrixAndCamera();

					// Generate shadows, just the car does shadows
					ShaderEffect.shadowMapping.GenerateShadows(
						delegate
						{
							RacingGameManager.CarModel.GenerateShadow(carMatrix);
						});
					// Render shadows (on both the plane and the car)
					ShaderEffect.shadowMapping.RenderShadows(
						delegate
						{
							RacingGameManager.CarModel.UseShadow(carMatrix);
							plane.UseShadow();
						});

					BaseGame.UI.RenderGameBackground();
					// Show car and ground plane
					RacingGameManager.CarModel.RenderCar(
						1, Color.White, false, carMatrix);
					plane.Render();

					// Just add brake tracks (we don't render the landscape here)
					RacingGameManager.Landscape.RenderBrakeTracks();
					BaseGame.MeshRenderManager.Render();

					// Add shadows
					ShaderEffect.shadowMapping.ShowShadows();

					RacingGameManager.Player.DisplayPhysicsValuesAndHelp();
				});
		} // TestCarPhysicsOnPlaneWithGuardRails()
		#endregion

		#region Test car physics on plane with guard rails
		/// <summary>
		/// Test car physics on a plane (xy) with some dummy guard rail for
		/// simple colision checking.
		/// </summary>
		static public void TestCarPhysicsOnPlaneWithGuardRails()
		{
			PlaneRenderer plane = null;
			// Use a simple object to simulate our guard rails we have in the game.
			Model guardRailModel = null;

			TestGame.Start("TestCarPhysicsOnPlaneWithGuardRails",
				delegate
				{
					plane = new PlaneRenderer(Vector3.Zero,
						new Plane(new Vector3(0, 0, 1), 0),
						new Material("CityGround", "CityGroundNormal"), 500.0f);
						//new Material("Road", "RoadNormal"), 500.0f);
					guardRailModel = new Model("RoadColumnSegment");

					// Put car 10m above the ground to test gravity and ground plane!
					RacingGameManager.Player.SetCarPosition(
						new Vector3(0, 0, 10),
						new Vector3(0, 1, 0),
						new Vector3(0, 0, 1));

					// Make sure we are not in free camera mode and can control the car
					RacingGameManager.Player.FreeCamera = false;
				},
				delegate
				{
					// Test slow computers by slowing down the framerate with Ctrl
					if (Input.Keyboard.IsKeyDown(Keys.LeftControl))
						Thread.Sleep(75);

					RacingGameManager.Player.SetGroundPlaneAndGuardRails(
						new Vector3(0, 0, 0), new Vector3(0, 0, 1),
						// Use our guardrails, always the same in this test!
						new Vector3(-10, 0, 0), new Vector3(-10, 10, 0),
						new Vector3(+10, 0, 0), new Vector3(+10, 10, 0));
					Matrix carMatrix = RacingGameManager.Player.UpdateCarMatrixAndCamera();

					// Generate shadows, just the car does shadows
					ShaderEffect.shadowMapping.GenerateShadows(
						delegate
						{
							RacingGameManager.CarModel.GenerateShadow(carMatrix);
						});
					// Render shadows (on both the plane and the car)
					ShaderEffect.shadowMapping.RenderShadows(
						delegate
						{
							RacingGameManager.CarModel.UseShadow(carMatrix);
							plane.UseShadow();
						});

					BaseGame.UI.RenderGameBackground();
					// Show car and ground plane
					RacingGameManager.CarModel.RenderCar(
						0, Color.White, false, carMatrix);
					plane.Render();

					// Just add brake tracks (we don't render the landscape here)
					RacingGameManager.Landscape.RenderBrakeTracks();

          guardRailModel.Render(new Vector3(-11.5f, 2.5f, 0));

          /*
					for (int num = -100; num < 100; num++)
					{
						// Add 1.5m for the size of our dummy guard rail object.
						guardRailModel.Render(new Vector3(-11.5f, num * 2.5f, 0));
						guardRailModel.Render(new Vector3(+11.5f, num * 2.5f, 0));
					} // for (num)
					BaseGame.DrawLine(
						new Vector3(-10, -1000, 0.1f),
						new Vector3(-10, +1000, 0.1f));
					BaseGame.DrawLine(
						new Vector3(+10, -1000, 0.1f),
						new Vector3(+10, +1000, 0.1f));
           */
					BaseGame.MeshRenderManager.Render();

					// Add shadows
					ShaderEffect.shadowMapping.ShowShadows();

					RacingGameManager.Player.DisplayPhysicsValuesAndHelp();
				});
		} // TestCarPhysicsOnPlaneWithGuardRails()
		#endregion
#endif
		#endregion
	} // class CarController
} // namespace RacingGame.GameLogic
