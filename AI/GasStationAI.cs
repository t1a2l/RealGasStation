﻿using ColossalFramework;
using RealGasStation.Util;
using System;
using UnityEngine;

namespace RealGasStation.NewAI
{
    public class GasStationAI
    {
        public static void ProcessGasBuildingIncoming(ushort buildingID, ref Building buildingData)
        {
            int num27 = 0;
            int num28 = 0;
            int num29 = 0;
            int value = 0;

            //Petrol
            TransferManager.TransferReason incomingTransferReason = TransferManager.TransferReason.Petrol;

            if (incomingTransferReason != TransferManager.TransferReason.None && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                CalculateGuestVehicles(buildingID, ref buildingData, incomingTransferReason, ref num27, ref num28, ref num29, ref value);
                buildingData.m_tempImport = (byte)Mathf.Clamp(value, buildingData.m_tempImport, 255);
            }

            int num34 = 50000 - MainDataStore.petrolBuffer[buildingID] - num29;
            if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
            {
                if (num34 >= 0)
                {
                    TransferManager.TransferOffer offer = default;
                    offer.Priority = 7;
                    offer.Building = buildingID;
                    offer.Position = buildingData.m_position;
                    offer.Amount = num34 / 8000;
                    offer.Active = false;

                    if (offer.Amount > 0)
                    {
                        Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                    }
                }
            }

            if ((MainDataStore.resourceCategory[buildingID] == 3) || (MainDataStore.resourceCategory[buildingID] == 1))
            {
                //Fuel
                incomingTransferReason = (TransferManager.TransferReason)126;
                num34 = MainDataStore.petrolBuffer[buildingID] - MainDataStore.finalVehicleForFuelCount[buildingID] * 400;
                if ((MainDataStore.resourceCategory[buildingID] == 1))
                    num34 >>= 1;
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    if (num34 >= 0)
                    {
                        System.Random rand = new System.Random();
                        TransferManager.TransferOffer offer = default;
                        offer.Priority = rand.Next(8);
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = ((num34 - 0) / 400);
                        offer.Active = false;

                        if ((int)(num34 / 400) > 0)
                        {
                            Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                        }
                    }
                }
            }

            if ((MainDataStore.resourceCategory[buildingID] == 2) || (MainDataStore.resourceCategory[buildingID] == 1))
            {
                //Fuel for Heavy
                incomingTransferReason = (TransferManager.TransferReason)127;
                num34 = MainDataStore.petrolBuffer[buildingID] - MainDataStore.finalVehicleForFuelCount[buildingID] * 400;
                if ((MainDataStore.resourceCategory[buildingID] == 1))
                    num34 >>= 1;
                if (buildingData.m_flags.IsFlagSet(Building.Flags.Active) && buildingData.m_flags.IsFlagSet(Building.Flags.Completed))
                {
                    if (num34 >= 0)
                    {
                        System.Random rand = new System.Random();
                        TransferManager.TransferOffer offer = default;
                        offer.Priority = rand.Next(8);
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = ((num34 - 0) / 400);
                        offer.Active = false;

                        if ((int)(num34 / 400) > 0)
                        {
                            Singleton<TransferManager>.instance.AddIncomingOffer(incomingTransferReason, offer);
                        }
                    }
                }
            }
        }

        protected static void CalculateGuestVehicles(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int count, ref int cargo, ref int capacity, ref int outside)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            ushort num = data.m_guestVehicles;
            int num2 = 0;
            while (num != 0)
            {
                if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num].m_transferType == material)
                {
                    VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
                    info.m_vehicleAI.GetSize(num, ref instance.m_vehicles.m_buffer[num], out int a, out int num3);
                    cargo += Mathf.Min(a, num3);
                    capacity += num3;
                    count++;
                    if ((instance.m_vehicles.m_buffer[num].m_flags & (Vehicle.Flags.Importing | Vehicle.Flags.Exporting)) != (Vehicle.Flags)0)
                    {
                        outside++;
                    }
                }
                num = instance.m_vehicles.m_buffer[num].m_nextGuestVehicle;
                if (++num2 > Singleton<VehicleManager>.instance.m_vehicles.m_size)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public static bool IsGasBuilding(ushort id, bool fastCheck = true)
        {
            if (!fastCheck)
            {
                var buildingData = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id];
                if (buildingData.Info.m_buildingAI is ParkBuildingAI)
                {
                    return false;
                }

                if (buildingData.Info.m_buildingAI is CampusBuildingAI)
                {
                    return false;
                }

                PlayerBuildingAI AI = buildingData.Info.m_buildingAI as PlayerBuildingAI;
                if (AI.RequireRoadAccess() == false)
                {
                    return false;
                }
            }

            if (MainDataStore.resourceCategory[id] != 0)
            {
                return true;
            }
            return false;
        }
    }
}
