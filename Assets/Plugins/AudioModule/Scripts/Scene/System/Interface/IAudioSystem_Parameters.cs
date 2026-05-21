namespace AudioModule
{
    public partial interface IAudioSystem
    {
        bool TryGetBool(string paramId, out bool result);
        bool TryGetInt(string paramId, out int result);
        bool TryGetFloat(string paramId, out float result);
        
        bool GetBool(string paramId);
        int GetInt(string paramId);
        float GetFloat(string paramId);
        
        void SetBool(string paramId, bool value);
        void SetInt(string paramId, int value);
        void SetFloat(string paramId, float value);
    }
}