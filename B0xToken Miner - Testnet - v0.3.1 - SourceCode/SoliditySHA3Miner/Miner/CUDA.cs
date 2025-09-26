/*
   Copyright 2018 Lip Wee Yeo Amano

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Collections.Generic;
namespace SoliditySHA3Miner.Miner
{
    public class CUDA : MinerBase
    {
        public bool UseNvSMI { get; protected set; }

        public CUDA(NetworkInterface.INetworkInterface networkInterface, Device.CUDA[] cudaDevices, bool isSubmitStale, int pauseOnFailedScans)
            : base(networkInterface, cudaDevices, isSubmitStale, pauseOnFailedScans)
        {
            try
            {
                var hasNvAPI64 = false;
                Helper.CUDA.Solver.FoundNvAPI64(ref hasNvAPI64);

                if (!hasNvAPI64)
                    PrintMessage("CUDA", string.Empty, -1, "Warn", "NvAPI64 library not found.");

                var foundNvSMI = API.NvSMI.FoundNvSMI();

                if (!foundNvSMI)
                    PrintMessage("CUDA", string.Empty, -1, "Warn", "NvSMI library not found.");

                UseNvSMI = !hasNvAPI64 && foundNvSMI;

                HasMonitoringAPI = hasNvAPI64 | UseNvSMI;

                if (!HasMonitoringAPI)
                    PrintMessage("CUDA", string.Empty, -1, "Warn", "GPU monitoring not available.");

                UnmanagedInstance = Helper.CUDA.Solver.GetInstance();

                AssignDevices();
            }
            catch (Exception ex)
            {
                PrintMessage("CUDA", string.Empty, -1, "Error", ex.Message);
            }
        }

        #region IMiner

        public override void Dispose()
        {
            try
            {
                foreach (var device in Devices.OfType<Device.CUDA>().Where(d => d.IsInitialized))
                {
                    CleanupDevice(device);
                }

                var disposeTask = Task.Factory.StartNew(() => base.Dispose());

                if (UnmanagedInstance != IntPtr.Zero)
                    Helper.CUDA.Solver.DisposeInstance(UnmanagedInstance);

                if (!disposeTask.IsCompleted)
                    disposeTask.Wait();
            }
            catch (Exception ex)
            {
                PrintMessage("CUDA", string.Empty, -1, "Error", $"Dispose error: {ex.Message}");
            }
        }

        #endregion IMiner

        #region MinerBase abstracts

        protected override void HashPrintTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var hashString = new StringBuilder();
            hashString.Append("Hashrates:");
            var zero = 0.0;
            foreach (var device in Devices.Where(d => d.AllowDevice))
                zero = GetHashRateByDevice(device) / 1000000.0f;
            foreach (var device in Devices.Where(d => d.AllowDevice))
                hashString.AppendFormat(" {0} MH/s", GetHashRateByDevice(device) / 1000000.0f);
            if (zero == 0)
            {
                hashString.Append(", Normal for it to say 0.  Means we have solved the current challenge and are awaiting current reward to be greater than MinBWORKperMint.");
            }
            PrintMessage("CUDA", string.Empty, -1, "Info", hashString.ToString());

            if (HasMonitoringAPI)
            {
                var coreClock = 0;
                var temperature = 0;
                var tachometerRPM = 0;
                var coreClockString = new StringBuilder();
                var temperatureString = new StringBuilder();
                var fanTachometerRpmString = new StringBuilder();

                coreClockString.Append("Core clocks:");
                foreach (Device.CUDA device in Devices)
                    if (device.AllowDevice)
                    {
                        if (UseNvSMI)
                            coreClock = API.NvSMI.GetDeviceCurrentCoreClock(device.PciBusID);
                        else
                            Helper.CUDA.Solver.GetDeviceCurrentCoreClock(device.DeviceCUDA_Struct, ref coreClock);
                        coreClockString.AppendFormat(" {0}MHz", coreClock);
                    }
                PrintMessage("CUDA", string.Empty, -1, "Info", coreClockString.ToString());

                temperatureString.Append("Temperatures:");
                foreach (Device.CUDA device in Devices)
                    if (device.AllowDevice)
                    {
                        if (UseNvSMI)
                            temperature = API.NvSMI.GetDeviceCurrentTemperature(device.PciBusID);
                        else
                            Helper.CUDA.Solver.GetDeviceCurrentTemperature(device.DeviceCUDA_Struct, ref temperature);
                        temperatureString.AppendFormat(" {0}C", temperature);
                    }
                PrintMessage("CUDA", string.Empty, -1, "Info", temperatureString.ToString());

                if (!UseNvSMI)
                {
                    fanTachometerRpmString.Append("Fan tachometers:");
                    foreach (Device.CUDA device in Devices)
                        if (device.AllowDevice)
                        {
                            Helper.CUDA.Solver.GetDeviceCurrentFanTachometerRPM(device.DeviceCUDA_Struct, ref tachometerRPM);
                            fanTachometerRpmString.AppendFormat(" {0}RPM", tachometerRPM);
                        }
                    PrintMessage("CUDA", string.Empty, -1, "Info", fanTachometerRpmString.ToString());
                }
            }

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        }

        protected override void AssignDevices()
        {
            if (!Program.AllowCUDA || Devices.All(d => !d.AllowDevice))
            {
                PrintMessage("CUDA", string.Empty, -1, "Info", "Device not set.");
                return;
            }

            var isKingMaking = !string.IsNullOrWhiteSpace(Work.GetKingAddressString());

            foreach (Device.CUDA device in Devices.Where(d => d.AllowDevice))
            {
                var errorMessage = new StringBuilder(1024);
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", "Assigning device...");

                device.DeviceCUDA_Struct.DeviceID = device.DeviceID;
                Helper.CUDA.Solver.GetDeviceProperties(UnmanagedInstance, ref device.DeviceCUDA_Struct, errorMessage);
                if (errorMessage.Length > 0)
                {
                    PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", errorMessage.ToString());
                    return;
                }

                if (device.DeviceCUDA_Struct.ComputeMajor < 5)
                    device.Intensity = (device.Intensity < 1.000f) ? Device.CUDA.DEFAULT_INTENSITY : device.Intensity; // For older GPUs
                else
                {
                    float defaultIntensity = Device.CUDA.DEFAULT_INTENSITY;
                    if (isKingMaking)
                    {
                        // RTX 40/50 series - highest performance
                        if (new string[] { "5090", "5080", "4090", "4080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 30.50f;
                        else if (new string[] { "5070", "4070", "3090", "3080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 30.00f;
                        else if (new string[] { "4060", "3070", "2080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 28.00f;
                        else if (new string[] { "3060", "2070", "1080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 27.54f;
                        else if (new string[] { "2060", "1070 TI", "1070TI" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 27.46f;
                        else if (new string[] { "2050", "1070", "980" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 27.01f;
                        else if (new string[] { "1060", "970" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 26.01f;
                        else if (new string[] { "1050", "960" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 25.01f;
                    }
                    else
                    {
                        // RTX 40/50 series - highest performance
                        if (new string[] { "5090", "5080", "4090", "4080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 30.00f;
                        else if (new string[] { "5070", "4070", "3090", "3080" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 29.50f;
                        else if (new string[] { "4060", "3070", "2080", "2070 TI", "2070TI", "1080 TI", "1080TI" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 27.50f;
                        else if (new string[] { "3060", "1080", "2070", "1070 TI", "1070TI" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 26.33f;
                        else if (new string[] { "2060", "1070", "980" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 26.00f;
                        else if (new string[] { "2050", "1060", "970" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 25.50f;
                        else if (new string[] { "1050", "960" }.Any(m => device.Name.IndexOf(m) > -1))
                            defaultIntensity = 25.00f;
                    }
                    device.Intensity = (device.Intensity < 1.000f) ? defaultIntensity : device.Intensity;
                }

                device.PciBusID = (uint)device.DeviceCUDA_Struct.PciBusID;
                device.ConputeVersion = (uint)((device.DeviceCUDA_Struct.ComputeMajor * 100) + (device.DeviceCUDA_Struct.ComputeMinor * 10));
                device.DeviceCUDA_Struct.MaxSolutionCount = Device.DeviceBase.MAX_SOLUTION_COUNT;
                device.DeviceCUDA_Struct.Intensity = device.Intensity;
                device.DeviceCUDA_Struct.Threads = device.Threads;
                device.DeviceCUDA_Struct.Block = device.Block;
                device.DeviceCUDA_Struct.Grid = device.Grid;
                device.IsAssigned = true;

                /*Add debug output to see what values are being set
                Program.Print($"CUDA Device {device.DeviceID} Configuration:");
                Program.Print($"  MaxSolutionCount: {device.DeviceCUDA_Struct.MaxSolutionCount}");
                Program.Print($"  Intensity: {device.DeviceCUDA_Struct.Intensity}");
                Program.Print($"  Threads: {device.DeviceCUDA_Struct.Threads}");
                Program.Print($"  Block.X: {device.DeviceCUDA_Struct.Block.X}");
                Program.Print($"  Block.Y: {device.DeviceCUDA_Struct.Block.Y}");
                Program.Print($"  Block.Z: {device.DeviceCUDA_Struct.Block.Z}");
                Program.Print($"  Grid.X: {device.DeviceCUDA_Struct.Grid.X}");
                Program.Print($"  Grid.Y: {device.DeviceCUDA_Struct.Grid.Y}");
                Program.Print($"  Grid.Z: {device.DeviceCUDA_Struct.Grid.Z}");
                Program.Print($"  Total CUDA Cores: {device.DeviceCUDA_Struct.Grid.X * device.DeviceCUDA_Struct.Block.X}");
                */

                PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", string.Format("Assigned device ({0})...", device.Name));
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", string.Format("Compute capability: {0}.{1}", device.DeviceCUDA_Struct.ComputeMajor, device.DeviceCUDA_Struct.ComputeMinor));
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", string.Format("Intensity: {0}", device.Intensity));

                if (!device.IsInitialized)
                {
                    PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", "Initializing device...");
                    errorMessage.Clear();

                    Helper.CUDA.Solver.InitializeDevice(UnmanagedInstance, ref device.DeviceCUDA_Struct, errorMessage);

                    if (errorMessage.Length > 0)
                    {
                        PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", $"InitializeDevice failed: {errorMessage}");
                        device.IsInitialized = false;
                        device.IsAssigned = false; // Prevent further use
                        return; // Skip this device
                    }

                    // Additional check to ensure device memory pointers are valid
                    if (device.DeviceCUDA_Struct.Solutions == IntPtr.Zero ||
                        device.DeviceCUDA_Struct.SolutionCount == IntPtr.Zero)
                    {
                        PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", "Device memory not allocated correctly.");
                        device.IsInitialized = false;
                        device.IsAssigned = false;
                        return;
                    }

                    device.IsInitialized = true;
                    PrintMessage(device.Type, device.Platform, device.DeviceID, "Info", "Device initialized successfully.");
                }
            }
        }

        protected override void PushHigh64Target(Device.DeviceBase device)
        {
            var errorMessage = new StringBuilder(1024);
            Helper.CUDA.Solver.PushHigh64Target(UnmanagedInstance, device.CommonPointers.High64Target, errorMessage);

            if (errorMessage.Length > 0)
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", errorMessage.ToString());
        }

        protected override void PushTarget(Device.DeviceBase device)
        {
            var errorMessage = new StringBuilder(1024);
            Helper.CUDA.Solver.PushTarget(UnmanagedInstance, device.CommonPointers.Target, errorMessage);

            if (errorMessage.Length > 0)
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", errorMessage.ToString());
        }

        protected override void PushMidState(Device.DeviceBase device)
        {
            var errorMessage = new StringBuilder(1024);
            Helper.CUDA.Solver.PushMidState(UnmanagedInstance, device.CommonPointers.MidState, errorMessage);

            if (errorMessage.Length > 0)
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", errorMessage.ToString());
        }

        protected override void PushMessage(Device.DeviceBase device)
        {
            var errorMessage = new StringBuilder(1024);
            Helper.CUDA.Solver.PushMessage(UnmanagedInstance, device.CommonPointers.Message, errorMessage);

            if (errorMessage.Length > 0)
                PrintMessage(device.Type, device.Platform, device.DeviceID, "Error", errorMessage.ToString());
        }

        bool wasJustUnpaused = false;
        protected override void StartFinding(Device.DeviceBase device, bool isKingMaking)
        {

            Program.Print("startfinding");
            Console.WriteLine("startfinding");
            string jsonContent = File.ReadAllText("B0xToken.conf"); // replace with your JSON file
            JObject jsonObj = JObject.Parse(jsonContent);

            // Access the 'MinZKBTCperMint' value
            double CheckMinAmountInterval = jsonObj["CheckMinAmountIntervalInSeconds"].Value<double>();
            // Use the value as needed
            Console.WriteLine("CheckMinAmountInterval: " + CheckMinAmountInterval);

            var deviceCUDA = (Device.CUDA)device;
            try
            {
                if (!deviceCUDA.IsInitialized) return;

                while (!deviceCUDA.HasNewTarget || !deviceCUDA.HasNewChallenge)
                    Task.Delay(500).Wait();

                PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Info", "Start mining...");

                PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug",
                             string.Format("Threads: {0} Grid size: {1} Block size: {2}",
                                           deviceCUDA.Threads, deviceCUDA.Grid.X, deviceCUDA.Block.X));

                var errorMessage = new StringBuilder(1024);
                var currentChallenge = (byte[])Array.CreateInstance(typeof(byte), UINT256_LENGTH);

                Helper.CUDA.Solver.SetDevice(UnmanagedInstance, deviceCUDA.DeviceID, errorMessage);
                if (errorMessage.Length > 0)
                {
                    PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error", errorMessage.ToString());
                    return;
                }

                deviceCUDA.HashStartTime = DateTime.Now;
                deviceCUDA.HashCount = 0;
                deviceCUDA.IsMining = true;
                DateTime lastSubmitTime = DateTime.MinValue;
                DateTime lastSubmitTime2 = DateTime.MinValue;

                unsafe
                {
                    ulong* solutions = (ulong*)deviceCUDA.DeviceCUDA_Struct.Solutions.ToPointer();
                    uint* solutionCount = (uint*)deviceCUDA.DeviceCUDA_Struct.SolutionCount.ToPointer();
                    *solutionCount = 0;
                    do
                    {

                        bool wasJustUnpaused = false;
                        while (deviceCUDA.IsPause)
                        {
                            Task.Delay(500).Wait();
                            deviceCUDA.HashStartTime = DateTime.Now;
                            deviceCUDA.HashCount = 0;
                            wasJustUnpaused = true;
                        }
                        if (wasJustUnpaused)
                        {
                            Task.Delay(125).Wait();
                        PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug", "CUDA stabilization delay after unpause");
                     //   Program.Print("CUDA stabilization delay after unpause");
                       // Console.WriteLine("CUDA stabilization delay after unpause");

                        }
                        // ✅ SAFETY GUARD: Check if the device is still valid
                        if (!deviceCUDA.IsInitialized)
                        {
                            Program.Print("CUDA DEVICE NOT INIALIZED CONTINUEING");
                            Console.WriteLine("CUDA DEVICE NOT INIALIZED CONTINUEING");
                            Task.Delay(1000).Wait(); // Give it time to possibly reinitialize (if designed that way)
                            continue; // Skip this loop iteration


                        }
                        // ✅ Small delay to let CUDA fully stabilize
                        Task.Delay(60).Wait();
                        CheckInputs(deviceCUDA, isKingMaking, ref currentChallenge);

                        Work.IncrementPosition(ref deviceCUDA.DeviceCUDA_Struct.WorkPosition, deviceCUDA.Threads);
                        deviceCUDA.HashCount += deviceCUDA.Threads;

                        errorMessage.Clear(); // Start fresh each iteration
                        try
                        {
                            if (isKingMaking)
                                Helper.CUDA.Solver.HashMessage(UnmanagedInstance, ref deviceCUDA.DeviceCUDA_Struct, errorMessage);
                            else
                                Helper.CUDA.Solver.HashMidState(UnmanagedInstance, ref deviceCUDA.DeviceCUDA_Struct, errorMessage);
                        }
                        catch (Exception nativeEx)
                        {
                            PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error",
                                $"Native CUDA call failed: {nativeEx.GetType().Name} - {nativeEx.Message}");

                            PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug",
                                $"StackTrace: {nativeEx.StackTrace}");

                            deviceCUDA.IsMining = false;
                            return; // Stop mining gracefully
                        }
                        if (errorMessage.Length > 0)
                        {
                            PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error", errorMessage.ToString());
                            deviceCUDA.IsMining = false;
                        }

                        if (*solutionCount > 0)
                        {
                            // Check if enough time has passed since last submission (0.250 seconds)
                            if ((DateTime.Now - lastSubmitTime2).TotalMilliseconds >= 250)
                            {

                                int solutionsToSubmit = Math.Min((int)*solutionCount, 6);
                                var solutionArray = new ulong[solutionsToSubmit];

                                try
                                {

                                    for (var i = 0; i < solutionsToSubmit; i++)
                                    {
                                        solutionArray[i] = solutions[i];
                                    }

                                    PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug", $"Submitting {solutionsToSubmit} solutions via SubmitSolutions");
                                    PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Info",
                                                                $"About to submit solutions. Array length: {solutionArray.Length}, First solution: 0x{solutionArray[0]:X16}");

                                    try
                                    {
                                        SubmitSolutions(solutionArray, currentChallenge, device.Type, device.Platform, deviceCUDA.DeviceID, (uint)solutionsToSubmit, isKingMaking);
                                    }
                                    catch (Exception submitEx)
                                    {
                                        PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error",
                                            $"SubmitSolutions failed: {submitEx.GetType().Name}: {submitEx.Message}");
                                        PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug",
                                            $"StackTrace: {submitEx.StackTrace}");
                                    }
                                    *solutionCount = 0;
                                    lastSubmitTime = DateTime.Now;
                                    lastSubmitTime2 = DateTime.Now;
                                }
                                catch (Exception submitEx)
                                {
                                    PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error", $"SubmitSolutions exception: {submitEx.Message}");
                                }



                            }
                        }
                        else if ((DateTime.Now - lastSubmitTime).TotalSeconds >= CheckMinAmountInterval)
                        {


                            try
                            {
                                lastSubmitTime = DateTime.Now;
                                var solutionArray = new ulong[0]; // Empty array since *solutionCount is 0

                                PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Debug", "Submitting empty solutions via SubmitSolutions2");
                                SubmitSolutions2(solutionArray, currentChallenge, device.Type, device.Platform, deviceCUDA.DeviceID, 0, isKingMaking);
                            }
                            catch (Exception submitEx)
                            {
                                PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error", $"SubmitSolutions2 exception: {submitEx.Message}");
                            }



                        }

                    } while (deviceCUDA.IsMining);
                }

                PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Info", "Stop mining...");

                deviceCUDA.HashCount = 0;


            }
            catch (Exception ex)
            {
                 PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Error", $"StartFinding exception: {ex.Message}\nStackTrace: {ex.StackTrace}");

            }
            PrintMessage(device.Type, device.Platform, deviceCUDA.DeviceID, "Info", "Mining stopped.");
        }










        private void CleanupDevice(Device.CUDA deviceCUDA)
        {

            if (deviceCUDA == null || !deviceCUDA.IsInitialized)
                return;



            // Check if device objects are still valid
            if (deviceCUDA.DeviceCUDA_Struct.Solutions == IntPtr.Zero ||
                deviceCUDA.DeviceCUDA_Struct.SolutionCount == IntPtr.Zero)
            {
                PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Info", "Device objects already released.");
                deviceCUDA.IsInitialized = false;
                return;
            }


            var errorMessage = new StringBuilder(1024);

            try
            {
                Helper.CUDA.Solver.ReleaseDeviceObjects(UnmanagedInstance, ref deviceCUDA.DeviceCUDA_Struct, errorMessage);
                if (errorMessage.Length > 0)
                    PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Error", $"ReleaseDeviceObjects failed: {errorMessage}");
            }
            catch (Exception ex)
            {
                PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Error", $"Exception in ReleaseDeviceObjects: {ex.Message}");
            }

            Task.Delay(1000).Wait(); // Cooldown delay

            errorMessage.Clear();
            try
            {
                Helper.CUDA.Solver.ResetDevice(UnmanagedInstance, deviceCUDA.DeviceID, errorMessage);
                if (errorMessage.Length > 0)
                    PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Error", $"ResetDevice failed: {errorMessage}");
            }
            catch (Exception ex)
            {
                PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Error", $"Exception in ResetDevice: {ex.Message}");
            }

            deviceCUDA.IsInitialized = false;
            deviceCUDA.IsStopped = true;
            deviceCUDA.IsAssigned = false;

            PrintMessage(deviceCUDA.Type, deviceCUDA.Platform, deviceCUDA.DeviceID, "Info", "Device cleanup completed.");
        }
        #endregion
    }
}