// File: Components/Robotics/BeepskyChassisComponent.cs

using Robust.Shared.GameObjects;

namespace Content.Server.Robotics
{
    [RegisterComponent]
    public class BeepskyChassisComponent : Component
    {
        public override string Name => "BeepskyChassis";
    }
}

// File: Components/Robotics/BeepskyChassisScrewedComponent.cs

using Robust.Shared.GameObjects;

namespace Content.Server.Robotics
{
    [RegisterComponent]
    public class BeepskyChassisScrewedComponent : Component
    {
        public override string Name => "BeepskyChassisScrewed";
    }
}
