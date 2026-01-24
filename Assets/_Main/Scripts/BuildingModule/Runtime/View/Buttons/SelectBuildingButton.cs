using BuildingModule.Runtime.Commands;
using CommandsModule;

namespace BuildingModule
{
    public class SelectBuildingButton : ExecuteCommandButton<SelectBuildingCommand, BuildingName>
    {
    }
}