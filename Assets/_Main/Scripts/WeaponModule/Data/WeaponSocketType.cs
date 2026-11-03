using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A kind of interface on a weapon view, for example where a sight, a magazine or a barrel attaches. A weapon
    /// has a socket of this type on its view, a modification names the type it needs. Compatibility between them
    /// is derived from the type, so a new slot or an incompatible interface is a new asset and not code.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponSocketType", menuName = "Configs/Weapon Socket Type")]
    public class WeaponSocketType : ScriptableObject
    {
        [Tooltip("Localization key of the name of the slot shown in the Arsenal.")]
        [SerializeField] private string nameKey;

        public string NameKey => nameKey;
    }
}
