using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Structures;

#nullable enable
namespace UncomplicatedCustomRoles.API.Features
{
    public class InfiniteEffect
    {

        /// <summary>
        /// Get all of the infinite effects of a player if it's a custom role
        /// </summary>
        public static List<IUCREffect>? List(Player Player)
        {
            return List(Player.Id);
        }

        /// <summary>
        /// Get all of the infinite effects of a player if it's a custom role in a List of <see cref="IUCREffect"/>
        /// </summary>
        public static List<IUCREffect>? List(int Id)
        {
            if (Plugin.PermanentEffectStatus.ContainsKey(Id))
            {
                return Plugin.PermanentEffectStatus[Id];
            }
            return null;
        }

        /// <summary>
        /// Add a <see cref="IUCREffect"/> istance to the player. The duration must be less or equal to 0
        /// </summary>
        public static bool Add(IUCREffect Effect, int Id)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Id) && Plugin.PermanentEffectStatus.ContainsKey(Id) && Effect.Duration < 0)
            {
                Plugin.PermanentEffectStatus[Id].Add(Effect);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a <see cref="IUCREffect"/> istance to the player. The duration must be less or equal to 0
        /// </summary>
        public static bool Add(IUCREffect Effect, Player Player)
        {
            return Add(Effect, Player.Id);
        }

        /// <summary>
        /// Remove a <see cref="IUCREffect"/> istance to the player, if it exists.
        /// </summary>
        public static bool Remove(IUCREffect Effect, int Id)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Id) && Plugin.PermanentEffectStatus.ContainsKey(Id) && Effect.Duration < 0)
            {
                List<IUCREffect> Effects = Plugin.PermanentEffectStatus[Id].Where(effect => effect == Effect).ToList();
                if (Effects.Count() > 0)
                {
                    foreach (IUCREffect InternalEffect in Effects)
                    {
                        Plugin.PermanentEffectStatus[Id].Remove(InternalEffect);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a <see cref="IUCREffect"/> istance to the player, if it exists.
        /// </summary>
        public static bool Remove(IUCREffect Effect, Player Player)
        {
            return Remove(Effect, Player.Id);
        }

        /// <summary>
        /// Get the <see cref="CoroutineHandle"/> of the infinite role coroutine. Can be <see cref="null"/>.
        /// </summary>
        public static CoroutineHandle Coroutine()
        {
            return Plugin.Instance.Handler.EffectCoroutine;
        }

        /// <summary>
        /// Stops the <see cref="CoroutineHandle"/> of the infinite role coroutine.
        /// </summary>
        public static void Stop()
        {
            Timing.KillCoroutines(Coroutine());
        }

        /// <summary>
        /// Check if the <see cref="CoroutineHandle"/> of the infinite role is still running.
        /// </summary>
        public static bool IsRunning()
        {
            return Coroutine().IsRunning;
        }

        /// <summary>
        /// Start the <see cref="CoroutineHandle"/> of the infinite role coroutine again. Return <see cref="false"/> if the coroutine was already running.
        /// </summary>
        public static bool Start()
        {
            if (!IsRunning())
            {
                Plugin.Instance.Handler.EffectCoroutine = Timing.RunCoroutine(Plugin.Instance.Handler.DoSetInfiniteEffectToPlayers());
                return true;
            }
            return false;
        }
    }
}
