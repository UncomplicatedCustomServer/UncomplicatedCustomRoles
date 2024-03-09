using Exiled.API.Features;
using System.Collections.Generic;
using System;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using UncomplicatedCustomRoles.Events;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";
        public override string Prefix => "UncomplicatedCustomRoles";
        public override string Author => "FoxWorn3365, Dr.Agenda";
        public override Version Version { get; } = new(1, 8, 0);
        public override Version RequiredExiledVersion { get; } = new(8, 8, 0);
        public static Plugin Instance;
        internal Handler Handler;
        internal ExternalPlayerEventHandler ExternalHandler;
        public static Dictionary<int, ICustomRole> CustomRoles;
        public static Dictionary<int, int> PlayerRegistry = new();
        // RolesCount: RoleId => [PlayerId, PlayerId]
        public static Dictionary<int, List<int>> RolesCount = new();
        public static Dictionary<int, List<IUCREffect>> PermanentEffectStatus = new();
        public static List<int> RoleSpawnQueue = new();
        public bool DoSpawnBasicRoles = false;
        public string PresenceUrl = "https://ucs.fcosma.it/api/plugin/presence";
        internal FileConfigs FileConfigs;
        public override void OnEnabled()
        {
            Instance = this;

            Handler = new();
            ExternalHandler = new();
            CustomRoles = new();

            FileConfigs = new();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.Spawning += Handler.OnSpawning;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            Scp049Handler.StartingRecall += Handler.OnScp049StartReviving;

            // Player Events for the external handler ONLY if the config agree
            if (Config.EnableExternalEventHandler)
            {
                PlayerHandler.Dying += ExternalHandler.OnDying;
                PlayerHandler.UsingItem += ExternalHandler.OnUsingItem;
                PlayerHandler.CancellingItemUse += ExternalHandler.OnCancellingUsingItem;
                PlayerHandler.SpawningRagdoll += ExternalHandler.OnSpawningRagdoll;
                PlayerHandler.ActivatingWarheadPanel += ExternalHandler.OnActivatingWarheadPanel;
                PlayerHandler.ActivatingGenerator += ExternalHandler.OnActivatingGenerator;
                PlayerHandler.ActivatingWorkstation += ExternalHandler.OnActivatingWorkstation;
                PlayerHandler.DeactivatingWorkstation += ExternalHandler.OnDeactivatingWorkstation;
                PlayerHandler.Hurting += ExternalHandler.OnHurting;
                PlayerHandler.DroppingItem += ExternalHandler.OnDroppingItem;
                PlayerHandler.PickingUpItem += ExternalHandler.OnPickingUpItem;
                PlayerHandler.Handcuffing += ExternalHandler.OnHandcuffing;
                PlayerHandler.RemovingHandcuffs += ExternalHandler.OnRemovingHandcuffs;
                PlayerHandler.IntercomSpeaking += ExternalHandler.OnIntercomSpeaking;
                PlayerHandler.Shooting += ExternalHandler.OnShooting;
                PlayerHandler.EnteringPocketDimension += ExternalHandler.OnEnteringPocketDimension;
                PlayerHandler.EscapingPocketDimension += ExternalHandler.OnEscapingPocketDimension;
                PlayerHandler.EnteringKillerCollision += ExternalHandler.OnEnteringKillerCollision;
                PlayerHandler.ReloadingWeapon += ExternalHandler.OnReloadingWeapon;
                PlayerHandler.ChangingItem += ExternalHandler.OnChangingItem;
                PlayerHandler.InteractingDoor += ExternalHandler.OnInteractingDoor;
                PlayerHandler.InteractingElevator += ExternalHandler.OnInteractingElevator;
                PlayerHandler.InteractingLocker += ExternalHandler.OnInteractingLocker;
                PlayerHandler.TriggeringTesla += ExternalHandler.OnTriggeringTesla;
                PlayerHandler.OpeningGenerator += ExternalHandler.OnOpeningGenerator;
                PlayerHandler.ClosingGenerator += ExternalHandler.OnClosingGenerator;
                PlayerHandler.StoppingGenerator += ExternalHandler.OnStoppingGenerator;
                PlayerHandler.UsingRadioBattery += ExternalHandler.OnUsingRadioBattery;
                PlayerHandler.UsingMicroHIDEnergy += ExternalHandler.OnUsingMicroHIDEnergy;
                PlayerHandler.DroppingAmmo += ExternalHandler.OnDroppingAmmo;
                PlayerHandler.FlippingCoin += ExternalHandler.OnFlippingCoin;
                PlayerHandler.VoiceChatting += ExternalHandler.OnVoiceChatting;
                PlayerHandler.MakingNoise += ExternalHandler.OnMakingNoise;
                PlayerHandler.Jumping += ExternalHandler.OnJumping;
                PlayerHandler.Transmitting += ExternalHandler.OnTransmitting;
                PlayerHandler.TogglingRadio += ExternalHandler.OnTogglingRadio;
                PlayerHandler.SearchingPickup += ExternalHandler.OnSearchingPickup;
                PlayerHandler.PlayerDamageWindow += ExternalHandler.OnDamagingWindow;
                PlayerHandler.KillingPlayer += ExternalHandler.OnKillingPlayer;
                PlayerHandler.EnteringEnvironmentalHazard += ExternalHandler.OnEnteringEnvHazard;
                PlayerHandler.StayingOnEnvironmentalHazard += ExternalHandler.OnStayingOnEnvHazard;
                PlayerHandler.ExitingEnvironmentalHazard += ExternalHandler.OnExitingEnvHazard;
            }


            foreach (ICustomRole CustomRole in Config.CustomRoles)
            {
                SpawnManager.RegisterCustomSubclass(CustomRole);
            }

            HttpManager.StartAll();

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomRoles");
            Log.Info("        by FoxWorn3365 & Dr.Agenda");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Instance = null;

            ServerHandler.RespawningTeam -= Handler.OnRespawningWave;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            PlayerHandler.Died -= Handler.OnDied;
            PlayerHandler.Spawning -= Handler.OnSpawning;
            PlayerHandler.Spawned -= Handler.OnPlayerSpawned;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            Scp049Handler.StartingRecall -= Handler.OnScp049StartReviving;

            // Player Events for the external handler
            if (Config.EnableExternalEventHandler)
            {
                PlayerHandler.Dying -= ExternalHandler.OnDying;
                PlayerHandler.UsingItem -= ExternalHandler.OnUsingItem;
                PlayerHandler.CancellingItemUse -= ExternalHandler.OnCancellingUsingItem;
                PlayerHandler.SpawningRagdoll -= ExternalHandler.OnSpawningRagdoll;
                PlayerHandler.ActivatingWarheadPanel -= ExternalHandler.OnActivatingWarheadPanel;
                PlayerHandler.ActivatingGenerator -= ExternalHandler.OnActivatingGenerator;
                PlayerHandler.ActivatingWorkstation -= ExternalHandler.OnActivatingWorkstation;
                PlayerHandler.DeactivatingWorkstation -= ExternalHandler.OnDeactivatingWorkstation;
                PlayerHandler.Hurting -= ExternalHandler.OnHurting;
                PlayerHandler.DroppingItem -= ExternalHandler.OnDroppingItem;
                PlayerHandler.PickingUpItem -= ExternalHandler.OnPickingUpItem;
                PlayerHandler.Handcuffing -= ExternalHandler.OnHandcuffing;
                PlayerHandler.RemovingHandcuffs -= ExternalHandler.OnRemovingHandcuffs;
                PlayerHandler.IntercomSpeaking -= ExternalHandler.OnIntercomSpeaking;
                PlayerHandler.Shooting -= ExternalHandler.OnShooting;
                PlayerHandler.EnteringPocketDimension -= ExternalHandler.OnEnteringPocketDimension;
                PlayerHandler.EscapingPocketDimension -= ExternalHandler.OnEscapingPocketDimension;
                PlayerHandler.EnteringKillerCollision -= ExternalHandler.OnEnteringKillerCollision;
                PlayerHandler.ReloadingWeapon -= ExternalHandler.OnReloadingWeapon;
                PlayerHandler.ChangingItem -= ExternalHandler.OnChangingItem;
                PlayerHandler.InteractingDoor -= ExternalHandler.OnInteractingDoor;
                PlayerHandler.InteractingElevator -= ExternalHandler.OnInteractingElevator;
                PlayerHandler.InteractingLocker -= ExternalHandler.OnInteractingLocker;
                PlayerHandler.TriggeringTesla -= ExternalHandler.OnTriggeringTesla;
                PlayerHandler.OpeningGenerator -= ExternalHandler.OnOpeningGenerator;
                PlayerHandler.ClosingGenerator -= ExternalHandler.OnClosingGenerator;
                PlayerHandler.StoppingGenerator -= ExternalHandler.OnStoppingGenerator;
                PlayerHandler.UsingRadioBattery -= ExternalHandler.OnUsingRadioBattery;
                PlayerHandler.UsingMicroHIDEnergy -= ExternalHandler.OnUsingMicroHIDEnergy;
                PlayerHandler.DroppingAmmo -= ExternalHandler.OnDroppingAmmo;
                PlayerHandler.FlippingCoin -= ExternalHandler.OnFlippingCoin;
                PlayerHandler.VoiceChatting -= ExternalHandler.OnVoiceChatting;
                PlayerHandler.MakingNoise -= ExternalHandler.OnMakingNoise;
                PlayerHandler.Jumping -= ExternalHandler.OnJumping;
                PlayerHandler.Transmitting -= ExternalHandler.OnTransmitting;
                PlayerHandler.TogglingRadio -= ExternalHandler.OnTogglingRadio;
                PlayerHandler.SearchingPickup -= ExternalHandler.OnSearchingPickup;
                PlayerHandler.PlayerDamageWindow -= ExternalHandler.OnDamagingWindow;
                PlayerHandler.KillingPlayer -= ExternalHandler.OnKillingPlayer;
                PlayerHandler.EnteringEnvironmentalHazard -= ExternalHandler.OnEnteringEnvHazard;
                PlayerHandler.StayingOnEnvironmentalHazard -= ExternalHandler.OnStayingOnEnvHazard;
                PlayerHandler.ExitingEnvironmentalHazard -= ExternalHandler.OnExitingEnvHazard;
            }

            Handler = null;
            ExternalHandler = null;
            CustomRoles = null;

            base.OnDisabled();
        }
    }
}