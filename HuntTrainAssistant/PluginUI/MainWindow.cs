using Dalamud.Interface.Style;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HuntTrainAssistant.Tasks;
using System.Runtime.Intrinsics.X86;

namespace HuntTrainAssistant.PluginUI;

public unsafe class MainWindow : ConfigWindow
{
    public MainWindow() : base()
    {
        TitleBarButtons.Add(new()
				{
						Click = (m) => { if (m == ImGuiMouseButton.Left) S.SettingsWindow.IsOpen = true; },
						Icon = FontAwesomeIcon.Cog,
						IconOffset = new(2, 2),
						ShowTooltip = () => ImGui.SetTooltip(Loc.Get("MainWindow.OpenSettings")),
				});
        TitleBarButtons.Add(new()
        {
            Click = (m) => { if(P.Config.PfinderEnable) { TaskCreateHuntPF.Enqueue(); } else { DuoLog.Warning(Loc.Get("Messages.PartyFinderDisabled")); } },
            Icon = FontAwesomeIcon.PeopleGroup,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip(Loc.Get("MainWindow.CreatePartyFinder")),
        });
		}

    public override void Draw()
    {
				try
				{
						ImGui.SetNextItemWidth(150f);
						var condIndex = 0;
						var condNames = P.Config.Conductors.Select(x => x.Name).ToArray();
						ImGuiEx.Text(Loc.Get("MainWindow.CurrentConductors"));
						ImGui.SameLine();
						if(ImGui.SmallButton(Loc.Get("Common.Clear")))
						{
								P.Config.Conductors.Clear();
						}
						ImGui.SameLine();
						// Remove selected conductor
						if(ImGui.SmallButton(Loc.Get("MainWindow.RemoveSelected")))
						{
								if(condIndex >= 0 && condIndex < P.Config.Conductors.Count)
								{
										P.Config.Conductors.RemoveAt(condIndex);
								}
						}
						ImGuiEx.SetNextItemFullWidth();
						ImGui.ListBox("##conds", ref condIndex, condNames, Math.Clamp(condNames.Length, 1, 3));
						ImGuiEx.Text(Loc.Get("MainWindow.AddConductor"));
						ImGui.SameLine();
						ImGui.SetNextItemWidth(150f);
						var newCond = "";
						if(ImGui.InputText("##newCond", ref newCond, 50, ImGuiInputTextFlags.EnterReturnsTrue))
						{
								if(newCond.Length > 0)
								{
										P.Config.Conductors.Add(new(newCond, 0));
										newCond = "";
								}
						}
						if(P.TeleportTo == null)
						{
								ImGuiEx.Text(ImGuiColors.DalamudGrey3, Loc.Get("MainWindow.AutoTeleportInactive"));
								if(ChatMessageHandler.LastMessageLoc != null && ImGui.Button(Loc.Get("MainWindow.AutoTeleportTo", ChatMessageHandler.LastMessageLoc.Aetheryte.PlaceName.Value.Name)))
								{
										P.TeleportTo = ChatMessageHandler.LastMessageLoc;
								}
						}
						else
						{
								ImGuiEx.Text(Loc.Get("MainWindow.AutoTeleportActive"));
								ImGui.SameLine();
								if(ImGui.SmallButton(Loc.Get("Common.Cancel")))
								{
										PluginLog.Debug($"TeleportTo reset (3)");
										P.TeleportTo = null;
								}
								ImGuiEx.Text($"{P.TeleportTo?.Aetheryte.GetPlaceName()}@{ExcelTerritoryHelper.GetName(P.TeleportTo?.Territory ?? 0)} i{P.TeleportTo?.Instance}");
						}
						if(CanFlyToFlag() && ImGui.Button(Loc.Get("MainWindow.FlyToFlag")))
						{
								Chat.ExecuteCommand("/vnav flyflag");
								PluginLog.Information("Requested vnavmesh fly-to-flag navigation");
						}
						if(P.TaskManager.IsBusy)
						{
								ImGuiEx.Text(Loc.Get("MainWindow.TasksProcessing", P.TaskManager.NumQueuedTasks));
								ImGui.SameLine();
								if(ImGui.SmallButton($"{Loc.Get("Common.Stop")}##tm"))
								{
										P.TaskManager.Abort();
								}
						}
						else
						{
								ImGuiEx.Text(ImGuiColors.DalamudGrey3, Loc.Get("MainWindow.TaskManagerInactive"));
						}
						ImGui.Checkbox(Loc.Get("MainWindow.SonarAutoTeleport"), ref P.Config.AutoVisitTeleportEnabled);
						if(P.Config.AutoVisitTeleportEnabled)
						{
								if(!Utils.IsInHuntingTerritory())
								{
										ImGuiEx.HelpMarker(Loc.Get("MainWindow.TeleportEnabledOutsideHuntZone"), EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
								}
								else
								{
										ImGuiEx.HelpMarker(Loc.Get("MainWindow.TeleportDisabledInsideHuntZone"), EColor.RedBright, "\uf00d");
								}
								ImGui.SameLine();
								ImGui.Checkbox("C/W", ref P.Config.AutoVisitCrossWorld);
								ImGui.SameLine();
								ImGui.Checkbox("C/DC", ref P.Config.AutoVisitCrossDC);
						}
						if(S.SonarMonitor.Continuation != null)
						{
								ImGuiEx.Text(GradientColor.Get(EColor.RedBright, EColor.YellowBright), Loc.Get("MainWindow.WaitingToArrive", S.SonarMonitor.Continuation.World, S.SonarMonitor.Continuation.Aetheryte.GetPlaceName(), S.SonarMonitor.Continuation.Instance));
								if(ImGui.SmallButton($"{Loc.Get("Common.Cancel")}##arrival"))
								{
										S.SonarMonitor.Continuation = null;
								}
						}
				}
				catch(Exception e)
				{
						e.LogWarning();
				}
    }

    private static bool CanFlyToFlag()
    {
        if(!Player.Interactable
            || !IsScreenReady()
            || Svc.Condition[ConditionFlag.InCombat]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Casting]
            || Svc.Condition[ConditionFlag.MountOrOrnamentTransition]
            || !Svc.Condition[ConditionFlag.InFlight]
            || !Svc.PluginInterface.InstalledPlugins.Any(x => x.IsLoaded && x.InternalName == "vnavmesh")
            || !S.VnavmeshIPC.IsReady())
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

    static void Help()
    {
        ImGuiEx.TextWrapped(Loc.Get("MainWindow.Help.HuntZone"));
        ImGuiEx.TextWrapped(Loc.Get("MainWindow.Help.AssignConductors"));
    }
}
