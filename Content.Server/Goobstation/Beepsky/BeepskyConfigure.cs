using System.Collections.Generic;
using Content.Shared.Sound;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Server.Beepsky.Components
{
    [RegisterComponent]
    [NetworkedComponent]
    public sealed class BeepskyComponent : Component
    {
        public override string Name => "Beepsky";

        /// <summary>
        ///     List of patrol paths that Beepsky will follow.
        /// </summary>
        [DataField("patrolPaths")]
        [ViewVariables(VVAccess.ReadWrite)]
        public List<string> PatrolPaths { get; set; } = new()
        {
            "security_zone_1",
            "security_zone_2"
        };

        /// <summary>
        ///     Determines the patrol mode: loop or random.
        /// </summary>
        [DataField("patrolMode")]
        [ViewVariables(VVAccess.ReadWrite)]
        public PatrolMode PatrolMode { get; set; } = PatrolMode.Loop;

        /// <summary>
        ///     The range within which Beepsky can detect and engage criminals.
        /// </summary>
        [DataField("arrestRange")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float ArrestRange { get; set; } = 7.5f;

        /// <summary>
        ///     Maximum health of Beepsky.
        /// </summary>
        [DataField("maxHealth")]
        [ViewVariables(VVAccess.ReadWrite)]
        public int MaxHealth { get; set; } = 100;

        /// <summary>
        ///     Health threshold at which Beepsky will seek repairs.
        /// </summary>
        [DataField("damageThreshold")]
        [ViewVariables(VVAccess.ReadWrite)]
        public int DamageThreshold { get; set; } = 20;

        /// <summary>
        ///     Determines if Beepsky automatically marks detected entities as wanted.
        /// </summary>
        [DataField("autoMarkAsWanted")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool AutoMarkAsWanted { get; set; } = true;

        /// <summary>
        ///     Roles that Beepsky will prioritize when engaging.
        /// </summary>
        [DataField("priorityTargets")]
        [ViewVariables(VVAccess.ReadWrite)]
        public List<string> PriorityTargets { get; set; } = new()
        {
            "Antagonist",
            "Criminal"
        };

        /// <summary>
        ///     Roles that Beepsky will ignore.
        /// </summary>
        [DataField("ignoreTargets")]
        [ViewVariables(VVAccess.ReadWrite)]
        public List<string> IgnoreTargets { get; set; } = new()
        {
            "Command",
            "Security"
        };

        /// <summary>
        ///     Determines if Beepsky will follow criminals until they are out of sight.
        /// </summary>
        [DataField("followUntilOutOfSight")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool FollowUntilOutOfSight { get; set; } = true;

        /// <summary>
        ///     Determines if Beepsky will attempt to cuff criminals upon proximity.
        /// </summary>
        [DataField("proximityCuffing")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool ProximityCuffing { get; set; } = true;

        /// <summary>
        ///     Sound played when Beepsky is critically damaged.
        /// </summary>
        [DataField("lowHealthSound")]
        [ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier LowHealthSound { get; set; } = new SoundPathSpecifier("/Audio/Effects/beepsky_low_health.ogg");
    }

    public enum PatrolMode
    {
        Loop,
        Random
    }
}
