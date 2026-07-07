using UnityEngine;

namespace Climate
{
    internal class Items
    {
        public static GameObject Barometer { get; internal set; }
        public static GameObject Thermometer { get; internal set; }
        public static GameObject Hygrometer { get; internal set; }

        internal static void InitializeBarometer()
        {
            var itemBarometer = Barometer.AddComponent<ShipItemBarometer>();
            itemBarometer.holdDistance = 0.82f;
            itemBarometer.furniturePlaceHeight = 0.15f;
            itemBarometer.mass = 1;
            itemBarometer.value = 600;
            itemBarometer.name = "barometer";
            itemBarometer.category = TransactionCategory.toolsAndSupplies;
            itemBarometer.inventoryScale = 1;
            itemBarometer.inventoryRotation = 180;
            itemBarometer.floaterHeight = 1.6f;
            itemBarometer.wallAttachment = true;
        }

        internal static void InitializeThermometer()
        {
            var itemThermometer = Thermometer.AddComponent<ShipItemThermometer>();
            itemThermometer.holdDistance = 0.82f;
            itemThermometer.furniturePlaceHeight = 0.15f;
            itemThermometer.mass = 1;
            itemThermometer.value = 600;
            itemThermometer.name = "thermometer";
            itemThermometer.category = TransactionCategory.toolsAndSupplies;
            itemThermometer.inventoryScale = 1;
            itemThermometer.inventoryRotation = 180;
            itemThermometer.floaterHeight = 1.6f;
            itemThermometer.wallAttachment = true;
        }

        internal static void InitializeHygrometer()
        {
            var itemHygrometer = Hygrometer.AddComponent<ShipItemHygrometer>();
            itemHygrometer.holdDistance = 0.82f;
            itemHygrometer.furniturePlaceHeight = 0.15f;
            itemHygrometer.mass = 1;
            itemHygrometer.value = 600;
            itemHygrometer.name = "hygrometer";
            itemHygrometer.category = TransactionCategory.toolsAndSupplies;
            itemHygrometer.inventoryScale = 1;
            itemHygrometer.inventoryRotation = 180;
            itemHygrometer.floaterHeight = 1.6f;
            itemHygrometer.wallAttachment = true;
        }

        internal static void Initialize()
        {
            InitializeBarometer();
            InitializeThermometer();
            InitializeHygrometer();
        }
    }
}
