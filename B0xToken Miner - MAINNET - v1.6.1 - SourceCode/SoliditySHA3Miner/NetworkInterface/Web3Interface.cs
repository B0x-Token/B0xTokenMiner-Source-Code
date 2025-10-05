﻿/*
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
using Nethereum.ABI;
// ADD THIS USING DIRECTIVE to the top of your Web3Interface.cs file:
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Contracts.CQS;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace SoliditySHA3Miner.NetworkInterface

{


    // Define the DTO at class level
    [FunctionOutput]
    public class TotalCostsOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "B0xYouGet", 1)]
        public BigInteger B0xYouGet { get; set; }
        
        [Parameter("uint256", "ETHYouGet", 2)]
        public BigInteger ETHYouGet { get; set; }
        
        [Parameter("uint256", "ETHyouSpend", 3)]
        public BigInteger ETHyouSpend { get; set; }
        
        [Parameter("uint256", "ETHPrice", 4)]
        public BigInteger ETHPrice { get; set; }
        
        [Parameter("uint256", "secondsFromPreviousMintreturn", 5)]
        public BigInteger SecondsFromPreviousMintReturn { get; set; }

        // Add parameterless constructor
        public TotalCostsOutputDTO() { }
    }

    // Define the DTO at class level
    [FunctionOutput]
    public class BlockInfoOutputDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "slowBlockz", 1)]
        public BigInteger SlowBlockz { get; set; }
        
        [Parameter("uint256", "secondsUntilAdjustmentz", 2)]
        public BigInteger SecondsUntilAdjustmentz { get; set; }
        
        [Parameter("uint256", "blocksFromReadjustz", 3)]
        public BigInteger BlocksFromReadjustz { get; set; }
        
        [Parameter("uint256", "blocksToReadjustz", 4)]
        public BigInteger BlocksToReadjustz { get; set; }

        // Add parameterless constructor
        public BlockInfoOutputDTO() { }
    }


    public class Web3Interface : NetworkInterfaceBase
    {



        public DateTime lastExecutionTime = DateTime.Now;

        public int prevEpcohToreadjust = 2017;
        public int MAXNUMBEROFMINTSPOSSIBLE = 2500;
        public HexBigInteger LastSubmitGasPrice { get; private set; }
        public int _BLOCKS_PER_READJUSTMENT_;
        //ONE BELOW _BLOCKS_PER_READJUSTMENT for _BLOCKS_PER_READJUSTMENT_;
        private const int MAX_TIMEOUT = 15;
        public bool OnlyRunPayMasterOnce = true;
        public int retryCount = 0;
        public byte[][] digestArray2 = new byte[][] { };
        public byte[][] challengeArray2 = new byte[][] { };
        public byte[] challengeArrayFirstOld = new byte[] { };
        public byte[] challengeArraySecondOld = new byte[] { };
        public byte[][] nonceArray2 = new byte[][] { };
        List<byte[]> digestList = new List<byte[]>();

        public  byte[] newEndOfCurrentCounterCurrentChallenge;
        public  int oldCurrentCounter = 0;
        public  int oldcurrentCounterLocatioNEnd = 0;


        private readonly Web3 m_web3;
        private readonly Contract m_contract;
        private readonly Contract m_contract_MULTICALL;
        private readonly Contract m_contract2Delegate;
        private readonly Account m_account;
        private readonly Function m_mintMethod;
        private readonly Function m_mintMethodwithETH;
        private readonly Function m_NFTmintMethod;
        private readonly Function m_ERC20mintMethod;
        private readonly Function m_mintNFTMethod;
        private readonly Function m_MintERC20;
        private readonly Function m_transferMethod;
        private readonly Function m_getPaymaster;
        private readonly Function m_getMiningDifficulty2;
        private readonly Function m_getMiningDifficulty22;
        private Function m_getMiningDifficulty23;
        private Function m_getMiningDifficulty;
        private readonly Function m_getETH2SEND;
        private readonly Function m_blocksFromReadjust;
        private readonly Function m_Multicall;
        private readonly Function m_getMiningDifficulty3;
        private readonly Function m_getMiningDifficulty4;
        private readonly Function m_getMiningDifficulty22Static;
        private readonly Function m_getEpoch;
        private readonly Function m_getEpochOld;
        private Function m_getMiningTarget;
        private Function m_getMiningTarget3;
        private readonly Function m_getMiningTarget2;
        private readonly Function m_getMiningTarget23Static;
        private Function m_getSecondsUntilAdjustment;
        private Function m_getBlockInfo;
        private Function m_getChallengeNumber;
        private readonly Function m_getChallengeNumber2;
        private readonly Function m_getChallengeNumber2Static;
        private readonly Function m_getMiningReward;

                private Function m_getCosts;
        
                private Function m_getCostsALL;
        
        private  float M_current_miningReward;
        
        private readonly Function m_MAXIMUM_TARGET;

        private readonly Function m_CLM_ContractProgress;


        private readonly int m_mintMethodInputParamCount;
        
        private bool RunThisIfExcessMints = true;
        private int errorGreaterThan5 = 0;
        private readonly float m_gasToMine;
        private float m_ResetIfEpochGoesUp = 3000;
        private bool m_ResetIfEpochGoesUpBOOL = true;
        private readonly float m_gasApiMax;
        public ulong m_gasLimit;
        private readonly float m_gasPricePriority;
        private bool GotitDoneFirst = true;
        private int m_multiplier2 = 1;
        private int m_chainID = 11155111;
        private readonly bool m_ETHwithMints;
        private readonly string m_ETHwithMints_Paymaster_address;
        private readonly string m_gasApiURL2;
        private readonly string m_gasApiPath2;
        private readonly string m_gasApiPath3;
        private readonly string m_gasApiURL;
        private readonly string m_gasApiPath;
        private readonly float m_gasApiOffset;
        private readonly float m_gasApiMultiplier;
        private readonly float m_gasApiMultiplier2;
        private int m_MinSolvesperMint;
        public bool didRunSpecialAndNeedsToRunAgain;
        private int m_MaxSolvesperMint;
        private float m_MaxZKBTCperMintOLD;
        private int m_MaxSolvesperMintORIGINAL;
        private int skipThisMany = 0;
        private readonly string[] ethereumAddresses2;
        private BigInteger epochNumber55552;
        private readonly float m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC;
        private string PayzAddresszes;
        private string m_mintToaddress;
        private string[] ERC20AddressesToMint;
        private string m_nftAddressesAndTokenIds;
        
        private string[] m_nftAddresses;
        private string[] m_nftTokenIds;

        private bool bool_PayzAddresszes = false;
        private System.Threading.ManualResetEvent m_newChallengeResetEvent;

        public int howManyHoursUntilTurnin = 12 * 60 * 60;
        #region Web3InterfaceBase

        public override bool IsPool => false;
            public  int m_maxAnswersPerSubmit;
            
            public  float m_checkIntervalinSeconds;

            public int m_minSecondsPerAnswer;
            public decimal m_USDperToken;
        public bool runOnceThenResetAfter = false;

        public override event GetMiningParameterStatusEvent OnGetMiningParameterStatus;
        public override event NewChallengeEvent OnNewChallenge;
        public override event NewTargetEvent OnNewTarget;
        public override event NewDifficultyEvent OnNewDifficulty;
        public override event NewDifficultyEvent2 OnNewDifficulty2;
        public override event StopSolvingCurrentChallengeEvent OnStopSolvingCurrentChallenge;
        public override event GetTotalHashrateEvent OnGetTotalHashrate;
        public static byte[] HexStringToByteArray(string hexString)
        {
            hexString = hexString.Replace("0x", ""); // Remove "0x" if it's present
            int length = hexString.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }
        static List<byte[]> ReadFileIntoByteArrayList(string filePath)
        {
            List<byte[]> byteArrayList = new List<byte[]>();

            // Read the entire file content
            string fileContent = File.ReadAllText(filePath);

            // Split the content by comma
            string[] hexStrings = fileContent.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string hex in hexStrings)
            {
                byte[] byteArray = HexStringToByteArray(hex.Trim()); // Trim to remove any whitespace
                byteArrayList.Add(byteArray);
            }

            return byteArrayList;
        }

        private static List<string> ConvertByteArrayListToHex(List<byte[]> byteArrayList)
        {
            var hexStrings = new List<string>();
            foreach (var byteArray in byteArrayList)
            {
                StringBuilder hex = new StringBuilder("0x" + byteArray.Length * 2);
                var workplz = Utils.Numerics.Byte32ArrayToHexString(byteArray);
                hexStrings.Add(workplz.ToString());
            }
            return hexStrings;
        }
        private static List<string> ConvertBigIntegersToHex(List<BigInteger> bigIntegers)
        {
            var hexStrings = new List<string>();
            foreach (var bigInteger in bigIntegers)
            {
                // Convert to byte array
                byte[] byteArray = bigInteger.ToByteArray();

                // Ensure little-endian order (Ethereum's format)
                Array.Reverse(byteArray);

                // Convert to hexadecimal string with '0x' prefix
                string hexString = "0x" + BitConverter.ToString(byteArray).Replace("-", "");

                hexStrings.Add(hexString);
            }
            return hexStrings;
        }
            private static object[] ConvertToHex(object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] is BigInteger bigInt)
                {
                    data[i] = "0x" + bigInt.ToString("X");
                }
                // Add additional type checks and conversions as necessary
            }
            return data;
        }



        private static object[] ConvertData(object[] data)
        {
            object[] convertedData = new object[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] is BigInteger[] bigIntegers)
                {
                    convertedData[i] = ConvertBigIntegersToHex(bigIntegers);
                }
                else
                {
                    convertedData[i] = data[i]; // No conversion for non-BigInteger types
                }
            }

            return convertedData;
        }

        private static string[] ConvertBigIntegersToHex(BigInteger[] bigIntegers)
        {
            string[] hexStrings = new string[bigIntegers.Length];
            for (int i = 0; i < bigIntegers.Length; i++)
            {
                hexStrings[i] = "0x" + bigIntegers[i].ToString("X");
            }
            return hexStrings;
        }
        static int ReadCounterFromFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            return int.Parse(content);
        }

        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                    return false;
            }
            return true;
        }




        // Helper function to match Solidity's log2 implementation
        private static int FloorLog2(BigInteger x)
        {
            int n = 0;
            while (x > 1)
            {
                x >>= 1;  // Right shift (divide by 2, integer division)
                n++;
            }
            return n;
        }



        private BigInteger ConvertToBigInteger(byte[] originalBytes)
        {
            byte[] byteszzz = new byte[originalBytes.Length];
            Array.Copy(originalBytes, byteszzz, originalBytes.Length);

            // Reverse the byte order if the system uses little-endian order
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteszzz);
            }

            // Append a zero byte to ensure the number is interpreted as positive
            byte[] signedBytes = byteszzz.Concat(new byte[] { 0 }).ToArray();
            return new BigInteger(signedBytes);
        }
        public class CustomData
        {
            public PaymasterParams PaymasterParams { get; set; }
            public HexBigInteger GasPerPubdata { get; set; }
        }

        public class PaymasterParams
        {
            public string PaymasterAddress { get; set; }
            public string TokenAddress { get; set; }
            public HexBigInteger MinimalAllowance { get; set; }
            public byte[] InnerInput { get; set; }
        }


private static DateTime lastMiningRewardCheck = DateTime.MinValue;
private static readonly TimeSpan SixHours = TimeSpan.FromHours(6);

        private static int s_successfulSubmissionCount = 0;
        public bool firstRun = true;
        private  BigInteger saveEpochNumber2 = -1;
        private  BigInteger saveEpochNumber = 2017;
        private static DateTime s_lastSubmissionTime = DateTime.MinValue;
        public int first = 0;

        public override bool SubmitSolution(string address, byte[] digest, byte[] challenge, HexBigInteger difficulty, byte[] nonce, object sender)
        {
            lock (this)
            {  //this goes 1000+ lines
               //Turn off solver
               //  OnStopSolvingCurrentChallenge(this);
                BigInteger getTotalETHowedtoSendtoContract = 0;


                try
                {

                    var nonceBytes = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                    byte[] byteArray = Encoding.UTF8.GetBytes(nonceBytes);
                    bool areEqual = CompareByteArrays(digest, byteArray);


                    if (areEqual)
                    {
                        Program.Print("This is to check for close to end of mining period to submit MinBWORKperMint if nessessary");
                        Program.Print("This is to check for close to end of mining period to submit MinBWORKperMint if nessessary");
                        Program.Print("This is to check for close to end of mining period to submit MinBWORKperMint if nessessary");
                        //   var miningParameters = GetMiningParameters();
                        //    Console.WriteLine($"1Old challenge: {BitConverter.ToString(challenge)}");
                        //    challenge = miningParameters.ChallengeByte32;
                        //  Console.WriteLine($"1New challenge: {BitConverter.ToString(challenge)}");
                        //   Console.WriteLine("adjusted challenge to current to check");

                    }
                    if (m_account.Address == "0x1755BA5e18DBaFb375E5036150c59240Ed61FA98" || m_account.Address == "0x851c0428ee0be11f80d93205f6cB96adBBED22e6" || m_account.Address == "0xF2E00a6DbA02eD1115e5BEf3CcD7B5dD47141dDC")
                    {

                        Console.ForegroundColor = ConsoleColor.Black; // Set text color to blue
                        Console.BackgroundColor = ConsoleColor.DarkRed; // Set background color to a darker blue

                        Console.WriteLine(string.Format("[INFO] Please enter your personal Address and Private Key in the B0xToken.conf Config File, using exposed privateKey"));


                        Console.WriteLine(string.Format("[INFO] Please enter your personal Address and Private Key in B0xToken.conf Config File, using exposed privateKey"));
                        Program.Print(string.Format("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"));
                        Program.Print(string.Format("[INFO] Please enter your personal Address and Private Key in B0xToken.conf Config File, using exposed privateKey"));

                        Console.ResetColor(); // Reset to default colors
                    }



                    // Then in your main loop or wherever you want to check: check only once every 6 hours for miningReward
                    if (DateTime.Now - lastMiningRewardCheck >= SixHours)
                    {
                        var resultfz = m_getMiningReward.CallAsync<BigInteger>().Result;
                        Program.Print($"Mining Reward currently is: {Web3.Convert.FromWei(resultfz, UnitConversion.EthUnit.Ether)} tokens");
                        M_current_miningReward = (float)Web3.Convert.FromWei(resultfz, UnitConversion.EthUnit.Ether);
                        lastMiningRewardCheck = DateTime.Now;
                    }
                    else
                    {

                        Program.Print($"Mining Reward currently is: {M_current_miningReward} tokens");
                    }


                    int maxRetries = 3;
                    int currentRetry = 0;
                    int baseDelay = 400;


                    BlockInfoOutputDTO result3 = new BlockInfoOutputDTO
                    {
                        SlowBlockz = BigInteger.Zero,
                        SecondsUntilAdjustmentz = new BigInteger(1000000000),
                        BlocksFromReadjustz = new BigInteger(-1),
                        BlocksToReadjustz = new BigInteger(2017)
                    }; // Initialize with default values instead of null

                    var minimumDelay = TimeSpan.FromMilliseconds(10000);

                    var timeSinceLastSubmission = DateTime.Now - s_lastSubmissionTime;
                    s_successfulSubmissionCount++;
                    if (s_successfulSubmissionCount % 8 == 0 || timeSinceLastSubmission > minimumDelay)
                    {

                        s_lastSubmissionTime = DateTime.Now;


                        while (currentRetry < maxRetries)
                        {
                            try
                            {
                                result3 = m_getBlockInfo.CallDeserializingToObjectAsync<BlockInfoOutputDTO>().GetAwaiter().GetResult();
                                break; // Success - exit the loop
                            }
                            catch (Exception e)
                            {
                                currentRetry++;
                                Program.Print($"[Error[API]] in blockINFO (attempt {currentRetry}/{maxRetries}): {e.Message}");

                                if (currentRetry >= maxRetries)
                                {
                                    Program.Print("Max retries reached. Unable to get block info. Max retries reached. Unable to get block info.");
                                    break;
                                }

                                // Exponential backoff with some jitter
                                int delay = baseDelay * (int)Math.Pow(2, currentRetry - 1);
                                Task.Delay(delay).Wait();
                            }
                        }

                    }
                    else
                    {
                        Program.Print("WAITING TO GET NEW BLOCK INFO TOO MANY RPC CALLS! 10 sec minimum between calls everything should work still");

                    }
                    BigInteger slowBlocksz = result3.SlowBlockz;     // First return value
                    BigInteger secondsFromTimetoTurnInAnswers = result3.SecondsUntilAdjustmentz;      // Second return value  
                    BigInteger blocksFromReadjustmentz = result3.BlocksFromReadjustz; // Third return value   
                    BigInteger blocksToReadjustmentz = result3.BlocksToReadjustz;      // Forth return value  

                    Program.Print("slowBlocksz " + slowBlocksz);
                    Program.Print("secondsFromTimetoTurnInAnswers " + secondsFromTimetoTurnInAnswers);
                    //     Program.Print("blocksFromReadjustmentz " + blocksFromReadjustmentz);
                    //   Program.Print("blocksToReadjustmentz " + blocksToReadjustmentz);



                    var ShouldweTurnInAnswersNow = secondsFromTimetoTurnInAnswers < howManyHoursUntilTurnin;
                    Program.Print("Should we turn in Answers because we are close to Emergency Difficulty Adjustment: " + ShouldweTurnInAnswersNow);
                    //if (challenge.SequenceEqual(CurrentChallenge))

                    //var miningParameters5555 = GetMiningParameters5();
                    var epochNumber5555 = result3.BlocksToReadjustz;
                    if (epochNumber5555 == 2017)
                    {
                        Program.Print("2017 Mint go to saved Epoch Please");

                        epochNumber5555 = saveEpochNumber;
                    }
                    else
                    {
                        Program.Print("NOT 2017 Mint go save Epoch Please");

                        saveEpochNumber = epochNumber5555;
                    }

                    var epochNumber5555112121212121 = result3.BlocksFromReadjustz;
                    if (epochNumber5555112121212121 == -1)
                    {
                        Program.Print("-1 EpochNumber go to saved Epoch2 Please");

                        epochNumber5555112121212121 = saveEpochNumber2;
                    }
                    else
                    {
                        Program.Print("NOT -1 EpochNumber go save Epoch2 Please");

                        saveEpochNumber2 = epochNumber5555112121212121;
                    }
                    // MiningDifficultyfff = new HexBigInteger(m_getSecondsUntilAdjustment.CallAsync<BigInteger>().Result);


                    blocksFromReadjustmentz = epochNumber5555112121212121;


                    //    var blocksFromReadjustmentz = new HexBigInteger(m_blocksFromReadjust.CallAsync<BigInteger>().Result).Value;
                    //   var blocksToReadjustmentz = epochNumber5555;
                    Program.Print("Blocks From readjustment: " + blocksFromReadjustmentz.ToString());
                    Program.Print("Blocks to readjustment: " + epochNumber5555.ToString());
                    Program.Print("Mining seconds until probably time to turn in answers: " + secondsFromTimetoTurnInAnswers.ToString());





                    // Specify the file path
                    string originalChallengeStringzf = BitConverter.ToString(challenge).Replace("-", "");
                    string directoryPathzfzfdzf = Path.Combine("solveData-", originalChallengeStringzf);
                    Directory.CreateDirectory(directoryPathzfzfdzf);

                    string filePathz = "counter.txt";
                    string filePathzOLD = "counterCounterOLD.txt";
                    string filePathzLocationStart = "counterLocationStart.txt";
                    string filePathzLocationEnd = "counterLocationEnd.txt";
                    string filePathzLocationEndzOLD = "counterLocationEndOLD.txt";

                                            string MaxSolvesPerMint = Path.Combine(directoryPathzfzfdzf, "maxSolves");
                                            string MinSolvesPerMint = Path.Combine(directoryPathzfzfdzf, "minSolves");

                    // Construct the file path
                    string filePath2f = Path.Combine(directoryPathzfzfdzf, filePathzLocationStart);
                    // Construct the file path
                    string filePath3 = Path.Combine(directoryPathzfzfdzf, filePathzLocationEnd);
                    // Construct the file path
                    string filePath4 = Path.Combine(directoryPathzfzfdzf, filePathz);
                    string filePath3OLDOLD = Path.Combine(directoryPathzfzfdzf, filePathzOLD);
                    string filePath4OLDOLD = Path.Combine(directoryPathzfzfdzf, filePathzLocationEndzOLD);

                    // Read the current counter from the file or start at 1 if the file doesn't exist
                    int currentCounter = File.Exists(filePath4) ? ReadCounterFromFile(filePath4) : 0;
                    int     currentCounterLocationStart = File.Exists(filePath2f) ? ReadCounterFromFile(filePath2f) : 0;
                    int currentCounterLocationEnd = File.Exists(filePath3) ? ReadCounterFromFile(filePath3) : 0;
                    oldCurrentCounter = File.Exists(filePath3OLDOLD) ? ReadCounterFromFile(filePath3OLDOLD) : 0;
                    oldcurrentCounterLocatioNEnd = File.Exists(filePath4OLDOLD) ? ReadCounterFromFile(filePath4OLDOLD) : 0;


                                         string firstSubmitRunDir2 = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");
                                string firstSubmitRunString2 = File.Exists(firstSubmitRunDir2) && int.TryParse(File.ReadAllText(firstSubmitRunDir2), out int value)
                                    ? value.ToString()
                                    : "0";

                    if (firstSubmitRunString2 == "1")
                    {


                        m_MaxSolvesperMint = File.Exists(MaxSolvesPerMint) ? ReadCounterFromFile(MaxSolvesPerMint) : m_MaxSolvesperMint;
                        m_MinSolvesperMint = File.Exists(MinSolvesPerMint) ? ReadCounterFromFile(MinSolvesPerMint) : m_MinSolvesperMint;
                    }
                    Program.Print($"Current counter Starts at:  {currentCounter}");
                    Program.Print($"Current currentCounterLocationStart Starts at:  {currentCounterLocationStart}");
                    Program.Print($"Current currentCounterLocationEnd Starts at:  {currentCounterLocationEnd}");
                    Program.Print($"OLD oldCurrentCounter Starts at:  {oldCurrentCounter}");
                    Program.Print($"OLD oldcurrentCounterLocatioNEnd Starts at:  {oldcurrentCounterLocatioNEnd}");
                    // Display the current 
                    Program.Print($"EPOCH UNTIL READJUSTMENT: {epochNumber5555}");
                    var isCloseToReadjustment = false;

                    m_ResetIfEpochGoesUp = (int)epochNumber5555;
                    if (currentCounter - currentCounterLocationStart == 0 && m_ResetIfEpochGoesUpBOOL && digestArray2.Length != 0)
                    {

                        //var miningParameters5555zzz = GetMiningParameters();
                        epochNumber55552 = CurrentTarget;

                        try
                        {

                            byte[] challengeCopy = new byte[challenge.Length];
                            Array.Copy(challenge, challengeCopy, challenge.Length);

                            string challengeHex = "0x" + BitConverter.ToString(challengeCopy).Replace("-", "");
                            // Program.Print("THIRD function copying challenge Array First Old: " + challengeHex);
                            challengeArrayFirstOld = challengeCopy;

                            m_getChallengeNumber = m_getChallengeNumber2;
                            m_getMiningTarget = m_getMiningTarget2;
                            m_getMiningDifficulty = m_getMiningDifficulty2;
                            // var miningParametersssf = GetMiningParameters();
                            //  var newDifficultymaybe2 = miningParametersssf.MiningDifficulty.Value;
                            //    Console.WriteLine($"sdfsdfsdfsdfdsCurrent newDifficultymaybe2: {newDifficultymaybe2}");
                            //  var newDifficultymaybe2z = miningParametersssf.MiningTarget.Value;
                            //    Console.WriteLine($"sdfsdfsdfsdfdsCurrent newDifficultymaybe2: {newDifficultymaybe2}");
                            //  var MiningTargetByte32 = Utils.Numerics.FilterByte32Array(newDifficultymaybe2z.ToByteArray(isUnsigned: true, isBigEndian: true));
                            //   byte[] bytes = MiningTargetByte32;
                            //    Console.WriteLine($"sdfsdfsdfsdfdsCurrent bytes: {bytes}");

                            //  var MiningTargetByte32String = Utils.Numerics.Byte32ArrayToHexString(bytes);
                            //     Console.WriteLine($"sdfsdfsdfsdfdsCurrent MiningTargetByte32String: {MiningTargetByte32String}");
                            //   HexBigInteger testsss = new HexBigInteger(MiningTargetByte32String);
                            //  this.OnNewDifficulty?.Invoke(this, miningParametersssf.MiningDifficulty2);
                            //    this.UpdateMinerTimer_Elapsed(this, null);
                            // this.OnNewTarget(this, testsss);
                            //    var miningParametersssffffff = GetMiningParameters5();

                            //  var miningParametersssffffffzzzz = GetMiningParameters();
                            //  if (!challenge.SequenceEqual(miningParametersssffffffzzzz.ChallengeByte32))
                            //  {
                            //   this.OnNewDifficulty?.Invoke(this, miningParametersssf.MiningDifficulty);
                            //  this.UpdateMinerTimer_Elapsed(this, null);
                            //   this.OnNewTarget(this, miningParametersssf.MiningTarget);
                            //    OnNewChallenge(this, miningParametersssffffffzzzz.ChallengeByte32, MinerAddress);
                            //      return false;
                            //  }

                        }
                        catch (Exception ex)
                        {
                            // Catch and handle the exception
                            Console.WriteLine("An error occurred: " + ex.Message);
                        }

                    }
                    //Console.WriteLine("LOG123123123123123123123");
                    if (digestArray2.Length == 0)
                    {
                        epochNumber55552 = CurrentTarget;

                        byte[] challengeCopy3 = new byte[challenge.Length];
                        Array.Copy(challenge, challengeCopy3, challenge.Length);
                        if (currentCounter - currentCounterLocationStart < (int)epochNumber5555)
                        {
                            string challengeHex = "0x" + BitConverter.ToString(challengeCopy3).Replace("-", "");
                            // Program.Print("FORTH function copying challenge Array First Old: " + challengeHex);
                            challengeArrayFirstOld = challengeCopy3;
                        }
                        string nonceHEx = "0x" + BitConverter.ToString(nonce).Replace("-", "");
                        //Program.Print("writing Nonce forth: " + nonceHEx);


                        if (!areEqual)
                        {


                            byte[] challengeCopy5 = new byte[challenge.Length];
                            Array.Copy(challenge, challengeCopy5, challenge.Length);
                            byte[] digestCopy5 = new byte[digest.Length];
                            Array.Copy(digest, digestCopy5, digest.Length);
                            byte[] nonceCopy5 = new byte[nonce.Length];
                            Array.Copy(nonce, nonceCopy5, nonce.Length);


                            digestArray2 = new byte[][] { digestCopy5 };

                            string digHex = "0x" + BitConverter.ToString(digestCopy5).Replace("-", "");
                            string digHex2222 = "0x" + BitConverter.ToString(digest).Replace("-", "");
                            //Program.Print("writing digHex digHex123123: " + digHex);
                            //Program.Print("writing DIGEST ORIGINAL 12: " + digHex2222);
                            //Program.Print("writing DIGEST ORIGINAL 12: " + digHex2222);
                            //Program.Print("writing digHex digHex123123: " + digHex);
                            challengeArray2 = new byte[][] { challengeCopy5 };
                            nonceArray2 = new byte[][] { nonceCopy5 };
                        }
                        else
                        {

                            digestArray2 = new byte[][] { };
                            challengeArray2 = new byte[][] { };
                            nonceArray2 = new byte[][] { };
                        }

                        Console.WriteLine("LOG444444444444444444444");
                        int fx = 0;
                        for (fx = currentCounterLocationStart; fx < currentCounter; fx++)
                        {
                            try
                            {





                                string originalChallengeStringz = BitConverter.ToString(challenge).Replace("-", "");
                                string directoryPath = Path.Combine("solveData-", originalChallengeStringz);
                                Directory.CreateDirectory(directoryPath);



                                // Construct the file path
                                string filePath2 = Path.Combine(directoryPath, $"data_set_{fx + 1}.txt");
                                // Read the strings back from the file
                                string fileContents2 = File.ReadAllText(filePath2);
                                //    Console.WriteLine(fileContents2);
                                // Parse the content and assign to variables
                                string[] lines2 = fileContents2.Split('\n');
                                string originalDigestFromFile2 = lines2[0].Split(':')[1].Trim();
                                string originalChallengeFromFile2 = lines2[1].Split(':')[1].Trim();
                                string originalNonceFromFile2 = lines2[2].Split(':')[1].Trim();

                                // Now you can use the variables as needed
                                //    Console.WriteLine($"Set {fx + 1} - Original digest from file: {originalDigestFromFile2}");
                                //    Console.WriteLine($"Set {fx + 1} - Original challenge from file: {originalChallengeFromFile2}");
                                Console.WriteLine($"Set {fx + 1} - Original nonce from file: {originalNonceFromFile2}");



                                var originalDigestBytes2 = HexStringToByteArray(originalDigestFromFile2);
                                var originalChallengeBytes2 = HexStringToByteArray(originalChallengeFromFile2);
                                var originalNonceBytes2 = HexStringToByteArray(originalNonceFromFile2);

                                // Now you have the byte arrays
                                //   Console.WriteLine($"Original Digest as bytes32: {BitConverter.ToString(originalDigestBytes2)}");
                                //   Console.WriteLine($"Original Challenge as bytes32: {BitConverter.ToString(originalChallengeBytes2)}");
                                //   Console.WriteLine($"Original Nonce as bytes32: {BitConverter.ToString(originalNonceBytes2)}");
                                digestArray2 = digestArray2.Concat(new byte[][] { originalDigestBytes2 }).ToArray(); ;
                                challengeArray2 = challengeArray2.Concat(new byte[][] { originalChallengeBytes2 }).ToArray();
                                nonceArray2 = nonceArray2.Concat(new byte[][] { originalNonceBytes2 }).ToArray();


                            }
                            catch
                            {
                                Console.WriteLine("[New Challenge] Probably a new challenge we havent seen, not using old asnwers!");
                                currentCounter = 0;
                                break;

                            }

                        }


                        Task.Delay(100).Wait();
                    }
                    else if (m_ResetIfEpochGoesUpBOOL && !areEqual)
                    {
                        byte[] challengeCopy5 = new byte[challenge.Length];
                        Array.Copy(challenge, challengeCopy5, challenge.Length);
                        byte[] digestCopy5 = new byte[digest.Length];
                        Array.Copy(digest, digestCopy5, digest.Length);
                        byte[] nonceCopy5 = new byte[nonce.Length];
                        Array.Copy(nonce, nonceCopy5, nonce.Length);


                        string digestREAL = "0x" + BitConverter.ToString(digest).Replace("-", "");
                        string digHex = "0x" + BitConverter.ToString(digestCopy5).Replace("-", "");

                        digestArray2 = digestArray2.Concat(new byte[][] { digestCopy5 }).ToArray();
                        challengeArray2 = challengeArray2.Concat(new byte[][] { challengeCopy5 }).ToArray();
                        nonceArray2 = nonceArray2.Concat(new byte[][] { nonceCopy5 }).ToArray();

                    }

                    // Console.WriteLine("LOG55555555");
                    if (!areEqual)
                    {

                        currentCounter = currentCounter + 1;
                    }


                    // Console.WriteLine($"LENTGTHS {(nonceArray2.Length)}");
                    byte[] originalDigestBytes = null;
                    byte[] originalChallengeBytes = null;
                    byte[] originalNonceBytes = null;
                    for (int i = 0; i < currentCounter - currentCounterLocationStart; i++)
                    {
                        // Example: Accessing elements in the jagged arrays
                        byte[] digestz = digestArray2[i];
                        byte[] challengez = challengeArray2[i];
                        byte[] noncez = nonceArray2[i];

                        // Print out the contents of the arrays
                        //   Console.WriteLine($"Set {i + 1} - Original digest: " + BitConverter.ToString(digestz));
                        //  Console.WriteLine($"Set {i + 1} - Original challenge: " + BitConverter.ToString(challengez));
                        //   Console.WriteLine($"Set {i + 1} - Original nonce: " + BitConverter.ToString(noncez));

                        string originalDigestStringz = BitConverter.ToString(digestz).Replace("-", "");
                        string originalChallengeStringz = BitConverter.ToString(challengez).Replace("-", "");
                        string originalNonceStringz = BitConverter.ToString(noncez).Replace("-", "");
                        string directoryPath = Path.Combine("solveData-", originalChallengeStringz);
                        Directory.CreateDirectory(directoryPath);

                        // Construct the file path
                        string filePath2 = Path.Combine(directoryPath, $"data_set_{i + currentCounterLocationStart + 1}.txt");

                        // Store the strings in a file
                        File.WriteAllText(filePath2,
                            $"Set {i + 1} - Original digest: {originalDigestStringz}\n" +
                            $"Set {i + 1} - Original challenge: {originalChallengeStringz}\n" +
                            $"Set {i + 1} - Original nonce: {originalNonceStringz}");

                        // Read the strings back from the file
                        string fileContents2 = File.ReadAllText(filePath2);
                        //Console.WriteLine(fileContents2);
                        // Parse the content and assign to variables
                        string[] lines2 = fileContents2.Split('\n');
                        // string originalDigestFromFile2 = lines2[0].Split(':')[1].Trim();
                        // string originalChallengeFromFile2 = lines2[1].Split(':')[1].Trim();
                        string originalNonceFromFile2 = lines2[2].Split(':')[1].Trim();

                        // Now you can use the variables as needed
                        //  Console.WriteLine($"Set {i + 1} - Original digest from file: {originalDigestFromFile2}");
                        //Console.WriteLine($"Set {i + 1} - Original challenge from file: {originalChallengeFromFile2}");
                        //    Console.WriteLine($"Set {i + 1} - Original nonce from file: {originalNonceFromFile2}");



                        //  originalDigestBytes = HexStringToByteArray(originalDigestFromFile2);
                        //  originalChallengeBytes = HexStringToByteArray(originalChallengeFromFile2);
                        //   originalNonceBytes = HexStringToByteArray(originalNonceFromFile2);

                        // Now you have the byte arrays
                        //  Console.WriteLine($"Original Digest as bytes32: {BitConverter.ToString(originalDigestBytes)}");
                        //      Console.WriteLine($"Original Challenge as bytes32: {BitConverter.ToString(originalChallengeBytes)}");
                        //  Console.WriteLine($"Original Nonce as bytes32: {BitConverter.ToString(originalNonceBytes)}");
                    }


                    File.WriteAllText(filePath4, currentCounter.ToString());
                    // Write the updated counter back to the file
                    // Display the updated counter
                    Console.WriteLine($"Updated Total Number of Mints accumulated: {currentCounter - currentCounterLocationStart}");
                    Console.WriteLine($"Updated currentCounter accumulated: {currentCounter}");
                    Console.WriteLine($"Updated currentCounterLocationStart accumulated: {currentCounterLocationStart}");



                    var difzzzzz = currentCounter - currentCounterLocationStart;
                    if (skipThisMany + difzzzzz > currentCounter)
                    {
                        Console.WriteLine("Fixing error Code[01.0.2.4]");
                        Program.Print(string.Format("Fixing error Code[01.0.2.4]"));
                        skipThisMany = skipThisMany - 1;

                    }





                    //  var miningParameters2f = GetMiningParameters2();
                    if (currentCounter - currentCounterLocationStart == 0)
                    {
                        Program.Print(string.Format("Waiting for next solution"));
                        // OnNewChallenge(this, miningParameters2f.ChallengeByte32, MinerAddress);
                        return false;
                    }

                    // if((currentCounter-currentCounterLocationStart) > m_maxAnswersPerSubmit)
                    var thiszzzzz = 0;
                    m_ResetIfEpochGoesUpBOOL = true;




                    BigInteger[] lastNonceArray = new BigInteger[] { new BigInteger(nonceArray2[nonceArray2.Length - 1], isBigEndian: true) };
                    BigInteger[] lastNonceArray2 = nonceArray2.Select(bytes => new BigInteger(bytes.Reverse().ToArray())).ToArray();

                    byte[][] lastDigestArray = new byte[][] { digestArray2[digestArray2.Length - 1] };
                    byte[][] lastChallengeArray = new byte[][] { challengeArray2[challengeArray2.Length - 1] };

                    byte[][] lastDigestArray2 = digestArray2.Select(array => array.ToArray()).ToArray();

                    // Copy all elements from challengeArray2
                    //BigInteger[] lastNonceArray2x = nonceArray2.Take(currentCounter).Select(bytes => new BigInteger(bytes.Reverse().ToArray())).ToArray();

                    List<BigInteger> lastNonceArray2x = nonceArray2
                         .Skip(skipThisMany)
                         .Take(currentCounter - currentCounterLocationStart)
                         .Select(bytes => new BigInteger(bytes.Reverse().Concat(new byte[] { 0x00 }).ToArray()))
                         .ToList();

                    byte[][] lastChallengeArray2 = challengeArray2.Select(array => array.ToArray()).ToArray();
                    // byte[][] lastChallengeArray2x = challengeArray2.Take(currentCounter).Select(array => array.ToArray()).ToArray();
                    List<byte[]> lastChallengeArray2x = challengeArray2
                        .Skip(skipThisMany)
                        .Take(currentCounter - currentCounterLocationStart)
                        .Select(array => array.ToArray())
                        .ToList();

                    //byte[][] lastDigestArray2x = digestArray2.Take(currentCounter).Select(array => array.ToArray()).ToArray();
                    List<byte[]> lastDigestArray2x = digestArray2
                        .Skip(skipThisMany)
                        .Take(currentCounter - currentCounterLocationStart)

                        .Select(array => array.ToArray())
                        .ToList();

                    // Output each byte array in the list

                    // Take the first 5 elements from challengeArray2
                    // dataInputMega = new object[] { lastNonceArray2x[0], address };
                    HashSet<int> indicesToRemove = new HashSet<int>();


                    int totalMultiple = 0;
                    if (true)
                    {

                        // Console.WriteLine($"TESTING LOOP TESTING LOOP LENGTH OF ARRAY : {lastNonceArray2x.Count}");
                        // Console.WriteLine($"TESTING LOOP needToCheck after this many epochs : {epochNumber5555}");

                        var removed = 0;
                        //  Console.WriteLine($"CURRENT MININNG DIFFICULTY: {epochNumber55552}");

                        var xfsdfsdf = 0;
                        /*
                        foreach (byte[] item in lastChallengeArray2)
                        {


                            xfsdfsdf = xfsdfsdf + 1;
                            string byteArrayString = BitConverter.ToString(item).Replace("-", "");
                            Program.Print("THE Challenge is : " + xfsdfsdf + " chal: " + byteArrayString);
                        }
                        */
                        // Create a list to store all results
                        var resultsComp = new List<(BigInteger abe23f, int compensation)>();

                        var indexid = 0;
                        for (int xas = 0; xas < (int)epochNumber5555;)
                        {
                            if (indexid >= currentCounter - currentCounterLocationStart) { break; }
                            if (xas >= currentCounter - currentCounterLocationStart) { break; }

                            BigInteger testd = BigInteger.Pow(2, 234);

                            BigInteger digestAsBigInteger = ConvertToBigInteger(lastDigestArray2x[indexid]);

                            // Convert HexBigInteger to BigInteger
                            BigInteger difficultyAsBigIntegerLargeNumber = epochNumber55552;


                            // Compare the digest with the difficulty
                            int comparisonResult = BigInteger.Compare(digestAsBigInteger, difficultyAsBigIntegerLargeNumber);




                            BigInteger abe23f = difficultyAsBigIntegerLargeNumber / digestAsBigInteger;

                            string ChallengeString = BitConverter.ToString(lastChallengeArray2x[indexid]).Replace("-", "");
                            //Program.Print("ChallengeString GO: "+ChallengeString);
                            int compensation = 0;
                            if (abe23f < 4 && comparisonResult < 0)
                            {
                                compensation = 1;
                            }
                            else if (comparisonResult < 0)
                            {
                                // Match Solidity exactly: integer division then floor(log2)
                                BigInteger halfValue = abe23f / 2;  // Integer division like Solidity
                                compensation = FloorLog2(halfValue) + 1;
                            }




                            resultsComp.Add((abe23f, compensation));

                            if (new BigInteger(totalMultiple + m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC) >= (epochNumber5555) && totalMultiple >= m_MinSolvesperMint)
                            {
                                if (!runOnceThenResetAfter)
                                {
                                    runOnceThenResetAfter = true;
                                    currentCounterLocationEnd = xas + currentCounterLocationStart + 1;

                                    //  Program.Print($"totalMultiple shorty because we are close to block. and  currentCounterLocationEnd: {currentCounterLocationEnd}");
                                    File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                                }

                            }
                            if (totalMultiple >= m_maxAnswersPerSubmit || totalMultiple >= m_MaxSolvesperMint)
                            {
                                if (!runOnceThenResetAfter)
                                {
                                    runOnceThenResetAfter = true;
                                    currentCounterLocationEnd = xas + currentCounterLocationStart + 1;

                                    //  Program.Print($"totalMultiple is greater than needed! currentCounterLocationEnd: {currentCounterLocationEnd}");
                                    File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                                }
                            }
                            // If the digest is greater than or equal to the difficulty, the solution is valid
                            if (comparisonResult >= 0)
                            {

                                indicesToRemove.Add(indexid);
                            }
                            else
                            {
                                if (!lastChallengeArray2x[indexid].SequenceEqual(challengeArrayFirstOld))
                                {
                                    // Program.Print($"REMOVED THIS digestAsBigInteger: {digestAsBigInteger}");

                                    //  indicesToRemove.Add(indexid);

                                }
                                else
                                {
                                    xas++;
                                }

                            }
                            indexid = indexid + 1;

                        }


                        // Now print everything organized
                        // Program.Print("=== MINT RESULTS ===");
                        totalMultiple = 0;

                        for (int i = 0; i < resultsComp.Count; i++)
                        {
                            var (abe23f, compensation) = resultsComp[i];
                            totalMultiple += compensation;

                            // Program.Print($"Mint {i+1}: abe23f={abe23f} | Compensation={compensation}");
                        }


                    }






                    Program.Print("prevEpcohToreadjust: " + prevEpcohToreadjust);
                    Program.Print("epochNumber5555: " + epochNumber5555);

                    // In your method:
                    var timeBetweenAnswers = DateTime.Now - lastExecutionTime;


                    float test2222 = (float)((double)BigInteger.Pow(2, 234) / (double)m_lastParameters.MiningTarget.Value);



                    Program.Print("[INFZ] DIFFICULTY NORMALIZED : " + test2222);
                    var tststs = TimeSpan.FromSeconds(120);
                    if (test2222 < 5)
                    {
                        tststs = TimeSpan.FromSeconds(10);
                    }
                    else if (test2222 < 100)
                    {
                        tststs = TimeSpan.FromSeconds(20);
                    }
                    else if (test2222 < 500)
                    {
                        tststs = TimeSpan.FromSeconds(25);
                    }
                    else if (test2222 < 1000)
                    {
                        tststs = TimeSpan.FromSeconds(50);
                    }
                    else if (test2222 < 5000)
                    {
                        tststs = TimeSpan.FromSeconds(70);
                    }
                    if ((timeBetweenAnswers > tststs || prevEpcohToreadjust < epochNumber5555) && epochNumber5555 != 2017)
                    {
                        //if we just use the difficulty we can use different TimeSpans for each to fix it for low difficulty.
                        Program.Print("RUNNING UPDATE NOW FOR UPDATE");

                        Program.Print("RUNNING UPDATE NOW FOR UPDATE");

                        Program.Print("RUNNING UPDATE NOW FOR UPDATE");

                        Program.Print("RUNNING UPDATE NOW FOR UPDATE");


                        lastExecutionTime = DateTime.Now;
                        Task.Delay(300).Wait();
                        var save = m_lastParameters.Challenge.Value;
                        CheckAndUpdateMiningParameters();

                        var save2 = m_lastParameters.Challenge.Value;



                        if (save != save2)
                        {
                            Program.Print(string.Format("[INFO] Submission cancelled, someone has solved this challenge. Try lowering MinBWORKperMint variable to submit before them."));
                            Task.Delay(300).Wait();
                            // UpdateMinerTimer_Elapsed(this, null);
                            //CheckAndUpdateMiningParameters();

                            //  OnNewChallenge(this, miningParameters2.ChallengeByte32, MinerAddress);

                            prevEpcohToreadjust = 2017;
                            return false;

                        }



                    }

                    //  CheckAndUpdateMiningParameters();

                    // Task.Delay(1000).Wait();
                    prevEpcohToreadjust = (int)epochNumber5555;







                    if (totalMultiple > m_maxAnswersPerSubmit)
                    {
                        Program.Print(string.Format("YOU WILL MINT a max of m_maxAnswersPerSubmit: " + m_maxAnswersPerSubmit + " mints instead of the total: " + totalMultiple));

                    }
                    else
                    {
                        Program.Print(string.Format("YOU WILL MINT THIS MANY IF YOU MINT NOW: " + totalMultiple + "x"));


                    }



                    if (new BigInteger(totalMultiple + m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC) <= (epochNumber5555))
                    {
                        thiszzzzz = (int)(m_MaxSolvesperMint - totalMultiple);
                    }
                    else
                    {
                        thiszzzzz = (int)(m_MinSolvesperMint - totalMultiple);
                    }

                    if (totalMultiple < m_maxAnswersPerSubmit && totalMultiple < m_MaxSolvesperMint && new BigInteger(totalMultiple + m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC) <= epochNumber5555&& !ShouldweTurnInAnswersNow && epochNumber5555 >= totalMultiple + 1)
                    {
                        Program.Print(string.Format("STILL SOLVING, Total Good Solves count: " + (currentCounter - currentCounterLocationStart)));
                        Program.Print(string.Format("STILL SOLVING, Solves til transaction sending: " + thiszzzzz));

                        Program.Print(string.Format("Waiting for next solution"));
                        /////////    CheckAndUpdateMiningParameters();


                        //  OnNewChallenge(this, miningParameters2f.ChallengeByte32, MinerAddress);
                        return false;
                    }
                    if (totalMultiple < m_maxAnswersPerSubmit && totalMultiple < m_MinSolvesperMint && epochNumber5555 >= totalMultiple + 1)
                    {
                        Program.Print(string.Format("STILL SOLVING, Total Good Solves count: " + (currentCounter - currentCounterLocationStart)));
                        Program.Print(string.Format("STILL SOLVING, Solves til transaction sending: " + thiszzzzz));

                        Program.Print(string.Format("Waiting for next solution"));
                        //  CheckAndUpdateMiningParameters();

                        // OnNewChallenge(this, miningParameters2f.ChallengeByte32, MinerAddress);
                        return false;
                    }

                    List<byte[]> lastDigestArray2xz = lastDigestArray2x
                        .Where((_, index) => !indicesToRemove.Contains(index))
                        .Take(currentCounter - currentCounterLocationStart - indicesToRemove.Count)
                        .ToList();
                    List<byte[]> lastChallengeArray2xz = lastChallengeArray2x
                        .Where((_, index) => !indicesToRemove.Contains(index))
                        .Take(currentCounter - currentCounterLocationStart - indicesToRemove.Count)
                        .ToList();
                    List<BigInteger> filteredLastNonceArray2x = lastNonceArray2x
                        .Where((_, index) => !indicesToRemove.Contains(index))
                        .Take(currentCounter - currentCounterLocationStart - indicesToRemove.Count)
                        .ToList();


                    File.WriteAllText(filePath4, currentCounter.ToString());
                    // Write the updated counter back to the file
                    // Display the updated counter
                    // Console.WriteLine($"2Updated Counter: {currentCounter}");

                    digestArray2 = lastDigestArray2xz.ToArray();
                    //  Program.Print(string.Format("Check these nonces"));

                    challengeArray2 = lastChallengeArray2xz.ToArray();
                    nonceArray2 = filteredLastNonceArray2x.Select(bi =>
                    {
                        byte[] byteArray = bi.ToByteArray();
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(byteArray);
                        }

                        return byteArray;
                    }).ToArray();
                    var miniz222 = Math.Min(m_MaxSolvesperMint, _BLOCKS_PER_READJUSTMENT_ + (int)epochNumber5555);

                    if (totalMultiple < m_maxAnswersPerSubmit && totalMultiple < miniz222 && totalMultiple < m_MaxSolvesperMint && new BigInteger(totalMultiple + m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC) <= (epochNumber5555) && !ShouldweTurnInAnswersNow && epochNumber5555 >= totalMultiple + 1)
                    {
                        Program.Print(string.Format("STILL SOLVING Total Good Solves count: " + filteredLastNonceArray2x.Count));
                        Program.Print(string.Format("STILL SOLVING Solves til mint: " + (miniz222 - filteredLastNonceArray2x.Count)));

                        Program.Print(string.Format("Waiting for next solution"));

                        //    var miningParameters2ff = GetMiningParameters2();
                        //OnNewChallenge(this, miningParameters2ff.ChallengeByte32, MinerAddress);
                        return false;
                    }

                    if (totalMultiple < m_maxAnswersPerSubmit && totalMultiple < miniz222 && totalMultiple < m_MinSolvesperMint && epochNumber5555 >= totalMultiple + 1)
                    {
                        Program.Print(string.Format("STILL SOLVING Total Good Solves count: " + filteredLastNonceArray2x.Count));
                        Program.Print(string.Format("STILL SOLVING Solves til mint: " + (miniz222 - filteredLastNonceArray2x.Count)));

                        Program.Print(string.Format("Waiting for next solution"));

                        return false;
                    }

                    if (ShouldweTurnInAnswersNow)
                    {
                        if (new BigInteger(currentCounter - currentCounterLocationStart + m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC) <= epochNumber5555)
                        {
                            if (totalMultiple <= m_MaxSolvesperMint / m_MinSolvesperMint)
                            {
                                howManyHoursUntilTurnin = howManyHoursUntilTurnin / 2;
                            }
                        }
                    }


                    if (totalMultiple >= m_maxAnswersPerSubmit)
                    {

                        totalMultiple = m_maxAnswersPerSubmit;
                    }



                    if (totalMultiple >= epochNumber5555)
                    {

                        totalMultiple = (int)epochNumber5555;
                    }


                    var mintNFT = false;
                    Program.Print("m_getCostsALL" + m_getCostsALL);
                    if (m_getCostsALL != null)
                    {
                        try
                        {

                            BigInteger getTotalB0x = 0;        // First return value
                            BigInteger getTotalETH = 0;      // Second return value  
                            BigInteger getTotalETHowed = 0;  // Add 10% for safety
                            BigInteger EthPriceUSDC = 0;      // Forth return value  
                            BigInteger secFromPrevious = 0;  // Fifth return value


                            int maxRetries2 = 5;
                            int currentRetry2 = 0;
                            int baseDelay2 = 400;
                            TotalCostsOutputDTO result = null;
                            var compensationValue = totalMultiple;
                            while (currentRetry2 < maxRetries2)
                            {
                                try
                                {
                                    result = m_getCostsALL.CallDeserializingToObjectAsync<TotalCostsOutputDTO>(compensationValue).GetAwaiter().GetResult();
                                    break;
                                }
                                catch (Exception e)
                                {
                                    currentRetry2++;
                                    Program.Print($"[Error[API]] in m_getCostsALL aka TotalTotalsAndETHpriceAndCurrentMintTime (attempt {currentRetry2}/{maxRetries2}): {e.Message}");

                                    if (currentRetry2 >= maxRetries2)
                                    {
                                        Program.Print("Max retries reached. Unable to get block info.");
                                        break; // Re-throw if you want to handle this upstream
                                    }

                                    // Exponential backoff with some jitter
                                    int delay = baseDelay2 * (int)Math.Pow(2, currentRetry2 - 1);

                                    Task.Delay(delay).Wait();
                                }
                            }
                            // ACCESS EACH VALUE using the property names from your DTO:
                            getTotalB0x = result.B0xYouGet;        // First return value
                            getTotalETH = result.ETHYouGet;      // Second return value  
                            getTotalETHowed = result.ETHyouSpend * 110 / 100;  // Add 10% for safety
                            EthPriceUSDC = result.ETHPrice;      // Forth return value  
                            secFromPrevious = result.SecondsFromPreviousMintReturn;  // Fifth return value


                            if (secFromPrevious < 60)
                            {
                                getTotalETHowed = result.ETHyouSpend * 135 / 100;  // Add 10% for safety
                            }
                            else if (secFromPrevious < 120)
                            {
                                getTotalETHowed = result.ETHyouSpend * 125 / 100;  // Add 10% for safety
                            }
                            var EthPriceUSDCReadable = EthPriceUSDC / BigInteger.Pow(10, 10);
                            var ETHUSDCPriceDecimal = (decimal)EthPriceUSDCReadable / (decimal)BigInteger.Pow(10, 2);
                            Program.Print("===Single Mint Cost Analysis ===");
                            Program.Print($"===ETH USDC cost {ETHUSDCPriceDecimal} ===");
                            Program.Print($"For each Mint you will spend {secFromPrevious / compensationValue} seconds per Solve");
                            Program.Print($"For each mint of the {compensationValue} mints:");
                            Program.Print($"===TOTALS FOR {compensationValue} MINTS!======TOTALS FOR {compensationValue} MINTS!===");
                            Program.Print($"For {compensationValue} Mints you will spend a total of {secFromPrevious / compensationValue} seconds");
                            Program.Print($"B0x You Get for  {compensationValue} mint(s): {Web3.Convert.FromWei(getTotalB0x)} B0x");
                            Program.Print($"ETH You Get for {compensationValue} mint(s): {Web3.Convert.FromWei(getTotalETH)} ETH");
                            Program.Print($"ETH You Spendfor {compensationValue} mint(s): {Web3.Convert.FromWei(getTotalETHowed)} ETH");
                            getTotalETHowedtoSendtoContract = getTotalETHowed;




                            // Calculate net ETH per B0x token ratio
                            var netETH = Web3.Convert.FromWei(getTotalETH) - Web3.Convert.FromWei(getTotalETHowed);
                            var totalB0xTokens = Web3.Convert.FromWei(getTotalB0x);

                            // Avoid division by zero
                            decimal netETHperB0xToken = 0;
                            decimal netUSDperB0xToken = 0;
                            if (totalB0xTokens == 0)
                            {
                                Program.Print("ERROR: No B0x tokens to calculate ratio! Probably setting up miner, so just use -0.000001 as our test");
                                netETHperB0xToken = -0.000001m; // Note the 'm' suffix for decimal
                                netUSDperB0xToken = netETHperB0xToken * ETHUSDCPriceDecimal;

                            }
                            else
                            {
                                netETHperB0xToken = netETH / totalB0xTokens;
                                netUSDperB0xToken = netETHperB0xToken * ETHUSDCPriceDecimal;
                            }

                            Program.Print($"Net ETH (Get - Spend): {netETH} ETH");
                            Program.Print($"Net ETH per B0x Token: {netETHperB0xToken} ETH");

                            // Price comparison logic for NEGATIVE values (costs)
                            Program.Print("\n=== Cost Analysis ===");
                            Program.Print($"Maximum Cost Willing to Pay: {m_USDperToken} $/token");
                            Program.Print($"Actual Cost per Token: {netUSDperB0xToken:F6} $/token");

                            // For negative numbers: "less negative" = better deal
                            // Example: -0.00001 is BETTER than -0.1 (less cost)

                            if (!runOnceThenResetAfter)
                            {
                                runOnceThenResetAfter = true;
                                currentCounterLocationEnd = currentCounter;
                                File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                            }
                            if (blocksFromReadjustmentz < 5 && blocksToReadjustmentz < 2 && slowBlocksz > 2016 / 8)
                            {
                                Program.Print("✅ SUCCESS NFT MINTING IS ACTIVE MINT NFT RIGHT NOW STORED IN B0xToken.conf");
                                //    Program.Print("✅ SUCCESS NFT MINTING IS ACTIVE MINT NFT RIGHT NOW STORED IN B0xToken.conf");
                                Program.Print("Minting the first successful NFT of these NFTs: " + string.Join(", ", m_nftAddresses));
                                Program.Print("Minting the first successful NFT of these NFTs IDS: " + string.Join(", ", m_nftTokenIds));
                                mintNFT = true;

                            }
                            decimal percentageDifference = 0.0m; // Note the 'm' suffix for decimal
                            if (netUSDperB0xToken > m_USDperToken)
                            {

                                Program.Print("✅ SUCCESS 1 OF 2: Cost is LOWER than maximum acceptable cost - ACCEPTABLE! ✅ ");

                                // Don't use Math.Abs() - preserve the signs!
                                decimal actualCost = netUSDperB0xToken;           // Keep original sign
                                decimal maxAcceptableCost = m_USDperToken;       // Keep original sign

                                Program.Print($"Paying {actualCost:F7} $/token vs willing to pay up to {maxAcceptableCost:F7} $/token");

                                decimal savings = m_USDperToken - netUSDperB0xToken;  // This will be positive when saving money
                                                                                      //Program.Print($"Cost savings per token: {savings:F2} $");
                                                                                      // Program.Print($"Total cost savings: {savings * totalEKTokens:F2} $");
                                percentageDifference = ((netUSDperB0xToken - m_USDperToken) / m_USDperToken) * 100;
                            }
                            else if (netUSDperB0xToken == m_USDperToken)
                            {
                                Program.Print("⚖️ BREAK EVEN: Cost equals maximum acceptable cost");
                                percentageDifference = 0;
                            }
                            else
                            {
                                Task.Delay((int)(1.25 * 1000)).Wait();
                                Program.Print("❌ ERROR: Cost is HIGHER than maximum acceptable cost!");

                                // Don't use Math.Abs() - preserve the signs!
                                decimal actualCost = netUSDperB0xToken;      // Keep original sign
                                decimal maxAcceptableCost = m_USDperToken;  // Keep original sign

                                // Calculate excess cost (how much MORE you're paying than willing to pay)
                                decimal excessCost = netUSDperB0xToken - m_USDperToken;  // This will be negative (bad)

                                Program.Print($"Excess cost per token: {Math.Abs(excessCost):F6} $");
                                Program.Print($"Total excess cost: {Math.Abs(excessCost) * totalB0xTokens:F6} $");
                                Program.Print("RECOMMENDATION: Do not proceed - cost is too high!");

                                Program.Print($"Would pay {actualCost:F6}$/token but only willing to pay up to {maxAcceptableCost:F6}$/token");

                                percentageDifference = ((netUSDperB0xToken - m_USDperToken) / m_USDperToken) * 100;

                                Task.Delay((int)(0.5 * 1000)).Wait();
                                Program.Print($"Change your maxTokenPriceToPay in .confg File to {actualCost:F6}$/token.  Currently you are only willing to pay up to {maxAcceptableCost:F6}$/token");


                                Task.Delay((int)(1.5 * 1000)).Wait();
                                var waittime = m_checkIntervalinSeconds;


                                if (secondsFromTimetoTurnInAnswers < new BigInteger(waittime))
                                {
                                    waittime = (float)secondsFromTimetoTurnInAnswers;
                                }
                                Program.Print($"🕐 Sleeping now for {waittime} seconds then retrying the submit.");
                                Task.Delay((int)(waittime * 1000)).Wait();

                                var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);



                                return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                            }


                            if (m_minSecondsPerAnswer <= secFromPrevious / compensationValue)
                            {


                                Program.Print($"✅✅ SUCCESS 2 of 2: MinSecondsPerAnswer is <= to the Average SecondsFromPrevious/compensationValue ✅✅ ");


                                Program.Print($"m_minSecondsPerAnswer: {m_minSecondsPerAnswer} seconds");
                                Program.Print($"Avg Seconds Per Answer currently: {secFromPrevious / compensationValue} seconds");


                            }
                            else
                            {

                                Task.Delay((int)(1.25 * 1000)).Wait();
                                Program.Print("❌❌   ERROR: MinSecondsPerAnswer is GREATER THAN SecondsFromPrevious/compensationValue   ❌❌ ");


                                Task.Delay((int)(0.25 * 1000)).Wait();
                                Program.Print($"m_minSecondsPerAnswer: {m_minSecondsPerAnswer} seconds");
                                Program.Print($"Avg Seconds Per Answer currently: {secFromPrevious / compensationValue} seconds");

                                Task.Delay((int)(1.5 * 1000)).Wait();
                                var totalSecsec = m_minSecondsPerAnswer * compensationValue - secFromPrevious;
                                var amountToWait = (float)m_checkIntervalinSeconds;
                                Program.Print($"🕐 Need to wait a total of {totalSecsec} seconds before settings allow an answer! Max Sleep is {m_checkIntervalinSeconds} seconds per check");

                                if ((decimal)totalSecsec <= (decimal)m_checkIntervalinSeconds)
                                {
                                    amountToWait = (float)totalSecsec + 1;
                                }


                                var waittime = amountToWait;

                                if (secondsFromTimetoTurnInAnswers < new BigInteger(waittime))
                                {
                                    waittime = (float)secondsFromTimetoTurnInAnswers;
                                }



                                Task.Delay((int)(0.55 * 1000)).Wait();
                                Program.Print($"🕐🕐 Sleeping now for {waittime} seconds then retrying the submit.");
                                Task.Delay((int)(0.5 * 1000)).Wait();
                                Program.Print($"Try lowering your minSecondsPerAnswer variable in the .config! If your worried about frontrunning on very difficult answers 1) use flashbots rpc and 60 seconds is usually plenty for minSecondsPerAnswer , the transaction will fail but the miner will save your answer and attempt to submit again when the time is right!");
                                Task.Delay((int)(waittime * 1000)).Wait();


                                var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);



                                return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                            }






                            // Additional detailed breakdown
                            Program.Print("\n=== Detailed Breakdown ===");
                            Program.Print($"Raw percentage difference: {percentageDifference:F4}%");
                            if (percentageDifference <= 0)
                            {
                                Program.Print($"Interpretation: Paying {Math.Abs(percentageDifference):F2}% less than maximum (GOOD)");
                            }
                            else if (percentageDifference > 0)
                            {
                                Program.Print($"Interpretation: Paying {percentageDifference:F2}% more than maximum (BAD)");
                            }














                        }
                        catch (Exception e)
                        {
                            Program.Print($"Exception type: {e.GetType().Name}");
                            Program.Print($"Exception message: {e.Message}");
                            Program.Print($"Inner exception: {e.InnerException?.Message}");
                        }
                    }




                    //MAXNUMBEROFMINTSPOSSIBLE is set in the top of the file at 2500 because thats max number it will accept
                    var miniz = Math.Min(Math.Min(currentCounter, (int)epochNumber5555), lastNonceArray2x.Count);
                    Program.Print("A TOTAL OF " + miniz + " mints are allowed during this mint");

                    lastDigestArray2xz = lastDigestArray2xz.Take(miniz).ToList();
                    lastChallengeArray2xz = lastChallengeArray2xz.Take(miniz).ToList();
                    filteredLastNonceArray2x = filteredLastNonceArray2x.Take(miniz).ToList();
                    // Assuming `filteredLastNonceArray2x` is an enumerable collection like List<int>


                    //   var miningParameters3 = GetMiningParameters2();

                    Program.Print(string.Format("Sending transaction now"));

                    //  var OriginalChal = miningParameters3.Challenge.Value;
                    // Program.Print(string.Format("[INFO] Original Challenge is  {0}", OriginalChal));
                    m_challengeReceiveDateTime = DateTime.MinValue;


                    var transactionID = string.Empty;
                    var gasLimit = new HexBigInteger(m_gasLimit);
                    var userGasPriority = new HexBigInteger(UnitConversion.Convert.ToWei(new BigDecimal(m_gasPricePriority), UnitConversion.EthUnit.Gwei));
                    var userGas = new HexBigInteger(UnitConversion.Convert.ToWei(new BigDecimal(m_gasToMine), UnitConversion.EthUnit.Gwei));

                    var ID = BigInteger.Parse("-1");
                    var apiGasPrice3 = "-1";
                    var apiGasPrice2 = "-1";



                    do
                    {



                        var startSubmitDateTime = DateTime.Now;

                        if (!string.IsNullOrWhiteSpace(m_gasApiURL))
                        {

                            try
                            {
                                var apiGasPrice = Utils.Json.DeserializeFromURL(m_gasApiURL).SelectToken(m_gasApiPath).Value<float>();
                                if (apiGasPrice > 0)
                                {
                                    apiGasPrice *= m_gasApiMultiplier;
                                    apiGasPrice += m_gasApiOffset;

                                    if (apiGasPrice < m_gasToMine)
                                    {
                                        Program.Print(string.Format("[INFO] Using 'gasToMine' price of {0} GWei, due to lower gas price from API: {1}",
                                                                    m_gasToMine, m_gasApiURL));
                                    }
                                    else if (apiGasPrice > m_gasApiMax)
                                    {
                                        userGas = new HexBigInteger(UnitConversion.Convert.ToWei(new BigDecimal(m_gasApiMax), UnitConversion.EthUnit.Gwei));
                                        Program.Print(string.Format("[INFO] Using 'gasApiMax' price of {0} GWei, due to higher gas price from API: {1}",
                                                                    m_gasApiMax, m_gasApiURL));
                                    }
                                    else
                                    {
                                        userGas = new HexBigInteger(UnitConversion.Convert.ToWei(new BigDecimal(apiGasPrice), UnitConversion.EthUnit.Gwei));
                                        Program.Print(string.Format("[INFO] Using gas price of {0} GWei (after {1} offset) from API: {2}",apiGasPrice, m_gasApiOffset, m_gasApiURL));
                                    }
                                }
                                else
                                {
                                    Program.Print(string.Format("[ERROR] Gas price of 0 GWei was retuned by API: {0}", m_gasApiURL));
                                    Program.Print(string.Format("[INFO] Using 'gasToMine' parameter of {0} GWei.", m_gasToMine));
                                }
                            }
                            catch (Exception ex)
                            {
                                HandleException(ex, string.Format("Failed to read gas price from API ({0})", m_gasApiURL));

                                if (LastSubmitGasPrice == null || LastSubmitGasPrice.Value <= 0)
                                    Program.Print(string.Format("[INFO] Using 'gasToMine' parameter of {0} GWei.", m_gasToMine));
                                else
                                {
                                    Program.Print(string.Format("[INFO] Using last submitted gas price of {0} GWei.",
                                                                UnitConversion.Convert.FromWeiToBigDecimal(LastSubmitGasPrice, UnitConversion.EthUnit.Gwei).ToString()));
                                    userGas = LastSubmitGasPrice;
                                }
                            }
                        }



                        object[] dataInput1 = null;
                        object[] dataInput2 = null;
                        object[] dataInput3 = null;
                        object[] dataInput4 = null;

                        object[] dataInputMegaLentum = null;
                        object[] dataInputNFT = null;
                        object[] dataInputERC20 = null;


                        object[] dataInputMega = null;
                        object[] dataInputMega2WithERC20 = null;
                        object[] dataInputMegaPaymaster = null;
                        dataInput1 = new object[] { apiGasPrice2, ID, new BigInteger(nonceArray2[nonceArray2.Length - 1], isBigEndian: true), digestArray2[digestArray2.Length - 1] };
                        // dataInput2 = new object[] { new BigInteger(nonceArray2[nonceArray2.Length - 1], isBigEndian: true), digestArray2[digestArray2.Length - 1], ethereumAddresses, address };
                        dataInput3 = new object[] { new BigInteger(nonceArray2[nonceArray2.Length - 1], isBigEndian: true), digestArray2[digestArray2.Length - 1], address };
                        dataInput4 = new object[] { new BigInteger(nonceArray2[nonceArray2.Length - 1], isBigEndian: true), digestArray2[digestArray2.Length - 1] };
                        //  dataInputMega = new object[] { lastNonceArray, lastDigestArray, lastChallangeArray };
                        string[] ethereumAddressesFFFFFFFFFF = new string[] {
                            "0x2Fe4abE63F6A2805D540F6da808527D21Bc9ea60",
                            "0x7fB3e26D054c2740610a855f5E2A39f0ab509eA6",
                            "0xD79c279F8d10AF90e0a3aCea9003f8f28dF68509",
                            "0x5fDCd08c9558041D76c11005ab8b66a7D4c3e8d4",
                            "0xC4E67509E49EdEFA7aBfEE81132a0f4c51292567",
                            "0xE04F5F58dF9FC6763980cB5f711fa6C180c5bfAE",
                            "0x3C3D01A3f2cF2DA7afa8b86A43AABeF780360839",
                            "0x406d11E82D8AD4358eE1990e082FC0AB11FdeA47",
                            "0x40BcA29F56e4f5fFf45765aA31b5eCda02413EB7",
                            "0x4196DEc2d0E06D310968AF2046FA58aa93C7Df58",
                            "0x43E024a535c5BE358C4E3e28BF728A7165d731dE"
                        };
                        // var filePath = "aDataToMintDigestsUsed.txt";
                        //var oldDigestArray = ReadFileIntoByteArrayList(filePath);
                        //List<byte[]> oldDigestArray = new List<byte[]>();
                        // dataInputMega = new object[] { address, filteredLastNonceArray2x, lastChallengeArray2xz };
                        // dataInputMega = new object[] { address, filteredLastNonceArray2x };
                        dataInputMega = new object[] { m_mintToaddress, filteredLastNonceArray2x };

                        dataInputNFT = new object[] { m_nftAddresses, m_nftTokenIds, filteredLastNonceArray2x, m_maxAnswersPerSubmit };

                        dataInputMegaLentum = new object[] { m_mintToaddress, filteredLastNonceArray2x, m_maxAnswersPerSubmit, m_minSecondsPerAnswer };
                        if (m_mintToaddress == "0x1755BA5e18DBaFb375E5036150c59240Ed61FA98" || m_mintToaddress == "0x851c0428ee0be11f80d93205f6cB96adBBED22e6" || m_mintToaddress == "" || m_mintToaddress == "0x")
                        {
                            dataInputMega = new object[] { address, filteredLastNonceArray2x };
                        }
                        dataInputMega2WithERC20 = new object[] { address, filteredLastNonceArray2x };
                        //dataInputMega = new object[] { lastNonceArray2x[0], lastDigestArray2x[0] };


                        // Program.Print("DATA INPUT MEGA LENTUM:filteredLastNonceArray2x: [" + string.Join(", ", filteredLastNonceArray2x) + "]");


                        Program.Print("[m_mintTo Address] m_mintToAddress: " + m_mintToaddress);
                        Program.Print("[m_mintTo Address] m_mintToAddress: " + m_mintToaddress);

                        Program.Print("[Miner Address] MinerAddress: " + MinerAddress);
                        Program.Print("[Miner Address] MinerAddress: " + MinerAddress);









                        try
                        {


                            var miningParameters4a = GetMiningParameters4();
                            var epochNumber = miningParameters4a.Epoch.Value;
                            var startEpoch = epochNumber;
                            var endEpoch = epochNumber + totalMultiple;

                            int largestPowerOf2Factor = 0;

                            // Check each epoch number in the range
                            for (var currentEpoch = startEpoch; currentEpoch <= endEpoch; currentEpoch++)
                            {
                                // Find the largest power of 2 that divides currentEpoch
                                int powerOf2Factor = GetLargestPowerOf2Factor((int)currentEpoch);

                                // Keep track of the largest one found
                                if (powerOf2Factor > largestPowerOf2Factor)
                                {
                                    largestPowerOf2Factor = powerOf2Factor;
                                }

                                // Program.Print($"[DEBUG] Epoch {currentEpoch}: largest 2^{powerOf2Factor} factor");
                            }

                            Program.Print($"[INFO] Largest power of 2 factor found: 2^{largestPowerOf2Factor}");

                            // Helper method to find the largest power of 2 that divides a number
                            static int GetLargestPowerOf2Factor(int number)
                            {
                                if (number == 0) return 0;

                                int power = 0;
                                while ((number & 1) == 0) // While number is even
                                {
                                    number >>= 1; // Divide by 2
                                    power++;
                                }
                                return power;
                            }






                            // Program.Print(string.Format("[EPOCH #] EPOCH # " + epochNumber));
                            //Program.Print(string.Format("[ETH2SEND #] ETH2SEND" + ETH2SENDa));
                            var txCount = m_web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address).Result;

                            // Commented as gas limit is dynamic in between submissions and confirmations
                            //var estimatedGasLimit = m_mintMethod.EstimateGasAsync(from: address,
                            //                                                      gas: gasLimit,
                            //                                                      value: new HexBigInteger(0),
                            //                                                      functionInput: dataInput).Result;
                            BigInteger estimatedGasLimit = 1;

                            TransactionInput transaction;
                            string[] finalERC20Addresses = new string[0];
                            if (ERC20AddressesToMint.Length > 0)
                            {
                                Program.Print(string.Format("[INFO] This many possible ERC20s to mint: {0}", ERC20AddressesToMint.Length));


                                // Remove the quotes - Nethereum handles string arrays automatically
                                if (largestPowerOf2Factor < ERC20AddressesToMint.Length)
                                {
                                    Program.Print(string.Format("[INFO] Limiting ERC20s from {0} to {1} (largestPowerOf2Factor)",
                                        ERC20AddressesToMint.Length, largestPowerOf2Factor));

                                    // Don't add quotes - just use the addresses as strings
                                    finalERC20Addresses = ERC20AddressesToMint.Take(largestPowerOf2Factor).ToArray();

                                    Program.Print(string.Format("[INFO] Selected ERC20s: {0}", string.Join(", ", finalERC20Addresses)));
                                }
                                else
                                {
                                    // Don't add quotes - just use the addresses as strings
                                    finalERC20Addresses = ERC20AddressesToMint;
                                    Program.Print(string.Format("[INFO] Using all {0} ERC20 addresses", finalERC20Addresses.Length));
                                }

                            }

                            dataInputERC20 = new object[] { filteredLastNonceArray2x, finalERC20Addresses, m_mintToaddress, m_maxAnswersPerSubmit, m_minSecondsPerAnswer };

                            // Print each element of the dataInputERC20 array
                            //Program.Print("[DEBUG] dataInputERC20 contents:");
                            for (int i = 0; i < dataInputERC20.Length; i++)
                            {
                                if (i == 0)//first element we need to specifically output easier
                                {
                                    Program.Print(string.Format("[Function Data] mint Function Input[0] (Array): [{0}]", string.Join(", ", filteredLastNonceArray2x)));
                                    continue;
                                }
                                if (dataInputERC20[i] is Array arr)
                                {
                                    // Handle arrays (like filteredLastNonceArray2x and finalERC20Addresses)
                                    var arrayItems = new List<string>();
                                    foreach (var item in arr)
                                    {
                                        arrayItems.Add(item?.ToString() ?? "null");
                                    }
                                    Program.Print(string.Format("[Function Data] mint Function Input[{0}] (Array): [{1}]", i, string.Join(", ", arrayItems)));
                                }
                                else
                                {
                                    // Handle single values
                                    Program.Print(string.Format("[Function Data] mint Function Input[{0}]: {1}", i, dataInputERC20[i]?.ToString() ?? "null"));
                                }
                            }
                            var mintNoERC20 = false;
                            if (mintNFT == true)
                            {




                                try
                                {

                                    estimatedGasLimit = m_mintNFTMethod.EstimateGasAsync(from: address,
                                                                                    gas: gasLimit,
                                                                                    value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1), // 1e17 as string
                                                                                    functionInput: dataInputNFT).Result;
                                    estimatedGasLimit = estimatedGasLimit * 12/10;
                                    Program.Print(string.Format("[INFO] Gas to mint simple is  {0}", estimatedGasLimit));
                                    
                                    
                                }
                                catch (Exception e)
                                {
                                    Program.Print("error estimating gas for users NFT mint");
                                    Program.Print("Error is: " + e.ToString());

                                    Program.Print("error estimating gas for users own ERC20 mint");
                                    Program.Print("Error is: " + e.ToString());
                                    Task.Delay(1500).Wait();

                                    estimatedGasLimit = 150000;


                                    if (e.ToString().Contains("One or more errors occurred. (out of gas: gas required exceeds"))
                                    {


                                        Program.Print($"2Current counter Starts at:  {currentCounter}");
                                        Program.Print($"2Current currentCounterLocationStart Starts at:  {currentCounterLocationStart}");
                                        Program.Print($"2Current currentCounterLocationEnd Starts at:  {currentCounterLocationEnd}");


                                        digestArray2 = new byte[][] { };
                                        challengeArray2 = new byte[][] { };
                                        nonceArray2 = new byte[][] { };
                                        var totalEpochs = currentCounter - currentCounterLocationStart;
                                        Program.Print($"Current TOTAL EPOCHS IS:  {totalEpochs}");

                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");

                                        // Define reduction thresholds
                                        int[] thresholds = { 2017, 1800, 1600, 1400, 1200, 1000, 800, 600, 450, 400, 350, 250, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20, 10 };
                                        int[] newValues = {  1800, 1600, 1400, 1200, 1000, 800, 700, 450, 400, 350, 300, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20 ,10, 5 };

                                        bool reduced = false;
                                        for (int i = 0; i < thresholds.Length; i++)
                                        {
                                            if (totalEpochs >= thresholds[i])
                                            {
                                                totalEpochs = newValues[i];
                                                reduced = true;
                                                break;
                                            }
                                        }

                                        if (!reduced)
                                        {
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                        }
                                        else
                                        {



                                            string filePathzCurrentCounterOLD = "counterCounterOLD.txt";
                                        //    string filePathzLocationEndOLD = "counterLocationEndOLD.txt";
                                         //   string filePath3OLD = Path.Combine(directoryPathzfzfdzf, filePathzLocationEndOLD);
                                            // Construct the file path
                                            string filePath4OLD = Path.Combine(directoryPathzfzfdzf, filePathzCurrentCounterOLD);

                                            string challengeFilePath = Path.Combine(directoryPathzfzfdzf, "challenge.dat");


                                            // Read previous challenge if file exists, otherwise use empty array
                                            byte[] previousChallenge = File.Exists(challengeFilePath)
                                                ? File.ReadAllBytes(challengeFilePath)
                                                : Array.Empty<byte>();

                                            newEndOfCurrentCounterCurrentChallenge = previousChallenge;



                                            if (!newEndOfCurrentCounterCurrentChallenge.SequenceEqual(challenge))
                                            {
                                                oldCurrentCounter = currentCounter;
                                              //  oldcurrentCounterLocatioNEnd = currentCounterLocationEnd;
                                                newEndOfCurrentCounterCurrentChallenge = challenge;
                                                File.WriteAllBytes(challengeFilePath, challenge);

                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(filePath4OLD, currentCounter.ToString());



                                            }


                                            currentCounterLocationEnd = currentCounterLocationStart+totalEpochs;
                                            currentCounter = totalEpochs + currentCounterLocationStart;
                                            if (m_MaxSolvesperMint > totalEpochs)
                                            {

                                                m_MaxSolvesperMint = totalEpochs ;
                                            }
                                            if (m_MinSolvesperMint > totalEpochs)
                                            {

                                                m_MinSolvesperMint = totalEpochs;
                                            }

                                            Program.Print($"WROTE {filePath3} {currentCounterLocationEnd}");

                                            File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                                            File.WriteAllText(filePath4, currentCounter.ToString());


                                            digestArray2 = new byte[][] { };
                                            challengeArray2 = new byte[][] { };
                                            nonceArray2 = new byte[][] { };
                                            File.WriteAllText(MinSolvesPerMint, m_MinSolvesperMint.ToString());
                                            File.WriteAllText(MaxSolvesPerMint, m_MaxSolvesperMint.ToString());
                                            var digest22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            var nonceBytes22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            byte[] nonceByt1 = Encoding.UTF8.GetBytes(nonceBytes22222221);
                                            byte[] digestByt1 = Encoding.UTF8.GetBytes(digest22222221);


                                         string firstSubmitRun = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");
                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(firstSubmitRun, "1");


                                            Task.Delay(5500).Wait();

                                            return SubmitSolution(address, digestByt1, challenge, difficulty, nonceByt1, sender);



                                        }

                                    }




                                    if (e.ToString().Contains("gas required exceeds allowance"))
                                    {

                                        ulong[] gasTargets = new ulong[] {
                                            250_000, 500_000, 900_000, 1_000_000, 2_000_000, 3_000_000,
                                            5_000_000, 10_000_000, 20_000_000, 30_000_000, 40_000_000,
                                            50_000_000
                                        };

                                        bool bumped = false;
                                        foreach (ulong target in gasTargets)
                                        {
                                            if (m_gasLimit < target)
                                            {
                                                m_gasLimit = target;
                                                bumped = true;
                                                break; // set to the next higher bracket and stop
                                            }
                                        }

                                        if (!bumped)
                                        {
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                        }
                                    }

                                    if (e.ToString().Contains("One or more errors occurred. (incorrect response body: wrong json - rpc response - there is neither result nor error: eth_estimateGas") || (e.ToString().Contains("Response status code does not indicate success: 500 (Internal Server Error)") && e.ToString().Contains("One or more errors occurred. (Error occurred when trying to send rpc requests(s): eth_estimateGas")))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }
                                    if (e.ToString().Contains("(gas limit is too high"))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }

                                    if (e.ToString().Contains("insufficient funds for gas") ||e.ToString().Contains(" Need to send required ETH") )
                                    {
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");

                                        Task.Delay(4500).Wait();
                                        return false;
                                    }

                                    var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                    byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);

                                Task.Delay(5500).Wait();

                                    return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                                }
                                transaction = m_mintNFTMethod.CreateTransactionInput(from: address,
                                                                gas: gasLimit,
                                                                gasPrice: userGas,
                                                                value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1),
                                                                functionInput: dataInputNFT);







                            }
                            else if (mintNoERC20 == true)
                            {

                                try
                                {

                                    estimatedGasLimit = m_mintMethod.EstimateGasAsync(from: address,
                                                                                    gas: gasLimit,
                                                                                    value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1), // 1e17 as string
                                                                                    functionInput: dataInputMegaLentum).Result;
                                    estimatedGasLimit = estimatedGasLimit * 12/10;
                                    Program.Print(string.Format("[INFO] Gas to mint NFT is  {0}", estimatedGasLimit)); 
              
                                }
                                catch (Exception e)
                                {
                                    Program.Print("error estimating gas for users own mint");
                                    Program.Print("Error is: " + e.ToString());

                                    Program.Print("error estimating gas for users own ERC20 mint");
                                    Program.Print("Error is: " + e.ToString());
                                    Task.Delay(1500).Wait();

                                    estimatedGasLimit = 150000;
                                  


                                    if (e.ToString().Contains("One or more errors occurred. (out of gas: gas required exceeds"))
                                    {


                                        Program.Print($"2Current counter Starts at:  {currentCounter}");
                                        Program.Print($"2Current currentCounterLocationStart Starts at:  {currentCounterLocationStart}");
                                        Program.Print($"2Current currentCounterLocationEnd Starts at:  {currentCounterLocationEnd}");


                                        var totalEpochs = currentCounter - currentCounterLocationStart;
                                        Program.Print($"Current TOTAL EPOCHS IS:  {totalEpochs}");

                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");

                                        // Define reduction thresholds
                                        int[] thresholds = { 2017, 1800, 1600, 1400, 1200, 1000, 800, 600, 450, 400, 350, 250, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20, 10 };
                                        int[] newValues = {  1800, 1600, 1400, 1200, 1000, 800, 700, 450, 400, 350, 300, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20 ,10, 5 };

                                        bool reduced = false;
                                        for (int i = 0; i < thresholds.Length; i++)
                                        {
                                            if (totalEpochs >= thresholds[i])
                                            {
                                                totalEpochs = newValues[i];
                                                reduced = true;
                                                break;
                                            }
                                        }

                                        if (!reduced)
                                        {
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                        
                                        }
                                        else
                                        {



                                            string filePathzCurrentCounterOLD = "counterCounterOLD.txt";
                                        //    string filePathzLocationEndOLD = "counterLocationEndOLD.txt";
                                         //   string filePath3OLD = Path.Combine(directoryPathzfzfdzf, filePathzLocationEndOLD);
                                            // Construct the file path
                                            string filePath4OLD = Path.Combine(directoryPathzfzfdzf, filePathzCurrentCounterOLD);

                                            string challengeFilePath = Path.Combine(directoryPathzfzfdzf, "challenge.dat");


                                            // Read previous challenge if file exists, otherwise use empty array
                                            byte[] previousChallenge = File.Exists(challengeFilePath)
                                                ? File.ReadAllBytes(challengeFilePath)
                                                : Array.Empty<byte>();

                                            newEndOfCurrentCounterCurrentChallenge = previousChallenge;



                                            if (!newEndOfCurrentCounterCurrentChallenge.SequenceEqual(challenge))
                                            {
                                                oldCurrentCounter = currentCounter;
                                              //  oldcurrentCounterLocatioNEnd = currentCounterLocationEnd;
                                                newEndOfCurrentCounterCurrentChallenge = challenge;
                                                File.WriteAllBytes(challengeFilePath, challenge);

                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(filePath4OLD, currentCounter.ToString());



                                            }



                                            currentCounterLocationEnd = currentCounterLocationStart+totalEpochs;
                                            currentCounter = totalEpochs + currentCounterLocationStart;
                                            if (m_MaxSolvesperMint > totalEpochs)
                                            {

                                                m_MaxSolvesperMint = totalEpochs ;
                                            }
                                            if (m_MinSolvesperMint > totalEpochs)
                                            {

                                                m_MinSolvesperMint = totalEpochs;
                                            }

                                            Program.Print($"WROTE {filePath3} {currentCounterLocationEnd}");

                                            File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                                            File.WriteAllText(filePath4, currentCounter.ToString());


                                            digestArray2 = new byte[][] { };
                                            challengeArray2 = new byte[][] { };
                                            nonceArray2 = new byte[][] { };
                                            File.WriteAllText(MinSolvesPerMint, m_MinSolvesperMint.ToString());
                                            File.WriteAllText(MaxSolvesPerMint, m_MaxSolvesperMint.ToString());
                                            var digest22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            var nonceBytes22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            byte[] nonceByt1 = Encoding.UTF8.GetBytes(nonceBytes22222221);
                                            byte[] digestByt1 = Encoding.UTF8.GetBytes(digest22222221);



                                         string firstSubmitRun = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");
                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(firstSubmitRun, "1");



                                            Task.Delay(5500).Wait();

                                            return SubmitSolution(address, digestByt1, challenge, difficulty, nonceByt1, sender);



                                        }

                                    }


                                 


                                    if (e.ToString().Contains("gas required exceeds allowance"))
                                    {

                                        ulong[] gasTargets = new ulong[] {
                                            250_000, 500_000, 900_000, 1_000_000, 2_000_000, 3_000_000,
                                            5_000_000, 10_000_000, 20_000_000, 30_000_000, 40_000_000,
                                            50_000_000
                                        };

                                        bool bumped = false;
                                        foreach (ulong target in gasTargets)
                                        {
                                            if (m_gasLimit < target)
                                            {
                                                m_gasLimit = target;
                                                bumped = true;
                                                break; // set to the next higher bracket and stop
                                            }
                                        }

                                        if (!bumped)
                                        {
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                        }
                                    }

                                    if (e.ToString().Contains("One or more errors occurred. (incorrect response body: wrong json - rpc response - there is neither result nor error: eth_estimateGas") || (e.ToString().Contains("Response status code does not indicate success: 500 (Internal Server Error)") && e.ToString().Contains("One or more errors occurred. (Error occurred when trying to send rpc requests(s): eth_estimateGas")))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }
                                    if (e.ToString().Contains("(gas limit is too high"))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }


                                    if (e.ToString().Contains("insufficient funds for gas") ||e.ToString().Contains(" Need to send required ETH") )
                                    {
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");

                                        Task.Delay(4500).Wait();
                                        return false;
                                    }

                                    var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                    byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);


                                Task.Delay(5500).Wait();

                                    return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                                }

                                transaction = m_mintMethod.CreateTransactionInput(from: address,
                                                                                        gas: gasLimit,
                                                                                        gasPrice: userGas,
                                                                                        value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1),
                                                                                        functionInput: dataInputMegaLentum);




                            }
                            else
                            {
                                try
                                {



                                    estimatedGasLimit = m_MintERC20.EstimateGasAsync(from: address,
                                                                                    gas: gasLimit,
                                                                                    value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1), // 1e17 as string
                                                                                    functionInput: dataInputERC20).Result;
                                    estimatedGasLimit = (int)estimatedGasLimit * 12 / 10;
                                    Program.Print(string.Format("[INFO] Gas to mint ERC20 is  {0}", estimatedGasLimit));

                                }
                                catch (Exception e)
                                {

                                    Program.Print("error estimating gas for users own ERC20 mint");
                                    Program.Print("Error is: " + e.ToString());
                                    Task.Delay(1500).Wait();

                                    estimatedGasLimit = 150000;


                                    if (e.ToString().Contains("One or more errors occurred. (out of gas: gas required exceeds"))
                                    {


                                        Program.Print($"2Current counter Starts at:  {currentCounter}");
                                        Program.Print($"2Current currentCounterLocationStart Starts at:  {currentCounterLocationStart}");
                                        Program.Print($"2Current currentCounterLocationEnd Starts at:  {currentCounterLocationEnd}");


                                        digestArray2 = new byte[][] { };
                                        challengeArray2 = new byte[][] { };
                                        nonceArray2 = new byte[][] { };
                                        var totalEpochs = currentCounter - currentCounterLocationStart;
                                        Program.Print($"Current TOTAL EPOCHS IS:  {totalEpochs}");

                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");
                                        Program.Print("[INFO] TOO MANY SOLUTIONS FOR GAS LIMIT! Try lowering MaxSolvesperMint and MinSolvesperMint or Increasing gasLimit in config file, automatically fixing the issue, give it a minute   ❌ ❌ ❌");

                                        // Define reduction thresholds
                                        int[] thresholds = { 2017, 1800, 1600, 1400, 1200, 1000, 800, 600, 450, 400, 350, 250, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20, 10 };
                                        int[] newValues = {  1800, 1600, 1400, 1200, 1000, 800, 700, 450, 400, 350, 300, 225, 200, 175, 150, 125, 110, 100, 90, 80, 70, 60, 50, 40, 30, 20 ,10, 5 };

                                        bool reduced = false;
                                        for (int i = 0; i < thresholds.Length; i++)
                                        {
                                            if (totalEpochs >= thresholds[i])
                                            {
                                                totalEpochs = newValues[i];
                                                reduced = true;
                                                break;
                                            }
                                        }

                                        if (!reduced)
                                        {
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                            Program.Print("[Lower total submissions to blockchain] UNABLE TO REDUCE Blocks submission anymore❌ ❌ ❌");
                                        
                                        }
                                        else
                                        {

                                             

                                            string filePathzCurrentCounterOLD = "counterCounterOLD.txt";
                                        //    string filePathzLocationEndOLD = "counterLocationEndOLD.txt";
                                         //   string filePath3OLD = Path.Combine(directoryPathzfzfdzf, filePathzLocationEndOLD);
                                            // Construct the file path
                                            string filePath4OLD = Path.Combine(directoryPathzfzfdzf, filePathzCurrentCounterOLD);

                                            string challengeFilePath = Path.Combine(directoryPathzfzfdzf, "challenge.dat");


                                            // Read previous challenge if file exists, otherwise use empty array
                                            byte[] previousChallenge = File.Exists(challengeFilePath)
                                                ? File.ReadAllBytes(challengeFilePath)
                                                : Array.Empty<byte>();

                                            newEndOfCurrentCounterCurrentChallenge = previousChallenge;



                                            if (!newEndOfCurrentCounterCurrentChallenge.SequenceEqual(challenge))
                                            {
                                                oldCurrentCounter = currentCounter;
                                              //  oldcurrentCounterLocatioNEnd = currentCounterLocationEnd;
                                                newEndOfCurrentCounterCurrentChallenge = challenge;
                                                File.WriteAllBytes(challengeFilePath, challenge);

                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(filePath4OLD, currentCounter.ToString());



                                            }
                                            currentCounterLocationEnd = currentCounterLocationStart+totalEpochs;
                                            currentCounter = totalEpochs + currentCounterLocationStart;
                                            if (m_MaxSolvesperMint > totalEpochs)
                                            {

                                                m_MaxSolvesperMint = totalEpochs ;
                                            }
                                            if (m_MinSolvesperMint > totalEpochs)
                                            {

                                                m_MinSolvesperMint = totalEpochs;
                                            }

                                            Program.Print($"WROTE {filePath3} {currentCounterLocationEnd}");

                                            File.WriteAllText(filePath3, currentCounterLocationEnd.ToString());
                                            File.WriteAllText(filePath4, currentCounter.ToString());

                                            digestArray2 = new byte[][] { };
                                            challengeArray2 = new byte[][] { };
                                            nonceArray2 = new byte[][] { };
                                            File.WriteAllText(MinSolvesPerMint, m_MinSolvesperMint.ToString());
                                            File.WriteAllText(MaxSolvesPerMint, m_MaxSolvesperMint.ToString());
                                            var digest22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            var nonceBytes22222221 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                            byte[] nonceByt1 = Encoding.UTF8.GetBytes(nonceBytes22222221);
                                            byte[] digestByt1 = Encoding.UTF8.GetBytes(digest22222221);


                                         string firstSubmitRun = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");
                                            //    File.WriteAllText(filePath3OLD, currentCounterLocationStart.ToString());
                                                File.WriteAllText(firstSubmitRun, "1");


                                            Task.Delay(5500).Wait();

                                            return SubmitSolution(address, digestByt1, challenge, difficulty, nonceByt1, sender);



                                        }

                                    }






                                    if (e.ToString().Contains("gas required exceeds allowance"))
                                    {

                                        ulong[] gasTargets = new ulong[] {
                                            250_000, 500_000, 900_000, 1_000_000, 2_000_000, 3_000_000,
                                            5_000_000, 10_000_000, 20_000_000, 30_000_000, 40_000_000,
                                            50_000_000
                                        };

                                        bool bumped = false;
                                        foreach (ulong target in gasTargets)
                                        {
                                            if (m_gasLimit < target)
                                            {
                                                m_gasLimit = target;
                                                bumped = true;
                                                break; // set to the next higher bracket and stop
                                            }
                                        }

                                        if (!bumped)
                                        {
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH MORE GAS ISN'T POSSIBLE] ❌ ❌ ❌");
                                        }
                                    }

                                    if (e.ToString().Contains("One or more errors occurred. (incorrect response body: wrong json - rpc response - there is neither result nor error: eth_estimateGas") || (e.ToString().Contains("Response status code does not indicate success: 500 (Internal Server Error)") && e.ToString().Contains("One or more errors occurred. (Error occurred when trying to send rpc requests(s): eth_estimateGas")))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }
                                    if (e.ToString().Contains("(gas limit is too high"))
                                    {

                                        Program.Print("[RESTART WITH LESS GAS]");
                                        Program.Print("[RESTART WITH LESS GAS]");

                                        (ulong threshold, ulong target)[] rules = new (ulong, ulong)[]
                                        {
                                            (50_000_000, 40_000_000),
                                            (40_000_000, 30_000_000),
                                            (30_000_000, 25_000_000),
                                            (25_000_000, 20_000_000),
                                            (20_000_000, 15_000_000),
                                            (15_000_000, 10_000_000),
                                            (10_000_000, 5_000_000),
                                            (5_000_000, 4_000_000),
                                            (4_000_000, 3_000_000),
                                            (3_000_000, 2_000_000),
                                            (2_000_000, 1_000_000),
                                            (1_000_000,   900_000)
                                        };

                                        bool lowered = false;
                                        foreach (var rule in rules)
                                        {
                                            if (m_gasLimit > rule.threshold)
                                            {
                                                m_gasLimit = rule.target;
                                                lowered = true;
                                                break; // apply the first matching (largest) threshold
                                            }
                                        }

                                        if (!lowered)
                                        {
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                            Program.Print("[RESTART WITH LESS GAS ISNT POSSIBLE]❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌");
                                        }
                                    }


                                    if (e.ToString().Contains("insufficient funds for gas") || e.ToString().Contains(" Need to send required ETH"))
                                    {
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");
                                        Program.Print("❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌  Insufficent Funds for minting, add more funds to your account to mint more ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ❌ ");

                                        Task.Delay(4500).Wait();
                                        return false;
                                    }
                                    Task.Delay(5500).Wait();


                                    var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                    byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);


                                    return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                                }


                                transaction = m_MintERC20.CreateTransactionInput(from: address,
                                                            gas: gasLimit,
                                                            gasPrice: userGas,
                                                            value: new HexBigInteger(getTotalETHowedtoSendtoContract + 1),
                                                            functionInput: dataInputERC20);



                            }
                            string encodedTx;

                            string encodedTx2;



                            var userGas2 = m_web3.Eth.GasPrice.SendRequestAsync().Result;

                            byte vvvvv = 28; // example V value
                            // Use TransactionInput.AccessList instead of List<AccessListItem>
                            List<AccessList> accessList = new List<AccessList>();
                            var maxgasPrior = new HexBigInteger(userGas2.Value / 100);
                            var maxGas = new HexBigInteger(userGas2.Value * 3 / 2);
                            var txCoun5t = new HexBigInteger(txCount.Value);
                            var gasLimit3 = new HexBigInteger(estimatedGasLimit);
                            var valueee = new HexBigInteger(getTotalETHowedtoSendtoContract + 1);//1e17


                            // Modern approach: Use TransactionInput with EIP-1559 fields
                            var transactionInput = new TransactionInput()
                            {
                                From = MinerAddress, // Your account address
                                To = m_contract.Address,
                                Value = valueee,
                                Data = transaction.Data,
                                Gas = gasLimit3,
                                MaxPriorityFeePerGas = maxgasPrior, // EIP-1559 priority fee
                                MaxFeePerGas = maxGas, // EIP-1559 max fee
                                Nonce = new HexBigInteger(txCount.Value),
                                Type = new HexBigInteger(2), // EIP-1559 transaction type
                                AccessList = accessList // Access list
                            };

                            // Create an account with your private key and chain ID
                            var account = new Nethereum.Web3.Accounts.Account(m_account.PrivateKey, m_chainID);

                            // Create Web3 instance with the account (this enables signing)
                            var web3WithSigner = new Web3(account, m_web3.Client);


                            Program.Print(string.Format("(maxGasPrice: {0}", maxGas));

                            Console.ForegroundColor = ConsoleColor.White; // Set text color to blue
                            Console.BackgroundColor = ConsoleColor.DarkBlue; // Set background color to a darker blue

                            Console.WriteLine((string.Format("[INFO] Total Multipier for this transaction is ~{0}X", totalMultiple)));
                            Console.WriteLine(string.Format("Building transaction now"));

                            Program.Print((string.Format("[INFO] Total Multipier for this transaction is ~{0}X", totalMultiple)));
                            Program.Print(string.Format("Building transaction now"));

                            Console.ResetColor(); // Reset to default colors
                            //Program.Print(string.Format("[INFO] Current Reward for Solve is {0} zkBTC", OutputtedAmount));


                            transactionID = null; // m_web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encodedTx).Result;

                            //Send data to PayMaster to mint with paymaster.
                            //wait here until PayMaster is ready to accept PayMaster Transaction





                            LastSubmitLatency = (int)((DateTime.Now - startSubmitDateTime).TotalMilliseconds);

                            // Send the signed raw transaction (synchronous)
                            //  transactionID = m_web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction).Result;
                            transactionID = web3WithSigner.Eth.TransactionManager.SendTransactionAsync(transactionInput).Result;

                            Program.Print($"🟢 🟢 🟢  Raw transaction sent: {transactionID} 🟢 🟢 🟢  ");

                            if (!string.IsNullOrWhiteSpace(transactionID))
                            {
                                Program.Print("[INFO] Nonce submitted with transaction ID: " + transactionID);
                                Program.Print("currentCounterLocationEnd: currentCounterLocationEnd: " + currentCounterLocationEnd);
                                
                                         string challengeFilePath = Path.Combine(directoryPathzfzfdzf, "challenge.dat");


                                        // Read previous challenge if file exists, otherwise use empty array
                                            byte[] previousChallenge = File.Exists(challengeFilePath) 
                                                ? File.ReadAllBytes(challengeFilePath) 
                                                : Array.Empty<byte>();



                                         string firstSubmitRunDir = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");
                                    string firstSubmitRunString = File.Exists(firstSubmitRunDir) && int.TryParse(File.ReadAllText(firstSubmitRunDir), out int value3)
                                        ? value3.ToString()
                                        : "0";

                                didRunSpecialAndNeedsToRunAgain = false;
                                if (!previousChallenge.SequenceEqual(challenge) && firstSubmitRunString != "1")
                                {
                                    File.WriteAllText(firstSubmitRunDir, "0");
                                    Task.Factory.StartNew(() => GetTransactionReciept(transactionID, address, gasLimit, userGas, LastSubmitLatency, DateTime.Now, currentCounterLocationEnd, challenge));

                                }
                                else
                                {
                                    File.WriteAllText(firstSubmitRunDir, "0");
                                    Program.Print("[Special] USING ADJUSTED TRANSACTION SUBMISSION TO SAVE OLDER SUBMISSIONS");
                                    Task.Factory.StartNew(() => GetTransactionRecieptSpecial(transactionID, address, gasLimit, userGas, LastSubmitLatency, DateTime.Now, currentCounterLocationEnd, challenge));

                                }
                                File.Delete(challengeFilePath);


                                           
                                 var attemptedExits = 0;
                                while (m_ResetIfEpochGoesUp != 3000 && attemptedExits < 18)
                                {
                                    attemptedExits = attemptedExits + 1;
                                    Program.Print("SLEEP after submit(to prevent errors)");
                                    Task.Delay(1000).Wait();
                                }

                                Task.Delay(500).Wait();
                                saveEpochNumber -= totalMultiple;
                                saveEpochNumber2 += totalMultiple;

                                if (didRunSpecialAndNeedsToRunAgain)
                                {
                                    
                                                if (blocksToReadjustmentz - totalMultiple * 10 < _BLOCKS_PER_READJUSTMENT_ / 10)
                                                {
                                                    Program.Print("SLEEP DONE after submit Time for another block, close to readjustment checking for new Challenge");

                                                    Program.Print("SLEEP DONE after submit Time for another block, close to readjustment checking for new Challenge");


                                                    Program.Print("SPECIAL UPDATE NOW FOR MINING PARAMETERS, submit again samesies");

                                                    Program.Print("SPECIAL UPDATE NOW FOR MINING PARAMETERS, submit again samesies");
                                                    CheckAndUpdateMiningParameters();

                                                    lastExecutionTime = DateTime.Now;
                                                    

                                                }

                                    var digest2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    var nonceBytes2222222 = "298482373074932023694429869006487738340224787850701197634954959663352085056";
                                    byte[] nonceByt = Encoding.UTF8.GetBytes(nonceBytes2222222);
                                    byte[] digestByt = Encoding.UTF8.GetBytes(digest2222222);


                                    return SubmitSolution(address, digestByt, challenge, difficulty, nonceByt, sender);

                                }
                                if (blocksToReadjustmentz - totalMultiple * 10 < _BLOCKS_PER_READJUSTMENT_ / 10)
                                {
                                    Program.Print("SLEEP DONE after submit Time for another block, close to readjustment checking for new Challenge");

                                    Program.Print("SLEEP DONE after submit Time for another block, close to readjustment checking for new Challenge");

                                    Program.Print("RUNNING UPDATE NOW FOR MINING PARAMETERS");

                                    Program.Print("RUNNING UPDATE NOW FOR MINING PARAMETERS");
                                    CheckAndUpdateMiningParameters();

                                    lastExecutionTime = DateTime.Now;

                                    Program.Print("DONE UPDATE  DONE FOR MINING PARAMETERS");

                                    Program.Print("DONE UPDATE  DONE FOR MINING PARAMETERS");
                                    
                                    return false;

                                }
                                else
                                {
                                    Program.Print("SLEEP DONE after submit Time for another block");
                                }
                                // OnGetMiningParameterStatus(this, true);
                                return false;
                            }
                            else
                            {
                                retryCount++;

                                if (retryCount > 2)
                                {
                                    string test2 = "[INFO] Two bad retries with no PayMaster, turning off PayMaster";
                                    OnlyRunPayMasterOnce = false;
                                    Program.Print(string.Format(test2));
                                    CheckAndUpdateMiningParameters();

                                    //OnNewChallenge(this, CurrentChallenge, MinerAddress);
                                    return false;
                                }
                                string test3 = "[INFO] Bad nonces/challenges starting over resubmitting";
                                Program.Print(string.Format(test3));
                                if (retryCount > 4)
                                {
                                    m_MaxSolvesperMint = m_MaxSolvesperMintORIGINAL;

                                    Task.Delay(500).Wait();
                                    Program.Print("Max_Mints is now : " + m_MaxSolvesperMint.ToString() + " and we are in");
                                    Task.Delay(500).Wait();

                                    Task.Delay(500).Wait();
                                    Program.Print("Adjusting Max_Mints to back to your normal mints of : " + m_MaxSolvesperMint.ToString() + " and switching to NonPayMaster Mode, try changing 'MaxSolvesperMint' variable in B0xToken.conf to a higher number");

                                    Task.Delay(500).Wait();
                                    Program.Print("Adjusting Max_Mints to back to your normal mints of : " + m_MaxSolvesperMint.ToString() + " and switching to NonPayMaster Mode, try changing 'MaxSolvesperMint' variable in B0xToken.conf to a higher number");

                                    Task.Delay(500).Wait();
                                    Program.Print("Adjusting Max_Mints to back to your normal mints of : " + m_MaxSolvesperMint.ToString() + " and switching to NonPayMaster Mode, try changing 'MaxSolvesperMint' variable in B0xToken.conf to a higher number");
                                    Task.Delay(500).Wait();

                                    string test5ff = "[INFO] Still Failling reseting counter and stuff, turning MaxSolvesperMint back to normal.";
                                    Program.Print(string.Format(test5ff));

                                    string filePathzff = "counter.txt";
                                    currentCounter = 0;
                                    File.WriteAllText(filePath4, currentCounter.ToString());
                                    retryCount = 0;

                                    m_ResetIfEpochGoesUp = 3000;
                                    OnlyRunPayMasterOnce = true;
                                    m_getChallengeNumber = m_getChallengeNumber2;
                                    m_getMiningTarget = m_getMiningTarget2;
                                    m_getMiningDifficulty = m_getMiningDifficulty2;

                                    RunThisIfExcessMints = true;
                                    digestArray2 = new byte[][] { };
                                    challengeArray2 = new byte[][] { };
                                    nonceArray2 = new byte[][] { };
                                }
                                string test5 = "[INFO] Still Failling reseting counter and stuff";
                                Program.Print(string.Format(test5));


                            }
                            string test = "[INFO] Bad nonces/challenges starting over resubmitting";
                            Program.Print(string.Format(test));





                            LastSubmitGasPrice = userGas;

                        }
                        catch (AggregateException ex)
                        {

                            HandleAggregateException(ex);
                            //OnNewChallenge(this, challenge, MinerAddress);

                            Program.Print("Invalid funds for xfer probably because we need to have fresh answers or deposit ETH on Base Blockchain into your mining account");
                           Program.Print("Invalid funds for xfer probably because we need to have fresh answers or deposit ETH on Base Blockchain into your mining account");
                            
                                Task.Delay(5500).Wait();
                                return false;
                        }


                        catch (Exception ex)
                        {

                            HandleException(ex);
                            //OnNewChallenge(this, challenge, MinerAddress);

                            Program.Print("Invalid funds for xfer probably because we need to have fresh answers or deposit ETH on Base Blockchain into your mining account");
                            Program.Print("Invalid funds for xfer probably because we need to have fresh answers or deposit ETH on Base Blockchain into your mining account");

                                Task.Delay(5500).Wait();

                            return false;

                        }

                        if (string.IsNullOrWhiteSpace(transactionID))
                        {


                            if (retryCount > 10)
                            {
                                Program.Print("[ERROR] Failed to submit solution for 50 times, submission cancelled.");
                                // OnNewChallenge(this, challenge, MinerAddress);

                                return false;
                            }
                            else { Task.Delay(m_updateInterval / 40).Wait(); }
                        }
                    } while (string.IsNullOrWhiteSpace(transactionID));

                    return !string.IsNullOrWhiteSpace(transactionID);

                }
                catch (Exception ex)
                {
                    Program.Print("[ERROR] Deep error just returning and getting out of here.");
                    Program.Print("Exception = " + ex);
                    HandleException(ex);
                    var miningParameters2fvvvvvv = GetMiningParameters2();

                    OnNewChallenge(this, miningParameters2fvvvvvv.ChallengeByte32, MinerAddress);
                    CheckAndUpdateMiningParameters();
                    return false;
                }
            }

        }

        // Add this method to check and update all mining parameters
        private void CheckAndUpdateMiningParameters()
        {
            Program.Print("[CheckAndUpdateMiningParameters] Calling CheckAndUpdateMiningParameters");
            try
            {
                var miningParameters = GetMiningParameters();
               // var miningParameters2 = GetMiningParameters2();
                if (miningParameters == null) return;

                bool hasUpdates = false;

                // Check for new challenge
                if (m_lastParameters == null || miningParameters.Challenge.Value != m_lastParameters.Challenge.Value)
                {
                    Program.Print($"[INFO] 4New challenge detected during submit check: {miningParameters.ChallengeByte32String}...");
                    OnNewChallenge(this, miningParameters.ChallengeByte32, MinerAddress);
                    CurrentChallenge = miningParameters.ChallengeByte32;
                    hasUpdates = true;

                    if (m_challengeReceiveDateTime == DateTime.MinValue)
                        m_challengeReceiveDateTime = DateTime.Now;

                    m_newChallengeResetEvent.Set();
                }

                // Check for new target
                if (m_lastParameters == null || miningParameters.MiningTarget.Value != m_lastParameters.MiningTarget.Value)
                {
                    Program.Print($"[INFO] New target detected during submit check: {miningParameters.MiningTargetByte32String}...");
                    OnNewTarget(this, miningParameters.MiningTarget);
                    CurrentTarget = miningParameters.MiningTarget;
                    hasUpdates = true;
                }

                // Check for new difficulty
                if (m_lastParameters == null || miningParameters.MiningDifficulty.Value != m_lastParameters.MiningDifficulty.Value)
                {
                    float test = (float)((double)BigInteger.Pow(2, 234) / (double)miningParameters.MiningTarget.Value);

                    // Convert to a string with high precision, then process it
                    string testStr = test.ToString("F20"); // Convert to a string with up to 20 decimal places

                    // Find the index of the first non-zero digit after the decimal point
                    int firstNonZeroIndex = testStr.IndexOfAny("123456789".ToCharArray(), testStr.IndexOf('.') + 1);

                    if (firstNonZeroIndex > -1 && test < 1)
                    {
                        // Extract up to two significant digits after the first non-zero digit
                        int endIndex = Math.Min(firstNonZeroIndex + 3, testStr.Length); // Ensure we don't go out of bounds
                        string formattedTest = testStr.Substring(0, endIndex);

                        Program.Print($"[INFO] New difficulty detected Normalized Difficulty = ({formattedTest})...");
                    }
                    else
                    {

                        double test12 = (double)BigInteger.Pow(2, 234) / (double)miningParameters.MiningTarget.Value;
                        string formattedTest12 = test12.ToString("F2"); // Formats the number to 2 decimal places

                        // Handle the unlikely case where no significant digit is found
                        Program.Print($"[INFO] New difficulty detected Normalized Difficulty = ({formattedTest12})...");
                    }



                    Program.Print(string.Format("[INFO] New difficulty detected ({0})...", miningParameters.MiningDifficulty.Value));


                    // Your existing difficulty calculation logic here
                    OnNewDifficulty?.Invoke(this, miningParameters.MiningDifficulty);
                    Difficulty = miningParameters.MiningDifficulty;
                    hasUpdates = true;

                    // Include your difficulty calculation and target update logic from UpdateMinerTimer_Elapsed
                }

                // Check for new target
                if (true)
                {
                    Program.Print($"[INFO] New blocksToReadjust detected during submit check: {miningParameters.epochToReadjustment.Value}...");

                    saveEpochNumber = miningParameters.epochToReadjustment;
                    hasUpdates = true;
                }
                


                    // Only update m_lastParameters if there were actual changes
                    if (hasUpdates)
                    {
                        m_lastParameters = miningParameters;
                    }
            }
            catch (Exception ex)
            {
                Program.Print($"[ERROR] Error checking mining parameters: {ex.Message}");
            }
        }


        public override void Dispose()
        {
            base.Dispose();

            if (m_newChallengeResetEvent != null)
                try
                {
                    m_newChallengeResetEvent.Dispose();
                    m_newChallengeResetEvent = null;
                }
                catch { }
            m_newChallengeResetEvent = null;
        }

        protected override void HashPrintTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var totalHashRate = 0ul;
            try
            {
                OnGetTotalHashrate(this, ref totalHashRate);
                Program.Print(string.Format("[INFO] Total Hashrate: {0} MH/s (Effective) / {1} MH/s (Local),",
                                            GetEffectiveHashrate() / 1000000.0f, totalHashRate / 1000000.0f));
                if (GetEffectiveHashrate() / 1000000.0f == 0)
                {
                    Program.Print(string.Format("[INFO] Total Hashrate: {0} MH/s (Effective),  If Effective stays 0 for longer than say half an averageadjustment period time, it means you didn't mine any blocks. Try lowering MinSolvesperMint & minSecondsPerAnswer & maxUSDperToken variable in B0xToken.conf file to mine blocks sooner than others.",
                                                GetEffectiveHashrate() / 1000000.0f));

                }
                else{
                
                Program.Print(string.Format("[INFO] Total Hashrate: {0} MH/s (Effective)",
                                            GetEffectiveHashrate() / 1000000.0f));

            }
            }
            catch (Exception)
            {
                try
                {
                    totalHashRate = GetEffectiveHashrate();
                    Program.Print(string.Format("[INFO] Effective Hashrate: {0} MH/s", totalHashRate / 1000000.0f));
                }
                catch { }
            }
            try
            {
                if (totalHashRate > 0)
                {
                    var timeLeftToSolveBlock = GetTimeLeftToSolveBlock(totalHashRate);

                    if (timeLeftToSolveBlock.TotalSeconds < 0)
                    {
                        Program.Print(string.Format("[INFO] Estimated time left to solution: -({0}d {1}h {2}m {3}s)",
                                                    Math.Abs(timeLeftToSolveBlock.Days),
                                                    Math.Abs(timeLeftToSolveBlock.Hours),
                                                    Math.Abs(timeLeftToSolveBlock.Minutes),
                                                    Math.Abs(timeLeftToSolveBlock.Seconds)));
                    }
                    else
                    {
                        Program.Print(string.Format("[INFO] Estimated time left to solution: {0}d {1}h {2}m {3}s",
                                                    Math.Abs(timeLeftToSolveBlock.Days),
                                                    Math.Abs(timeLeftToSolveBlock.Hours),
                                                    Math.Abs(timeLeftToSolveBlock.Minutes),
                                                    Math.Abs(timeLeftToSolveBlock.Seconds)));
                    }
                }
            }
            catch { }
        }

        protected override void UpdateMinerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var miningParameters = GetMiningParameters();
                if (miningParameters == null)
                {
                    OnGetMiningParameterStatus(this, false);
                    return;
                }

        // Add debugging to check what's null
        Program.Print($"[DEBUG] miningParameters.ChallengeByte32 is null: {miningParameters.ChallengeByte32 == null}");
        Program.Print($"[DEBUG] miningParameters.ChallengeByte32String is null: {miningParameters.ChallengeByte32String == null}");
        Program.Print($"[DEBUG] miningParameters.Challenge is null: {miningParameters.Challenge == null}");
        Program.Print($"[DEBUG] miningParameters.MiningTarget is null: {miningParameters.MiningTarget == null}");
        Program.Print($"[DEBUG] miningParameters.MiningTargetByte32String is null: {miningParameters.MiningTargetByte32String == null}");

                CurrentChallenge = miningParameters.ChallengeByte32;

                if (m_lastParameters == null || miningParameters.Challenge.Value != m_lastParameters.Challenge.Value)
                {
                    Program.Print(string.Format("[INFO] New challenge detected {0}...", miningParameters.ChallengeByte32String));

                    OnNewChallenge(this, miningParameters.ChallengeByte32, MinerAddress);

                    if (m_challengeReceiveDateTime == DateTime.MinValue)
                        m_challengeReceiveDateTime = DateTime.Now;

                    m_newChallengeResetEvent.Set();
                }

                if (m_lastParameters == null || miningParameters.MiningTarget.Value != m_lastParameters.MiningTarget.Value)
                {
                    Program.Print(string.Format("[INFO] New target detected {0}...", miningParameters.MiningTargetByte32String));
                    OnNewTarget(this, miningParameters.MiningTarget);
                    CurrentTarget = miningParameters.MiningTarget;
                }

                if (m_lastParameters == null || miningParameters.MiningDifficulty.Value != m_lastParameters.MiningDifficulty.Value)
                {

                    float test = (float)((double)BigInteger.Pow(2, 234) / (double)miningParameters.MiningTarget.Value);

                    // Convert to a string with high precision, then process it
                    string testStr = test.ToString("F20"); // Convert to a string with up to 20 decimal places

                    // Find the index of the first non-zero digit after the decimal point
                    int firstNonZeroIndex = testStr.IndexOfAny("123456789".ToCharArray(), testStr.IndexOf('.') + 1);

                    if (firstNonZeroIndex > -1 && test < 1)
                    {
                        // Extract up to two significant digits after the first non-zero digit
                        int endIndex = Math.Min(firstNonZeroIndex + 3, testStr.Length); // Ensure we don't go out of bounds
                        string formattedTest = testStr.Substring(0, endIndex);

                        Program.Print($"[INFO] New difficulty detected Normalized Difficulty = ({formattedTest})...");
                    }
                    else
                    {

                        double test12 = (double)BigInteger.Pow(2, 234) / (double)miningParameters.MiningTarget.Value;
                        string formattedTest12 = test12.ToString("F2"); // Formats the number to 2 decimal places

                        // Handle the unlikely case where no significant digit is found
                        Program.Print($"[INFO] New difficulty detected Normalized Difficulty = ({formattedTest12})...");
                    }



                    Program.Print(string.Format("[INFO] New difficulty detected ({0})...", miningParameters.MiningDifficulty.Value));
                    OnNewDifficulty?.Invoke(this, miningParameters.MiningDifficulty);
                    Difficulty = miningParameters.MiningDifficulty;

                    // Actual difficulty should have decimals
                    var calculatedDifficulty = BigDecimal.Exp(BigInteger.Log(MaxTarget.Value) - BigInteger.Log(miningParameters.MiningTarget.Value));
                    var calculatedDifficultyBigInteger = BigInteger.Parse(calculatedDifficulty.ToString().Split(",.".ToCharArray())[0]);

                  
                    try // Perform rounding
                    {
                        if (uint.Parse(calculatedDifficulty.ToString().Split(",.".ToCharArray())[1].First().ToString()) >= 5)
                            calculatedDifficultyBigInteger++;
                    }
                    catch { }

                    if (Difficulty.Value != calculatedDifficultyBigInteger)
                    {
                        Difficulty = new HexBigInteger(calculatedDifficultyBigInteger);
                        var expValue = BigInteger.Log10(calculatedDifficultyBigInteger);
                        var calculatedTarget = BigInteger.Parse(
                            (BigDecimal.Parse(MaxTarget.Value.ToString()) * BigDecimal.Pow(10, expValue) / (BigDecimal.Parse(calculatedDifficultyBigInteger.ToString()) * BigDecimal.Pow(10, expValue))).
                            ToString().Split(",.".ToCharArray())[0]);
                        var calculatedTargetHex = new HexBigInteger(calculatedTarget);

                        Program.Print(string.Format("[INFO] Update target 0x{0}...", calculatedTarget.ToString("x64")));
                        OnNewTarget(this, calculatedTargetHex);
                        CurrentTarget = calculatedTargetHex;
                    }
                }

                if (m_lastParameters == null || miningParameters.MiningDifficulty.Value != m_lastParameters.MiningDifficulty.Value)
                {
                   
                    OnNewDifficulty?.Invoke(this, miningParameters.MiningDifficulty);
                    Difficulty = miningParameters.MiningDifficulty;

                    // Actual difficulty should have decimals
                    var calculatedDifficulty = BigDecimal.Exp(BigInteger.Log(MaxTarget.Value) - BigInteger.Log(miningParameters.MiningTarget.Value));
                    var calculatedDifficultyBigInteger = BigInteger.Parse(calculatedDifficulty.ToString().Split(",.".ToCharArray())[0]);

                    try // Perform rounding
                    {
                        if (uint.Parse(calculatedDifficulty.ToString().Split(",.".ToCharArray())[1].First().ToString()) >= 5)
                            calculatedDifficultyBigInteger++;
                    }
                    catch { }

                    if (Difficulty.Value != calculatedDifficultyBigInteger)
                    {
                        Difficulty = new HexBigInteger(calculatedDifficultyBigInteger);
                        var expValue = BigInteger.Log10(calculatedDifficultyBigInteger);
                        var calculatedTarget =  BigInteger.Parse(
                            (BigDecimal.Parse(MaxTarget.Value.ToString()) * BigDecimal.Pow(10, expValue) / (BigDecimal.Parse(calculatedDifficultyBigInteger.ToString()) * BigDecimal.Pow(10, expValue))).
                            ToString().Split(",.".ToCharArray())[0]);
                        var calculatedTargetHex = new HexBigInteger(calculatedTarget);

                       Program.Print(string.Format("[INFODDDDD] Update target 0x{0}...", calculatedTarget.ToString("x64")));
                        OnNewTarget(this, calculatedTargetHex);
                        CurrentTarget = calculatedTargetHex;
                    }
                }
                  
                m_lastParameters = miningParameters;

                  
                OnGetMiningParameterStatus(this, true);
 Task.Delay(300).Wait();
                  
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion



        public Web3Interface(string web3ApiPath, string contractAddress, string minerAddress, string NFTAddresses, string ERC20Addresses, int maxAnswersPerSub, int minSecPerAnswer, decimal maxTokenPriceToPay, string privateKey, int multiplier2, int _chainID, string abc,
                             float gasToMine, string abiFileName, int updateInterval, int hashratePrintInterval,
                             ulong gasLimit, float gasPricePriority, string gasApiURL, string gasApiPath, float gasApiMultiplier, int MaxSolvesperMint, int MinSolvesperMint, float HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC, float gasApiOffset, float gasApiMax, string _mintToaddress, float CheckMinAmountIntervalInSeconds)
            : base(updateInterval, hashratePrintInterval)
        {
            Nethereum.JsonRpc.Client.ClientBase.ConnectionTimeout = TimeSpan.FromMilliseconds(MAX_TIMEOUT * 1000);
            m_gasPricePriority = gasPricePriority;
            m_mintToaddress = _mintToaddress;
            m_checkIntervalinSeconds = CheckMinAmountIntervalInSeconds;

            m_nftAddressesAndTokenIds = NFTAddresses;
            Program.Print("[INFO] Attempting to Mint these NFTs when time is right: " + m_nftAddressesAndTokenIds);

            // Parse the input string and pair addresses with token IDs

            var addressUtil = new AddressUtil();


//Program.Print("[INFO] Attempting to Mint these ERC20s when epoch is correct: " + ERC20Addresses);


try
{
    // Parse ERC20 addresses from string format "[0xAddress1, 0xAddress2]"
    var erc20AddressesList = new List<string>();
    
    // Remove brackets and split by comma
    string cleanInput = ERC20Addresses.Trim().Trim('[', ']', '"', ';');
    Program.Print($"[DEBUG] Clean ERC20 input: '{cleanInput}'");
    
    if (!string.IsNullOrEmpty(cleanInput))
    {
        string[] items = cleanInput.Split(',');
        Program.Print($"[DEBUG] Split into {items.Length} ERC20 addresses");
        
        // Process each address
        for (int i = 0; i < items.Length; i++)
        {
            string address = items[i].Trim();
            Program.Print($"[DEBUG] Processing ERC20 address {i}: '{address}'");
            
            // Validate address format
            if (address.StartsWith("0x"))
            {
                // Validate address length (should be 42 characters: 0x + 40 hex chars)
                if (address.Length == 42)
                {
                    // Validate checksum for real addresses
                    try
                    {
                        if (addressUtil.IsChecksumAddress(address))
                        {
                            erc20AddressesList.Add(address);
                            Program.Print($"[INFO] Added ERC20 address: {address}");
                        }
                        else
                        {
                            Program.Print($"[ERROR] ERC20 address {address} is not in correct checksum format");
                        }
                    }
                    catch (Exception checksumEx)
                    {
                        Program.Print($"[ERROR] Checksum validation failed for ERC20 address {address}: {checksumEx.Message}");
                    }
                }
                else
                {
                    Program.Print($"[ERROR] Invalid ERC20 address length for {address}. Should be 42 characters (0x + 40 hex)");
                }
            }
            else
            {
                Program.Print($"[WARNING] Invalid ERC20 address format: '{address}' (should start with 0x)");
            }
        }
    }
    
    // Convert to array
    ERC20AddressesToMint = erc20AddressesList.ToArray();
    
    Program.Print($"[INFO] Successfully parsed {ERC20AddressesToMint.Length} ERC20 addresses");
    if (ERC20AddressesToMint.Length > 0)
    {
        Program.Print($"[INFO] ERC20 addresses to mint: {string.Join(", ", ERC20AddressesToMint)}");
    }
}
catch (Exception ex)
{
    Program.Print($"[ERROR] Exception during ERC20 address parsing: {ex.Message}");
    Program.Print($"[ERROR] Stack trace: {ex.StackTrace}");
    
    // Initialize empty array to prevent further errors
    ERC20AddressesToMint = new string[0];
    Program.Print("[INFO] Initialized empty ERC20 addresses array due to parsing error");
}



            // Remove brackets and split by comma
          try
{
    m_nftAddressesAndTokenIds = NFTAddresses;
    Program.Print("[INFO] Attempting to Mint these NFTs when time is right: " + m_nftAddressesAndTokenIds);

    // Parse the input string and pair addresses with token IDs
    var nftAddressesList = new List<string>();
    var nftTokenIdsList = new List<string>();

    // Remove brackets and split by comma
    string cleanInput = m_nftAddressesAndTokenIds.Trim().Trim('[', ']', '"', ';');
    //Program.Print($"[DEBUG] Clean input: '{cleanInput}'");
    
    string[] items = cleanInput.Split(',');
   // Program.Print($"[DEBUG] Split into {items.Length} items");
    
    // Debug: print all items
    for (int j = 0; j < items.Length; j++)
    {
       // Program.Print($"[DEBUG] Item {j}: '{items[j].Trim()}'");
    }

    // Process items in pairs (address, tokenId)
    for (int i = 0; i < items.Length; i += 2)
    {
        //Program.Print($"[DEBUG] Processing pair starting at index {i}");
        
        if (i + 1 < items.Length)
        {
            string address = items[i].Trim();
            string tokenId = items[i + 1].Trim();
            
            //Program.Print($"[DEBUG] Address: '{address}', TokenID: '{tokenId}'");
            
            // Validate that first item is address and second is number
            if (address.StartsWith("0x") && !tokenId.StartsWith("0x"))
            {
                // Validate address length (should be 42 characters: 0x + 40 hex chars)
                if (address.Length != 42) // Allow short test addresses
                {
                    Program.Print($"[ERROR] Invalid NFT address length for {address}. Should be 42 characters (0x + 40 hex)");
                    continue;
                }
                
                // Validate tokenId is a valid number
                if (!int.TryParse(tokenId, out int tokenIdNum) || tokenIdNum < 0)
                {
                    Program.Print($"[ERROR] Invalid NFT token ID: {tokenId}. Must be a non-negative integer");
                    continue;
                }
                
                // For test addresses like 0x0, 0x1, skip checksum validation
                if (addressUtil.IsChecksumAddress(address))
                {
                    nftAddressesList.Add(address);
                    nftTokenIdsList.Add(tokenId);
                    Program.Print($"[INFO] Added NFT to mint when possible: Address={address}, TokenID={tokenId}");
                }
                else
                {
                    Program.Print($"[ERROR] Your NFT address: {address} is not correctly in checkSumFormat, reEnter it with correct formating");
                }
            }
            else
            {
                Program.Print($"[WARNING] Invalid NFT pair format: Address='{address}', TokenID='{tokenId}'");
            }
        }
        else
        {
            Program.Print($"[WARNING] Odd number of items - missing token ID for address at index {i}");
        }
    }

    // Convert to arrays
    m_nftAddresses = nftAddressesList.ToArray();
    m_nftTokenIds = nftTokenIdsList.ToArray();

    Program.Print($"[INFO] Successfully parsed {m_nftAddresses.Length} NFT pairs");
}
catch (Exception ex)
{
    Program.Print($"[ERROR] Exception during NFT parsing: {ex.Message}");
    Program.Print($"[ERROR] Stack trace: {ex.StackTrace}");
    
    // Initialize empty arrays to prevent further errors
    m_nftAddresses = new string[0];
    m_nftTokenIds = new string[0];
}



            m_maxAnswersPerSubmit = maxAnswersPerSub;
            Program.Print("[INFO] m_maxAnswersPerSubmit: "+m_maxAnswersPerSubmit);
            m_minSecondsPerAnswer = minSecPerAnswer;
            Program.Print("[INFO] m_minSecondsPerAnswer: "+m_minSecondsPerAnswer);

            m_USDperToken = maxTokenPriceToPay;
            m_newChallengeResetEvent = new System.Threading.ManualResetEvent(false);
            m_multiplier2 = multiplier2 - 1;
            Program.Print("[Multiplier] IS: "+ (m_multiplier2 + 1));
            m_chainID = _chainID;
            if (string.IsNullOrWhiteSpace(contractAddress))
            {
                Program.Print("[INFO] Contract address not specified, default Based Work Token");
                contractAddress = Config.Defaults.contractAddress;
            }

            if (!addressUtil.IsValidAddressLength(contractAddress))
                throw new Exception("Invalid contract address provided, ensure address is 42 characters long (including '0x').");

            else if (!addressUtil.IsChecksumAddress(contractAddress))
                throw new Exception("Invalid contract address provided, ensure capitalization is correct.");

            if (!addressUtil.IsValidAddressLength(m_mintToaddress) && m_mintToaddress != "")
                throw new Exception("Invalid mintToaddress address provided, ensure address is 42 characters long (including '0x').");

            else if (!addressUtil.IsChecksumAddress(m_mintToaddress) && m_mintToaddress !="")
                throw new Exception("Invalid mintToaddress address provided, ensure capitalization is correct.");

            Program.Print("[INFO] Contract address : " + contractAddress);
            Program.Print("[INFO] mintToAddress address : " + m_mintToaddress);

            if (!string.IsNullOrWhiteSpace(privateKey))
                try
                {
                    m_account = new Account(privateKey);
                    minerAddress = m_account.Address;
                }
                catch (Exception)
                {
                    throw new FormatException("Invalid private key: " + privateKey ?? string.Empty);
                }

            if (!addressUtil.IsValidAddressLength(minerAddress))
            {
                throw new Exception("Invalid miner address provided, ensure address is 42 characters long (including '0x').");
            }
            else if (!addressUtil.IsChecksumAddress(minerAddress))
            {
                throw new Exception("Invalid miner address provided, ensure capitalization is correct.");
            }

            if(m_mintToaddress == ""){
                Program.Print("m_mint_ToAddress == '' so we assign minerAddress to mintToAddress");
                m_mintToaddress = minerAddress;

            }
            MinerAddress = minerAddress;
            SubmitURL = string.IsNullOrWhiteSpace(web3ApiPath) ? Config.Defaults.InfuraAPI_mainnet : web3ApiPath;

            m_web3 = new Web3(SubmitURL);

            var erc20AbiPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "ERC-20.abi");

            if (!string.IsNullOrWhiteSpace(abiFileName))
                Program.Print(string.Format("[INFO] ABI specified, using \"{0}\"", abiFileName));
            else
            {
                Program.Print("[INFO] ABI not specified, default \"0xBTC.abi\"");
                abiFileName = Config.Defaults.AbiFile0xBTC;
            }
            var tokenAbiPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), abiFileName);

            var erc20Abi = JArray.Parse(File.ReadAllText(erc20AbiPath));
            var tokenAbi = JArray.Parse(File.ReadAllText(tokenAbiPath));
            tokenAbi.Merge(erc20Abi, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            var contractAddress_MULTICALL = "0xcA11bde05977b3631167028862bE2a173976CA11";
            m_contract_MULTICALL = m_web3.Eth.GetContract(tokenAbi.ToString(), contractAddress_MULTICALL);
            m_contract = m_web3.Eth.GetContract(tokenAbi.ToString(), contractAddress);
            var contractABI = m_contract.ContractBuilder.ContractABI;
            FunctionABI mintABI = null;
            FunctionABI mintABIwithETH = null;
            FunctionABI mintABIwithETH_ERC20Extra = null;
            FunctionABI mintNFTABI = null;
            FunctionABI mintERC20ABI = null;
            FunctionABI blocksFromReadjust = null;
            FunctionABI MULTICALL = null;
            if (string.IsNullOrWhiteSpace(privateKey)) // look for maximum target method only
            {
                if (m_MAXIMUM_TARGET == null)
                {
                    #region ERC918 methods

                    if (contractABI.Functions.Any(f => f.Name == "MAX_TARGET"))
                        m_MAXIMUM_TARGET = m_contract.GetFunction("MAX_TARGET");

                    #endregion

                    #region ABI methods checking

                    if (m_MAXIMUM_TARGET == null)
                    {
                        var maxTargetNames = new string[] { "MAX_TARGET", "MAXIMUM_TARGET", "maxTarget", "maximumTarget" };

                        // ERC541 backwards compatibility
                        if (contractABI.Functions.Any(f => f.Name == "_MAXIMUM_TARGET"))
                        {
                            m_MAXIMUM_TARGET = m_contract.GetFunction("_MAXIMUM_TARGET");
                        }
                        else
                        {
                            var maxTargetABI = contractABI.Functions.
                                                           FirstOrDefault(function =>
                                                           {
                                                               return maxTargetNames.Any(targetName =>
                                                               {
                                                                   return function.Name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) > -1;
                                                               });
                                                           });
                            if (maxTargetABI == null)
                                m_MAXIMUM_TARGET = null; // Mining still can proceed without MAX_TARGET
                            else
                            {
                                if (!maxTargetABI.OutputParameters.Any())
                                    Program.Print(string.Format("[ERROR] '{0}' function must have output parameter.", maxTargetABI.Name));

                                else if (maxTargetABI.OutputParameters[0].Type != "uint256")
                                    Program.Print(string.Format("[ERROR] '{0}' function output parameter type must be uint256.", maxTargetABI.Name));

                                else
                                    m_MAXIMUM_TARGET = m_contract.GetFunction(maxTargetABI.Name);
                            }
                        }
                    }

                    #endregion
                }
            }
            else
            {
                m_gasToMine = gasToMine;
                Program.Print(string.Format("[INFO] Gas to mine: {0} GWei", m_gasToMine));

                m_gasLimit = gasLimit;
                Program.Print(string.Format("[INFO] Gas limit: {0}", m_gasLimit));

                m_MinSolvesperMint = MinSolvesperMint;
                m_MaxSolvesperMint = MaxSolvesperMint;

                /*
                if ((int)m_gasLimit < 5_000_000 / 25000 * m_MaxSolvesperMint)
                {
                    Program.Print(string.Format("[INFO] MaxSolvesperMint: {0} we suggset MAX of 200 mints @ 5M Gas MIGHT need to increase Gas Limit or Lower m_MaxSolvesPerMint, if stuck with too many answer adjust maxAnswersPerSubmit in conf file", MaxSolvesperMint));
                    Program.Print(string.Format("[INFO] MaxSolvesperMint: {0} we suggset MAX of 200 mints @ 5M Gas MIGHT need to increase Gas Limit or Lower m_MaxSolvesPerMint, if stuck with too many answer adjust maxAnswersPerSubmit in conf file", MaxSolvesperMint));
                    if (m_MaxSolvesperMint > 2200)
                    {
                        m_MaxSolvesperMint = (int)m_gasLimit / 25000;
                    }
                    if (m_MinSolvesperMint > 2200)
                    {
                        m_MinSolvesperMint = (int)m_gasLimit / 50000;
                    }
                }   
                */




                m_MaxSolvesperMintORIGINAL = MaxSolvesperMint;
               
                Program.Print(string.Format("[INFO] Minimum Solutions per Mint: {0}", m_MinSolvesperMint));
                if (!string.IsNullOrWhiteSpace(gasApiURL))
                {
                    m_gasApiURL = gasApiURL;
                    Program.Print(string.Format("[INFO] Gas API URL: {0}", m_gasApiURL));

                    m_gasApiPath = gasApiPath;
                    Program.Print(string.Format("[INFO] Gas API path: {0}", m_gasApiPath));

                    m_gasApiOffset = gasApiOffset;
                    Program.Print(string.Format("[INFO] Gas API offset: {0}", m_gasApiOffset));

                    m_gasApiMultiplier = gasApiMultiplier;
                    Program.Print(string.Format("[INFO] Gas API multiplier: {0}", m_gasApiMultiplier));

                    m_gasApiMax = gasApiMax;
                    Program.Print(string.Format("[INFO] Gas API maximum: {0} GWei", m_gasApiMax));
                }
               //m_ETHwithMints = UsePayMasterz;
               // m_ETHwithMints_Paymaster_address = UsePayMasterz_addy;
             //   Program.Print(string.Format("[INFO] Minting ETH from contract: {0}", m_ETHwithMints));
            //    Program.Print(string.Format("[INFO] PAYMASTER CONTRACT: {0}", m_ETHwithMints_Paymaster_address));


                #region ERC20 methods

                m_transferMethod = m_contract.GetFunction("transfer");

                #endregion
                #region ERC918-B methods
                mintABI = contractABI.Functions.FirstOrDefault(f => f.Name == "multi_MintTo");
                if (mintABI != null) m_mintMethod = m_contract.GetFunction(mintABI.Name);
                mintABIwithETH = contractABI.Functions.FirstOrDefault(f => f.Name == "multi_MintTo");
                if (mintABI != null) m_mintMethod = m_contract.GetFunction(mintABI.Name);

                mintNFTABI = contractABI.Functions.FirstOrDefault(f => f.Name == "mintNFT");
                if (mintNFTABI != null) m_mintNFTMethod = m_contract.GetFunction(mintNFTABI.Name);

                mintNFTABI = contractABI.Functions.FirstOrDefault(f => f.Name == "mintNFT");
                if (mintNFTABI != null) m_mintNFTMethod = m_contract.GetFunction(mintNFTABI.Name);


                mintERC20ABI = contractABI.Functions.FirstOrDefault(f => f.Name == "mintTokensSameAddress");
                if (mintERC20ABI != null) m_MintERC20 = m_contract.GetFunction(mintERC20ABI.Name);




                m_Multicall = m_contract_MULTICALL.GetFunction("aggregate3");
           

            
                blocksFromReadjust = contractABI.Functions.FirstOrDefault(f => f.Name == "blocksFromReadjust");
                if (blocksFromReadjust != null) m_blocksFromReadjust = m_contract.GetFunction("blocksFromReadjust");

                if (contractABI.Functions.Any(f => f.Name == "reAdjustsToWhatDifficulty_MaxPain_Target"))
                    m_getETH2SEND = m_contract.GetFunction("reAdjustsToWhatDifficulty_MaxPain_Target");
                if (contractABI.Functions.Any(f => f.Name == "getMiningDifficulty"))
                    m_getMiningDifficulty = m_contract.GetFunction("getMiningDifficulty");
                if (contractABI.Functions.Any(f => f.Name == "getMiningDifficulty"))
                    m_getMiningDifficulty2 = m_contract.GetFunction("getMiningDifficulty");
                if (contractABI.Functions.Any(f => f.Name == "reAdjustsToWhatDifficulty_MaxPain_Difficulty"))
                    m_getMiningDifficulty22 = m_contract.GetFunction("reAdjustsToWhatDifficulty_MaxPain_Difficulty");

                if (contractABI.Functions.Any(f => f.Name == "reAdjustsToWhatDifficulty_MaxPain_Difficulty"))
                    m_getMiningDifficulty22Static = m_contract.GetFunction("reAdjustsToWhatDifficulty_MaxPain_Difficulty");

                if (contractABI.Functions.Any(f => f.Name == "mintNFTGO"))
                    m_getMiningDifficulty3 = m_contract.GetFunction("mintNFTGO");
                if (contractABI.Functions.Any(f => f.Name == "mintNFTGOBlocksUntil"))
                    m_getMiningDifficulty4 = m_contract.GetFunction("mintNFTGOBlocksUntil");
                if (contractABI.Functions.Any(f => f.Name == "epochCount"))
                    m_getEpoch = m_contract.GetFunction("epochCount");

                if (contractABI.Functions.Any(f => f.Name == "blocksToReadjust"))
                    m_getEpochOld = m_contract.GetFunction("blocksToReadjust");

                if (contractABI.Functions.Any(f => f.Name == "getMiningTarget"))
                    m_getMiningTarget2 = m_contract.GetFunction("getMiningTarget");
                if (contractABI.Functions.Any(f => f.Name == "reAdjustsToWhatDifficulty_MaxPain_Target"))
                    m_getMiningTarget23Static = m_contract.GetFunction("reAdjustsToWhatDifficulty_MaxPain_Target");
                if (contractABI.Functions.Any(f => f.Name == "seconds_Until_adjustmentSwitch"))
                    m_getSecondsUntilAdjustment = m_contract.GetFunction("seconds_Until_adjustmentSwitch");
                if (contractABI.Functions.Any(f => f.Name == "seconds_Until_adjustmentSwitch"))
                    m_getSecondsUntilAdjustment = m_contract.GetFunction("seconds_Until_adjustmentSwitch");

                if (contractABI.Functions.Any(f => f.Name == "getMiningTarget"))
                    m_getMiningTarget = m_contract.GetFunction("getMiningTarget");
                /*
                if (contractABI.Functions.Any(f => f.Name == "getMultiMintChallengeNumber"))
                    m_getChallengeNumber = m_contract.GetFunction("getMultiMintChallengeNumber");
                if (contractABI.Functions.Any(f => f.Name == "mint"))
                    m_getPaymaster = m_contract.GetFunction("mint");
                if (contractABI.Functions.Any(f => f.Name == "getMultiMintChallengeNumber"))
                    m_getChallengeNumber2 = m_contract.GetFunction("getMultiMintChallengeNumber");
                if (contractABI.Functions.Any(f => f.Name == "getChallengeNumber"))
                    m_getChallengeNumber2Static = m_contract.GetFunction("getChallengeNumber");
                */
                if (contractABI.Functions.Any(f => f.Name == "getChallengeNumber"))
                    m_getChallengeNumber = m_contract.GetFunction("getChallengeNumber");
                if (contractABI.Functions.Any(f => f.Name == "mint"))
                    m_getPaymaster = m_contract.GetFunction("mint");
                if (contractABI.Functions.Any(f => f.Name == "getChallengeNumber"))
                    m_getChallengeNumber2 = m_contract.GetFunction("getChallengeNumber");
                if (contractABI.Functions.Any(f => f.Name == "getChallengeNumber"))
                    m_getChallengeNumber2Static = m_contract.GetFunction("getChallengeNumber");

                if (contractABI.Functions.Any(f => f.Name == "getMiningReward"))
                    m_getMiningReward = m_contract.GetFunction("getMiningReward");
                if (contractABI.Functions.Any(f => f.Name == "TotalTotal_EK_ETHyouGet_EthyouSpend_miner")){

                    m_getCosts = m_contract.GetFunction("TotalTotal_EK_ETHyouGet_EthyouSpend_miner");
                        Program.Print("m_getCosts Function retrieved successfully");
                }else{
                        Program.Print("Function TotalTotal_EK_ETHyouGet_EthyouSpend_miner not found in contract ABI");
                        m_getCosts = null;
                    }
                


                if (contractABI.Functions.Any(f => f.Name == "TotalTotalsAndETHpriceAndCurrentMintTime")){

                    m_getCostsALL = m_contract.GetFunction("TotalTotalsAndETHpriceAndCurrentMintTime");
                        Program.Print("m_getCostsALL Function retrieved successfully");
                }else{
                        Program.Print("Function TotalTotal_EK_ETHyouGet_EthyouSpend not found in contract ABI");
                        m_getCostsALL = null;
                    }
                




                if (contractABI.Functions.Any(f => f.Name == "getBlockInfo")){

                    m_getBlockInfo = m_contract.GetFunction("getBlockInfo");
                        Program.Print("m_getBlockInfo Function retrieved successfully");
                }else{
                        Program.Print("Function getBlockInfo not found in contract ABI");
                        m_getBlockInfo = null;
                    }
                



                var m_getBlocks_PER = m_contract.GetFunction("_BLOCKS_PER_READJUSTMENT");
                var blocksPerReadjustmentTotal = new HexBigInteger(m_getBlocks_PER.CallAsync<BigInteger>().Result);
                var fMiningTargetByte32String = (int)blocksPerReadjustmentTotal.Value; 

                Program.Print("BLOCKS PER READJSTMENT "+ fMiningTargetByte32String);
                _BLOCKS_PER_READJUSTMENT_ = (int)blocksPerReadjustmentTotal.Value - 1;
                m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC = HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC;
                if (m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC < -1 * ((int)blocksPerReadjustmentTotal.Value - 1) )
                {
                    m_HowManyBlocksAWAYFromAdjustmentToSendMinimumZKBTC = -1 * ((int)blocksPerReadjustmentTotal.Value - 1);
                }
                Task.Delay(0000).Wait();
                #endregion

                #region ERC918 methods

                if (contractABI.Functions.Any(f => f.Name == "MAX_TARGET"))
                    m_MAXIMUM_TARGET = m_contract.GetFunction("MAX_TARGET");

                #endregion

                #region CLM MN/POW methods

                if (contractABI.Functions.Any(f => f.Name == "contractProgress"))
                    m_CLM_ContractProgress = m_contract.GetFunction("contractProgress");

                if (m_CLM_ContractProgress != null)
                    m_getMiningReward = null; // Do not start mining if cannot get POW reward value, exception will be thrown later

                #endregion

                #region ABI methods checking

                if (m_mintMethod == null)
                {
                    mintABI = contractABI.Functions.
                                          FirstOrDefault(f => f.Name.IndexOf("mint", StringComparison.OrdinalIgnoreCase) > -1);
                    if (mintABI == null)
                        throw new InvalidOperationException("'mint' function not found, mining cannot proceed.");

                    else if (!mintABI.InputParameters.Any())
                        throw new InvalidOperationException("'mint' function must have input parameter, mining cannot proceed.");

                    
                    m_mintMethod = m_contract.GetFunction(mintABI.Name);
                }
                if (m_mintMethodwithETH == null)
                {
                    mintABIwithETH = contractABI.Functions.
                                          FirstOrDefault(f => f.Name.IndexOf("mint", StringComparison.OrdinalIgnoreCase) > -1);
                    if (mintABI == null)
                        throw new InvalidOperationException("'mint' function not found, mining cannot proceed.");

                    else if (!mintABI.InputParameters.Any())
                        throw new InvalidOperationException("'mint' function must have input parameter, mining cannot proceed.");

                    m_mintMethodwithETH = m_contract.GetFunction(mintABIwithETH.Name);
                }

                if (m_MintERC20 == null)
                {
                    mintERC20ABI = contractABI.Functions.
                                          FirstOrDefault(f => f.Name.IndexOf("mintTokensSameAddress", StringComparison.OrdinalIgnoreCase) > -1);
                    if (mintERC20ABI == null)
                        throw new InvalidOperationException("'mint' function not found, mining cannot proceed.");

                    else if (!mintERC20ABI.InputParameters.Any())
                        throw new InvalidOperationException("'mint' function must have input parameter, mining cannot proceed.");

                    
                    m_MintERC20 = m_contract.GetFunction(mintERC20ABI.Name);
                }
                if (m_NFTmintMethod == null)
                {
                    mintNFTABI = contractABI.Functions.
                                          FirstOrDefault(f => f.Name.IndexOf("mintNFT", StringComparison.OrdinalIgnoreCase) > -1);
                    if (mintNFTABI == null)
                        throw new InvalidOperationException("'mint' function not found, mining cannot proceed.");

                    else if (!mintNFTABI.InputParameters.Any())
                        throw new InvalidOperationException("'mint' function must have input parameter, mining cannot proceed.");

                   
                    m_NFTmintMethod = m_contract.GetFunction(mintNFTABI.Name);
                }
                if (m_getMiningDifficulty == null)
                {
                    var miningDifficultyABI = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("miningDifficulty", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI == null)
                        miningDifficultyABI = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("mining_difficulty", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI == null)
                        throw new InvalidOperationException("'miningDifficulty' function not found, mining cannot proceed.");

                    else if (!miningDifficultyABI.OutputParameters.Any())
                        throw new InvalidOperationException("'miningDifficulty' function must have output parameter, mining cannot proceed.");

                    else if (miningDifficultyABI.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningDifficulty' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningDifficulty = m_contract.GetFunction(miningDifficultyABI.Name);
                }


                if (m_getMiningDifficulty2 == null)
                {
                    var miningDifficultyABI2 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("rewardAtCurrentTime", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI2 == null)
                        miningDifficultyABI2 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("rewardAtCurrentTime", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI2 == null)
                        throw new InvalidOperationException("'miningDifficulty' function not found, mining cannot proceed.");

                    else if (!miningDifficultyABI2.OutputParameters.Any())
                        throw new InvalidOperationException("'miningDifficulty' function must have output parameter, mining cannot proceed.");

                    else if (miningDifficultyABI2.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningDifficulty' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningDifficulty2 = m_contract.GetFunction(miningDifficultyABI2.Name);
                }

                if (m_getMiningDifficulty3 == null)
                {
                    var miningDifficultyABI3 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("mintNFTGO", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI3 == null)
                        miningDifficultyABI3 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("rewardAtCurrentTime", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI3 == null)
                        throw new InvalidOperationException("'miningDifficulty' function not found, mining cannot proceed.");

                    else if (!miningDifficultyABI3.OutputParameters.Any())
                        throw new InvalidOperationException("'miningDifficulty' function must have output parameter, mining cannot proceed.");

                    else if (miningDifficultyABI3.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningDifficulty' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningDifficulty3 = m_contract.GetFunction(miningDifficultyABI3.Name);
                }


                if (m_getMiningDifficulty4 == null)
                {
                    var miningDifficultyABI4 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("mintNFTGOBlocksUntil", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI4 == null)
                        miningDifficultyABI4 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("rewardAtCurrentTime", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningDifficultyABI4 == null)
                        throw new InvalidOperationException("'miningDifficulty' function not found, mining cannot proceed.");

                    else if (!miningDifficultyABI4.OutputParameters.Any())
                        throw new InvalidOperationException("'miningDifficulty' function must have output parameter, mining cannot proceed.");

                    else if (miningDifficultyABI4.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningDifficulty' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningDifficulty4 = m_contract.GetFunction(miningDifficultyABI4.Name);
                }

                if (m_getEpoch == null)
                {
                    var miningEpochABI4 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("epochCount", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningEpochABI4 == null)
                        miningEpochABI4 = contractABI.Functions.
                                                          FirstOrDefault(f => f.Name.IndexOf("epochCount", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningEpochABI4 == null)
                        throw new InvalidOperationException("'miningDifficulty' function not found, mining cannot proceed.");

                    else if (!miningEpochABI4.OutputParameters.Any())
                        throw new InvalidOperationException("'miningDifficulty' function must have output parameter, mining cannot proceed.");

                    else if (miningEpochABI4.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningDifficulty' function output parameter type must be uint256, mining cannot proceed.");

                    m_getEpoch = m_contract.GetFunction(miningEpochABI4.Name);
                }


                if (m_getMiningTarget == null)
                {
                    var miningTargetABI = contractABI.Functions.
                                                      FirstOrDefault(f => f.Name.IndexOf("miningTarget", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningTargetABI == null)
                        miningTargetABI = contractABI.Functions.
                                                      FirstOrDefault(f => f.Name.IndexOf("mining_target", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningTargetABI == null)
                        throw new InvalidOperationException("'miningTarget' function not found, mining cannot proceed.");

                    else if (!miningTargetABI.OutputParameters.Any())
                        throw new InvalidOperationException("'miningTarget' function must have output parameter, mining cannot proceed.");

                    else if (miningTargetABI.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningTarget' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningTarget = m_contract.GetFunction(miningTargetABI.Name);
                }

                if (m_getChallengeNumber == null)
                {
                    var challengeNumberABI = contractABI.Functions.
                                                         FirstOrDefault(f => f.Name.IndexOf("challengeNumber", StringComparison.OrdinalIgnoreCase) > -1);
                    if (challengeNumberABI == null)
                        challengeNumberABI = contractABI.Functions.
                                                         FirstOrDefault(f => f.Name.IndexOf("challenge_number", StringComparison.OrdinalIgnoreCase) > -1);
                    if (challengeNumberABI == null)
                        throw new InvalidOperationException("'challengeNumber' function not found, mining cannot proceed.");

                    else if (!challengeNumberABI.OutputParameters.Any())
                        throw new InvalidOperationException("'challengeNumber' function must have output parameter, mining cannot proceed.");

                    else if (challengeNumberABI.OutputParameters[0].Type != "bytes32")
                        throw new InvalidOperationException("'challengeNumber' function output parameter type must be bytes32, mining cannot proceed.");

                    m_getChallengeNumber = m_contract.GetFunction(challengeNumberABI.Name);
                }

                if (m_getMiningReward == null)
                {
                    var miningRewardABI = contractABI.Functions.
                                                      FirstOrDefault(f => f.Name.IndexOf("miningReward", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningRewardABI == null)
                        miningRewardABI = contractABI.Functions.
                                                      FirstOrDefault(f => f.Name.IndexOf("mining_reward", StringComparison.OrdinalIgnoreCase) > -1);
                    if (miningRewardABI == null)
                        throw new InvalidOperationException("'miningReward' function not found, mining cannot proceed.");

                    else if (!miningRewardABI.OutputParameters.Any())
                        throw new InvalidOperationException("'miningReward' function must have output parameter, mining cannot proceed.");

                    else if (miningRewardABI.OutputParameters[0].Type != "uint256")
                        throw new InvalidOperationException("'miningReward' function output parameter type must be uint256, mining cannot proceed.");

                    m_getMiningReward = m_contract.GetFunction(miningRewardABI.Name);
                }

                if (m_MAXIMUM_TARGET == null)
                {
                    var maxTargetNames = new string[] { "MAX_TARGET", "MAXIMUM_TARGET", "maxTarget", "maximumTarget" };

                    // ERC541 backwards compatibility
                    if (contractABI.Functions.Any(f => f.Name == "_MAXIMUM_TARGET"))
                    {
                        m_MAXIMUM_TARGET = m_contract.GetFunction("_MAXIMUM_TARGET");
                    }
                    else
                    {
                        var maxTargetABI = contractABI.Functions.
                                                       FirstOrDefault(function =>
                                                       {
                                                           return maxTargetNames.Any(targetName =>
                                                           {
                                                               return function.Name.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) > -1;
                                                           });
                                                       });
                        if (maxTargetABI == null)
                            m_MAXIMUM_TARGET = null; // Mining still can proceed without MAX_TARGET
                        else
                        {
                            if (!maxTargetABI.OutputParameters.Any())
                                Program.Print(string.Format("[ERROR] '{0}' function must have output parameter.", maxTargetABI.Name));

                            else if (maxTargetABI.OutputParameters[0].Type != "uint256")
                                Program.Print(string.Format("[ERROR] '{0}' function output parameter type must be uint256.", maxTargetABI.Name));

                            else
                                m_MAXIMUM_TARGET = m_contract.GetFunction(maxTargetABI.Name);
                        }
                    }
                }

                m_mintMethodInputParamCount = mintABI?.InputParameters.Count() ?? 0;

                #endregion

                if (m_hashPrintTimer != null)
                    m_hashPrintTimer.Start();
            }
        }

        public void OverrideMaxTarget(HexBigInteger maxTarget)
        {
            if (maxTarget.Value > 0u)
            {
                Program.Print("[INFO] Override maximum difficulty: " + maxTarget.HexValue);
                MaxTarget = maxTarget;
            }
            else { MaxTarget = GetMaxTarget(); }
        }

        public HexBigInteger GetMaxTarget()
        {
            if (MaxTarget != null && MaxTarget.Value > 0)
                return MaxTarget;

            Program.Print("[INFO] Checking maximum target from network...");
            while (true)
            {
                try
                {
                    if (m_MAXIMUM_TARGET == null) // assume the same as 0xBTC
                        return new HexBigInteger("0x40000000000000000000000000000000000000000000000000000000000");

                    var maxTarget = new HexBigInteger(m_MAXIMUM_TARGET.CallAsync<BigInteger>().Result);

                    if (maxTarget.Value > 0)
                        return maxTarget;
                    else
                        throw new InvalidOperationException("Network returned maximum target of zero.");
                }
                catch (Exception ex)
                {
                    HandleException(ex, "zzzFailed to get maximum target");
                    Task.Delay(m_updateInterval / 2).Wait();
                }
            }
        }

        private MiningParameters GetMiningParameters()
        {
            Program.Print("[INFO] Checking latest parameters from network...");

            //Program.Print("[multiplier] IS: " + (m_multiplier2 + 1));
            var success = true;
            var startTime = DateTime.Now;
            try
            {
                return MiningParameters.GetSoloMiningParameters(MinerAddress, m_getMiningDifficulty, m_Multicall, m_getMiningTarget, m_getChallengeNumber, m_multiplier2);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                success = false;
                return null;
            }
            finally
            {
                if (success)
                {
                    var tempLatency = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var submitUrl = SubmitURL.Contains("://") ? SubmitURL.Split(new string[] { "://" }, StringSplitOptions.None)[1] : SubmitURL;
                            try
                            {
                                var response = ping.Send(submitUrl);
                                if (response.RoundtripTime > 0)
                                    tempLatency = (int)response.RoundtripTime;
                            }
                            catch
                            {
                                try
                                {
                                    submitUrl = submitUrl.Split('/').First();
                                    var response = ping.Send(submitUrl);
                                    if (response.RoundtripTime > 0)
                                        tempLatency = (int)response.RoundtripTime;
                                }
                                catch
                                {
                                    try
                                    {
                                        submitUrl = submitUrl.Split(':').First();
                                        var response = ping.Send(submitUrl);
                                        if (response.RoundtripTime > 0)
                                            tempLatency = (int)response.RoundtripTime;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    Latency = tempLatency;
                }
            }
        }

        private MiningParameters2 GetMiningParameters2()
        {
            //Program.Print("[INFO] Checking latest parameters from network...");
            var success = true;
            var startTime = DateTime.Now;
            try
            {
                return MiningParameters2.GetSoloMiningParameters2(MinerAddress,m_getMiningDifficulty2,m_getChallengeNumber, m_multiplier2);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                success = false;
                return null;
            }
            finally
            {
                if (success)
                {
                    var tempLatency = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var submitUrl = SubmitURL.Contains("://") ? SubmitURL.Split(new string[] { "://" }, StringSplitOptions.None)[1] : SubmitURL;
                            try
                            {
                                var response = ping.Send(submitUrl);
                                if (response.RoundtripTime > 0)
                                    tempLatency = (int)response.RoundtripTime;
                            }
                            catch
                            {
                                try
                                {
                                    submitUrl = submitUrl.Split('/').First();
                                    var response = ping.Send(submitUrl);
                                    if (response.RoundtripTime > 0)
                                        tempLatency = (int)response.RoundtripTime;
                                }
                                catch
                                {
                                    try
                                    {
                                        submitUrl = submitUrl.Split(':').First();
                                        var response = ping.Send(submitUrl);
                                        if (response.RoundtripTime > 0)
                                            tempLatency = (int)response.RoundtripTime;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    Latency = tempLatency;
                }
            }
        }

        private MiningParameters3 GetMiningParameters3()
        {
            //Program.Print("[INFO] Checking latest parameters from network...");
            var success = true;
            var startTime = DateTime.Now;
            try
            {
                return MiningParameters3.GetSoloMiningParameters3(MinerAddress, m_getMiningDifficulty3, m_getMiningDifficulty4, m_getChallengeNumber);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                success = false;
                return null;
            }
            finally
            {
                if (success)
                {
                    var tempLatency = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var submitUrl = SubmitURL.Contains("://") ? SubmitURL.Split(new string[] { "://" }, StringSplitOptions.None)[1] : SubmitURL;
                            try
                            {
                                var response = ping.Send(submitUrl);
                                if (response.RoundtripTime > 0)
                                    tempLatency = (int)response.RoundtripTime;
                            }
                            catch
                            {
                                try
                                {
                                    submitUrl = submitUrl.Split('/').First();
                                    var response = ping.Send(submitUrl);
                                    if (response.RoundtripTime > 0)
                                        tempLatency = (int)response.RoundtripTime;
                                }
                                catch
                                {
                                    try
                                    {
                                        submitUrl = submitUrl.Split(':').First();
                                        var response = ping.Send(submitUrl);
                                        if (response.RoundtripTime > 0)
                                            tempLatency = (int)response.RoundtripTime;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    Latency = tempLatency;
                }
            }
        }



        private MiningParameters4 GetMiningParameters4()
        {
            //Program.Print("[INFO] Checking latest parameters from network...");
            var success = true;
            var startTime = DateTime.Now;
            try
            {
                return MiningParameters4.GetSoloMiningParameters4(MinerAddress, m_getEpoch, m_getETH2SEND, m_getChallengeNumber);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                success = false;
                return null;
            }
            finally
            {
                if (success)
                {
                    var tempLatency = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var submitUrl = SubmitURL.Contains("://") ? SubmitURL.Split(new string[] { "://" }, StringSplitOptions.None)[1] : SubmitURL;
                            try
                            {
                                var response = ping.Send(submitUrl);
                                if (response.RoundtripTime > 0)
                                    tempLatency = (int)response.RoundtripTime;
                            }
                            catch
                            {
                                try
                                {
                                    submitUrl = submitUrl.Split('/').First();
                                    var response = ping.Send(submitUrl);
                                    if (response.RoundtripTime > 0)
                                        tempLatency = (int)response.RoundtripTime;
                                }
                                catch
                                {
                                    try
                                    {
                                        submitUrl = submitUrl.Split(':').First();
                                        var response = ping.Send(submitUrl);
                                        if (response.RoundtripTime > 0)
                                            tempLatency = (int)response.RoundtripTime;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    Latency = tempLatency;
                }
            }
        }


        private MiningParameters4 GetMiningParameters5()
        {
            //Program.Print("[INFO] Checking latest parameters from network...");
            var success = true;
            var startTime = DateTime.Now;
            try
            {
                return MiningParameters4.GetSoloMiningParameters4(MinerAddress, m_getEpochOld, m_getMiningTarget2, m_getChallengeNumber);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                success = false;
                return null;
            }
            finally
            {
                if (success)
                {
                    var tempLatency = (int)(DateTime.Now - startTime).TotalMilliseconds;
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var submitUrl = SubmitURL.Contains("://") ? SubmitURL.Split(new string[] { "://" }, StringSplitOptions.None)[1] : SubmitURL;
                            try
                            {
                                var response = ping.Send(submitUrl);
                                if (response.RoundtripTime > 0)
                                    tempLatency = (int)response.RoundtripTime;
                            }
                            catch
                            {
                                try
                                {
                                    submitUrl = submitUrl.Split('/').First();
                                    var response = ping.Send(submitUrl);
                                    if (response.RoundtripTime > 0)
                                        tempLatency = (int)response.RoundtripTime;
                                }
                                catch
                                {
                                    try
                                    {
                                        submitUrl = submitUrl.Split(':').First();
                                        var response = ping.Send(submitUrl);
                                        if (response.RoundtripTime > 0)
                                            tempLatency = (int)response.RoundtripTime;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                    catch { }
                    Latency = tempLatency;
                }
            }
        }
        


        private void GetTransactionReciept(string transactionID, string address, HexBigInteger gasLimit, HexBigInteger userGas,
                                           int responseTime, DateTime submitDateTime, int counterzsLast, byte[] challenge)
        {
            try
            {
                var attempts = 0;
                var success = false;
                var hasWaited = false;
                var reciept = m_web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionID).Result;
                if (transactionID == "0x750207aedaaf9abb7d485de5bcdec289a7ab4a58dddd6bbddbed8089ec289111")
                {
                    Program.Print(string.Format("[INFO] We submitted a repeat block, throwing out old answers."));



                    if (m_submitDateTimeList.Count >= MAX_SUBMIT_DTM_COUNT)
                        m_submitDateTimeList.RemoveAt(0);

                    m_submitDateTimeList.Add(submitDateTime);

                    string filePathz = "counter.txt";
                    int currentCounter = 0;
                    File.WriteAllText(filePathz, currentCounter.ToString());
                    retryCount = 0;

                    RunThisIfExcessMints = true;
                    m_ResetIfEpochGoesUp = 3000;
                    OnlyRunPayMasterOnce = true;
                    m_getChallengeNumber = m_getChallengeNumber2;
                    m_getMiningTarget = m_getMiningTarget2;
                    m_getMiningDifficulty = m_getMiningDifficulty2;

                    digestArray2 = new byte[][] { };
                    challengeArray2 = new byte[][] { };
                    nonceArray2 = new byte[][] { };

                    //var devFee = (ulong)Math.Round(100 / Math.Abs(DevFee.UserPercent));

                    //if (((SubmittedShares - RejectedShares) % devFee) == 0)
                    //SubmitDevFee(address, gasLimit, userGas, SubmittedShares);
                    return;
                }
                do
                {
                    attempts = attempts + 1;
                    Program.Print("attempts " + attempts);

                    reciept = m_web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionID).Result;

                    if (reciept == null)
                    {
                        if (hasWaited) Task.Delay(1000).Wait();
                        else
                        {
                            m_newChallengeResetEvent.Reset();
                            m_newChallengeResetEvent.WaitOne(1000 * 2);
                            hasWaited = true;
                        }
                    }
                    else
                    {
                        // Program.Print("reciept" + reciept);
                        // Program.Print("reciept.Status.Value" + reciept.Status.Value);
                        // Program.Print("reciept.Status" + reciept.Status);
                        //  Program.Print("reciept" + reciept);
                        if (hasWaited && reciept.BlockNumber.Value == 0) { Task.Delay(100).Wait(); }
                        else { hasWaited = true; }
                    }
                } while (reciept == null && attempts < 120);

                success = (reciept.Status.Value == 1);

                if (!success) RejectedShares++;

                if (SubmittedShares == ulong.MaxValue)
                {
                    SubmittedShares = 0ul;
                    RejectedShares = 0ul;
                }
                else SubmittedShares++;

                Console.ForegroundColor = ConsoleColor.White; // Set text color to blue
                Console.BackgroundColor = ConsoleColor.DarkBlue; // Set background color to a darker blue

                Console.WriteLine(string.Format("🏁🏁🏁🏁[INFO] Miner share [{0}] submitted: {1} ({2}ms), block: {3}, transaction ID: {4}    🏁🏁🏁🏁",
                                                SubmittedShares,
                                                success ? "success" : "failed",
                                                responseTime,
                                                reciept.BlockNumber.Value,
                                                reciept.TransactionHash));
                Program.Print(string.Format("🏁🏁🏁🏁[INFO] Miner share [{0}] submitted: {1} ({2}ms), block: {3}, transaction ID: {4}    🏁🏁🏁🏁",
                                            SubmittedShares,
                                            success ? "success" : "failed",
                                            responseTime,
                                            reciept.BlockNumber.Value,
                                            reciept.TransactionHash));

                Console.ResetColor(); // Reset to default colors
                if (success)
                {
                    skipThisMany = 0;
                    if (m_submitDateTimeList.Count >= MAX_SUBMIT_DTM_COUNT)
                        m_submitDateTimeList.RemoveAt(0);

                    m_submitDateTimeList.Add(submitDateTime);

                    retryCount = 0;


                    errorGreaterThan5 = 0;

                    // Specify the file path
                    string originalChallengeStringzf = BitConverter.ToString(challenge).Replace("-", "");
                    string directoryPathzfzfdzf = Path.Combine("solveData-", originalChallengeStringzf);
                    Directory.CreateDirectory(directoryPathzfzfdzf);

                    string filePathzLocationStart = "counterLocationStart.txt";

                    // Construct the file path
                    //console.log(")
                    string filePath2f = Path.Combine(directoryPathzfzfdzf, filePathzLocationStart);
                    // Construct the file path
                    runOnceThenResetAfter = false;
                    File.WriteAllText(filePath2f, counterzsLast.ToString());
                    Program.Print(string.Format("filePath2f [{0}] counterzsLast: {1} 🏁",
                                                  filePath2f,
                                                  counterzsLast));
                    RunThisIfExcessMints = true;
                    OnlyRunPayMasterOnce = true;
                    m_getChallengeNumber = m_getChallengeNumber2;
                    m_getMiningTarget = m_getMiningTarget2;
                    m_getMiningDifficulty = m_getMiningDifficulty2;

                    m_ResetIfEpochGoesUp = 3000;
                    digestArray2 = new byte[][] { };
                    challengeArray2 = new byte[][] { };
                    nonceArray2 = new byte[][] { };

                    //var devFee = (ulong)Math.Round(100 / Math.Abs(DevFee.UserPercent));

                    //if (((SubmittedShares - RejectedShares) % devFee) == 0)
                    //SubmitDevFee(address, gasLimit, userGas, SubmittedShares);
                }



                //UpdateMinerTimer_Elapsed(this, null);
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }



        private void GetTransactionRecieptSpecial(string transactionID, string address, HexBigInteger gasLimit, HexBigInteger userGas,
                                           int responseTime, DateTime submitDateTime, int counterzsLast, byte[] challenge)
        {
            try
            {
                var attempts = 0;
                var success = false;
                var hasWaited = false;
                var reciept = m_web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionID).Result;
                if (transactionID == "0x750207aedaaf9abb7d485de5bcdec289a7ab4a58dddd6bbddbed8089ec289111")
                {
                    Program.Print(string.Format("[INFO] We submitted a repeat block, throwing out old answers."));



                    if (m_submitDateTimeList.Count >= MAX_SUBMIT_DTM_COUNT)
                        m_submitDateTimeList.RemoveAt(0);

                    m_submitDateTimeList.Add(submitDateTime);

                    string filePathz = "counter.txt";
                    int currentCounter = 0;
                    File.WriteAllText(filePathz, currentCounter.ToString());
                    retryCount = 0;

                    RunThisIfExcessMints = true;
                    m_ResetIfEpochGoesUp = 3000;
                    OnlyRunPayMasterOnce = true;
                    m_getChallengeNumber = m_getChallengeNumber2;
                    m_getMiningTarget = m_getMiningTarget2;
                    m_getMiningDifficulty = m_getMiningDifficulty2;

                    digestArray2 = new byte[][] { };
                    challengeArray2 = new byte[][] { };
                    nonceArray2 = new byte[][] { };

                    //var devFee = (ulong)Math.Round(100 / Math.Abs(DevFee.UserPercent));

                    //if (((SubmittedShares - RejectedShares) % devFee) == 0)
                    //SubmitDevFee(address, gasLimit, userGas, SubmittedShares);
                    return;
                }
                do
                {
                    attempts = attempts + 1;
                    Program.Print("attempts " + attempts);

                    reciept = m_web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionID).Result;

                    if (reciept == null)
                    {
                        if (hasWaited) Task.Delay(1000).Wait();
                        else
                        {
                            m_newChallengeResetEvent.Reset();
                            m_newChallengeResetEvent.WaitOne(1000 * 2);
                            hasWaited = true;
                        }
                    }
                    else
                    {
                        // Program.Print("reciept" + reciept);
                        // Program.Print("reciept.Status.Value" + reciept.Status.Value);
                        // Program.Print("reciept.Status" + reciept.Status);
                        //  Program.Print("reciept" + reciept);
                        if (hasWaited && reciept.BlockNumber.Value == 0) { Task.Delay(100).Wait(); }
                        else { hasWaited = true; }
                    }
                } while (reciept == null && attempts < 120);

                success = (reciept.Status.Value == 1);

                if (!success) RejectedShares++;

                if (SubmittedShares == ulong.MaxValue)
                {
                    SubmittedShares = 0ul;
                    RejectedShares = 0ul;
                }
                else SubmittedShares++;

                Console.ForegroundColor = ConsoleColor.White; // Set text color to blue
                Console.BackgroundColor = ConsoleColor.DarkBlue; // Set background color to a darker blue

                Console.WriteLine(string.Format("🏁🏁🏁🏁[INFO] Miner share [{0}] submitted: {1} ({2}ms), block: {3}, transaction ID: {4}    🏁🏁🏁🏁",
                                                SubmittedShares,
                                                success ? "success" : "failed",
                                                responseTime,
                                                reciept.BlockNumber.Value,
                                                reciept.TransactionHash));
                Program.Print(string.Format("🏁🏁🏁🏁[INFO] Miner share [{0}] submitted: {1} ({2}ms), block: {3}, transaction ID: {4}    🏁🏁🏁🏁",
                                            SubmittedShares,
                                            success ? "success" : "failed",
                                            responseTime,
                                            reciept.BlockNumber.Value,
                                            reciept.TransactionHash));

                Console.ResetColor(); // Reset to default colors
                if (success)
                {
                    skipThisMany = 0;
                    if (m_submitDateTimeList.Count >= MAX_SUBMIT_DTM_COUNT)
                        m_submitDateTimeList.RemoveAt(0);

                    m_submitDateTimeList.Add(submitDateTime);

                    retryCount = 0;


                    errorGreaterThan5 = 0;


                    // Specify the file path
                    string originalChallengeStringzf = BitConverter.ToString(challenge).Replace("-", "");
                    string directoryPathzfzfdzf = Path.Combine("solveData-", originalChallengeStringzf);
                    Directory.CreateDirectory(directoryPathzfzfdzf);

                    string filePathzLocationStart = "counterLocationStart.txt";

                    // Construct the file path
                    //console.log(")
                    string filePath2f = Path.Combine(directoryPathzfzfdzf, filePathzLocationStart);
                    // Construct the file path
                    runOnceThenResetAfter = false;


                                      
                        File.WriteAllText(filePath2f, counterzsLast.ToString());
                        Program.Print(string.Format("filePath: [{0}] counterLocationStart: {1} 🏁",
                                                      filePath2f,
                                                      counterzsLast));


                        didRunSpecialAndNeedsToRunAgain = true;

                    if (oldCurrentCounter - counterzsLast > m_MaxSolvesperMint)
                    {
                        string firstSubmitRunDir = Path.Combine(directoryPathzfzfdzf, "firstRunSubmit");

                        File.WriteAllText(firstSubmitRunDir, "1");

                        var maxCountz = counterzsLast + m_MaxSolvesperMint;
                        Program.Print("[INFO] COUNTERS IS GREATER THAN NEXT SUBMIT LIMIT AGAIN ");


                        string filePathCurrentCounter = "counter.txt";

                        // Construct the file path
                        //console.log(")
                        string filePath2f2 = Path.Combine(directoryPathzfzfdzf, filePathCurrentCounter);
                        // Construct the file path
                        runOnceThenResetAfter = false;
                        File.WriteAllText(filePath2f2, maxCountz.ToString());
                        Program.Print(string.Format("filePath:  [{0}] currentCounter adjusted: {1} 🏁",
                                                      filePath2f2,
                                                      maxCountz));
                    }
                    else
                    {

                        string filePathCurrentCounter = "counter.txt";

                        // Construct the file path
                        //console.log(")
                        string filePath2f2 = Path.Combine(directoryPathzfzfdzf, filePathCurrentCounter);
                        // Construct the file path
                        runOnceThenResetAfter = false;
                        File.WriteAllText(filePath2f2, oldCurrentCounter.ToString());
                        Program.Print(string.Format("filePath:  [{0}] counterzsLast: {1} 🏁",
                                                      filePath2f2,
                                                      oldCurrentCounter));

                    }
                    RunThisIfExcessMints = true;
                    OnlyRunPayMasterOnce = true;
                    m_getChallengeNumber = m_getChallengeNumber2;
                    m_getMiningTarget = m_getMiningTarget2;
                    m_getMiningDifficulty = m_getMiningDifficulty2;

                    m_ResetIfEpochGoesUp = 3000;
                    digestArray2 = new byte[][] { };
                    challengeArray2 = new byte[][] { };
                    nonceArray2 = new byte[][] { };

                    //var devFee = (ulong)Math.Round(100 / Math.Abs(DevFee.UserPercent));

                    //if (((SubmittedShares - RejectedShares) % devFee) == 0)
                    //SubmitDevFee(address, gasLimit, userGas, SubmittedShares);
                }



                //UpdateMinerTimer_Elapsed(this, null);
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        private BigInteger GetMiningReward()
        {
            var failCount = 0;
            Program.Print("[INFO] Checking mining reward amount from network...");
            while (failCount < 10)
            {
                try
                {
                    if (m_CLM_ContractProgress != null)
                        return m_CLM_ContractProgress.CallDeserializingToObjectAsync<CLM_ContractProgress>().Result.PowReward;

                    return m_getMiningReward.CallAsync<BigInteger>().Result; // including decimals
                }
                catch (Exception) { failCount++; }
            }
            throw new Exception("Failed checking mining reward amount.");
        }

        private void SubmitDevFee(string address, HexBigInteger gasLimit, HexBigInteger userGas, ulong shareNo)
        {
            var success = false;
            var devTransactionID = string.Empty;
            TransactionReceipt devReciept = null;
            try
            {
                var miningReward = GetMiningReward();

                Program.Print(string.Format("[INFO] Transferring dev. fee for successful miner share [{0}]...", shareNo));

                var txInput = new object[] { DevFee.Address, miningReward };

                var txCount = m_web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address).Result;

                // Commented as gas limit is dynamic in between submissions and confirmations
                //var estimatedGasLimit = m_transferMethod.EstimateGasAsync(from: address,
                //                                                          gas: gasLimit,
                //                                                          value: new HexBigInteger(0),
                //                                                          functionInput: txInput).Result;

                var transaction = m_transferMethod.CreateTransactionInput(from: address,
                                                                          gas: gasLimit /*estimatedGasLimit*/,
                                                                          gasPrice: userGas,
                                                                          value: new HexBigInteger(0),
                                                                          functionInput: txInput);
                /*
                var encodedTx = Web3.OfflineTransactionSigner.SignTransaction(privateKey: m_account.PrivateKey,
                                                                              to: m_contract.Address,
                                                                              amount: 0,
                                                                              nonce: txCount.Value,
                                                                              chainId: new HexBigInteger(m_chainID),
                                                                              gasPrice: userGas,
                                                                              gasLimit: gasLimit
                                                                              data: transaction.Data);

                if (!Web3.OfflineTransactionSigner.VerifyTransaction(encodedTx))
                    throw new Exception("Failed to verify transaction.");
                
                devTransactionID = m_web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encodedTx).Result;
                */
                if (string.IsNullOrWhiteSpace(devTransactionID)) throw new Exception("Failed to submit dev fee.");

                while (devReciept == null)
                {
                    try
                    {
                        Task.Delay(m_updateInterval / 2).Wait();
                        devReciept = m_web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(devTransactionID).Result;
                    }
                    catch (AggregateException ex)
                    {
                        HandleAggregateException(ex);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
                }

                success = (devReciept.Status.Value == 1);

                if (!success) throw new Exception("Failed to submit dev fee.");
                else
                {
                    Program.Print(string.Format("[INFO] Transferred dev fee for successful mint share [{0}] : {1}, block: {2}," +
                                                "\n transaction ID: {3}",
                                                shareNo,
                                                success ? "success" : "failed",
                                                devReciept.BlockNumber.Value,
                                                devReciept.TransactionHash));
                }
            }
            catch (AggregateException ex)
            {
                HandleAggregateException(ex);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex, string errorPrefix = null)
        {
            var errorMessage = new StringBuilder("[ERROR] ");

            if (!string.IsNullOrWhiteSpace(errorPrefix))
                errorMessage.AppendFormat("{0}: ", errorPrefix);

            errorMessage.Append(ex.Message);

            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                errorMessage.AppendFormat("\n {0}", innerEx.Message);
                innerEx = innerEx.InnerException;
            }
            Program.Print(errorMessage.ToString());
        }

        private void HandleAggregateException(AggregateException ex, string errorPrefix = null)
        {
            var errorMessage = new StringBuilder("[ERROR] ");

            if (!string.IsNullOrWhiteSpace(errorPrefix))
                errorMessage.AppendFormat("{0}: ", errorPrefix);

            errorMessage.Append(ex.Message);

            foreach (var innerException in ex.InnerExceptions)
            {
                errorMessage.AppendFormat("\n {0}", innerException.Message);

                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage.AppendFormat("\n  {0}", innerEx.Message);
                    innerEx = innerEx.InnerException;
                }
            }
            Program.Print(errorMessage.ToString());
        }
    }
}
