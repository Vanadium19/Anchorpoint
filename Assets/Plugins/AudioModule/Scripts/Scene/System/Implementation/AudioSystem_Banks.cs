using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    public partial class AudioSystem
    {
        [Header("Setup")]
        [SerializeField, HideInPlayMode]
        private AudioBank[] initialBanks;
        
        public bool LoadBank(AudioBank audioBank)
        {
            if (audioBank == null)
                return false;

            string bankIdentifier = audioBank.identifier;

            foreach (EventParameter eventParameter in audioBank.eventParameters)
                this.audioEventPool.RegisterPrefab($"{bankIdentifier}.{eventParameter.identifier}", eventParameter.value);

            foreach (BoolParameter boolParameter in audioBank.boolParameters)
                this.boolParameters[$"{bankIdentifier}.{boolParameter.identifier}"] = boolParameter.value;

            foreach (IntParameter intParameter in audioBank.intParameters)
                this.intParameters[$"{bankIdentifier}.{intParameter.identifier}"] = intParameter.value;

            foreach (FloatParameter floatParameter in audioBank.floatParameters)
                this.floatParameters[$"{bankIdentifier}.{floatParameter.identifier}"] = floatParameter.value;

            return true;
        }

        public bool UnloadBank(AudioBank audioBank)
        {
            if (audioBank == null)
                return false;

            string bankIdentifier = audioBank.identifier;

            foreach (EventParameter eventParameter in audioBank.eventParameters)
                this.audioEventPool.UnregisterPrefab($"{bankIdentifier}.{eventParameter.identifier}");

            foreach (BoolParameter boolParameter in audioBank.boolParameters)
                this.boolParameters.Remove($"{bankIdentifier}.{boolParameter.identifier}");

            foreach (IntParameter intParameter in audioBank.intParameters)
                this.intParameters.Remove($"{bankIdentifier}.{intParameter.identifier}");

            foreach (FloatParameter floatParameter in audioBank.floatParameters)
                this.floatParameters.Remove($"{bankIdentifier}.{floatParameter.identifier}");

            return true;
        }
        
        private void LoadInitialBanks()
        {
            foreach (AudioBank bank in this.initialBanks)
                if (bank != null)
                    this.LoadBank(bank);
        }
    }
}