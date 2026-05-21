using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace AudioModule
{
    internal static class UISoundComponentGenerator
    {
        internal static void Generate(IReadOnlyList<UISoundCatalog.SoundInfo> sounds)
        {
            const string selectedPath = "Assets/Plugins/AudioModule/Codegen/UI/UISoundComponent.cs";

            using StreamWriter writer = new StreamWriter(selectedPath);
            
            writer.WriteLine("/**");
            writer.WriteLine("* Code generation. Don't modify! ");
            writer.WriteLine(" */");
            
            writer.WriteLine();
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using System.Runtime.CompilerServices;");
            writer.WriteLine();
            
            writer.WriteLine("namespace AudioModule");
            writer.WriteLine("{");
            
            //Write class:
            writer.WriteLine("    [AddComponentMenu(\"Audio/UI Sound Component\")]");
            writer.WriteLine("    public sealed class UISoundComponent : MonoBehaviour");
            writer.WriteLine("    {");
            
            //Write API
            for (int i = 0, count = sounds.Count; i < count; i++)
            {
                UISoundCatalog.SoundInfo sound = sounds[i];
                string soundID = sound.id;
                
                writer.WriteLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)] " +
                                 $"public void SOUND_{soundID.ToUpper()}() => " +
                                 $"this.SOUND_CUSTOM(UISoundAPI.{soundID.ToUpper()});");
            }
            
            writer.WriteLine();
            
            //Write main method:
            writer.WriteLine("        public void SOUND_CUSTOM(string soundKey)");
            writer.WriteLine("        {");
            writer.WriteLine("              UISoundPlayer player = UISoundPlayer.Instance;");
            writer.WriteLine("              if (player != null) player.PlayOneShot(soundKey);");
            writer.WriteLine("        }");

            //Write end:
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
    }
}