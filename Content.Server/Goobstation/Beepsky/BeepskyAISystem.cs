using System.Linq;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.AI
{
    public class BeepskyAI : EntitySystem
    {
        private const float ScanInterval = 2.5f;
        private float _scanTimer;

        private IEntity _currentTarget;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<EntityAttackedEvent>(OnEntityAttacked);
        }

        public override void Update(float frameTime)
        {
            _scanTimer -= frameTime;

            if (_scanTimer <= 0)
            {
                _scanTimer = ScanInterval;
                DetectAndEngageCriminals();
            }

            if (_currentTarget != null)
            {
                HandleTarget();
            }
        }

        private void DetectAndEngageCriminals()
        {
            var nearbyEntities = IoCManager.Resolve<IEntityLookup>()
                .GetEntitiesInRange(Owner.Transform.Coordinates, 10f);

            foreach (var entity in nearbyEntities)
            {
                if (IsValidCriminal(entity))
                {
                    _currentTarget = entity;
                    break;
                }
            }

            if (_currentTarget != null)
            {
                if (IsInArrestRange(_currentTarget))
                {
                    StunAndCuff(_currentTarget);
                    AnnounceArrest(_currentTarget);
                    ReturnToPatrolling();
                }
                else
                {
                    FollowCriminal(_currentTarget);
                }
            }
        }

        private bool IsValidCriminal(IEntity entity)
        {
            if (!entity.TryGetComponent(out CriminalComponent criminalComponent))
                return false;

            return criminalComponent.IsWanted && !criminalComponent.IsCuffed;
        }

        private bool IsInArrestRange(IEntity criminal)
        {
            return Owner.Transform.Coordinates.InRange(criminal.Transform.Coordinates, 2f);
        }

        private void FollowCriminal(IEntity criminal)
        {
            // Implement pathfinding or movement logic to follow the criminal.
        }

        private void StunAndCuff(IEntity criminal)
        {
            // Implement the logic to stun and cuff the criminal.
            // Include delays and animations as needed.
        }

        private void AnnounceArrest(IEntity criminal)
        {
            var location = GetNearestStationBeacon();
            var message = Loc.GetString("beepsky-arrest-announcement",
                ("criminal", criminal.Name), ("location", location));

            var chat = IoCManager.Resolve<IChatManager>();
            chat.EntitySay(Owner, message, message);
        }

        private void ReturnToPatrolling()
        {
            _currentTarget = null;
            // Reset to patrolling behavior.
        }

        private string GetNearestStationBeacon()
        {
            // Implement logic to find the nearest station beacon's name.
            return "Unknown Location"; // Placeholder return value.
        }

        private void OnEntityAttacked(EntityAttackedEvent ev)
        {
            if (ev.Target != Owner)
                return;

            if (IsBelowSecurityCommandAccess(ev.Attacker))
            {
                MarkAsWanted(ev.Attacker);
                _currentTarget = ev.Attacker;
                FollowCriminal(ev.Attacker);
            }
        }

        private bool IsBelowSecurityCommandAccess(IEntity attacker)
        {
            if (!attacker.TryGetComponent(out AccessComponent accessComponent))
                return true; // Consider entities without access as below command level.

            return accessComponent.AccessLevel < AccessLevels.SecurityCommand;
        }

        private void MarkAsWanted(IEntity attacker)
        {
            if (attacker.TryGetComponent(out CriminalComponent criminalComponent))
            {
                criminalComponent.IsWanted = true;

                var chat = IoCManager.Resolve<IChatManager>();
                var message = Loc.GetString("beepsky-marked-wanted", ("attacker", attacker.Name));
                chat.EntitySay(Owner, message, message);
            }
        }
    }
}
