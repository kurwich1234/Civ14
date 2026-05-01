using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Animals;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class UdderComponent : Component
{
    [DataField("reagentId"), AutoNetworkedField]
    public string ReagentId = "Milk";
    
    [DataField("solutionName")]
    public string SolutionName = "udder";

    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<SolutionComponent>? Solution = null;

    [DataField("quantityPerUpdate"), AutoNetworkedField]
    public FixedPoint2 QuantityPerUpdate = 25;

    [DataField("hungerUsage"), AutoNetworkedField]
    public float HungerUsage = 10f;

    [DataField("growthDelay"), AutoNetworkedField]
    public TimeSpan GrowthDelay = TimeSpan.FromMinutes(1);

    [DataField("nextGrowth"), AutoPausedField, Access(typeof(UdderSystem))]
    public TimeSpan NextGrowth = TimeSpan.Zero;
}