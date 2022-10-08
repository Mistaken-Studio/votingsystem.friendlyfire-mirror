﻿// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Mistaken.VotingSystem.FriendlyFire
{
    internal class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("If true, then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        [Description("Analytics settings")]
        public string AnalyticsLogin { get; set; }

        public string AnalyticsPassword { get; set; }
    }
}
