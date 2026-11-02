namespace ArsenalModule
{
    /// <summary>
    /// The result of checking whether a modification can be installed on a weapon.
    /// </summary>
    public enum ArsenalInstallStatus
    {
        /// <summary>
        /// The modification can be installed right now.
        /// </summary>
        Available = 0,

        /// <summary>
        /// This modification is already installed on the weapon.
        /// </summary>
        AlreadyInstalled = 1,

        /// <summary>
        /// Another modification already occupies the slot of this modification.
        /// </summary>
        SlotOccupied = 2,

        /// <summary>
        /// The player does not hold every required resource.
        /// </summary>
        NotEnoughResources = 3,

        /// <summary>
        /// The weapon does not list this modification among its available modifications.
        /// </summary>
        NotCompatible = 4
    }
}
