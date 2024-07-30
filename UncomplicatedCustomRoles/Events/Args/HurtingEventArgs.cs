using PlayerStatsSystem;
using PluginAPI.Core;
using UncomplicatedCustomRoles.Events.Interfaces;

namespace UncomplicatedCustomRoles.Events.Args
{
    public class HurtingEventArgs : EventArgs, IPlayerEvent, IDeniableEvent, IAttackerEvent
    {
        /// <summary>
        /// Gets the <see cref="ReferenceHub"/> of the hurted player
        /// </summary>
        public ReferenceHub Hub { get; }

        /// <summary>
        /// Gets the hurted <see cref="PluginAPI.Core.Player"/>
        /// </summary>
        public Player Player { get; }

        /// <summary>
        /// Gets the <see cref="ReferenceHub"/> of the attacker
        /// </summary>
        public ReferenceHub AttackerHub { get; }

        /// <summary>
        /// Gets the attacker's <see cref="PluginAPI.Core.Player"/>
        /// </summary>
        public Player Attacker { get; }

        /// <summary>
        /// Gets or sets whether the event is allowed or not
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="AttackerDamageHandler"/> instance for this event
        /// </summary>
        public AttackerDamageHandler DamageHandler { get; }

        public HurtingEventArgs(ReferenceHub player, AttackerDamageHandler damageHandler)
        {
            Hub = player;
            Player = Player.Get(player);
            AttackerHub = damageHandler.Attacker.Hub;
            Attacker = Player.Get(damageHandler.Attacker.Hub);
            DamageHandler = damageHandler;
        }
    }
}
