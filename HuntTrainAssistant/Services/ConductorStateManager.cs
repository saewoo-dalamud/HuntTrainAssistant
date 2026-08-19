namespace HuntTrainAssistant.Services;

internal sealed class ConductorStateManager
{
    private DateTime? lastActivity;

    internal void RecordActivity()
    {
        lastActivity = DateTime.UtcNow;
    }

    internal void HandleTerritoryChanged()
    {
        if(P.Config.ClearConductorsOutsideHuntingTerritory && !Utils.IsInHuntingTerritory())
        {
            Clear();
        }
    }

    internal void Update()
    {
        if(P.Config.Conductors.Count == 0)
        {
            lastActivity = null;
            return;
        }

        lastActivity ??= DateTime.UtcNow;
        if(P.Config.ClearConductorsOutsideHuntingTerritory
            || !P.Config.ClearInactiveConductors
            || P.Config.ConductorInactivityTimeoutMinutes <= 0
            || DateTime.UtcNow - lastActivity.Value < TimeSpan.FromMinutes(P.Config.ConductorInactivityTimeoutMinutes)
            || Svc.Condition[ConditionFlag.InCombat]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51])
        {
            return;
        }

        Clear();
        PluginLog.Information($"Cleared conductors after {P.Config.ConductorInactivityTimeoutMinutes} minutes of inactivity");
    }

    internal void Clear()
    {
        P.Config.Conductors.Clear();
        lastActivity = null;
    }
}
