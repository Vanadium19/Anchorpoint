using System;
using UnityEngine;
using InputModule.Configs;

namespace InputModule.Core
{
    public class GameInputService : IGameInput
    {
        private readonly InputSystem_Actions _actions;

        public GameInputService()
        {
            _actions = new InputSystem_Actions();
            _actions.Player.Enable(); // Включаем карту Player
        }

        // --- Основное движение ---
        public Vector2 MoveInput => _actions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _actions.Player.Look.ReadValue<Vector2>();
        public bool IsJumpPressed => _actions.Player.Jump.WasPressedThisFrame();
        public bool IsCrouchPressed => _actions.Player.Crouch.IsPressed();
        public float LeanInput => _actions.Player.Lean.ReadValue<float>();

        // --- Бой ---
        public bool IsFirePressed => _actions.Player.Attack.IsPressed();
        public bool IsAimPressed => _actions.Player.Aim.IsPressed();
        public bool IsAimHeld => _actions.Player.Aim.IsPressed();
        public bool IsAimTriggered => _actions.Player.Aim.WasPressedThisFrame();
        public bool IsReloadPressed => _actions.Player.Reload.WasPressedThisFrame();
        public bool IsHolsterPressed => _actions.Player.Holster.WasPressedThisFrame();

        // --- Инвентарь ---

        // Возвращает индекс оружия (0, 1, 2) или -1, если ничего не нажато
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

        // Возвращает направление прокрутки: 1 (вверх), -1 (вниз) или 0
        public float WeaponScroll
        {
            get
            {
                float scrollValue = _actions.Player.WeaponScroll.ReadValue<float>();
                // Нормализуем значение, так как мыши бывают разные (кто-то выдает 120, кто-то 0.1)
                if (scrollValue > 0) return 1f;
                if (scrollValue < 0) return -1f;
                return 0f;
            }
        }

        // --- Управление сервисом ---
        public void Enable() => _actions.Enable();
        public void Disable() => _actions.Disable();
        public void Dispose()
        {
            _actions.Disable();
            _actions.Dispose();
        }
    }
}