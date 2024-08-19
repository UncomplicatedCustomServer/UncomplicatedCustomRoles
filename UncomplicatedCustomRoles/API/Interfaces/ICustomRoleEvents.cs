using Exiled.Events.EventArgs.Player;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.API.Interfaces
{
    public interface ICustomRoleEvents
    {
        public abstract void OnKicking(KickingEventArgs ev);

        public abstract void OnKicked(KickedEventArgs ev);

        public abstract void OnBanning(BanningEventArgs ev);

        public abstract void OnChangingDangerState(ChangingDangerStateEventArgs ev);

        public abstract void OnBanned(BannedEventArgs ev);

        public abstract void OnEarningAchievement(EarningAchievementEventArgs ev);

        public abstract void OnUsingItem(UsingItemEventArgs ev);

        public abstract void OnUsingItemCompleted(UsingItemCompletedEventArgs ev);

        public abstract void OnUsedItem(UsedItemEventArgs ev);

        public abstract void OnCancellingItemUse(CancellingItemUseEventArgs ev);

        public abstract void OnCancelledItemUse(CancelledItemUseEventArgs ev);

        public abstract void OnInteracted(InteractedEventArgs ev);

        public abstract void OnSpawningRagdoll(SpawningRagdollEventArgs ev);

        public abstract void OnSpawnedRagdoll(SpawnedRagdollEventArgs ev);

        public abstract void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev);

        public abstract void OnActivatingWorkstation(ActivatingWorkstationEventArgs ev);

        public abstract void OnDeactivatingWorkstation(DeactivatingWorkstationEventArgs ev);

        public abstract void OnLeft(LeftEventArgs ev);

        public abstract void OnDied(DiedEventArgs ev);

        public abstract void OnChangingRole(ChangingRoleEventArgs ev);

        public abstract void OnThrowingProjectile(ThrownProjectileEventArgs ev);

        public abstract void OnThrowingRequest(ThrowingRequestEventArgs ev);

        public abstract void OnDroppingItem(DroppingItemEventArgs ev);

        public abstract void OnDroppedItem(DroppedItemEventArgs ev);

        public abstract void OnDroppingNothing(DroppingNothingEventArgs ev);

        public abstract void OnPickingUpItem(PickingUpItemEventArgs ev);

        public abstract void OnHandcuffing(HandcuffingEventArgs ev);

        public abstract void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev);

        public abstract void OnEscaping(EscapingEventArgs ev);

        public abstract void OnIntercomSpeaking(IntercomSpeakingEventArgs ev);

        public abstract void OnShot(ShotEventArgs ev);

        public abstract void OnShooting(ShootingEventArgs ev);

        public abstract void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev);

        public abstract void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev);

        public abstract void OnFailingEscapePocketDimension(FailingEscapePocketDimensionEventArgs ev);

        public abstract void OnEnteringKillerCollision(EnteringKillerCollisionEventArgs ev);

        public abstract void OnReloadingWeapon(ReloadingWeaponEventArgs ev);

        public abstract void OnSpawning(SpawningEventArgs ev);

        public abstract void OnSpawned(SpawnedEventArgs ev);

        public abstract void OnChangedItem(ChangedItemEventArgs ev);

        public abstract void OnChangingItem(ChangingItemEventArgs ev);

        public abstract void OnChangingGroup(ChangingGroupEventArgs ev);

        public abstract void OnInteractingElevator(InteractingElevatorEventArgs ev);

        public abstract void OnInteractingLocker(InteractingLockerEventArgs ev);

        public abstract void OnTriggeringTesla(TriggeringTeslaEventArgs ev);

        public abstract void OnReceivingEffect(ReceivingEffectEventArgs ev);

        public abstract void OnUsingRadioBattery(UsingRadioBatteryEventArgs ev);

        public abstract void OnChangingMicroHIDState(ChangingMicroHIDStateEventArgs ev);

        public abstract void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev);

        public abstract void OnInteractingShootingTarget(InteractingShootingTargetEventArgs ev);

        public abstract void OnDamagingShootingTarget(DamagingShootingTargetEventArgs ev);

        public abstract void OnFlippingCoin(FlippingCoinEventArgs ev);

        public abstract void OnTogglingFlashlight(TogglingFlashlightEventArgs ev);

        public abstract void OnUnloadingWeapon(UnloadingWeaponEventArgs ev);

        public abstract void OnAimingDownSight(AimingDownSightEventArgs ev);

        public abstract void OnTogglingWeaponFlashlight(TogglingWeaponFlashlightEventArgs ev);

        public abstract void OnDryfiringWeapon(DryfiringWeaponEventArgs ev);

        public abstract void OnVoiceChatting(VoiceChattingEventArgs ev);

        public abstract void OnMakingNoise(MakingNoiseEventArgs ev);

        public abstract void OnJumping(JumpingEventArgs ev);

        public abstract void OnLanding(LandingEventArgs ev);

        public abstract void OnTransmitting(TransmittingEventArgs ev);

        public abstract void OnChangingMoveState(ChangingMoveStateEventArgs ev);

        public abstract void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev);

        public abstract void OnTogglingNoClip(TogglingNoClipEventArgs ev);

        public abstract void OnTogglingOverwatch(TogglingOverwatchEventArgs ev);

        public abstract void OnTogglingRadio(TogglingRadioEventArgs ev);

        public abstract void OnSearchPickupRequest(SearchingPickupEventArgs ev);

        public abstract void OnSendingAdminChatMessage(SendingAdminChatMessageEventsArgs ev);

        public abstract void OnKillPlayer(KillingPlayerEventArgs ev);

        public abstract void OnItemAdded(ItemAddedEventArgs ev);

        public abstract void OnItemRemoved(ItemRemovedEventArgs ev);

        public abstract void OnEnteringEnvironmentalHazard(EnteringEnvironmentalHazardEventArgs ev);

        public abstract void OnStayingOnEnvironmentalHazard(StayingOnEnvironmentalHazardEventArgs ev);

        public abstract void OnExitingEnvironmentalHazard(ExitingEnvironmentalHazardEventArgs ev);

        public abstract void OnPlayerDamageWindow(DamagingWindowEventArgs ev);

        public abstract void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev);

        public abstract void OnOpeningGenerator(OpeningGeneratorEventArgs ev);

        public abstract void OnClosingGenerator(ClosingGeneratorEventArgs ev);

        public abstract void OnActivatingGenerator(ActivatingGeneratorEventArgs ev);

        public abstract void OnStoppingGenerator(StoppingGeneratorEventArgs ev);

        public abstract void OnInteractingDoor(InteractingDoorEventArgs ev);

        public abstract void OnDroppingAmmo(DroppingAmmoEventArgs ev);

        public abstract void OnDroppedAmmo(DroppedAmmoEventArgs ev);

        public abstract void OnIssuingMute(IssuingMuteEventArgs ev);

        public abstract void OnRevokingMute(RevokingMuteEventArgs ev);

        public abstract void OnChangingRadioPreset(ChangingRadioPresetEventArgs ev);

        public abstract void OnHurting(HurtingEventArgs ev);

        public abstract void OnHurt(HurtEventArgs ev);

        public abstract void OnHealing(HealingEventArgs ev);

        public abstract void OnHealed(HealedEventArgs ev);

        public abstract void OnDying(DyingEventArgs ev);

        public abstract void OnChangingNickname(ChangingNicknameEventArgs ev);
    }
}
