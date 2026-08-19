using ECommons.Funding;
using ECommons.SimpleGui;
using NightmareUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntTrainAssistant.PluginUI;
public unsafe class SettingsWindow : ConfigWindow
{
		private static string LocalizedWindowName => $"{Loc.Get("SettingsWindow.Title")}###HuntTrainAssistant Configuration";

		public TabSettings TabSettings = new();
		public TabDebug TabDebug = new();

		private SettingsWindow() : base()
		{
				WindowName = LocalizedWindowName;
				EzConfigGui.WindowSystem.AddWindow(this);
		}

		public override void Draw()
		{
				WindowName = LocalizedWindowName;
				PatreonBanner.DrawRight();
				ImGuiEx.EzTabBar("Bar", PatreonBanner.Text,
            ($"{Loc.Get("SettingsWindow.Tabs.Settings")}###Settings", TabSettings.Draw, null, true),
            ($"{Loc.Get("SettingsWindow.Tabs.Integrations")}###Integrations", S.TabIntegrations.Draw, null, true),
            ($"{Loc.Get("SettingsWindow.Tabs.AetheryteBlacklist")}###AetheryteBlacklist", S.TabAetheryteBlacklist.Draw, null, true),
            ($"{Loc.Get("SettingsWindow.Tabs.Debug")}###Debug", TabDebug.Draw, ImGuiColors.DalamudGrey3, true),
						($"{Loc.Get("SettingsWindow.Tabs.Log")}###Log", InternalLog.PrintImgui, ImGuiColors.DalamudGrey3, false)
						);
		}
}
