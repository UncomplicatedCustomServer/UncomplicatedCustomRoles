using Exiled.Events.EventArgs.Player;

namespace UncomplicatedCustomRoles.Events
{
    internal class ExternalPlayerEventHandler
    {
        public void OnDying(DyingEventArgs Dying)
        {
            API.Features.Events.__CallEvent(UCREvents.Dying, Dying);
        }

        public void OnUsingItem(UsingItemEventArgs UsingItem)
        {
            API.Features.Events.__CallEvent(UCREvents.UsingItem, UsingItem);
        }

        public void OnCancellingUsingItem(CancellingItemUseEventArgs CancelledItemUseEventArgs)
        {
            API.Features.Events.__CallEvent(UCREvents.CancellingUsingItem, CancelledItemUseEventArgs);
        }

        public void OnSpawningRagdoll(SpawningRagdollEventArgs SpawningRagdoll)
        {
            API.Features.Events.__CallEvent(UCREvents.SpawningRagdoll, SpawningRagdoll);
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ActivatingWarheadPanel)
        {
            API.Features.Events.__CallEvent(UCREvents.ActivatingWarheadPanel, ActivatingWarheadPanel);
        }

        public void OnActivatingWorkstation(ActivatingWorkstationEventArgs ActivatingWorkstation)
        {
            API.Features.Events.__CallEvent(UCREvents.ActivatingWorkstation, ActivatingWorkstation);
        }

        public void OnDeactivatingWorkstation(DeactivatingWorkstationEventArgs DeactivatingWorkstation)
        {
            API.Features.Events.__CallEvent(UCREvents.DeactivatingWorkstation, DeactivatingWorkstation);
        }

        public void OnHurting(HurtingEventArgs Hurting)
        {
            API.Features.Events.__CallEvent(UCREvents.Hurting, Hurting);
        }

        public void OnDroppingItem(DroppingItemEventArgs DroppingItem)
        {
            API.Features.Events.__CallEvent(UCREvents.DroppingItem, DroppingItem);
        }

        public void OnPickingUpItem(PickingUpItemEventArgs PickingUpItem)
        {
            API.Features.Events.__CallEvent(UCREvents.PickingUpItem, PickingUpItem);
        }

        public void OnHandcuffing(HandcuffingEventArgs Handcuffing)
        {
            API.Features.Events.__CallEvent(UCREvents.Handcuffing, Handcuffing);
        }

        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs RemovingHandcuffsEventArgs)
        {
            API.Features.Events.__CallEvent(UCREvents.RemovingHandcuffs, RemovingHandcuffsEventArgs);
        }

        public void OnIntercomSpeaking(IntercomSpeakingEventArgs IntercomSpeaking)
        {
            API.Features.Events.__CallEvent(UCREvents.IntercomSpeaking, IntercomSpeaking);
        }

