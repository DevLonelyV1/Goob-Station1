using System;
using System.Collections.Generic;
using Content.Server.AI.Components;
using Content.Server.AI.Pathfinding;
using Content.Server.Beepsky.Components;
using Content.Server.Chat.Managers;
using Content.Server.Hands.Components;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Server.Weapon.Melee;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Wearable.Components;
using Robust.Server.AI;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Beepsky.Systems
{
    public sealed class BeepskySystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PathfindingSystem _pathfindingSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly MeleeWeaponSystem _meleeWeaponSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BeepskyComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<BeepskyComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<BeepskyComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<BeepskyComponent, EntityUnpausedEvent>(OnUnpaused);
        }

        private void OnStartup(EntityUid uid, BeepskyComponent component, ComponentStartup args)
        {
            InitializePatrol(uid, component);
        }

        private void OnShutdown(EntityUid uid, BeepskyComponent component, ComponentShutdown args)
        {
            // Cleanup logic if necessary
        }

        private void OnUnpaused(EntityUid uid, BeepskyComponent component, EntityUnpausedEvent args)
        {
            // Handle unpausing logic if necessary
        }

        private void InitializePatrol(EntityUid uid, BeepskyComponent component)
        {
            if (component.PatrolPaths.Count == 0)
            {
                Logger.Warning($"Beepsky {uid} has no patrol paths defined.");
                return;
            }

            EnsureComp<AiControllerComponent>(uid);
            switch (component.PatrolMode)
            {
                case PatrolMode.Loop:
                    SetupLoopPatrol(uid, component);
                    break;
                case PatrolMode.Random:
                    SetupRandomPatrol(uid, component);
                    break;
            }
        }

        private void SetupLoopPatrol(EntityUid uid, BeepskyComponent component)
        {
            var aiComponent = EntityManager.GetComponent<AiControllerComponent>(uid);
            aiComponent.PatrolRoutes = new Queue<Vector2>();

            foreach (var path in component.PatrolPaths)
            {
                var waypoints = GetWaypointsForPath(path);
                foreach (var waypoint in waypoints)
                {
                    aiComponent.PatrolRoutes.Enqueue(waypoint);
                }
            }

            aiComponent.IsPatrolling = true;
        }

        private void SetupRandomPatrol(EntityUid uid, BeepskyComponent component)
        {
            var aiComponent = EntityManager.GetComponent<AiControllerComponent>(uid);
            aiComponent.PatrolRoutes = new Queue<Vector2>();

            var randomPath = _random.Pick(component.PatrolPaths);
            var waypoints = GetWaypointsForPath(randomPath);

            foreach (var waypoint in waypoints)
            {
                aiComponent.PatrolRoutes.Enqueue(waypoint);
            }

            aiComponent.IsPatrolling = true;
        }

        private IEnumerable<Vector2> GetWaypointsForPath(string pathName)
        {
            // Implement logic to retrieve waypoints based on the path name.
            // This is a placeholder implementation.
            return new List<Vector2>();
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var (beepsky, ai, xform) in EntityManager.EntityQuery<BeepskyComponent, AiControllerComponent, TransformComponent>())
            {
                if (!ai.IsActive)
                    continue;

                ProcessBehavior(beepsky.Owner, beepsky, ai, xform, frameTime);
            }
        }

        private void ProcessBehavior(EntityUid uid, BeepskyComponent component, AiControllerComponent ai, TransformComponent xform, float frameTime)
        {
            var nearbyEntities = EntityManager.GetEntitiesInRange(uid, component.ArrestRange);

            foreach (var entity in nearbyEntities)
            {
                if (entity == uid)
                    continue;

                if (TryComp<MindComponent>(entity, out var mind) && mind.Mind != null)
                {
                    var role = GetEntityRole(mind.Mind);

                    if (component.IgnoreTargets.Contains(role))
                        continue;

                    if (component.PriorityTargets.Contains(role) || (component.AutoMarkAsWanted && IsEntityWanted(entity)))
                    {
                        EngageTarget(uid, entity, component, ai, xform);
                        return;
                    }
                }
            }

            ContinuePatrol(ai, xform, frameTime);
            CheckHealthStatus(uid, component);
        }

        private string GetEntityRole(Mind.Mind mind)
        {
            foreach (var role in mind.AllRoles)
            {
                if (role is AntagonistRole)
                    return "Antagonist";
                if (role is Job { CanBeAntag: false })
                    return role.Name;
            }

            return "Civilian";
        }

        private bool IsEntityWanted(EntityUid entity)
        {
            return HasComp<WantedComponent>(entity);
        }

        private void EngageTarget(EntityUid uid, EntityUid target, BeepskyComponent component, AiControllerComponent ai, TransformComponent xform)
        {
            ai.TargetEntity = target;
            ai.IsPatrolling = false;

            if (TryComp<TransformComponent>(target, out var targetXform))
            {
                var distance = (xform.WorldPosition - targetXform.WorldPosition).Length;

                if (distance <= component.ArrestRange && component.ProximityCuffing)
                {
                    AttemptCuff(uid, target, component);
                }
                else
                {
                    MoveTowardsTarget(uid, target, ai);
                }
            }
        }

        private void AttemptCuff(EntityUid uid, EntityUid target, BeepskyComponent component)
        {
            if (TryComp<HandsComponent>(uid, out var hands))
            {
                foreach (var item in hands.GetAllHeldItems())
                {
                    if (HasComp<HandcuffComponent>(item.Owner))
                    {
                        if (TryComp<CuffableComponent>(target, out var cuffable))
                        {
                            cuffable.TryAddNewCuffs(uid, item.Owner);
                            AnnounceArrestSuccess(target, component);
                            InitializePatrol(uid, component);
                            return;
                        }
                    }
                }
            }

            Logger.Warning($"Beepsky {uid} attempted to cuff {target} but has no cuffs available.");
        }

        private void MoveTowardsTarget(EntityUid uid, EntityUid target, AiControllerComponent ai)
        {
            if (TryComp<TransformComponent>(target, out var targetXform))
            {
                ai.MoveTo(targetXform.Coordinates);
            }
        }

        private void AnnounceArrestSuccess(EntityUid target, BeepskyComponent component)
        {
            var targetName = Name(target);
            var location = Transform(target).Coordinates.ToString();

            _chatManager.DispatchStationAnnouncement(
                $"Arrest successful. Convict: {targetName} cuffed at {location}.",
                "Beepsky",
                playDefaultSound: false,
                colorOverride: Color.Purple);
        }

        private void ContinuePatrol(AiControllerComponent ai, TransformComponent xform, float frameTime)
        {
            if (!ai.IsPatrolling || ai.PatrolRoutes.Count == 0)
                return;

            var nextWaypoint = ai.PatrolRoutes.Peek();
            var currentPos = xform.WorldPosition;
            var distance = (currentPos - nextWaypoint).Length;

            if (distance <= 0.5f)
            {
                ai.PatrolRoutes.Dequeue();

                if (ai.PatrolRoutes.Count == 0 && ai.PatrolMode == PatrolMode.Loop)
                {
                    InitializePatrol(ai.Owner, Comp<BeepskyComponent>(ai.Owner));
                }
            }
            else
            {
                ai.MoveTo(nextWaypoint);
            }
        }

        private void OnDamageChanged(EntityUid uid, BeepskyComponent component, DamageChangedEvent args)
        {
            if (args.DamageIncreased && args.NewTotalDamage >= component.DamageThreshold)
            {
                HandleCriticalDamage(uid, component);
            }
        }

        private void HandleCriticalDamage(EntityUid uid, BeepskyComponent component)
        {
            _chatManager.DispatchStationAnnouncement(
                "Beepsky critically damaged. Returning for repairs.",
                "Beepsky",
                playDefaultSound: false,
                colorOverride: Color.Red);

            _audioSystem.PlayPvs(component.LowHealthSound, uid);
            MoveToRepairStation(uid);
        }

        private void MoveToRepairStation(EntityUid uid)
        {
            // Implement logic for moving Beepsky to the nearest repair station.
            // This is a placeholder implementation.
            Logger.Info($"Beepsky {uid} is moving to the repair station.");
        }

        private void CheckHealthStatus(EntityUid uid, BeepskyComponent component)
        {
            if (TryComp<DamageableComponent>(uid, out var damageable))
            {
                if (damageable.TotalDamage >= component.DamageThreshold)
                {
                    HandleCriticalDamage(uid, component);
                }
            }
        }
    }
}
