using Content.Server.AI;
using Content.Server.Bot;
using Content.Server.Bot.Components;
using Content.Server.Stun;
using Content.Server.PowerCell.Components;
using Content.Shared.PowerCell;
using Content.Shared.Interaction;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.Beepsky
{
    public sealed class BeepskyLogic : AIBaseLogic
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IStunbatonUsage _stunbatonUsage = default!;
        [Dependency] private readonly IStateManagementSystem _stateManagementSystem = default!;
        [Dependency] private readonly IPatrolSystem _patrolSystem = default!;
        [Dependency] private readonly IHealthSystem _healthSystem = default!;

        private EntityUid _target;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<FollowAndArrestComponent, TargetAcquiredEvent>(OnTargetAcquired);
            SubscribeLocalEvent<FollowAndArrestComponent, TargetLostEvent>(OnTargetLost);
            SubscribeLocalEvent<DamageableComponent, DamagedEvent>(OnDamaged);
        }

        private void OnTargetAcquired(EntityUid uid, FollowAndArrestComponent component, TargetAcquiredEvent args)
        {
            _target = args.Target;
            // Switch to Chase state
            _stateManagementSystem.SetState(uid, "Chase");
            // Start following the target
            _entityManager.GetComponent<MoveToOperatorComponent>(uid).MoveTo(_target);
        }

        private void OnTargetLost(EntityUid uid, FollowAndArrestComponent component, TargetLostEvent args)
        {
            _target = EntityUid.Invalid;
            // Return to Patrol state
            _stateManagementSystem.SetState(uid, "Patrol");
            // Resume patrol
            _patrolSystem.StartPatrolling(uid);
        }

        private void OnDamaged(EntityUid uid, DamageableComponent component, DamagedEvent args)
        {
            // If health is below a certain threshold, retreat or disable
            if (_healthSystem.GetHealth(uid) <= 20)
            {
                _stateManagementSystem.SetState(uid, "Retreat");
                // Implement retreat logic
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (_target == EntityUid.Invalid)
                return;

            // If close to the target, attempt to use the stunbaton
            if (_entityManager.GetComponent<TransformComponent>(_target).Coordinates
                .InRange(_entityManager.GetComponent<TransformComponent>(Owner).Coordinates, 7.5f))
            {
                _stunbatonUsage.UseStunbaton(Owner, _target);
                // Switch to Arrest state
                _stateManagementSystem.SetState(Owner, "Arrest");
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();
            UnsubscribeLocalEvent<FollowAndArrestComponent, TargetAcquiredEvent>(OnTargetAcquired);
            UnsubscribeLocalEvent<FollowAndArrestComponent, TargetLostEvent>(OnTargetLost);
            UnsubscribeLocalEvent<DamageableComponent, DamagedEvent>(OnDamaged);
        }
    }
}

public interface IStunbatonUsage
{
    void UseStunbaton(EntityUid beepsky, EntityUid target);
}

public class StunbatonUsageSystem : IStunbatonUsage
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public void UseStunbaton(EntityUid beepsky, EntityUid target)
    {
        if (!_entityManager.TryGetComponent<PowerCellSlotComponent>(beepsky, out var battery))
            return;

        if (battery.Cell != null && battery.Cell.Charge > 0)
        {
            // Perform the stun action
            // Implementation of stunning logic goes here, specific to your game mechanics

            // Drain power if needed (but we have infinite charges here)
        }
    }
}
