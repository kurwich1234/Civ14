using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Udder;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Content.Shared.FixedPoint;

namespace Content.Shared.Animals;

public sealed class UdderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UdderComponent, MapInitEvent>((u, c, a) => c.NextGrowth = _timing.CurTime + c.GrowthDelay);
        SubscribeLocalEvent<UdderComponent, GetVerbsEvent<AlternativeVerb>>(AddMilkVerb);
        SubscribeLocalEvent<UdderComponent, MilkingDoAfterEvent>(OnDoAfter);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<UdderComponent>();
        while (query.MoveNext(out var uid, out var udder))
        {
            if (_timing.CurTime < udder.NextGrowth) continue;
            udder.NextGrowth = _timing.CurTime + udder.GrowthDelay;
            
            if (_mobState.IsDead(uid) || string.IsNullOrEmpty(udder.ReagentId)) continue;

            if (!_sol.TryGetSolution(uid, udder.SolutionName, out var solEnt, out _)) continue;

            // ИСПРАВЛЕННЫЙ ВЫЗОВ: Всего 3 аргумента, без out.
            _sol.TryAddReagent(solEnt.Value, udder.ReagentId, udder.QuantityPerUpdate);
        }
    }
    private void OnDoAfter(Entity<UdderComponent> entity, ref MilkingDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null) return;
        
        if (!_sol.TryGetSolution(entity.Owner, entity.Comp.SolutionName, out var solEnt, out var solution)) return;
        
        // Передаем правильные аргументы для старого метода
        if (!_sol.TryGetRefillableSolution(args.Args.Used.Value, out var targetSoln, out var targetSolution)) return;

        args.Handled = true;
        var qty = _sol.GetTotalPrototypeQuantity(solEnt.Value, entity.Comp.ReagentId);
        
        if (qty.Equals(FixedPoint2.Zero))
        {
            _popup.PopupClient(Loc.GetString("udder-system-dry"), entity.Owner, args.Args.User);
            return;
        }

        var finalQty = FixedPoint2.Min(qty, targetSolution.AvailableVolume);
        var split = _sol.SplitSolution(solEnt.Value, finalQty);
        _sol.TryAddSolution(targetSoln.Value, split);
        _popup.PopupClient(Loc.GetString("udder-system-success", ("amount", finalQty.Float()), ("target", Identity.Entity(args.Args.Used.Value, EntityManager))), entity.Owner, args.Args.User, PopupType.Medium);
    }

    private void AddMilkVerb(Entity<UdderComponent> entity, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (args.Using == null || !args.CanInteract || !HasComp<RefillableSolutionComponent>(args.Using.Value)) return;
        var u = args.User; var i = args.Using.Value;
        args.Verbs.Add(new AlternativeVerb() { 
            Act = () => _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, u, 5, new MilkingDoAfterEvent(), entity.Owner, entity.Owner, used: i) { BreakOnMove = true, NeedHand = true }),
            Text = Loc.GetString("udder-system-verb-milk"), 
            Priority = 2 
        });
    }
}