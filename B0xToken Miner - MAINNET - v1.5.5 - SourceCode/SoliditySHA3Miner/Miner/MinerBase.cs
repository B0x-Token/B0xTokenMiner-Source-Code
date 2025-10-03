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

using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SoliditySHA3Miner.Miner
{
    public abstract class MinerBase : IMiner
    {
        public const int UINT32_LENGTH = 4;
        public const int UINT64_LENGTH = 8;
        public const int SPONGE_LENGTH = 200;
        public const int ADDRESS_LENGTH = 20;
        public const int UINT256_LENGTH = 32;
        public const int MESSAGE_LENGTH = UINT256_LENGTH + ADDRESS_LENGTH + UINT256_LENGTH;

        private static readonly object m_submissionQueueLock = new object();
        
        // Pre-allocated buffers to reduce GC pressure
        private static readonly byte[] s_messageBuffer = new byte[UINT256_LENGTH + ADDRESS_LENGTH + UINT256_LENGTH];
        private static readonly byte[] s_digestBuffer = new byte[UINT256_LENGTH];
        private static readonly object s_bufferLock = new object();

        // PRIORITY QUEUE SYSTEM - NO MORE TASK EXPLOSION
        private static readonly PriorityQueue<SubmissionData, int> PrioritySubmissionQueue = new PriorityQueue<SubmissionData, int>();
        private static readonly object PriorityQueueLock = new object();
        private static readonly SemaphoreSlim SubmissionSemaphore = new SemaphoreSlim(1, 1);
        private static string LastProcessedChallenge = string.Empty;
        private static bool PriorityProcessorStarted = false;

        // SEPARATE PRIORITY QUEUE FOR SUBMITSOLUTIONS2
        private static readonly PriorityQueue<SubmissionData2, int> PrioritySubmissionQueue2 = new PriorityQueue<SubmissionData2, int>();
        private static readonly object PriorityQueueLock2 = new object();
        private static readonly SemaphoreSlim SubmissionSemaphore2 = new SemaphoreSlim(1, 1);
        private static string LastProcessedChallenge2 = string.Empty;
        private static bool PriorityProcessor2Started = false;

        // Data structure for queued submissions
        private class SubmissionData
        {
            public ulong[] Solutions { get; set; }
            public byte[] Challenge { get; set; }
            public string PlatformType { get; set; }
            public string Platform { get; set; }
            public int DeviceID { get; set; }
            public uint SolutionCount { get; set; }
            public bool IsKingMaking { get; set; }
            public DateTime CreatedAt { get; set; }
            public int Priority { get; set; }
        }

        // Data structure for SubmitSolutions2 queue
        private class SubmissionData2
        {
            public ulong[] Solutions { get; set; }
            public byte[] Challenge { get; set; }
            public string PlatformType { get; set; }
            public string Platform { get; set; }
            public int DeviceID { get; set; }
            public uint SolutionCount { get; set; }
            public bool IsKingMaking { get; set; }
            public DateTime CreatedAt { get; set; }
            public int Priority { get; set; }
        }

        protected System.Timers.Timer m_hashPrintTimer;
        protected int m_pauseOnFailedScan;
        protected int m_failedScanCount;
        protected bool m_isCurrentChallengeStopSolving;
        protected bool m_isSubmitStale;        
        protected string m_AddressString;

        protected HexBigInteger m_Target;
        protected ulong m_High64Target;

        protected byte[] m_ChallengeBytes;
        protected byte[] m_AddressBytes;
        protected byte[] m_SolutionTemplateBytes;
        protected byte[] m_MidStateBytes;

        public IntPtr UnmanagedInstance { get; protected set; }
        
        #region IMiner

        public NetworkInterface.INetworkInterface NetworkInterface { get; protected set; }

        public Device.DeviceBase[] Devices { get; }

        public bool HasAssignedDevices => Devices?.Any(d => d.IsAssigned) ?? false;

        public bool HasMonitoringAPI { get; protected set; }

        public bool IsAnyInitialised => Devices?.Any(d => d.IsInitialized) ?? false;

        public bool IsMining => Devices?.Any(d => d.IsMining) ?? false;

        public bool IsPause => Devices?.Any(d => d.IsPause) ?? false;

        public bool IsStopped => Devices?.Any(d => d.IsStopped) ?? true;

        public void StartMining(int networkUpdateInterval, int hashratePrintInterval)
        {
            try
            {
                NetworkInterface.ResetEffectiveHashrate();
                NetworkInterface.UpdateMiningParameters();

                m_hashPrintTimer = new System.Timers.Timer(hashratePrintInterval);
                m_hashPrintTimer.Elapsed += HashPrintTimer_Elapsed;
                m_hashPrintTimer.Start();

                var isKingMaking = !string.IsNullOrWhiteSpace(Work.GetKingAddressString());
                StartFindingAll(isKingMaking);

                // Start priority processors once when mining starts
                StartPriorityProcessor();
                StartPriorityProcessor2();
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
                StopMining();
            }
        }

        public void StopMining()
        {
            try
            {
                if (m_hashPrintTimer != null)
                    m_hashPrintTimer.Stop();

                NetworkInterface.ResetEffectiveHashrate();

                foreach (var device in Devices)
                    device.IsMining =  false;
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
            }
        }

        public ulong GetHashRateByDevice(Device.DeviceBase device)
        {
            var hashRate = 0ul;

            if (!IsPause)
                hashRate = (ulong)(device.HashCount / (DateTime.Now - device.HashStartTime).TotalSeconds);

            return hashRate;
        }

        protected void NetworkInterface_OnGetTotalHashrate(NetworkInterface.INetworkInterface sender, ref ulong totalHashrate)
        {
            totalHashrate += GetTotalHashrate();
        }

        public ulong GetTotalHashrate()
        {
            var totalHashrate = 0ul;
            try
            {
                foreach (var device in Devices)
                    totalHashrate += GetHashRateByDevice(device);
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
            }
            return totalHashrate;
        }

        public virtual void Dispose()
        {
            NetworkInterface.OnGetTotalHashrate -= NetworkInterface_OnGetTotalHashrate;
            NetworkInterface.OnGetMiningParameterStatus -= NetworkInterface_OnGetMiningParameterStatus;
            NetworkInterface.OnNewChallenge -= NetworkInterface_OnNewChallenge;
            NetworkInterface.OnNewTarget -= NetworkInterface_OnNewTarget;
            NetworkInterface.OnStopSolvingCurrentChallenge -= NetworkInterface_OnStopSolvingCurrentChallenge;

            if (m_hashPrintTimer != null)
            {
                try
                {
                    m_hashPrintTimer.Elapsed -= HashPrintTimer_Elapsed;
                    m_hashPrintTimer.Dispose();
                }
                catch { }
                m_hashPrintTimer = null;
            }

            try
            {
                if (Devices != null)
                    Devices.AsParallel().
                            ForAll(d => d.Dispose());
            }
            catch { }

            try { NetworkInterface.Dispose(); }
            catch { }
        }

        #endregion IMiner
        
        protected abstract void HashPrintTimer_Elapsed(object sender, ElapsedEventArgs e);
        protected abstract void AssignDevices();
        protected abstract void PushHigh64Target(Device.DeviceBase device);
        protected abstract void PushTarget(Device.DeviceBase device);
        protected abstract void PushMidState(Device.DeviceBase device);
        protected abstract void PushMessage(Device.DeviceBase device);
        protected abstract void StartFinding(Device.DeviceBase device, bool isKingMaking);

        public MinerBase(NetworkInterface.INetworkInterface networkInterface, Device.DeviceBase[] devices, bool isSubmitStale, int pauseOnFailedScans)
        {
            m_failedScanCount = 0;
            m_pauseOnFailedScan = pauseOnFailedScans;
            m_isSubmitStale = isSubmitStale;
            NetworkInterface = networkInterface;
            Devices = devices;

            m_ChallengeBytes = (byte[])Array.CreateInstance(typeof(byte), UINT256_LENGTH);
            m_AddressBytes = (byte[])Array.CreateInstance(typeof(byte), ADDRESS_LENGTH);

            NetworkInterface.OnGetTotalHashrate += NetworkInterface_OnGetTotalHashrate;
            NetworkInterface.OnGetMiningParameterStatus += NetworkInterface_OnGetMiningParameterStatus;
            NetworkInterface.OnNewChallenge += NetworkInterface_OnNewChallenge;
            NetworkInterface.OnNewTarget += NetworkInterface_OnNewTarget;
            NetworkInterface.OnStopSolvingCurrentChallenge += NetworkInterface_OnStopSolvingCurrentChallenge;
        }

        protected void PrintMessage(string platformType, string platform, int deviceEnum, string type, string message)
        {
            var sFormat = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(platformType)) sFormat.Append(platformType + " ");

            if (!string.IsNullOrWhiteSpace(platform))
            {
                if (sFormat.Length > 0)
                    sFormat.AppendFormat("({0}) ", platform);
                else
                    sFormat.Append(platform + " ");
            }
            if (deviceEnum > -1) sFormat.Append("ID: {0} ");

            switch (type.ToUpperInvariant())
            {
                case "INFO":
                    sFormat.Append(deviceEnum > -1 ? "[INFO] {1}" : "[INFO] {0}");
                    break;

                case "WARN":
                    sFormat.Append(deviceEnum > -1 ? "[WARN] {1}" : "[WARN] {0}");
                    break;

                case "ERROR":
                    sFormat.Append(deviceEnum > -1 ? "[ERROR] {1}" : "[ERROR] {0}");
                    break;

                case "DEBUG":
                default:
#if DEBUG
                    sFormat.Append(deviceEnum > -1 ? "[DEBUG] {1}" : "[DEBUG] {0}");
                    break;
#else
                    return;
#endif
            }
            Program.Print(deviceEnum > -1
                ? string.Format(sFormat.ToString(), deviceEnum, message)
                : string.Format(sFormat.ToString(), message));
        }

        private void NetworkInterface_OnGetMiningParameterStatus(NetworkInterface.INetworkInterface sender, bool success)
        {
            try
            {
                if (UnmanagedInstance != null && UnmanagedInstance.ToInt64() != 0)
                {
                    if (success)
                    {
                        var isPause = Devices.All(d => d.IsPause);

                        if (m_isCurrentChallengeStopSolving) { isPause = true; }
                        else if (isPause)
                        {
                            if (m_failedScanCount > m_pauseOnFailedScan)
                                m_failedScanCount = 0;

                            isPause = false;
                        }
                        foreach (var device in Devices)
                            device.IsPause = isPause;
                    }
                    else
                    {
                        m_failedScanCount++;

                        var isMining = Devices.Any(d => d.IsMining);

                        if (m_failedScanCount > m_pauseOnFailedScan && IsMining)
                            foreach (var device in Devices)
                                device.IsPause = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
            }
        }


        // 2. MODIFIED NetworkInterface_OnNewChallenge - Clear all queued solutions when new challenge arrives



        // 2. MODIFIED NetworkInterface_OnNewChallenge - Clear all queued solutions when new challenge arrives
        private void NetworkInterface_OnNewChallenge(NetworkInterface.INetworkInterface sender, byte[] challenge, string address)
        {
            try
            {
                if (UnmanagedInstance != null && UnmanagedInstance.ToInt64() != 0)
                {
                    var newChallengeString = Convert.ToHexString(challenge);

                    // Check if this is actually a new challenge
                    var currentChallengeString = m_ChallengeBytes != null ? Convert.ToHexString(m_ChallengeBytes) : string.Empty;

                    if (newChallengeString == currentChallengeString)
                    {
                        PrintMessage(string.Empty, string.Empty, -1, "Info", "Challenge unchanged, skipping update");
                        return; // Same challenge, no need to process
                    }

                    PrintMessage(string.Empty, string.Empty, -1, "Info", $"New challenge detected: {newChallengeString.Substring(0, 16)}...");

                    // CLEAR ALL QUEUED SOLUTIONS FOR OLD CHALLENGES - only when challenge actually changes
                    ClearAllOldChallengeSubmissions(newChallengeString);

                    for (var i = 0; i < challenge.Length; i++)
                        m_ChallengeBytes[i] = challenge[i];

                    m_AddressString = address;
                    Utils.Numerics.AddressStringToByte20Array(address, ref m_AddressBytes, isChecksum: false);

                    m_SolutionTemplateBytes = Work.SolutionTemplate;
                    m_MidStateBytes = Helper.CPU.GetMidState(m_ChallengeBytes, m_AddressBytes, m_SolutionTemplateBytes);

                    foreach (var device in Devices)
                    {
                        Array.ConstrainedCopy(m_ChallengeBytes, 0, device.Message, 0, UINT256_LENGTH);
                        Array.ConstrainedCopy(m_AddressBytes, 0, device.Message, UINT256_LENGTH, ADDRESS_LENGTH);
                        Array.ConstrainedCopy(m_SolutionTemplateBytes, 0, device.Message, UINT256_LENGTH + ADDRESS_LENGTH, UINT256_LENGTH);

                        Array.Copy(m_ChallengeBytes, device.Challenge, UINT256_LENGTH);
                        Array.Copy(m_MidStateBytes, device.MidState, SPONGE_LENGTH);
                        device.HasNewChallenge = true;
                    }

                    if (m_isCurrentChallengeStopSolving)
                    {
                        foreach (var device in Devices)
                            device.IsPause = false;

                        m_isCurrentChallengeStopSolving = false;
                    }

                    PrintMessage(string.Empty, string.Empty, -1, "Info", $"Challenge updated successfully: {newChallengeString.Substring(0, 16)}...");
                }
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
            }
        }

        // 3. NEW FUNCTION - Clear ALL solutions for old challenges (more aggressive than before)
        private void ClearAllOldChallengeSubmissions(string newChallengeString)
        {
            int clearedCount = 0;

            lock (PriorityQueueLock)
            {
                // Create a new queue and only keep solutions for the new challenge
                var newQueue = new PriorityQueue<SubmissionData, int>();

                while (PrioritySubmissionQueue.TryDequeue(out var submission, out var priority))
                {
                    var submissionChallengeString = Convert.ToHexString(submission.Challenge);

                    if (submissionChallengeString == newChallengeString)
                    {
                        // Keep solutions for the new challenge
                        newQueue.Enqueue(submission, priority);
                    }
                    else
                    {
                        // Discard solutions for old challenges
                        clearedCount++;
                    }
                }

                // Replace the old queue with the new one
                while (newQueue.TryDequeue(out var submission, out var priority))
                {
                    PrioritySubmissionQueue.Enqueue(submission, priority);
                }
            }

            // Do the same for SubmitSolutions2 queue
            lock (PriorityQueueLock2)
            {
                var newQueue2 = new PriorityQueue<SubmissionData2, int>();

                while (PrioritySubmissionQueue2.TryDequeue(out var submission, out var priority))
                {
                    var submissionChallengeString = Convert.ToHexString(submission.Challenge);

                    if (submissionChallengeString == newChallengeString)
                    {
                        newQueue2.Enqueue(submission, priority);
                    }
                    else
                    {
                        clearedCount++;
                    }
                }

                while (newQueue2.TryDequeue(out var submission, out var priority))
                {
                    PrioritySubmissionQueue2.Enqueue(submission, priority);
                }
            }

            if (clearedCount > 0)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Info", $"Cleared {clearedCount} solutions for old challenges");
            }
        }

        private void NetworkInterface_OnNewTarget(NetworkInterface.INetworkInterface sender, HexBigInteger target)
        {
            try
            {
                var targetBytes = Utils.Numerics.FilterByte32Array(target.Value.ToByteArray(isUnsigned: true, isBigEndian:true));
                var high64Bytes = targetBytes.Take(UINT64_LENGTH).Reverse().ToArray();

                m_Target = target;
                m_High64Target = BitConverter.ToUInt64(high64Bytes);

                foreach (var device in Devices)
                {
                    Array.Copy(targetBytes, device.Target, UINT256_LENGTH);
                    device.High64Target[0] = m_High64Target;
                    device.HasNewTarget = true;
                }
            }
            catch (Exception ex)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Error", ex.Message);
            }
        }

        private void NetworkInterface_OnStopSolvingCurrentChallenge(NetworkInterface.INetworkInterface sender, bool stopSolving = true)
        {
            if (stopSolving)
            {
                if (m_isCurrentChallengeStopSolving) return;

                m_isCurrentChallengeStopSolving = true;

                foreach (var device in Devices)
                    device.IsPause = true;

            }
            else if (m_isCurrentChallengeStopSolving)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Info", "Resume mining...");

                m_isCurrentChallengeStopSolving = false;

                foreach (var device in Devices)
                    device.IsPause = false;
            }
        }

        private void StartFindingAll(bool isKingMaking)
        {
            foreach (var device in Devices)
                Task.Factory.StartNew(() => StartFinding(device, isKingMaking));
        }

        protected void CheckInputs(Device.DeviceBase device, bool isKingMaking, ref byte[] currentChallenge)
        {
            lock (device) // <- prevent race if another thread mutates device state
            {
                if (device.HasNewTarget || device.HasNewChallenge)
                {
                    if (device.HasNewTarget)
                    {
                        if (isKingMaking) PushTarget(device);
                        else PushHigh64Target(device);
                        device.HasNewTarget = false;
                    }

                    if (device.HasNewChallenge)
                    {
                        if (isKingMaking) PushMessage(device);
                        else PushMidState(device);

                        Array.Copy(device.Challenge, currentChallenge, UINT256_LENGTH);
                        device.HasNewChallenge = false;
                    }

                    device.HashStartTime = DateTime.Now;
                    device.HashCount = 0;
                }
            }
        }

        // PRIORITY PROCESSOR FOR SUBMITSOLUTIONS - ONE BACKGROUND THREAD
        private void StartPriorityProcessor()
        {
            if (PriorityProcessorStarted) return;
            PriorityProcessorStarted = true;
            
            ThreadPool.QueueUserWorkItem(_ => ProcessSolutionsQueue());
            Console.WriteLine("Priority processor started for SubmitSolutions");
        }

        private void ProcessSolutionsQueue()
        {
            while (true)
            {
                try
                {



                    
                    SubmissionData nextSubmission = null;
                    
                    lock (PriorityQueueLock)
                    {
                        if (PrioritySubmissionQueue.Count > 0)
                        {
                            PrioritySubmissionQueue.TryDequeue(out nextSubmission, out var priority);
                        }
                    }
                    
                    if (nextSubmission != null)
                    {
                        if (nextSubmission.Priority == 1)
                        {
                            Console.WriteLine($"Processing HIGH PRIORITY new challenge from device {nextSubmission.DeviceID}");
                        }
                        
                        ProcessSubmission(nextSubmission);
                    }
                    else
                    {
                        Thread.Sleep(10); // Brief wait if no work
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Priority processor error: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }

        // PRIORITY PROCESSOR FOR SUBMITSOLUTIONS2 - SEPARATE BACKGROUND THREAD
        private void StartPriorityProcessor2()
        {
            if (PriorityProcessor2Started) return;
            PriorityProcessor2Started = true;
            
            ThreadPool.QueueUserWorkItem(_ => ProcessSolutions2Queue());
            //    Console.WriteLine("Priority processor started for SubmitSolutions2");
        }

        private void ProcessSolutions2Queue()
        {
            while (true)
            {
                try
                {
                    SubmissionData2 nextSubmission = null;

                    lock (PriorityQueueLock2)
                    {
                        if (PrioritySubmissionQueue2.Count > 0)
                        {
                            PrioritySubmissionQueue2.TryDequeue(out nextSubmission, out var priority);
                        }
                    }

                    if (nextSubmission != null)
                    {
                        if (nextSubmission.Priority == 1)
                        {
                            //    Console.WriteLine($"Processing HIGH PRIORITY new challenge for SubmitSolutions2 from device {nextSubmission.DeviceID}");
                        }

                        ProcessSubmission2(nextSubmission);
                    }
                    else
                    {
                        Thread.Sleep(10); // Brief wait if no work
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SubmitSolutions2 processor error: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }



        // 1. MODIFIED SubmitSolutions - Queue solutions instead of processing immediately
        protected void SubmitSolutions(ulong[] solutions, byte[] challenge, string platformType, string platform, int deviceID, uint solutionCount, bool isKingMaking)
        {
            var challengeString = Convert.ToHexString(challenge);
            var currentNetworkChallenge = Convert.ToHexString(NetworkInterface.CurrentChallenge);

            // Only queue solutions for the current challenge
            if (challengeString != currentNetworkChallenge)
            {
                // Discard solutions for old challenges
                PrintMessage(platformType, platform, deviceID, "Info", $"Discarding {solutions.Length} solutions for old challenge");
                return;
            }

            // Queue each solution individually for the current challenge
            foreach (var solution in solutions)
            {
                var submissionData = new SubmissionData
                {
                    Solutions = new ulong[] { solution }, // Single solution
                    Challenge = (byte[])challenge.Clone(),
                    PlatformType = platformType,
                    Platform = platform,
                    DeviceID = deviceID,
                    SolutionCount = 1, // Always 1 since we're queuing individually
                    IsKingMaking = isKingMaking,
                    CreatedAt = DateTime.UtcNow,
                    Priority = 2 // Normal priority for current challenge solutions
                };

                lock (PriorityQueueLock)
                {
                    PrioritySubmissionQueue.Enqueue(submissionData, 2);
                }
            }

            PrintMessage(platformType, platform, deviceID, "Info", $"Queued {solutions.Length} solutions for current challenge");
            MonitorQueueSizeAndControlMining();
        }

        // FIXED SUBMITSOLUTIONS2 - Move monitoring AFTER queuing
        protected void SubmitSolutions2(ulong[] solutions, byte[] challenge, string platformType, string platform, int deviceID, uint solutionCount, bool isKingMaking)
{
         Program.Print("SubmitSolution2");
    // Determine priority based on challenge
    var challengeString = Convert.ToHexString(challenge);
    var currentNetworkChallenge = Convert.ToHexString(NetworkInterface.CurrentChallenge);
    
    int priority;
    if (challengeString == currentNetworkChallenge && LastProcessedChallenge2 != challengeString)
    {
        priority = 1; // HIGHEST PRIORITY - New current challenge
        LastProcessedChallenge2 = challengeString;
       // Console.WriteLine($"NEW CHALLENGE DETECTED for SubmitSolutions2: {challengeString.Substring(0, 16)}...");
        
        // Clear old challenge submissions
        ClearOldChallengeSubmissions2(challengeString);
    }
    else if (challengeString == currentNetworkChallenge)
    {
        priority = 2; // High priority - Current challenge
    }
    else
    {
        priority = 3; // Low priority - Old challenges
    }

    // Create submission data
    var submissionData = new SubmissionData2
    {
        Solutions = (ulong[])solutions.Clone(),
        Challenge = (byte[])challenge.Clone(),
        PlatformType = platformType,
        Platform = platform,
        DeviceID = deviceID,
        SolutionCount = solutionCount,
        IsKingMaking = isKingMaking,
        CreatedAt = DateTime.UtcNow,
        Priority = priority
    };

    // Queue with priority (NO Task.Factory.StartNew!)
    lock (PriorityQueueLock2)
    {
        PrioritySubmissionQueue2.Enqueue(submissionData, priority);
        
        if (priority == 1)
        {
            Console.WriteLine($"New challenge queued for SubmitSolutions2 with HIGHEST priority - Queue size: {PrioritySubmissionQueue2.Count}");
        }
    }

    // CHECK QUEUE SIZE AFTER QUEUING - THIS IS THE KEY FIX
    MonitorQueueSizeAndControlMining();
}




        // Clear old challenge submissions
        private void ClearOldChallengeSubmissions(string newChallengeString)
        {
            lock (PriorityQueueLock)
            {
                var tempSubmissions = new List<(SubmissionData data, int priority)>();
                int droppedCount = 0;
                
                while (PrioritySubmissionQueue.TryDequeue(out var submission, out var priority))
                {
                    var submissionChallengeString = Convert.ToHexString(submission.Challenge);
                    var submissionAge = DateTime.UtcNow - submission.CreatedAt;
                    
                    if (submissionChallengeString == newChallengeString || submissionAge.TotalSeconds < 5)
                    {
                        var newPriority = submissionChallengeString == newChallengeString ? 1 : priority;
                        tempSubmissions.Add((submission, newPriority));
                    }
                    else
                    {
                        droppedCount++;
                    }
                }
                
                foreach (var (data, priority) in tempSubmissions)
                {
                    PrioritySubmissionQueue.Enqueue(data, priority);
                }
                
                if (droppedCount > 0)
                {
                    Console.WriteLine($"Cleared old challenge submissions: ",droppedCount);
                }
            }
        }

        // Clear old challenge submissions for SubmitSolutions2
        private void ClearOldChallengeSubmissions2(string newChallengeString)
        {
            lock (PriorityQueueLock2)
            {
                var tempSubmissions = new List<(SubmissionData2 data, int priority)>();
                int droppedCount = 0;
                
                while (PrioritySubmissionQueue2.TryDequeue(out var submission, out var priority))
                {
                    var submissionChallengeString = Convert.ToHexString(submission.Challenge);
                    var submissionAge = DateTime.UtcNow - submission.CreatedAt;
                    
                    if (submissionChallengeString == newChallengeString || submissionAge.TotalSeconds < 5)
                    {
                        var newPriority = submissionChallengeString == newChallengeString ? 1 : priority;
                        tempSubmissions.Add((submission, newPriority));
                    }
                    else
                    {
                        droppedCount++;
                    }
                }
                
                foreach (var (data, priority) in tempSubmissions)
                {
                    PrioritySubmissionQueue2.Enqueue(data, priority);
                }
                
                if (droppedCount > 0)
                {
                    Console.WriteLine($"Cleared old challenge submissions for SubmitSolutions2");
                }
            }
        }




        // 4. MODIFIED MonitorQueueSizeAndControlMining - Adjusted thresholds for 2016-solution batches
        private void MonitorQueueSizeAndControlMining()
        {
            int totalQueueSize;
            int queue1Size;
            int queue2Size;

            lock (PriorityQueueLock)
            {
                queue1Size = PrioritySubmissionQueue.Count;
            }

            lock (PriorityQueueLock2)
            {
                queue2Size = PrioritySubmissionQueue2.Count;
            }

            totalQueueSize = queue1Size + queue2Size;

            if (queue1Size > 0)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Info",
                    $"Queue status: Total={totalQueueSize}, SubmitSolutions={queue1Size}, SubmitSolutions2={queue2Size}");
            }

            // Stop solving if queue size exceeds 20 (allowing for more solutions to accumulate)
            if (totalQueueSize > 20 && !m_isCurrentChallengeStopSolving)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Warn",
                    $"Queue size ({totalQueueSize}) exceeded 20, pausing mining to process backlog...");
                NetworkInterface_OnStopSolvingCurrentChallenge(NetworkInterface, true);
            }
            // Resume solving if queue size drops below 5
            else if (totalQueueSize < 5 && m_isCurrentChallengeStopSolving)
            {
                PrintMessage(string.Empty, string.Empty, -1, "Info",
                    $"Queue size ({totalQueueSize}) below 5, resuming mining...");
                NetworkInterface_OnStopSolvingCurrentChallenge(NetworkInterface, false);
            }
        }







        private static int s_successfulSubmissionCount = 0;
private static DateTime s_lastSubmissionTime = DateTime.MinValue;
        uint solutionNumber = 0;

        // 5. MODIFIED ProcessSubmission - Process solutions one by one with better challenge validation
        private void ProcessSubmission(SubmissionData submissionData)
        {
            try
            {
                SubmissionSemaphore.Wait();

                try
                {

                var timeSinceLastSubmission = DateTime.Now - s_lastSubmissionTime;
                var minimumDelay = TimeSpan.FromMilliseconds(100);

                    s_successfulSubmissionCount++;
                    // Check if we need a longer delay (every 20 submissions)
                    if (s_successfulSubmissionCount % 8 == 0)
                    {
                        PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                            "Info", $"8 submissions reached (#{s_successfulSubmissionCount}). Taking 1.5-second break...");
                        Task.Delay(1500).Wait(); // 5 second delay
                    }
                    else if (timeSinceLastSubmission < minimumDelay)
                    {
                        var remainingDelay = minimumDelay - timeSinceLastSubmission;
                        PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                            "Info", $"Rate limiting: waiting {remainingDelay.TotalMilliseconds:F0}ms...");
                        Task.Delay((int)remainingDelay.TotalMilliseconds).Wait();
                    }
                    else
                    {
                        s_successfulSubmissionCount = 0;
                    }
                
               // s_lastSubmissionTime = DateTime.Now;


                    // Double-check that this solution is still for the current challenge
                    var submissionChallengeString = Convert.ToHexString(submissionData.Challenge);
                    var currentChallengeString = Convert.ToHexString(NetworkInterface.CurrentChallenge);

                    if (submissionChallengeString != currentChallengeString)
                    {
                        PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                            "Info", "Skipping solution for old challenge");
                        return;
                    }

                    lock (m_submissionQueueLock)
                    {
                        // Process the single solution
                        var solution = submissionData.Solutions[0];

                        if (NetworkInterface.GetType().IsAssignableFrom(typeof(NetworkInterface.SlaveInterface)))
                            if (((NetworkInterface.SlaveInterface)NetworkInterface).IsPause)
                                return;

                        PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                            "Info", "Processing solution...");

                        var solutionBytes = BitConverter.GetBytes(solution);
                        var nonceBytes = Utils.Numerics.FilterByte32Array(m_SolutionTemplateBytes.ToArray());

                        byte[] messageBytes;
                        byte[] digestBytes;
                        GCHandle messageHandle;
                        GCHandle digestHandle;
                        IntPtr messagePointer;
                        IntPtr digestPointer;

                        lock (s_bufferLock)
                        {
                            messageBytes = new byte[s_messageBuffer.Length];
                            digestBytes = new byte[s_digestBuffer.Length];

                            messageHandle = GCHandle.Alloc(messageBytes, GCHandleType.Pinned);
                            messagePointer = messageHandle.AddrOfPinnedObject();
                            digestHandle = GCHandle.Alloc(digestBytes, GCHandleType.Pinned);
                            digestPointer = digestHandle.AddrOfPinnedObject();
                        }

                        try
                        {
                            if (submissionData.IsKingMaking)
                                Array.ConstrainedCopy(solutionBytes, 0, nonceBytes, ADDRESS_LENGTH, UINT64_LENGTH);
                            else
                                Array.ConstrainedCopy(solutionBytes, 0, nonceBytes, (UINT256_LENGTH / 2) - (UINT64_LENGTH / 2), UINT64_LENGTH);

                            Array.ConstrainedCopy(submissionData.Challenge, 0, messageBytes, 0, UINT256_LENGTH);
                            Array.ConstrainedCopy(m_AddressBytes, 0, messageBytes, UINT256_LENGTH, ADDRESS_LENGTH);
                            Array.ConstrainedCopy(nonceBytes, 0, messageBytes, UINT256_LENGTH + ADDRESS_LENGTH, UINT256_LENGTH);

                            Helper.CPU.Solver.SHA3(messagePointer, digestPointer);

                            var nonceString = Utils.Numerics.Byte32ArrayToHexString(nonceBytes);
                            var challengeString = Utils.Numerics.Byte32ArrayToHexString(submissionData.Challenge);
                            var digestString = Utils.Numerics.Byte32ArrayToHexString(digestBytes);
                            var digest = new HexBigInteger(digestString);

                            if (digest.Value >= m_Target.Value)
                            {
                                // Invalid solution - skip silently
                                PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                                    "Info", "Solution did not meet target difficulty");
                            }
                            else
                            {
                                PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                                    "Info", "Valid solution found! Submitting...");

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"VALID SOLUTION from device {submissionData.DeviceID}!");
                                Console.ResetColor();

                                try
                                {
                                    NetworkInterface.SubmitSolution(m_AddressString, digest, submissionData.Challenge,
                                        NetworkInterface.Difficulty, nonceBytes, this);

                                    s_lastSubmissionTime = DateTime.Now;
                
                                }
                                catch (Exception netEx)
                                {
                                    PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                                        "Error", $"Network submission failed: {netEx.Message}");
                                }
                            }
                        }
                        finally
                        {
                            messageHandle.Free();
                            digestHandle.Free();
                        }
                    }
                }
                finally
                {
                    SubmissionSemaphore.Release();
                }
                MonitorQueueSizeAndControlMining();
            }
            catch (Exception ex)
            {
                PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID,
                    "Error", $"Solution processing failed: {ex.Message}");
            }
        }

        // Process individual SubmitSolutions2 submission
        private void ProcessSubmission2(SubmissionData2 submissionData)
        {
            try
            {
                SubmissionSemaphore2.Wait();
                
                try
                {
                    PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID, "Info", 
                        "Every 100 Seconds we will check if conditions are correct to submit verified answers");
                    
                    // Your original digest and nonce values
                    var digest = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                    var nonceBytes = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                    
                    // Convert to byte arrays as you had them
                    byte[] byteArray = Encoding.UTF8.GetBytes(nonceBytes);
                    byte[] byteArra2y = Encoding.UTF8.GetBytes(digest);

                    // Submit directly without creating a new task
                    NetworkInterface.SubmitSolution(m_AddressString,
                                                    byteArra2y,
                                                    submissionData.Challenge,
                                                    NetworkInterface.Difficulty,
                                                    byteArray,
                                                    this);


                }
                finally
                {
                    SubmissionSemaphore2.Release();
                }

                MonitorQueueSizeAndControlMining();
            }
            catch (Exception ex)
            {
                PrintMessage(submissionData.PlatformType, submissionData.Platform, submissionData.DeviceID, "Error", 
                    $"SubmitSolutions2 processing failed: {ex.Message}");
            }
        }

    }
}