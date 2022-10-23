// -----------------------------------------------------------------------
// <copyright file="VotingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;

namespace Mistaken.VotingSystem.FriendlyFire
{
    internal sealed class VotingHandler : Module
    {
        public VotingHandler(IPlugin<IConfig> plugin)
            : base(plugin)
        {
            if (string.IsNullOrEmpty(PluginHandler.Instance.Config.AnalyticsLogin) || string.IsNullOrEmpty(PluginHandler.Instance.Config.AnalyticsPassword))
                return;

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PluginHandler.Instance.Config.AnalyticsLogin}:{PluginHandler.Instance.Config.AnalyticsPassword}")));
        }

        public override string Name => nameof(VotingHandler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.Transmitting += this.Player_Transmitting;
            Exiled.Events.Handlers.Player.Verified += this.Player_Verified;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Server_WaitingForPlayers;
            Exiled.Events.Handlers.Player.Transmitting -= this.Player_Transmitting;
            Exiled.Events.Handlers.Player.Verified -= this.Player_Verified;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
        }

        private static readonly List<Player> PlayersVoted = new();

        private static readonly HttpClient HttpClient = new();

        private static bool friendlyFireEnabled = true;

        private static IEnumerator<float> UpdateVotingInfo()
        {
            while (!Round.IsStarted)
            {
                foreach (var player in RealPlayers.List.ToArray())
                    player.SetGUI("FriendlyFire-Info", PseudoGUIPosition.TOP, string.Format(PluginHandler.Instance.Translation.VotingInfo, PlayersVoted.Count, RealPlayers.List.Count()));

                yield return Timing.WaitForSeconds(1f);
            }

            foreach (var player in RealPlayers.List.ToArray())
                player.SetGUI("FriendlyFire-Info", PseudoGUIPosition.TOP, null);
        }

        private static async Task SendAnalytics(int players)
        {
            if (HttpClient.DefaultRequestHeaders.Authorization is null)
            {
                Exiled.API.Features.Log.Info("Not sending analytics, Authorization is null");
                return;
            }

            // var toSend = $"{{\"isOn\":{friendlyFireEnabled.ToString().ToLower()},\"onlinePlayers\":{players},\"votePlayers\":{PlayersVoted.Count}}}";
            var toSend = $"{{\"isOn\":{friendlyFireEnabled.ToString().ToLower()},\"onlinePlayers\":{players},\"votePlayers\":{players - PlayersVoted.Count}}}";
            var content = new StringContent(toSend, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await HttpClient.PostAsync("https://new-api.mistaken.pl/scpsl/friendlyFireStats", content);
            Exiled.API.Features.Log.Debug(response, PluginHandler.Instance.Config.VerbouseOutput);
        }

        private void Server_RoundStarted()
        {
            static void RemoveVotingInfo()
            {
                foreach (var player in RealPlayers.List.ToArray())
                    player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.MIDDLE, null);
            }

            this.CallDelayed(0.5f, RemoveVotingInfo, nameof(RemoveVotingInfo));

            var players = RealPlayers.List.Count();

            if (PluginHandler.AtksModule is not null)
                PluginHandler.AtksModule.OnDisable();

            /*if ((PlayersVoted.Count - (players / 2f)) < 0)
            {
                if (PluginHandler.AtksModule is not null)
                    PluginHandler.AtksModule.OnEnable();

                friendlyFireEnabled = true;
                Task.Run(async () => await SendAnalytics(players));
                Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireEnabledInfo);
                return;
            }

            friendlyFireEnabled = false;
            Task.Run(async () => await SendAnalytics(players));
            Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireDisabledInfo);
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;*/

            if ((PlayersVoted.Count - (players / 2f)) < 0)
            {
                friendlyFireEnabled = false;
                Task.Run(async () => await SendAnalytics(players));
                Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireDisabledInfo);
                Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
                return;
            }

            if (PluginHandler.AtksModule is not null)
                PluginHandler.AtksModule.OnEnable();

            friendlyFireEnabled = true;
            Task.Run(async () => await SendAnalytics(players));
            Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireEnabledInfo);
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Attacker is null)
                return;

            if (ev.Attacker == ev.Target)
                return;

            if (ev.Attacker.Role.Team == Team.CDP && ev.Target.Role.Team == Team.CDP)
                return;

            if (!HitboxIdentity.CheckFriendlyFire(ev.Attacker.Role.Team, ev.Target.Role.Team, true))
                ev.IsAllowed = false;
        }

        private void Server_WaitingForPlayers()
        {
            PlayersVoted.Clear();
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            this.RunCoroutine(UpdateVotingInfo(), nameof(UpdateVotingInfo));
        }

        private void Player_Transmitting(Exiled.Events.EventArgs.TransmittingEventArgs ev)
        {
            if (Round.IsStarted)
                return;

            if (!ev.IsTransmitting)
                return;

            if (!PlayersVoted.Contains(ev.Player))
            {
                PlayersVoted.Add(ev.Player);
                ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.MIDDLE, "<br><br><br><br><br><br><br>" + PluginHandler.Instance.Translation.PlayerVotedInfo, 5);
                return;
            }

            ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.MIDDLE, "<br><br><br><br><br><br><br>" + PluginHandler.Instance.Translation.PlayerAlreadyVotedInfo, 3);
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            if (Round.IsStarted)
            {
                if (friendlyFireEnabled)
                    ev.Player.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireEnabledInfo);
                else
                    ev.Player.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireDisabledInfo);

                return;
            }

            ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.MIDDLE, "<br><br><br><br><br><br><br>" + PluginHandler.Instance.Translation.PlayerVerifiedVotingInfo);
        }
    }
}
