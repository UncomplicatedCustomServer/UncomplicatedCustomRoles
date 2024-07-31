using PlayerRoles;
using PluginAPI.Core;
using Respawning;
using UncomplicatedCustomRoles.Events.Interfaces;
using static Escape;

namespace UncomplicatedCustomRoles.Events.Args
{
    public class EscapingEventArgs : EventArgs, IPlayerEvent, IDeniableEvent
    {
        /// <summary>
        /// Gets the <see cref="ReferenceHub"/> of the hurted player
        /// </summary>
        internal ReferenceHub Hub { get; }

        /// <summary>
        /// Gets the hurted <see cref="PluginAPI.Core.Player"/>
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets or sets whether the event is allowed or not
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets or sets the new role that will be granted after the escape
        /// </summary>
        public RoleTypeId NewRole { get; set; }

        /// <summary>
        /// Gets or sets the team that will receive the tokens
        /// </summary>
        public SpawnableTeamType Team { get; set; }

        /// <summary>
        /// Gets or sets the escape scenario
        /// </summary>
        public EscapeScenarioType Scenario { get; set; }

        /// <summary>
        /// Gets or sets the amout of tokens that the escape would grant
        /// </summary>
        public float Tokens { get; set; }

        ReferenceHub IPlayerEvent.Hub => throw new System.NotImplementedException();

        public EscapingEventArgs(ReferenceHub hub, RoleTypeId role, EscapeScenarioType scenarioType, SpawnableTeamType team, float tokens)
        {
            Hub = hub;
            Player = Player.Get(hub);
            Scenario = Scenario;
            NewRole = role;
            Team = team;
            Tokens = tokens;
        }
    }
}
