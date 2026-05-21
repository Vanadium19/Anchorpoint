using System.Globalization;
using System.IO;
using System.Text;
using Sirenix.Utilities;

namespace AudioModule
{
    internal static class AudioBankAPIGenerator
    {
        public static void Generate(AudioBank audioBank)
        {
            string path = audioBank.GetCodeGenPath();
            string bankId = audioBank.GetIdentifier();
            
            string className = $"{ToTitleCase(bankId)}BankAPI";
            string selectedPath = $"{path}/{className}.cs";

            using StreamWriter writer = new StreamWriter(selectedPath);
            
            writer.WriteLine("/**");
            writer.WriteLine("* Code generation. Don't modify! ");
            writer.WriteLine(" */");
            
            writer.WriteLine();
            
            writer.WriteLine("namespace AudioModule");
            writer.WriteLine("{");
            writer.WriteLine($"    public static class {className}");
            writer.WriteLine("    {");

            writer.WriteLine("        ///Events");

            //Generate event ids:
            foreach (string eventId in audioBank.GetEventIds())
                writer.WriteLine($"        public const string {ToTitleCase(eventId)}Event = \"{bankId}.{eventId}\";");

            writer.WriteLine();

            //Generate event ids:
            writer.WriteLine("        ///Parameters");
            foreach (string parameterId in audioBank.GetAllParameterIds())
            {
                writer.WriteLine($"        public const string {ToTitleCase(parameterId)}Parameter = \"{bankId}.{parameterId}\";");
            }
            
            writer.WriteLine();

            //Generate callbacks:
            writer.WriteLine("        ///Callbacks");
            foreach (string callbackId in audioBank.GetCallbacks())
            {
                writer.WriteLine($"        public const string {ToTitleCase(callbackId)}Callback = \"{bankId}.{callbackId}\";");
            }
            
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
        
        public static string ToTitleCase(string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(char.ToUpper(input[0]));
                
                
            for (int index = 1; index < input.Length; ++index)
            {
                char ch = input[index];
                if (ch == '_' && index + 1 < input.Length)
                {
                    char upper = input[index + 1];
                    if (char.IsLower(upper))
                        upper = char.ToUpper(upper, CultureInfo.InvariantCulture);
                    stringBuilder.Append(upper);
                    ++index;
                }
                else
                    stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }
    }
}