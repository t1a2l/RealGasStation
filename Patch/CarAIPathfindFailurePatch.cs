using ColossalFramework;
using HarmonyLib;
using RealGasStation.Util;
using System.Reflection;

namespace RealGasStation.Patch
{
    [HarmonyPatch]
    public static class CarAIPathfindFailurePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(CarAI).GetMethod("PathfindFailure", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Postfix(CarAI __instance, ushort vehicleID, ref Vehicle data)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            if ((data.m_transferType == 126) || (data.m_transferType == 127))
            {
                if (data.Info.m_vehicleAI is CargoTruckAI && (data.m_targetBuilding != 0))
                {
                    CargoTruckAI AI = (CargoTruckAI)data.Info.m_vehicleAI;

                    var vehicle = instance.m_vehicles.m_buffer[vehicleID];
#if DEBUG
                    DebugLog.LogToFileOnly("PathFind not success " + vehicleID.ToString() + "transferType = " + vehicle.m_transferType.ToString() + "And MainDataStore.TargetGasBuilding[vehicleID] = " + MainDataStore.TargetGasBuilding[vehicleID].ToString() + "data.m_targetBuilding = " + vehicle.m_targetBuilding.ToString());
#endif
                    AI.SetTarget(vehicleID, ref data, data.m_targetBuilding);
#if DEBUG
                                DebugLog.LogToFileOnly("Reroute to target " + vehicleID.ToString() + "vehicle.m_path = " + vehicle.m_path.ToString() + vehicle.m_flags.ToString());
#endif
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                        MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
                    MainDataStore.TargetGasBuilding[vehicleID] = 0;
                    return;
                }
                else if (data.Info.m_vehicleAI is PassengerCarAI && data.Info.m_class.m_subService == ItemClass.SubService.ResidentialLow)
                {
                    PassengerCarAI AI = (PassengerCarAI)data.Info.m_vehicleAI;
                    AI.SetTarget(vehicleID, ref data, 0);
                    data.m_transferType = MainDataStore.preTranferReason[vehicleID];
                    if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                        MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
                    MainDataStore.TargetGasBuilding[vehicleID] = 0;
                    return;
                }
            }
        }
    }
}
