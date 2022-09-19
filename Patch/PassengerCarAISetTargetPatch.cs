using HarmonyLib;
using RealGasStation.Util;
using System;
using System.Reflection;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class PassengerCarAISetTargetPatch
    {
        private delegate bool StartPathFindCargoTruckAIDelegate(CargoTruckAI __instance, ushort vehicleID, ref Vehicle vehicleData);
        private static readonly StartPathFindCargoTruckAIDelegate StartPathFindCargoTruckAI = AccessTools.MethodDelegate<StartPathFindCargoTruckAIDelegate>(typeof(CargoTruckAI).GetMethod("StartPathFind", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, new ParameterModifier[] { }), null, false);

        public static MethodBase TargetMethod()
        {
            return typeof(PassengerCarAI).GetMethod("SetTarget", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(ushort) }, null);
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref CargoTruckAI __instance, ushort vehicleID, ref Vehicle data, ushort targetBuilding)
        {
            if ((data.m_transferType == 127) || (data.m_transferType == 126))
            {
                data.m_targetBuilding = targetBuilding;
                if (!StartPathFindCargoTruckAI(__instance, vehicleID, ref data))
                {
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    data.m_targetBuilding = 0;
                    __instance.SetTarget(vehicleID, ref data, 0);
                    if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                        MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
                    MainDataStore.TargetGasBuilding[vehicleID] = 0;
                    data.Unspawn(vehicleID);
                }
                return false;
            }
            return true;
        }
    }
}
