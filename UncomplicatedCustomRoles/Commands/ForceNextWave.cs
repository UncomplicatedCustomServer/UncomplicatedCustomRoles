using CommandSystem;
using System;

namespace UncomplicatedCustomRoles.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class ForceNextWave : ParentCommand
    {
        public ForceNextWave() => LoadGeneratedCommands();

        public override string Command { get; } = "fne";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Share the UCR Debug logs with the developers";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Respawning.RespawnManager.Singleton.ForceSpawnTeam(Respawning.SpawnableTeamType.NineTailedFox);

            response = "OK";

            return true;
        }
    }
}