using UnityEngine;
using System;
using InputModule.Configs;

namespace InputModule
{
    public class GameInputService : IInputMap, IInputService, IDisposable
    {
        private readonly InputSystem_Actions _actions;
        private bool _isBuildMode;

        public bool IsBuildMode => _isBuildMode;

        public GameInputService()
        {
            _actions = new();
            _actions.Player.Enable();
            _actions.Global.Enable();
            _actions.Build.Enable();
        }
        
        public void SetUIMode(bool isActive)
        {
            if (isActive)
            {
                _actions.Player.Disable();
                _actions.Build.Disable();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _actions.Player.Enable();
                _actions.Build.Enable();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        public void SetBuildMode(bool isActive)
        {
            _isBuildMode = isActive;

            if (isActive)
            {
                _actions.Player.Disable();
                _actions.Global.Disable();
                _actions.Build.Enable();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                _actions.Player.Enable();
                _actions.Global.Enable();
                _actions.Build.Disable();

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        public Vector2 MoveInput => _actions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _actions.Player.Look.ReadValue<Vector2>();
        public bool IsJumpPressed => _actions.Player.Jump.WasPressedThisFrame();
        public bool IsCrouchPressed => _actions.Player.Crouch.IsPressed();
        public float LeanInput => _actions.Player.Lean.ReadValue<float>();
        public bool IsInteractPressed => _actions.Player.Interact.WasPressedThisFrame();
        
        public bool IsFirePressed => _actions.Player.Attack.WasPressedThisFrame();
        public bool IsFireHeld => _actions.Player.Attack.IsPressed();
        public bool IsAimPressed => _actions.Player.Aim.IsPressed();
        public bool IsAimHeld => _actions.Player.Aim.IsPressed();
        public bool IsAimTriggered => _actions.Player.Aim.WasPressedThisFrame();
        public bool IsReloadPressed => _actions.Player.Reload.WasPressedThisFrame();
        public bool IsHolsterPressed => _actions.Player.Holster.WasPressedThisFrame();
        
        public bool IsInventoryPressed => _actions.Global.ToggleInventory.WasPressedThisFrame();
        public bool IsRotatePressed => _actions.Global.RotateItem.WasPressedThisFrame();
        public bool IsSplitPressed => _actions.Global.SplitStack.IsPressed();
        public bool IsPausePressed => _actions.Global.Escape.WasPressedThisFrame();
        
        public bool IsBuildPressed => _actions.Player.Build.WasPressedThisFrame() ||
                                _actions.Build.Build.WasPressedThisFrame();

        public Vector2 BuildMoveInput => _actions.Build.Move.ReadValue<Vector2>();
        public Vector2 BuildLookInput => _actions.Build.Look.ReadValue<Vector2>();
        public bool IsBuildJumpPressed => _actions.Build.Jump.WasPressedThisFrame();
        public bool IsBuildPlacePressed => _actions.Build.Place.WasPressedThisFrame();
        public bool IsBuildCancelPressed => _actions.Build.Cancel.WasPressedThisFrame();
        public bool IsBuildRotateLeftPressed => _actions.Build.RotateLeft.IsPressed();
        public bool IsBuildRotateRightPressed => _actions.Build.RotateRight.IsPressed();
        public bool IsBuildSelectLeftPressed => _actions.Build.SelectLeft.WasPressedThisFrame();
        public bool IsBuildSelectRightPressed => _actions.Build.SelectRight.WasPressedThisFrame();
        public bool IsBuildCategoryUpPressed => _actions.Build.CategoryUp.WasPressedThisFrame();
        public bool IsBuildCategoryDownPressed => _actions.Build.CategoryDown.WasPressedThisFrame();
        public float BuildScroll
        {
            get
            {
                float scrollValue = _actions.Build.Scroll.ReadValue<float>();
                if (scrollValue > 0) return 1f;
                if (scrollValue < 0) return -1f;
                return 0f;
            }
        }

        public int SelectWeaponIndex
        {
            get
            {
                if (_actions.Player.Weapon1.WasPressedThisFrame()) return 0;
                if (_actions.Player.Weapon2.WasPressedThisFrame()) return 1;
                if (_actions.Player.Weapon3.WasPressedThisFrame()) return 2;

                return -1;
            }
        }
        
        public float WeaponScroll
        {
            get
            {
                float scrollValue = _actions.Player.WeaponScroll.ReadValue<float>();

                if (scrollValue > 0) return 1f;

                if (scrollValue < 0) return -1f;

                return 0f;
            }
        }
        
        public void Enable() => _actions.Enable();
        public void Disable() => _actions.Disable();

        public void Reset()
        {
            _actions.Player.Enable();
            _actions.Global.Enable();
            _actions.Build.Disable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Dispose()
        {
            _actions.Player.Disable();
            _actions.Global.Disable();
            _actions.Build.Disable();
            _actions.Dispose();
        }
    }
}