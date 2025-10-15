using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenQuickSlotUse.Patch
{
    public class QuickSlotPatch
    {
        private static PropertyInfo inventoryProperty = AccessTools.Property(typeof(InventoryController), "Inventory");

        private static readonly FieldInfo FastAccessSlotsField = AccessTools.Field(typeof(Inventory), "FastAccessSlots");

        private static HashSet<string> flareItemIds = new HashSet<string>
        {
            "62178c4d4ecf221597654e3d", // Red Flare
            "624c0b3340357b5f566e8766", // Yellow Flare
            "6217726288ed9f0845317459", // Green Flare
            "62178be9d0050232da3485d9"  // White Flare
        };

        // Patch 1
        public class InventoryControllerBindablePatcher : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(InventoryController), nameof(InventoryController.IsAtBindablePlace));
            }

            [PatchPostfix]
            private static void Postfix(InventoryController __instance, ref bool __result, Item item)
            {
                var inventory = (Inventory)inventoryProperty.GetValue(__instance);

                if (item == null || inventory == null)
                {
                    return;
                }

                if (IsItemAllowed(item))
                {
                    return;
                }

                if (IsItemInConfiguredFastAccess(inventory, item))
                {
                    __result = true;
                }
            }
        }

        // Patch 2
        public class InventoryControllerReachablePatcher : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(InventoryController), nameof(InventoryController.IsAtReachablePlace));
            }

            [PatchPostfix]
            private static void Postfix(InventoryController __instance, ref bool __result, Item item)
            {
                var inventory = (Inventory)inventoryProperty.GetValue(__instance);

                if (item == null || inventory == null)
                {
                    return;
                }

                if (item.CurrentAddress == null)
                {
                    return;
                }

                if (!IsItemAllowed(item))
                {
                    return;
                }

                if (!__instance.Examined(item))
                {
                    return;
                }

                if (IsItemInConfiguredFastAccess(inventory, item))
                {
                    __result = true;
                }
            }
        }


        // Determines if an item is in the configured fast access slots of an inventory.
        private static bool IsItemInConfiguredFastAccess(Inventory inventory, Item item)
        {
            var slotsObj = FastAccessSlotsField.GetValue(null);
            if (!(slotsObj is EquipmentSlot[] slots) || slots.Length == 0)
            {
                return false;
            }

            // Check if the item is in the slots
            return inventory.GetItemsInSlots(slots).Contains(item);
        }

        // Checks if an item is allowed to be accessed quickly.
        private static bool IsItemAllowed(Item item)
        {
            return item is Weapon
                || item is ThrowWeapItemClass
                || item.GetItemComponent<KnifeComponent>() != null
                || item is MedsItemClass
                || item is FoodDrinkItemClass
                || item is PortableRangeFinderItemClass
                || item is CompassItemClass
                || item is RadioTransmitterItemClass
                || flareItemIds.Contains(item.TemplateId);
        }

        public class InventoryControllerPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod() => null;
            
            new public void Enable()
            {
                new InventoryControllerBindablePatcher().Enable();
                new InventoryControllerReachablePatcher().Enable();
            }
        }
    }
}
