// -----------------------------------------------------------------------
// <copyright file="Translations.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;

namespace Mistaken.VotingSystem.FriendlyFire
{
    internal class Translations : ITranslation
    {
        public string PlayerVerifiedVotingInfo { get; set; } = "Naciśnij klaiwsz '<color=blue>V</color>' aby oddać głos za <color=green>wyłączeniem</color> <color=yellow>ognia bratobójczego</color>";

        public string PlayerVotedInfo { get; set; } = "Oddałeś głos za <color=green>wyłączeniem</color> <color=yellow>ognia bratobójczego</color> na tą rundę!";

        public string PlayerAlreadyVotedInfo { get; set; } = "Już oddałeś swój głos na tą rundę!";

        public string VotingInfo { get; set; } = "Za <color=green>wyłączeniem</color> <color=yellow>ognia bratobójczego</color> w tej rundzie zagłosowało: <color=green>{0} na {1}</color>";

        public string RoundStartedFriendlyFireEnabledInfo { get; set; } = "W tej rundzie <color=yellow>ogień bratobójczy</color> jest <color=red>włączony</color>!";

        public string RoundStartedFriendlyFireDisabledInfo { get; set; } = "W tej rundzie <color=yellow>ogień bratobójczy</color> jest <color=green>wyłączony</color>!";
    }
}
