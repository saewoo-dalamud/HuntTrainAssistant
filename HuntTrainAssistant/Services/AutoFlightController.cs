using ECommons.Automation;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HuntTrainAssistant.Tasks;

namespace HuntTrainAssistant.Services;

internal sealed unsafe class AutoFlightController
{
    private (uint TerritoryId, Vector2 Position, DateTime ExpiresAt)? pendingLocation;

    internal void Queue(uint territoryId, Vector2 position)
    {
        pendingLocation = territoryId == Svc.ClientState.TerritoryType
            ? (territoryId, position, DateTime.UtcNow.AddMinutes(5))
            : null;
    }

    internal void Reset()
    {
        pendingLocation = null;
    }

    internal void Update()
    {
        if(!P.Config.AutoFlyToConductorLocation || P.Config.Conductors.Count == 0)
        {
            Reset();
            return;
        }

        if(pendingLocation is not { } request)
        {
            return;
        }

        if(DateTime.UtcNow >= request.ExpiresAt)
        {
            Reset();
            PluginLog.Debug("Discarded expired auto-fly request");
            return;
        }

        if(!Player.Interactable
            || !IsScreenReady()
            || Svc.Condition[ConditionFlag.InCombat]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Casting]
            || Svc.Condition[ConditionFlag.MountOrOrnamentTransition]
            || Svc.ClientState.TerritoryType != request.TerritoryId
            || !IsVnavmeshReady())
        {
            return;
        }

        if(!Svc.Condition[ConditionFlag.InFlight])
        {
            if(!P.Config.AutoMountForAutoFly)
            {
                return;
            }
            if(!Svc.Condition[ConditionFlag.Mounted])
            {
                TaskMount.MountIfCan();
                return;
            }
            if(!Player.CanFly)
            {
                Reset();
                PluginLog.Warning("Unable to auto-fly in the current territory");
                return;
            }
        }

        if(!EzThrottler.Throttle("AutoFlyToConductorLocation", 500))
        {
            return;
        }

        var destination = S.VnavmeshIPC.PointOnFloor(new(request.Position.X, 1024, request.Position.Y), false, 5);
        if(destination != null && S.VnavmeshIPC.PathfindAndMoveTo(destination.Value, true))
        {
            Reset();
            PluginLog.Information($"Requested vnavmesh auto-flight to {destination.Value}");
        }
    }

    internal bool CanFlyToFlag()
    {
        if(!Player.Interactable
            || !IsScreenReady()
            || Svc.Condition[ConditionFlag.InCombat]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Casting]
            || Svc.Condition[ConditionFlag.MountOrOrnamentTransition]
            || !Svc.Condition[ConditionFlag.InFlight]
            || !IsVnavmeshReady())
        {
            return false;
        }

        var map = AgentMap.Instance();
        if(map == null || map->FlagMarkerCount == 0)
        {
            return false;
        }

        var flag = map->FlagMapMarkers[0];
        return flag.TerritoryId == Svc.ClientState.TerritoryType && flag.MapId == Svc.ClientState.MapId;
    }

    internal void FlyToFlag()
    {
        Chat.ExecuteCommand("/vnav flyflag");
        PluginLog.Information("Requested vnavmesh fly-to-flag navigation");
    }

    private static bool IsVnavmeshReady()
    {
        return Svc.PluginInterface.InstalledPlugins.Any(x => x.IsLoaded && x.InternalName == "vnavmesh")
            && S.VnavmeshIPC.IsReady();
    }
}
