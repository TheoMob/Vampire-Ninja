using UnityEngine;

namespace TarodevController
{
    [CreateAssetMenu]
    public class ScriptableStats : ScriptableObject
    {
        [Header("LAYERS")] [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;

        [Header("INPUT")] [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
        public bool SnapInput = true;

        [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float GroundDeceleration = 60;

        [Tooltip("Deceleration in air only after stopping input mid-air")]
        public float AirDeceleration = 30;

        [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        [Header("JUMP")] [Tooltip("The immediate velocity applied when jumping")]
        public float JumpPower = 36;

        [Tooltip("The maximum vertical movement speed")]
        public float MaxFallSpeed = 40;

        [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
        public float FallAcceleration = 110;

        [Tooltip("The gravity multiplier added when jump is released early")]
        public float JumpEndEarlyGravityModifier = 3;

        [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public float CoyoteTime = .15f;

        [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
        public float JumpBuffer = .2f;

        // my own edits
        [Header("DASH")] [Tooltip("The immediate velocity applied when dashing")]
        public float DashSpeed = 20;

        [Tooltip("Dash Speed on the Y Axis")]
        public float DashVerticalSpeed = 20;

        [Tooltip("The duration of the dash in seconds")]
        public float DashDuration = .6f;

        [Tooltip("Time between te usage of one dash and the other")]
        public float DashCooldown = 1.5f;

        [Tooltip("Max Time a Dash can be stored before being released")]
        public float DashBuffer = 0.5f;

        [Tooltip("Time between pressing the dash and actually dashing, helps with input buffering")]
        public float DashInputDelay = 0.2f;

        [Tooltip("Time between the end of the dash and the return of the gravity")]
        public float DashGravityReturnDelay = 0.15f;

        [Tooltip("Slow on time after the dash")]
        public float DashSlowOnTime = 0.9f;

        [Tooltip("Dash Shake Intensity on the screen")]
        public float DashShakeIntensity = 0.9f;

        [Tooltip("Dash Shake Duration on the screen")]
        public float DashShakeDuration = 0.9f;

        [Header("WALLSLIDE")] [Tooltip("The Speed that the character slips from the wall")]
        public float WallSlidingSpeed = 1f;

        [Tooltip("Distance that the player needs to be from a wall to count as wallSliding")]
        public float WallSlideDistance = 0.2f;

        [Tooltip("Strengh of the wallJump in the X and Y axis")]
        public Vector2 WallJumpPower = new Vector2(0.2f, 0.2f);

        [Tooltip("Wall Jump Duration")]
        public float WallJumpDuration = 1f;

        [Tooltip("Time between letting go of the wall and still being able to wallJump")]
        public float WallJumpBuffer = 1f;

    }
}