        public void OnShooting(ShootingEventArgs Shooting)
        {
            API.Features.Events.__CallEvent(UCREvents.Shooting, Shooting);
        }

        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs EnteringPocketDimension)
        {
            API.Features.Events.__CallEvent(UCREvents.EnteringPocketDimension, EnteringPocketDimension);
        }

        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs EscapingPocketDimension)
        {
            API.Features.Events.__CallEvent(UCREvents.EscapingPocketDimension, EscapingPocketDimension);
        }

        public void OnEnteringKillerCollision(EnteringKillerCollisionEventArgs EnteringKillerCollision)
        {
            API.Features.Events.__CallEvent(UCREvents.EnteringKillerCollision, EnteringKillerCollision);
        }

        public void OnReloadingWeapon(ReloadingWeaponEventArgs ReloadingWeapon)
        {
            API.Features.Events.__CallEvent(UCREvents.ReloadingWeapon, ReloadingWeapon);
        }

        public void OnChangingItem(ChangingItemEventArgs ChangingItem)
        {
            API.Features.Events.__CallEvent(UCREvents.ChangingItem, ChangingItem);
        }

        public void OnInteractingDoor(InteractingDoorEventArgs InteractingDoor)
        {
            API.Features.Events.__CallEvent(UCREvents.InteractingDoor, InteractingDoor);
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs InteractingElevator)
        {
            API.Features.Events.__CallEvent(UCREvents.InteractingElevator, InteractingElevator);
        }

        public void OnInteractingLocker(InteractingLockerEventArgs InteractingLocker)
        {
            API.Features.Events.__CallEvent(UCREvents.InteractingLocker, InteractingLocker);
        }

        public void OnTriggeringTesla(TriggeringTeslaEventArgs TriggeredTesla)
        {
            API.Features.Events.__CallEvent(UCREvents.TriggeringTesla, TriggeredTesla);
        }

        public void OnOpeningGenerator(OpeningGeneratorEventArgs OpeningGenerator)
        {
            API.Features.Events.__CallEvent(UCREvents.OpeningGenerator, OpeningGenerator);
        }

        public void OnClosingGenerator(ClosingGeneratorEventArgs CloseClosingGenerator)
        {
            API.Features.Events.__CallEvent(UCREvents.ClosingGenerator, CloseClosingGenerator);
        }

        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ActivatingGenerator)
        {
            API.Features.Events.__CallEvent(UCREvents.ActivatingGenerator, ActivatingGenerator);
        }

        public void OnStoppingGenerator(StoppingGeneratorEventArgs StoppingGenerator)
        {
            API.Features.Events.__CallEvent(UCREvents.StoppingGenerator, StoppingGenerator);
        }

        public void OnUsingRadioBattery(UsingRadioBatteryEventArgs UsingRadioBattery)
        {
            API.Features.Events.__CallEvent(UCREvents.UsingRadioBattery, UsingRadioBattery);
        }

        public void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs UsingMicroHIDEnergy)
        {
            API.Features.Events.__CallEvent(UCREvents.UsingMicroHIDEnergy, UsingMicroHIDEnergy);
        }

        public void OnDroppingAmmo(DroppingAmmoEventArgs DroppingAmmo)
        {
            API.Features.Events.__CallEvent(UCREvents.DroppingAmmo, DroppingAmmo);
        }

        public void OnFlippingCoin(FlippingCoinEventArgs FlippingCoin)
        {
            API.Features.Events.__CallEvent(UCREvents.FlippingCoin, FlippingCoin);
        }

        public void OnVoiceChatting(VoiceChattingEventArgs VoiceChatting)
        {
            API.Features.Events.__CallEvent(UCREvents.VoiceChatting, VoiceChatting);
        }

        public void OnMakingNoise(MakingNoiseEventArgs MakingNoise)
        {
            API.Features.Events.__CallEvent(UCREvents.MakingNoise, MakingNoise);
        }

        public void OnJumping(JumpingEventArgs Jumping)
        {
            API.Features.Events.__CallEvent(UCREvents.Jumping, Jumping);
        }

        public void OnTransmitting(TransmittingEventArgs Transmitting)
        {
            API.Features.Events.__CallEvent(UCREvents.Transmitting, Transmitting);
        }

        public void OnTogglingRadio(TogglingRadioEventArgs TogglingRadio)
        {
            API.Features.Events.__CallEvent(UCREvents.TogglingRadio, TogglingRadio);
        }

        public void OnSearchingPickup(SearchingPickupEventArgs SearchingPickup)
        {
            API.Features.Events.__CallEvent(UCREvents.SearchingPickup, SearchingPickup);
        }

        public void OnDamagingWindow(DamagingWindowEventArgs DamagingWindow)
        {
            API.Features.Events.__CallEvent(UCREvents.DamagingWindow, DamagingWindow);
        }

        public void OnKillingPlayer(KillingPlayerEventArgs KillingPlayer)
        {
            API.Features.Events.__CallEvent(UCREvents.KillingPlayer, KillingPlayer);
        }

        public void OnEnteringEnvHazard(EnteringEnvironmentalHazardEventArgs EnteringEnvHazard)
        {
            API.Features.Events.__CallEvent(UCREvents.EnteringEnvironmentalHazard, EnteringEnvHazard);
        }

        public void OnStayingOnEnvHazard(StayingOnEnvironmentalHazardEventArgs StayingOnEnvHazard)
        {
            API.Features.Events.__CallEvent(UCREvents.StayingOnEnvironmentalHazard, StayingOnEnvHazard);
        }

        public void OnExitingEnvHazard(ExitingEnvironmentalHazardEventArgs ExitingEnvHazard)
        {
            API.Features.Events.__CallEvent(UCREvents.ExitingEnvironmentalHazard, ExitingEnvHazard);
        }
    }
}
