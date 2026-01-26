using UnityEngine;
using Zenject;
using InputModule.Core;

namespace InputModule.Installers
{
    [CreateAssetMenu(fileName = "InputInstaller", menuName = "Installers/InputInstaller")]
    public class InputInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameInputService>()
                .AsSingle();
        }
    }
}