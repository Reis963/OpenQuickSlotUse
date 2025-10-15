using BepInEx;
using EFT.InventoryLogic;
using HarmonyLib;
using OpenQuickSlotUse.Patch;
using System.Collections.Generic;

namespace OpenQuickSlotUse
{
    [BepInPlugin("com.reis963.OpenQuickSlotUse", "OpenQuickSlotUse", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal void Awake()
        {
            DontDestroyOnLoad(this);

            ApplyFastAccessSlots();

            EnablePlaceChecks();
        }

        private void ApplyFastAccessSlots()
        {
            // Slots base
            var slots = new List<EquipmentSlot>
            { 
                EquipmentSlot.Pockets,
                EquipmentSlot.TacticalVest,
                EquipmentSlot.ArmBand,
                EquipmentSlot.Backpack,
                EquipmentSlot.SecuredContainer
            };

            var fastAccessSlotsField = AccessTools.Field(typeof(Inventory), "FastAccessSlots");
            fastAccessSlotsField.SetValue(null, slots.ToArray());
        }

        private void EnablePlaceChecks()
        {
            new QuickSlotPatch.InventoryControllerBindablePatcher().Enable();
            new QuickSlotPatch.InventoryControllerReachablePatcher().Enable();
        }
    }
}
