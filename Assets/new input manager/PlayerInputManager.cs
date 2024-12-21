using MovementStatsController;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    private PlayerInputActions _inputActions; // list of controls available to configuration to the player
    public static PlayerFrameInput FrameInput;
    [SerializeField] private ScriptableStats _stats;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();
    }
    void Update()
    {
        FrameInput = new PlayerFrameInput
        {
            Move = _inputActions.Player.Move.ReadValue<Vector2>(),
            JumpDown = _inputActions.Player.Jump.WasPressedThisFrame(),
            JumpHeld = _inputActions.Player.Jump.IsPressed(),
            DashPressed = _inputActions.Player.Dash.WasPressedThisFrame(),
        };

        //Debug.Log("MoveY" + FrameInput.Move.y);

        if (_stats.SnapInput)
        {
            FrameInput.Move.x = Mathf.Abs(FrameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(FrameInput.Move.x);
            FrameInput.Move.y = Mathf.Abs(FrameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(FrameInput.Move.y);
        }
    }

   public struct PlayerFrameInput
    {
      public Vector2 Move;
      public bool JumpDown;
      public bool JumpHeld;
      public bool DashPressed;
    }
}
