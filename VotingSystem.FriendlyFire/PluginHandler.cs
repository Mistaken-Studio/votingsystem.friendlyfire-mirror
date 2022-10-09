// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using Mistaken.Updater.API.Config;

namespace Mistaken.VotingSystem.FriendlyFire
{
    internal sealed class PluginHandler : Plugin<Config, Translations>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "VotingSystem-FriendlyFire";

        public override string Prefix => "MVS-FF";

        public override PluginPriority Priority => PluginPriority.Default;

        public override Version RequiredExiledVersion => new(5, 2, 2);

        public AutoUpdateConfig AutoUpdateConfig => new()
        {
            Type = this.Config.SourceType,
            Url = this.Config.Url,
        };

        public override void OnEnabled()
        {
            Instance = this;

            harmony = new Harmony("com.votingsystem.friendlyfire.patch");
            harmony.PatchAll();

            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins += this.CustomEvents_LoadedPlugins;

            new VotingHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins -= this.CustomEvents_LoadedPlugins;

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        internal static Mistaken.API.Diagnostics.Module AtksModule { get; private set; }

        private static Harmony harmony;

        private void CustomEvents_LoadedPlugins()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).FirstOrDefault(x => x.FullName == "Mistaken.AntyTeamKillSystem.AntyTeamkillHandler");
            if (type is not null)
                AtksModule = (Mistaken.API.Diagnostics.Module)AccessTools.PropertyGetter(type, "Instance").Invoke(null, null);
        }
    }
}
