// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace Mistaken.VotingSystem.FriendlyFire
{
    internal sealed class PluginHandler : Plugin<Config, Translations>
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "VotingSystem-FriendlyFire";

        public override string Prefix => "MVS-FF";

        public override PluginPriority Priority => PluginPriority.Default;

        public override Version RequiredExiledVersion => new (5, 2, 2);

        public override void OnEnabled()
        {
            Instance = this;

            harmony = new HarmonyLib.Harmony("com.votingsystem.friendlyfire.patch");
            harmony.PatchAll();

            new VotingHandler(this);

            API.Diagnostics.Module.OnEnable(this);

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            API.Diagnostics.Module.OnDisable(this);

            base.OnDisabled();
        }

        internal static PluginHandler Instance { get; private set; }

        private static HarmonyLib.Harmony harmony;
    }
}
