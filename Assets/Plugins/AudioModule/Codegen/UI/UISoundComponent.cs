/**
* Code generation. Don't modify! 
 */

using UnityEngine;
using System.Runtime.CompilerServices;

namespace AudioModule
{
    [AddComponentMenu("Audio/UI Sound Component")]
    public sealed class UISoundComponent : MonoBehaviour
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SOUND_CLICK() => this.SOUND_CUSTOM(UISoundAPI.CLICK);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SOUND_BUY() => this.SOUND_CUSTOM(UISoundAPI.BUY);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SOUND_ERROR() => this.SOUND_CUSTOM(UISoundAPI.ERROR);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SOUND_ILYA() => this.SOUND_CUSTOM(UISoundAPI.ILYA);

        public void SOUND_CUSTOM(string soundKey)
        {
              UISoundPlayer player = UISoundPlayer.Instance;
              if (player != null) player.PlayOneShot(soundKey);
        }
    }
}
