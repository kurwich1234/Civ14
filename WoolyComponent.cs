using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Animals;

[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause, NetworkedComponent]
public sealed partial class WoolyComponent : Component
{
    [DataField("reagentId"), AutoNetworkedField]
    public string ReagentId = "Fiber";

    [DataField("solutionName")]
    public string SolutionName = "wool";

    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<SolutionComponent>? Solution;

    [DataField("quantity"), AutoNetworkedField]
    public FixedPoint2 Quantity = 25;

    [DataField("hungerUsage"), AutoNetworkedField]
    public float HungerUsage = 10f;

    [DataField("growthDelay"), AutoNetworkedField]
    public TimeSpan GrowthDelay = TimeSpan.FromMinutes(1);

    [DataField("nextGrowth"), AutoPausedField, Access(typeof(WoolySystem))]
    public TimeSpan NextGrowth = TimeSpan.Zero;
}