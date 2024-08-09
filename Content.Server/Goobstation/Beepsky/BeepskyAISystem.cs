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

using System;
using System.Collections.Generic;

public class Beepsky
{
    public string Name { get; private set; } = "Beepsky";
    public List<string> PatrolPaths { get; private set; } = new List<string> { "security_zone_1", "security_zone_2" };
    public string PatrolMode { get; private set; } = "loop";
    public float ArrestRange { get; private set; } = 7.5f;
    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }
    public int DamageThreshold { get; private set; } = 20;
    public bool IsRepairable { get; private set; } = true;
    public List<string> RepairItems { get; private set; }
    public bool AutoMarkAsWanted { get; private set; } = true;
    public List<string> PriorityTargets { get; private set; }
    public List<string> IgnoreTargets { get; private set; }
    public bool FollowUntilOutOfSight { get; private set; } = true;
    public bool ProximityCuffing { get; private set; } = true;

    public Beepsky()
    {
        CurrentHealth = MaxHealth;
        RepairItems = new List<string> { "welder", "cable" };
        PriorityTargets = new List<string> { "antagonist", "criminal" };
        IgnoreTargets = new List<string> { "command", "security" };
    }

    public void Patrol()
    {
        // Logic for patrolling multiple paths
        foreach (string path in PatrolPaths)
        {
            Console.WriteLine($"{Name} is patrolling {path}.");
            // Path-specific patrol logic
        }

        if (PatrolMode == "loop")
        {
            // Logic to loop patrol routes
        }
        else if (PatrolMode == "random")
        {
            // Logic for random patrol paths
        }
    }

    public void DetectCriminal(Criminal criminal)
    {
        if (IgnoreTargets.Contains(criminal.Role))
        {
            Console.WriteLine($"{Name} ignoring {criminal.Name} due to role: {criminal.Role}.");
            return;
        }

        if (PriorityTargets.Contains(criminal.Role))
        {
            Console.WriteLine($"{Name} prioritizing target: {criminal.Name}.");
        }

        if (IsWithinRange(criminal.Position, ArrestRange))
        {
            if (AutoMarkAsWanted && criminal.IsBelowSecurityLevel())
            {
                criminal.SetWantedStatus(true);
            }
            EngageCriminal(criminal);
        }
    }

    private void EngageCriminal(Criminal criminal)
    {
        FollowCriminal(criminal);

        if (IsInProximity(criminal.Position, ArrestRange))
        {
            Announce($"Arrest in progress. Convict: {criminal.Name} at {GetCurrentLocation()}.");

            if (criminal.IsParalyzed)
            {
                criminal.Cuff();
                Announce($"Arrest successful. Convict: {criminal.Name} cuffed at {GetCurrentLocation()}.");
                ReturnToPatrol();
            }
        }
    }

    private void FollowCriminal(Criminal criminal)
    {
        if (FollowUntilOutOfSight)
        {
            // Logic for following the criminal until out of sight
        }
    }

    private bool IsWithinRange(Vector3 position, float range)
    {
        return Vector3.Distance(this.Position, position) <= range;
    }

    private bool IsInProximity(Vector3 position, float range)
    {
        return Vector3.Distance(this.Position, position) <= range;
    }

    private string GetCurrentLocation()
    {
        // Logic to get Beepsky's current location
        return "Security Sector 1"; // Example location
    }

    private void ReturnToPatrol()
    {
        Announce("Returning to patrol.");
        Patrol(); // Resume patrolling along the set path
    }

    public void Announce(string message)
    {
        // Logic to send message to the primary and backup communication channels
        Console.WriteLine(message);
    }

    public void Repair(int repairAmount)
    {
        if (IsRepairable)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + repairAmount);
            Console.WriteLine($"{Name} repaired to {CurrentHealth}/{MaxHealth} health.");
        }
    }

    public void CheckHealthStatus()
    {
        if (CurrentHealth <= DamageThreshold)
        {
            Announce("Beepsky critically damaged. Returning for repairs.");
            // Logic to handle return to a safe area for repairs
        }
    }
}

public class Criminal
{
    public string Name { get; set; }
    public string Role { get; set; }
    public Vector3 Position { get; set; }
    public bool IsParalyzed { get; set; }
    public bool WantedStatus { get; private set; }

    public bool IsBelowSecurityLevel()
    {
        // Logic to determine if the person is below the access level of command or security
        return true; // Placeholder logic
    }

    public void SetWantedStatus(bool status)
    {
        WantedStatus = status;
    }

    public void Cuff()
    {
        // Logic to cuff the criminal
        Console.WriteLine($"{Name} has been cuffed.");
    }
}

public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public static float Distance(Vector3 a, Vector3 b)
    {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
    }
}
