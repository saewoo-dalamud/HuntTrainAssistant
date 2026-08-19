using ECommons.ExcelServices;
using HuntTrainAssistant.DataStructures;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntTrainAssistant.PluginUI;
public class TabIntegrations
{
    private TabIntegrations() { }
    public void Draw()
    {
        new NuiBuilder()
        .Section(Loc.Get("Integrations.Sections.Plugins"))
        .Widget(() =>
        {
            ImGui.Checkbox(Loc.Get("Integrations.EnableSonar"), ref P.Config.SonarIntegration);
            ImGuiEx.PluginAvailabilityIndicator([new("SonarPlugin", "Sonar")]);
            ImGui.Indent();
            ImGuiEx.TextWrapped(Loc.Get("Integrations.SonarDescription"));
            ImGui.Checkbox(Loc.Get("Integrations.AddTeleportLink"), ref P.Config.AutoVisitModifyChat);
            ImGui.Checkbox(Loc.Get("Integrations.ChangeInstance"), ref P.Config.EnableSonarInstanceSwitching);
            ImGui.Unindent();
            ImGui.Separator();
            ImGui.Checkbox(Loc.Get("Integrations.EnableHuntAlerts"), ref P.Config.HuntAlertsIntegration);
            ImGuiEx.PluginAvailabilityIndicator([new("HuntAlerts", new Version("1.2.1.3"))]);
            ImGuiEx.TextWrapped(Loc.Get("Integrations.HuntAlertsDescription"));
        })

        .Section(Loc.Get("Integrations.Sections.CommonSettings"))
        .Widget(() =>
        {
            ImGuiEx.TextWrapped(Loc.Get("Integrations.CommonDescription"));
            ImGui.Separator();
            ImGui.Checkbox(Loc.Get("Integrations.TeleportNearestAetheryte"), ref P.Config.AutoVisitTeleportEnabled);
            ImGuiEx.PluginAvailabilityIndicator([new("TeleporterPlugin", "Teleporter")]);
            ImGuiEx.PluginAvailabilityIndicator([new("Lifestream")]);
            ImGui.Checkbox(Loc.Get("Integrations.AllowCrossWorld"), ref P.Config.AutoVisitCrossWorld);
            ImGuiEx.PluginAvailabilityIndicator([new("TeleporterPlugin", "Teleporter"), new("Lifestream")]);
            ImGuiEx.PluginAvailabilityIndicator([new("Lifestream")]);
            ImGui.Checkbox(Loc.Get("Integrations.AllowCrossDatacenter"), ref P.Config.AutoVisitCrossDC);
            ImGuiEx.PluginAvailabilityIndicator([new("TeleporterPlugin", "Teleporter"), new("Lifestream")]);
            ImGuiEx.PluginAvailabilityIndicator([new("Lifestream")]);
            ImGuiEx.TreeNodeCollapsingHeader($"{Loc.Get("Integrations.BlacklistWorlds", P.Config.WorldBlacklist.Count)}###blworlds", DrawWorldBlacklist);
        })

        .Section(Loc.Get("Integrations.Sections.TriggerFilters"))
        .Widget(() =>
        {
            foreach(var rank in Enum.GetValues<Rank>())
            {
                if (rank == Rank.Unknown) continue;
                ImGui.PushID($"{rank}");
                if (!P.Config.AutoVisitExpansionsBlacklist.TryGetValue(rank, out var list))
                {
                    list = [];
                    P.Config.AutoVisitExpansionsBlacklist[rank] = list;
                }
                ImGuiEx.CollectionCheckbox($"{rank}", Enum.GetValues<Expansion>(), list, true);
                ImGui.Indent();
                foreach(var ex in Enum.GetValues<Expansion>())
                {
                    if(ex == Expansion.Unknown) continue;
                    ImGuiEx.CollectionCheckbox($"{ex}", ex, list, true);
                }
                ImGui.Unindent();
                ImGui.PopID();
            }
        })

        .Draw();
    }

    void DrawWorldBlacklist()
    {
        ImGuiEx.TextWrapped(Loc.Get("Integrations.WorldBlacklistDescription"));
        foreach(var r in Enum.GetValues<ExcelWorldHelper.Region>())
        {
            ImGuiEx.CollectionCheckbox($"{Loc.Get("Integrations.Region")} {r}", ExcelWorldHelper.GetPublicWorlds(r).Select(x => x.RowId), P.Config.WorldBlacklist);
            ImGui.Indent();
            foreach(var dc in ExcelWorldHelper.GetDataCenters(r))
            {
                ImGuiEx.CollectionCheckbox($"{dc.Name} {Loc.Get("Integrations.DataCenter")}", ExcelWorldHelper.GetPublicWorlds(dc.RowId).Select(x => x.RowId), P.Config.WorldBlacklist);
                ImGui.Indent();
                foreach(var w in ExcelWorldHelper.GetPublicWorlds(dc.RowId))
                {
                    ImGuiEx.CollectionCheckbox($"{w.Name}", w.RowId, P.Config.WorldBlacklist);
                }
                ImGui.Unindent();
            }
            ImGui.Unindent();
        }
    }
}
