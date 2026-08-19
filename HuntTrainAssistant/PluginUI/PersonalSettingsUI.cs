namespace HuntTrainAssistant.PluginUI;

internal static class PersonalSettingsUI
{
    internal static void DrawAutoFlight()
    {
        ImGui.Checkbox(Loc.Get("Settings.AutoFlyAfterCombat"), ref P.Config.AutoFlyToConductorLocation);
        ImGuiEx.PluginAvailabilityIndicator([new("vnavmesh")]);
        if(P.Config.AutoFlyToConductorLocation)
        {
            ImGui.Indent();
            ImGui.Checkbox(Loc.Get("Settings.AutoMountForFlight"), ref P.Config.AutoMountForAutoFly);
            ImGui.Unindent();
        }
    }

    internal static void DrawConductorPersistence()
    {
        var keepConductors = !P.Config.ClearConductorsOutsideHuntingTerritory;
        if(ImGui.Checkbox(Loc.Get("Settings.KeepConductorsOutsideTerritory"), ref keepConductors))
        {
            P.Config.ClearConductorsOutsideHuntingTerritory = !keepConductors;
        }
        if(keepConductors)
        {
            ImGui.Indent();
            ImGui.Checkbox(Loc.Get("Settings.ClearInactiveConductors"), ref P.Config.ClearInactiveConductors);
            if(P.Config.ClearInactiveConductors)
            {
                ImGui.Indent();
                ImGui.SetNextItemWidth(150f);
                ImGui.SliderInt(Loc.Get("Settings.ConductorTimeoutMinutes"), ref P.Config.ConductorInactivityTimeoutMinutes, 5, 120);
                ImGui.Unindent();
            }
            ImGui.Unindent();
        }
    }
}
