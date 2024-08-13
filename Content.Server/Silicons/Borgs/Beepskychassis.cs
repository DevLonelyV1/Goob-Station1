// This should be the top of the file
using System;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;

namespace Content.Server.Silicons.Borgs
{
    public sealed partial class BeepskyChassisSystem
    {
        // Ensure that your class implementation, properties, and methods are within this class definition

        public void InitializeModules()
        {
            SubscribeLocalEvent<BorgModuleComponent, EntGotInsertedIntoContainerMessage>(OnModuleGotInserted);
            SubscribeLocalEvent<BorgModuleComponent, EntGotRemovedFromContainerMessage>(OnModuleGotRemoved);

            SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleInstalledEvent>(OnSelectableInstalled);
            SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleUninstalledEvent>(OnSelectableUninstalled);
            SubscribeLocalEvent<SelectableBorgModuleComponent, BorgModuleActionSelectedEvent>(OnSelectableAction);

            SubscribeLocalEvent<ItemBorgModuleComponent, ComponentStartup>(OnProvideItemStartup);
            SubscribeLocalEvent<ItemBorgModuleComponent, BorgModuleSelectedEvent>(OnItemModuleSelected);
            SubscribeLocalEvent<ItemBorgModuleComponent, BorgModuleUnselectedEvent>(OnItemModuleUnselected);
        }

        // Implement other methods and properties inside this class
    }
}
