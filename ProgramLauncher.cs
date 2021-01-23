﻿using Inferno_Mod_Manager.InfernoMods;
using Inferno_Mod_Manager.MelonMods;
using Inferno_Mod_Manager.Utils;
using Steamworks;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Inferno_Mod_Manager.Properties;

namespace Inferno_Mod_Manager
{
    class ProgramLauncher
    {
        [STAThread]
        public static void Main(string[] args) {
            MainWindow.SetupSettings();
            if (!ProcessHelpers.IsVC2019x64Installed() && !Storage.Settings.ShownVCScreen) {
                MessageBox.Show("You do not have Visual C installed! Continue to install it.\nTHIS WILL NOT BE SHOWN AGAIN!\nDO NOT REPORT THE APP NOT RUNNING IF YOU DIDNT INSTALL THIS");
                Process.Start("https://aka.ms/vs/16/release/VC_redist.x64.exe");
                Storage.Settings.ShownVCScreen = true;
                throw new("Need Visual C Installed!");
            }

            try {
                SteamClient.Init(960090);
                Storage.InstallDir = SteamApps.AppInstallDir();
                MelonHandler.EnsureMelonInstalled();
                FileAssociations.EnsureAssociationsSet();
            }
            catch (Exception e) {
                MessageBox.Show("ERROR 0x3ef93 PLEASE REPORT IN THE DISCORD");
            }

            _ = Directory.CreateDirectory(Storage.InstallDir + @"\Mods\Inferno");
            _ = Directory.CreateDirectory(Storage.InstallDir + @"\Mods\Inferno\Disabled");
            _ = Directory.CreateDirectory(Storage.InstallDir + @"\Mods");
            _ = Directory.CreateDirectory(Storage.InstallDir + @"\Mods\Disabled");
            _ = Directory.CreateDirectory(Environment.ExpandEnvironmentVariables("%AppData%\\InfernoOmnia\\"));
            if (args.Length != 0)
                foreach (var file in args)
                    if (!file.Contains(@"\Mods\Inferno")) {
                        if (File.Exists(Storage.InstallDir + @"\Mods\Inferno\" + Path.GetFileName(file)))
                            File.Delete(Storage.InstallDir + @"\Mods\Inferno\" + Path.GetFileName(file));
                        File.Move(file, Storage.InstallDir + @"\Mods\Inferno\" + Path.GetFileName(file));
                    }

            if (Directory.GetFiles(Storage.InstallDir + @"\Mods\Inferno").Combine(Directory.GetFiles(Storage.InstallDir + @"\Mods\Inferno\Disabled")).Length > 0) {
                var flag = false;
                foreach (var file in Directory.GetFiles(Storage.InstallDir + @"\Mods", "*.dll").Combine(Directory.GetFiles(Storage.InstallDir + @"\Mods\Inferno\Disabled"))) {
                    MelonHandler.GetMelonAttrib(file, out var att);
                    if (att != null)
                        flag |= att.Name.Equals("Inferno API Injector");
                }
                if (!flag)
                    File.Create(Storage.InstallDir + @"\Mods\Inferno API Injector.dll").Write(Resources.Inferno_API_Injector, 0, Resources.Inferno_API_Injector.Length);
            }
            var app = new App {ShutdownMode = ShutdownMode.OnMainWindowClose};
            app.InitializeComponent();
            app.Run();
        }
    }
}
