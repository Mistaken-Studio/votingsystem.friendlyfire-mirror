// -----------------------------------------------------------------------
// <copyright file="VotingHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
        }

        private static readonly List<Player> PlayersVoted = new ();

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

        private void Server_RoundStarted()
        {
            if (PlayersVoted.Count - (RealPlayers.List.Count() / 2) < 0)
            {
                Server.FriendlyFire = true;
                Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireEnabledInfo);
                return;
            }

            Map.Broadcast(10, PluginHandler.Instance.Translation.RoundStartedFriendlyFireDisabledInfo);
            Server.FriendlyFire = false;
        }

        private void Server_WaitingForPlayers()
        {
            PlayersVoted.Clear();
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
                ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.BOTTOM, PluginHandler.Instance.Translation.PlayerVotedInfo, 5);
                return;
            }

            ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.BOTTOM, PluginHandler.Instance.Translation.PlayerAlreadyVotedInfo, 3);
        }

        private void Player_Verified(Exiled.Events.EventArgs.VerifiedEventArgs ev)
        {
            ev.Player.SetGUI("FriendlyFire-InfoVoted", PseudoGUIPosition.MIDDLE, "<br><br><br><br><br><br><br>" + PluginHandler.Instance.Translation.PlayerVerifiedVotingInfo, 5);
        }
    }
}
