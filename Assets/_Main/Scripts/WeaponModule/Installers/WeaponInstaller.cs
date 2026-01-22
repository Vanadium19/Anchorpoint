using UnityEngine;
using Zenject;
using System.Collections.Generic; // Обязательно для List
using WeaponModule.Configs;
using WeaponModule.Controllers; // Тут лежит класс WeaponSetupData
using WeaponModule.Core;
using WeaponModule.View;

namespace WeaponModule.Installers
{
    public class WeaponInstaller : MonoInstaller
    {
        [Header("Inventory Setup")]
        // Вот этот список должен появиться в Инспекторе
        [SerializeField] private List<WeaponSetupData> loadout;

        public override void InstallBindings()
        {
            // 1. Передаем список настроек (Loadout) в Инвентарь
            // Zenject отдаст этот список в конструктор WeaponInventory
            Container.BindInstance(loadout).AsSingle();

            // 2. Биндим Модель
            // Важно: AsTransient() означает "создавай новую модель для каждого нового оружия".
            // Иначе у всех пушек будут общие патроны.
            Container.Bind<WeaponModel>().AsTransient();

            // 3. Биндим ФАБРИКУ
            // Это магия Zenject: он создает класс, который умеет делать WeaponController,
            // используя Config и View, которые мы передадим из Инвентаря.
            Container.BindFactory<WeaponConfig, WeaponView, WeaponController, WeaponController.Factory>();

            // 4. Биндим ИНВЕНТАРЬ
            // InterfacesAndSelf означает, что он работает и как IInitializable (Start), и как ITickable (Update)
            Container.BindInterfacesAndSelfTo<WeaponInventory>().AsSingle();
        }
    }
}