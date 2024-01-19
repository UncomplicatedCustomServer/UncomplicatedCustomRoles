using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;
using PowerYaml;
using PowerYaml.Elements;
using UncomplicatedCustomRoles.Elements;
using UncomplicatedCustomRoles.Structures;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UncomplicatedCustomRoles.Manager
{
    internal class ScriptConfig
    {
        public string Dir = Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", "YamlPowerScripts");

        public bool Is()
        {
            return Directory.Exists(Dir);
        }

        public string[] List()
        {
            return Directory.GetFiles(Dir);
        }

        public void Init()
        {
            if (!Is())
            {
                Directory.CreateDirectory(Dir);
                Log.Warn($"The directory of YamlPowerScripts didn't exist, created a new one at {Dir}!");
                Log.Info("The function of YamlPowerScript has NOT been enabled due to the folder didn't exist\nTIP: Restart the server to enable that!");
                ISerializer Serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                List<Condition> Conditions = new() {
                    new(ConditionType.Team, null, ComparisonType.EqualThan, PlayerRoles.Team.Scientists)
                };
                List<Executor> Executors = new() {
                    new(CollectionType.Player, "Kill", null)
                };
                File.WriteAllText(Path.Combine(Dir, "example-script.yml"), Serializer.Serialize(new Dictionary<string, Dictionary<PlayerEvent, Action>>() {
                  {
                    "events", new ()
                    {
                        { PlayerEvent.Jumping, new(Conditions, false, Executors) }
                    }
                  }
                }));
                return;
            }
            foreach (ICustomRole CustomRole in Plugin.CustomRoles.Values)
            {
                Power PowerElement = new();
                int Loaded = 0;
                Log.Debug($"Script PPS: {CustomRole.PowerYamlScripts}");
                if (CustomRole.PowerYamlScripts is not null)
                {
                    foreach (string ScriptPath in CustomRole.PowerYamlScripts)
                    {
                        Log.Debug($"Trying to load script from {ScriptPath} - Complete: {Path.Combine(Dir, ScriptPath)}");
                        if (File.Exists(Path.Combine(Dir, ScriptPath)))
                        {
                            IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                            Dictionary<string, Dictionary<PlayerEvent, Action>> Script = Deserializer.Deserialize<Dictionary<string, Dictionary<PlayerEvent, Action>>>(File.ReadAllText(Path.Combine(Dir, ScriptPath)));
                            if (!Script.ContainsKey("events"))
                            {
                                Log.Warn($"The PowerYaml script located at {Path.Combine(Dir, ScriptPath)} is not valid!\nTIP: The file must start with 'events:'");
                                continue;
                            }
                            else
                            {
                                foreach (KeyValuePair<PlayerEvent, Action> Event in Script["events"])
                                {
                                    PowerElement.RegisterEvent(Event.Key, Event.Value);
                                    Loaded++;
                                    Log.Send($"Successfully loaded event {Event.Key} for CustomRole {CustomRole.Name} [{CustomRole.Id}]\nFile: {Path.Combine(Dir, ScriptPath)}", Discord.LogLevel.Info, System.ConsoleColor.Green);
                                }
                            }
                        }
                    }
                }
                Log.Info($"Successfully loaded {Loaded} event(s) for CustomRole {CustomRole.Name} [{CustomRole.Id}]");
                /*
                if (PowerElement.Events.Count > 0)
                {
                    PowerElement.AddCoreExecutor("SetCustomRole", (Player Player, Dictionary<string, dynamic> Args) =>
                    {
                        if (!Args.ContainsKey("id"))
                        {
                            return;
                        }
                        int Id;
                        if (Args["id"] is string)
                        {
                            int.TryParse(Args["id"], out Id);
                        } else if (Args["id"] is int)
                        {
                            Id = Args["id"];
                        }
                        else
                        {
                            return;
                        }
                        SpawnManager.SummonCustomSubclass(Player, Id);
                    })
                    Plugin.RoleActions.Add(CustomRole.Id, PowerElement);
                }
                */
            }
            Log.Info("Finished the loading of PowerYaml Scripts for every CustomRole");
        }
    }
}
