using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using InputModule.Core;
using WeaponModule.Core;
using WeaponModule.Configs;
using WeaponModule.View;

namespace WeaponModule.Controllers
{
    public class WeaponInventory : IInitializable, ITickable, ILateTickable
    {
        private readonly WeaponController.Factory _factory;
        private readonly IInputMap _input;
        private readonly List<WeaponSetupData> _loadout;

        // Список всех созданных контроллеров (как IWeapon)
        private readonly List<IWeapon> _weapons = new();

        private IWeapon _currentWeapon;
        private int _currentIndex = -1; // -1 значит "руки пусты"

        // Флаг блокировки: true, пока идет анимация смены оружия
        private bool _isSwitching = false;

        public WeaponInventory(
            WeaponController.Factory factory,
            IInputMap input,
            List<WeaponSetupData> loadout)
        {
            _factory = factory;
            _input = input;
            _loadout = loadout;
        }

        public void Initialize()
        {
            // 1. Создаем всё оружие при старте игры
            foreach (var setup in _loadout)
            {
                var weapon = _factory.Create(setup.Config, setup.View);

                weapon.Initialize();

                // Сразу прячем визуально (без анимации, мгновенно)
                // Т.к. Unequip теперь асинхронный, при инициализации лучше
                // вручную выключить объект через View, если есть доступ, 
                // или просто вызвать Unequip().Forget(), так как игра еще на старте.
                // Но правильнее всего при старте просто выключить объект.
                setup.View.gameObject.SetActive(false);

                _weapons.Add(weapon);
            }

            // 2. Достаем первое оружие (если есть)
            if (_weapons.Count > 0)
            {
                // Запускаем без ожидания (Forget), так как это старт игры
                EquipWeapon(0).Forget();
            }
        }

        public void Tick()
        {
            // Если сейчас идет смена оружия — блокируем всё (и стрельбу, и новую смену)
            if (_isSwitching) return;

            // Обновляем активное оружие (стрельба, прицеливание)
            _currentWeapon?.Tick();

            // Слушаем ввод для смены
            HandleInput();
        }

        public void LateTick()
        {
            _currentWeapon?.LateTick();
        }

        private void HandleInput()
        {
            // А) Прямой выбор кнопками (1, 2, 3...)
            int requestedIndex = _input.SelectWeaponIndex;
            if (requestedIndex != -1)
            {
                EquipWeapon(requestedIndex).Forget();
                return; // Если выбрали кнопку, скролл в этом кадре игнорируем
            }

            // Б) Прокрутка колесиком
            float scroll = _input.WeaponScroll;
            if (scroll > 0.1f) // Вверх -> Следующее
            {
                EquipNext();
            }
            else if (scroll < -0.1f) // Вниз -> Предыдущее
            {
                EquipPrevious();
            }
        }

        private void EquipNext()
        {
            // Если рук нет (пустые), начинаем с 0
            if (_currentIndex == -1)
            {
                EquipWeapon(0).Forget();
                return;
            }

            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _weapons.Count) nextIndex = 0; // Зацикливаем

            EquipWeapon(nextIndex).Forget();
        }

        private void EquipPrevious()
        {
            if (_currentIndex == -1)
            {
                EquipWeapon(_weapons.Count - 1).Forget();
                return;
            }

            int prevIndex = _currentIndex - 1;
            if (prevIndex < 0) prevIndex = _weapons.Count - 1; // Зацикливаем

            EquipWeapon(prevIndex).Forget();
        }

        // Главный метод смены (Асинхронный)
        private async UniTaskVoid EquipWeapon(int index)
        {
            // Проверка на валидность индекса
            if (index < 0 || index >= _weapons.Count) return;

            // Блокируем ввод на время анимаций
            _isSwitching = true;

            // СЦЕНАРИЙ 1: TOGGLE (Убираем текущее, если выбрали его же)
            if (_weapons[index] == _currentWeapon)
            {
                if (_currentWeapon != null)
                {
                    await _currentWeapon.Unequip(); // Ждем анимацию Holster
                }

                _currentWeapon = null;
                _currentIndex = -1;

                _isSwitching = false; // Разблокируем
                return;
            }

            // СЦЕНАРИЙ 2: СМЕНА (Swap)

            // 1. Если что-то есть в руках — убираем и ЖДЕМ
            if (_currentWeapon != null)
            {
                await _currentWeapon.Unequip();
            }

            // 2. Меняем ссылку на новое
            _currentIndex = index;
            _currentWeapon = _weapons[index];

            // 3. Достаем новое (анимация Draw запустится внутри Equip)
            // Equip у нас синхронный (fire-and-forget анимацию), 
            // но если нужно ждать доставания перед стрельбой — это делает сам контроллер.
            _currentWeapon.Equip();

            // Небольшая задержка, чтобы флаг не снялся раньше времени, 
            // если Equip мгновенный (опционально)
            // await UniTask.Yield(); 

            _isSwitching = false;
        }
    }

    // Этот класс нужен для настройки списка в Инспекторе (в WeaponInstaller)
    [System.Serializable]
    public class WeaponSetupData
    {
        public string Name;
        public WeaponConfig Config;
        public WeaponView View;
    }
}