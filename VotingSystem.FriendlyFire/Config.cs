// -----------------------------------------------------------------------
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

        [Description("If set, then this endpoint will be used for sending data for analytics")]
        public string EndpointForAnalytics { get; set; }
    }
}
