using UnityEngine;
using UnityEngine.InputSystem;

namespace HellSlice
{
    public sealed class SliceInput : System.IDisposable
    {
        readonly InputActionAsset actions;
        readonly InputAction move, look, jump, attack, dodge, interact, slow, pause;
        public SliceInput()
        {
            // The JSON TextAsset is owned by this project, independent of template project-wide actions.
            var json = Resources.Load<TextAsset>("SliceControls");
            if (json == null) throw new System.InvalidOperationException("Missing Resources/SliceControls.json");
            actions = InputActionAsset.FromJson(json.text);
            move = actions.FindAction("Move", true); look = actions.FindAction("Look", true);
            jump = actions.FindAction("Jump", true); attack = actions.FindAction("Attack", true);
            dodge = actions.FindAction("Dodge", true); interact = actions.FindAction("Interact", true);
            slow = actions.FindAction("Slow", true);
            pause = actions.FindAction("Pause", true); actions.Enable();
        }
        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 Look => look.ReadValue<Vector2>();
        public bool Jump => jump.WasPressedThisFrame();
        public bool Attack => attack.WasPressedThisFrame();
        public bool Dodge => dodge.WasPressedThisFrame();
        public bool Interact => interact.WasPressedThisFrame();
        public bool Slow => slow.WasPressedThisFrame();
        public bool Pause => pause.WasPressedThisFrame();
        public void Dispose() { actions.Disable(); Object.Destroy(actions); }
    }
}

