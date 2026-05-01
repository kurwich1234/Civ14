using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Shared.Animals;

public sealed class WoolySystem : EntitySystem
{
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WoolyComponent, MapInitEvent>((uid, comp, args) => comp.NextGrowth = _timing.CurTime + comp.GrowthDelay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<WoolyComponent>();
        while (query.MoveNext(out var uid, out var wooly))
        {
            if (wooly.GrowthDelay.TotalSeconds < 1) continue;
            if (_timing.CurTime < wooly.NextGrowth) continue;

            // Обновляем таймер СРАЗУ, чтобы не ловить Assert
            wooly.NextGrowth = _timing.CurTime + wooly.GrowthDelay;

            if (_mobState.IsDead(uid) || string.IsNullOrEmpty(wooly.ReagentId)) continue;
            if (!_solutionContainer.TryGetSolution(uid, wooly.SolutionName, out var solEnt, out _)) continue;

            if (TryComp<HungerComponent>(uid, out var hunger))
            {
                if (_hunger.GetHungerThreshold(hunger) < HungerThreshold.Okay) continue;
                _hunger.ModifyHunger(uid, -wooly.HungerUsage, hunger);
            }

            // Используем имя wooly.Quantity из компонента
            _solutionContainer.TryAddReagent(solEnt.Value, wooly.ReagentId, wooly.Quantity, out _);
        }
    }
}