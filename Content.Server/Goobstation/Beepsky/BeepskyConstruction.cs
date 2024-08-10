// File: Systems/Construction/BeepskyConstructionSystem.cs

using Content.Server.Construction;
using Content.Server.GameObjects.Components.Items;
using Content.Shared.Interaction;
using Robust.Shared.GameObjects;

namespace Content.Server.Robotics
{
    public class BeepskyConstructionSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BeepskyChassisComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<BeepskyChassisScrewedComponent, InteractUsingEvent>(OnInteractUsingScrewed);
        }

        private void OnInteractUsing(EntityUid uid, BeepskyChassisComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (TryComp<ToolComponent>(args.Used, out var tool))
            {
                if (tool.ToolQuality.Contains("Screwing"))
                {
                    // Transform into BeepskyChassisScrewed
                    var transformSystem = EntityManager.System<TransformSystem>();
                    var position = transformSystem.GetWorldPosition(uid);
                    EntityManager.DeleteEntity(uid);
                    var screwedEntity = EntityManager.SpawnEntity("BeepskyChassisScrewed", position);
                    args.Handled = true;
                }
            }
        }

        private void OnInteractUsingScrewed(EntityUid uid, BeepskyChassisScrewedComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (TryComp<ToolComponent>(args.Used, out var tool))
            {
                if (tool.ToolQuality.Contains("Welding"))
                {
                    // Transform into Beepsky
                    var transformSystem = EntityManager.System<TransformSystem>();
                    var position = transformSystem.GetWorldPosition(uid);
                    EntityManager.DeleteEntity(uid);
                    var beepskyEntity = EntityManager.SpawnEntity("Beepsky", position);
                    args.Handled = true;
                }
            }
        }
    }
}
