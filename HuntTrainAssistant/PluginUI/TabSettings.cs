using Dalamud.Memory;
using Dalamud.Memory.Exceptions;
using ECommons.Automation;
using ECommons.Configuration;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using NightmareUI.PrimaryUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntTrainAssistant.PluginUI;
public unsafe class TabSettings
{
    OrderedDictionary<int, string> Mounts = [new KeyValuePair<int, string>(0, "Mount roulette"), .. Svc.Data.GetExcelSheet<Mount>().Where(x => x.Singular != "").OrderBy(x => x.Singular.GetText().ToUpper()).ToDictionary(x => (int)x.RowId, x => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(x.Singular.GetText()))];
    public void Draw()
		{
				ImGui.AlignTextToFramePadding();
				ImGui.Text(Loc.Get("Settings.Language"));
				ImGui.SameLine();
				var languages = LocalizationManager.AvailableLanguages;
				var languageIndex = Math.Max(0, Array.IndexOf(languages, P.Config.Language));
				ImGui.SetNextItemWidth(150f);
				if(ImGui.Combo("##Language", ref languageIndex, languages, languages.Length))
				{
						LocalizationManager.SetLanguage(languages[languageIndex]);
						EzConfig.Save();
				}
				ImGui.Separator();
				if(OpenFileDialog.IsSelecting())
				{
						ImGuiEx.Text(Loc.Get("Settings.WaitingForFile"));
						return;
				}
				Mounts[0] = Loc.Get("Settings.MountRoulette");
				new NuiBuilder().
						Section(Loc.Get("Settings.Sections.General"))
						.Widget(() =>
						{
								ImGui.Checkbox(Loc.Get("Settings.PluginEnabled"), ref P.Config.Enabled);
								ImGui.SameLine();
								ImGui.Checkbox(Loc.Get("Settings.DebugMode"), ref P.Config.Debug);
								ImGui.Checkbox(Loc.Get("Settings.AutoTeleportDifferentZone"), ref P.Config.AutoTeleport);
                ImGui.Indent();
                ImGui.Checkbox(Loc.Get("Settings.AutoSwitchInstanceOne"), ref P.Config.AutoSwitchInstanceToOne);
								ImGui.Checkbox(Loc.Get("Settings.MountAfterTeleport"), ref P.Config.UseMount);
								if(P.Config.UseMount)
								{
										ImGui.Indent();
                    ImGui.SetNextItemWidth(200f);
                    ImGuiEx.Combo(Loc.Get("Settings.PreferredMount"), ref P.Config.Mount, Mounts.Keys, names: Mounts);
                    ImGui.Unindent();
								}
                ImGui.Unindent();
                ImGui.Checkbox(Loc.Get("Settings.AutoOpenMap"), ref P.Config.AutoOpenMap);
								ImGui.Indent();
								ImGui.Checkbox(Loc.Get("Settings.NoDuplicateFlags"), ref P.Config.NoDuplicateFlags);
								ImGui.Unindent();
								ImGui.Checkbox(Loc.Get("Settings.SuppressOthers"), ref P.Config.SuppressChatOtherPlayers);
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
												ImGuiEx.SliderIntAsFloat(Loc.Get("Settings.ConductorTimeoutMinutes"), ref P.Config.ConductorInactivityTimeoutMinutes, 5, 120);
												ImGui.Unindent();
										}
										ImGui.Unindent();
								}
								ImGui.Checkbox(Loc.Get("Settings.AetheryteCompensation"), ref P.Config.DistanceCompensationHack);
								ImGui.Checkbox(Loc.Get("Settings.AutoNextInstance"), ref P.Config.AutoSwitchInstanceTwoRanks);
								ImGui.Checkbox(Loc.Get("Settings.ContextMenu"), ref P.Config.ContextMenu);
								ImGui.Checkbox(Loc.Get("Settings.EnablePartyFinderButton"), ref P.Config.PfinderEnable);
								ImGui.Indent();
								ImGuiEx.Text(Loc.Get("Settings.PartyFinderComment"));
								ImGuiEx.SetNextItemFullWidth();
								ImGui.InputText($"##pfindercommenr", ref P.Config.PfinderString, 150);
								ImGui.Unindent();
								ImGui.Checkbox(Loc.Get("Settings.RandomTeleportDelay"), ref P.Config.TeleportDelayEnabled);
								ImGui.Indent();
								ImGui.SetNextItemWidth(150f);
                ImGuiEx.SliderIntAsFloat(Loc.Get("Settings.MinimumDelay"), ref P.Config.TeleportDelayMin, 0, 1000);
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.SliderIntAsFloat(Loc.Get("Settings.MaximumDelay"), ref P.Config.TeleportDelayMax, 0, 1000);
                ImGui.Unindent();
						})
						.Section(Loc.Get("Settings.Sections.Notifications"))
						.Widget(() =>
						{
								ImGuiEx.Text(Loc.Get("Settings.NotificationMasterRequired"));
								ImGuiEx.PluginAvailabilityIndicator([new("NotificationMaster"), new("NotificationMaster.NXIV", "NotificationMaster (from NightmareXIV repo)")], "", false);
								ImGui.Checkbox(Loc.Get("Settings.PlayConductorAudio"), ref P.Config.AudioAlert);
								ImGui.Indent();
								ImGuiEx.InputWithRightButtonsArea(() => ImGui.InputTextWithHint("##pathToAudio", Loc.Get("Settings.AudioFilePath"), ref P.Config.AudioAlertPath, 500), () =>
								{
										if(ImGui.Button(Loc.Get("Common.Select")))
										{
												OpenFileDialog.SelectFile((x) =>
												{
														if(x != null) new TickScheduler(() => P.Config.AudioAlertPath = x.file);
												});
										}
										ImGui.SameLine();
										if(ImGuiEx.IconButton(FontAwesomeIcon.Play))
										{
												S.Notificator.PlaySound(P.Config.AudioAlertPath, P.Config.AudioAlertVolume, false, false);
										}
								});
								ImGui.SetNextItemWidth(150f);
								ImGui.SliderFloat(Loc.Get("Settings.Volume"), ref P.Config.AudioAlertVolume, 0f, 1f);
								ImGui.Checkbox(Loc.Get("Settings.AudioOnlyMinimized"), ref P.Config.AudioAlertOnlyMinimized);
                ImGui.SetNextItemWidth(150f);
                ImGuiEx.SliderIntAsFloat(Loc.Get("Settings.AudioMinimumInterval"), ref P.Config.AudioThrottle, 0, 10000);
                ImGui.Unindent();
                ImGui.Checkbox(Loc.Get("Settings.FlashTaskbar"), ref P.Config.FlashTaskbar);
								ImGui.Checkbox(Loc.Get("Settings.TrayNotification"), ref P.Config.TrayNotification);
            })
						.Section(Loc.Get("Settings.Sections.Triggers"))
						.Widget(() =>
						{
								ImGui.Checkbox(Loc.Get("Settings.ExecuteMacroOnFlag"), ref P.Config.ExecuteMacroOnFlag);
								if(P.Config.ExecuteMacroOnFlag)
								{
										ImGui.Indent();
										var m = RaptureMacroModule.Instance();
										var macroName = Loc.Get("Common.NotSet");
										if(P.Config.MacroIndex >= 0 && P.Config.MacroIndex < m->Shared.Length)
										{
												var macro = m->Shared[P.Config.MacroIndex];
												if(macro.IsNotEmpty())
												{
														macroName = MemoryHelper.ReadSeString(&macro.Name).ToString();
												}
										}
										if(ImGui.BeginCombo(Loc.Get("Settings.SelectSystemMacro"), macroName, ImGuiComboFlags.HeightLarge))
										{
												for(int i = 0; i < m->Shared.Length; i++)
												{
														if(m->Shared[i].IsNotEmpty())
														{
                                var macro = m->Shared[i];
                                macroName = MemoryHelper.ReadSeString(&macro.Name).ToString();
																if(ImGui.Selectable($"#{i + 1}: {macroName}", i == P.Config.MacroIndex))
																{
																		P.Config.MacroIndex = i;
																}
														}
												}
												ImGui.EndCombo();
										}
										ImGui.Unindent();
								}
						})
						.Draw();
		}
}
