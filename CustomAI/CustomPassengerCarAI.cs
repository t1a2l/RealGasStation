﻿using ColossalFramework;
using ColossalFramework.Math;
using RealGasStation.Util;
using System;
using System.Reflection;
using UnityEngine;

namespace RealGasStation.CustomAI
{
    public class CustomPassengerCarAI : PassengerCarAI
    {
        public void PassengerCarAIArriveAtTargetForRealGasStationPre(ushort vehicleID, ref Vehicle data)
        {
            if (MainDataStore.petrolBuffer[data.m_targetBuilding] > 400)
            {
                MainDataStore.petrolBuffer[data.m_targetBuilding] -= 400;
            }
            SetTarget(vehicleID, ref data, 0);
            data.m_transferType = MainDataStore.preTranferReason[vehicleID];
            if (MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]] > 0)
                MainDataStore.finalVehicleForFuelCount[MainDataStore.TargetGasBuilding[vehicleID]]--;
            MainDataStore.TargetGasBuilding[vehicleID] = 0;
            if (Loader.isRealCityRunning)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PublicIncome, (int)(400f * CustomCargoTruckAI.GetResourcePrice(TransferManager.TransferReason.Petrol) + 1800f), ItemClass.Service.Vehicles, ItemClass.SubService.None, ItemClass.Level.Level1);
            }
        }

        private ushort GetDriverInstance(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            return instance2;
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }

        public static void GetVehicleRunningTimingForRealCity(ushort vehicleID, ref Vehicle vehicleData)
        {
            DebugLog.LogToFileOnly("Error: Should be detour by RealCity @ GetVehicleRunningTimingForRealCity");
        }

        private bool CustomArriveAtTarget(ushort vehicleID, ref Vehicle data)
        {
            // NON-STOCK CODE START
            //RealCity Mod related
            if (Loader.isRealCityRunning)
            {
                GetVehicleRunningTimingForRealCity(vehicleID, ref data);
            }
            //RealGasStation Mod related
            if (data.m_transferType == 112)
            {
                if (!MainDataStore.alreadyPaidForFuel[vehicleID])
                {
                    PassengerCarAIArriveAtTargetForRealGasStationPre(vehicleID, ref data);
                    MainDataStore.alreadyPaidForFuel[vehicleID] = false;
                }
                else
                {
                    DebugLog.LogToFileOnly("Vehicle is been paid for fuel");
                    data.Unspawn(vehicleID);
                }
                return true;
            }
            // NON-STOCK CODE END
            if ((data.m_flags & Vehicle.Flags.Parking) != (Vehicle.Flags)0)
            {
                VehicleManager instance = Singleton<VehicleManager>.instance;
                CitizenManager instance2 = Singleton<CitizenManager>.instance;
                ushort driverInstance = this.GetDriverInstance(vehicleID, ref data);
                if (driverInstance != 0)
                {
                    uint citizen = instance2.m_instances.m_buffer[(int)driverInstance].m_citizen;
                    if (citizen != 0u)
                    {
                        ushort parkedVehicle = instance2.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_parkedVehicle;
                        if (parkedVehicle != 0)
                        {
                            Vehicle.Frame lastFrameData = data.GetLastFrameData();
                            instance.m_parkedVehicles.m_buffer[(int)parkedVehicle].m_travelDistance = lastFrameData.m_travelDistance;
                            VehicleParked[] expr_A1_cp_0 = instance.m_parkedVehicles.m_buffer;
                            ushort expr_A1_cp_1 = parkedVehicle;
                            expr_A1_cp_0[(int)expr_A1_cp_1].m_flags = (ushort)(expr_A1_cp_0[(int)expr_A1_cp_1].m_flags & 65527);
                            InstanceID empty = InstanceID.Empty;
                            empty.Vehicle = vehicleID;
                            InstanceID empty2 = InstanceID.Empty;
                            empty2.ParkedVehicle = parkedVehicle;
                            Singleton<InstanceManager>.instance.ChangeInstance(empty, empty2);
                        }
                    }
                }
            }
            this.UnloadPassengers(vehicleID, ref data);
            if (data.m_targetBuilding == 0)
            {
                return true;
            }
            data.m_targetPos0 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)data.m_targetBuilding].CalculateSidewalkPosition();
            data.m_targetPos0.w = 2f;
            data.m_targetPos1 = data.m_targetPos0;
            data.m_targetPos2 = data.m_targetPos0;
            data.m_targetPos3 = data.m_targetPos0;
            this.RemoveTarget(vehicleID, ref data);
            return true;
        }

        private void UnloadPassengers(ushort vehicleID, ref Vehicle data)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = data.m_citizenUnits;
            int num2 = 0;
            while (num != 0u)
            {
                uint nextUnit = instance.m_units.m_buffer[(int)((UIntPtr)num)].m_nextUnit;
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num)].GetCitizen(i);
                    if (citizen != 0u)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            CitizenInfo info = instance.m_instances.m_buffer[(int)instance2].Info;
                            info.m_citizenAI.SetCurrentVehicle(instance2, ref instance.m_instances.m_buffer[(int)instance2], 0, 0u, data.m_targetPos0);
                        }
                    }
                }
                num = nextUnit;
                if (++num2 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        private void RemoveTarget(ushort vehicleID, ref Vehicle data)
        {
            if (data.m_targetBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveGuestVehicle(vehicleID, ref data);
                data.m_targetBuilding = 0;
            }
        }
    }
}
 