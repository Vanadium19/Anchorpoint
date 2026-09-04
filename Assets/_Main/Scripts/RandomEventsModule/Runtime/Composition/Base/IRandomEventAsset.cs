namespace RandomEventsModule
{
    /// <summary>
    /// Shared by every polymorphic <c>[SerializeReference]</c> asset in the module
    /// (actions, conditions) so the editor can show one label for any of them
    /// without knowing which concrete asset interface it is.
    /// </summary>
    public interface IRandomEventAsset
    {
        /// <summary>Display name shown by the editor's type-picker button.</summary>
        string EditorLabel => GetType().Name;
    }
}
