using System;
using System.ComponentModel.Design;
using System.Linq;

using Mirle.Logger;

using PSDriver.PSDriver;

namespace Mirle.IASC
{
    public class ShuttleController : IDisposable
    {
        private readonly PSWrapperXClass _psWrapperXClass = new();
        private readonly Log _log = new();

        public delegate void CommandReceiveHandler(object sender, CommandReceiveEventArgs e);
        public delegate void ShuttleAlarmHandler(object sender, ShuttleAlarmEventArgs e);
        public delegate void CommandStatusHandler(object sender, CommandStatusEventArgs e);
        public delegate void QueryCommandStatusHandler(object sender, QueryCommandStatusEventArgs e);
        public delegate void VehicleStatusHandler(object sender, VehicleStatusEventArgs e);
        public delegate void AreaHandler(object sender, AreaEventArgs e);
        public delegate void ShuttleServiceHandler(object sender, ShuttleServiceEventArgs e);
        public delegate void UnknowCarrierOnVehicleHandler(object sender, UnknowCarrierOnVehicleEventArgs e);
        public delegate void LayerChangeHandler(object sender, ChangeLayerEventArgs e);
        public delegate void ChangeLayerHandler(object sender, ChangeLayerEventArgsLayer e);

        public event CommandReceiveHandler OnCommandReceive;
        public event CommandReceiveHandler OnCommandReceiveError;
        public event ShuttleAlarmHandler OnShuttleAlarmSet;
        public event ShuttleAlarmHandler OnShuttleAlarmClear;
        public event CommandStatusHandler OnCommandStatusChange;
        public event QueryCommandStatusHandler OnQueryCommandStatus;
        public event VehicleStatusHandler OnVehicleStatusChange;
        public event AreaHandler OnAreaBlock;
        public event AreaHandler OnAreaRelease;
        public event ShuttleServiceHandler OnShuttleServiceChange;
        public event UnknowCarrierOnVehicleHandler OnUnknowCarrierOnVehicle;
        public event LayerChangeHandler OnLayerChange;
        public event ChangeLayerHandler ChangeLayer;

        public static PSTransactionXClass S62E;

        public static PSTransactionXClass S84E;

        public bool IsConnected => _psWrapperXClass.IsConnected();

        public ShuttleController(string ipAddress, int tcpPort)
        {
            _psWrapperXClass.Address = ipAddress;
            _psWrapperXClass.Port = tcpPort;
            _psWrapperXClass.ConnectMode = enumConnectMode.Passive;

            _psWrapperXClass.OnPrimaryReceived += PSWrapperXClass_OnPrimaryReceived;
            _psWrapperXClass.OnSecondaryReceived += PSWrapperXClass_OnSecondaryReceived;
            _psWrapperXClass.OnConnected += PSWrapperXClass_OnConnected;
            _psWrapperXClass.OnDisconnected += PSWrapperXClass_OnDisconnected;
            _psWrapperXClass.OnTransactionError += PSWrapperXClass_OnTransactionError;

            //if (!Open())
            //{
            //    //MessageBox.Show("SHC連線異常", "Communication System", MessageBoxButtons.OK);
            //    //Environment.Exit(0);
            //}

        }

        private void PSWrapperXClass_OnTransactionError(string errorString, ref PSMessageXClass msg)
        {
        }

        private void PSWrapperXClass_OnDisconnected()
        {
        }

        private void PSWrapperXClass_OnConnected()
        {
        }

        private void PSWrapperXClass_OnPrimaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                PrimaryMessageLog(transaction);
                switch (transaction.PSPrimaryMessage.Type + transaction.PSPrimaryMessage.Number)
                {
                    case "P11":
                        S12(transaction);
                        break;

                    case "P13":
                        S14(transaction);
                        break;

                    case "P25":
                        S26(transaction);
                        break;

                    case "P51":
                        S52(transaction);
                        break;

                    case "P61":
                        S62(transaction);
                        break;

                    case "P71":
                        S72(transaction);
                        break;

                    case "P75":
                        S76(transaction);
                        break;

                    case "P81":
                        S82(transaction);
                        break;

                    case "P83":
                        P83(transaction);
                        break;

                    case "S86":
                        S86(transaction);
                        break;
                    //case "P85":
                    //    S86(transaction);
                    //    break;

                    case "P95":
                        S96(transaction);
                        break;
                }
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void PSWrapperXClass_OnSecondaryReceived(ref PSTransactionXClass transaction)
        {
            try
            {
                switch (transaction.PSSecondaryMessage.Type + transaction.PSSecondaryMessage.Number)
                {
                    case "S16":
                        S16(transaction);
                        break;

                    case "S22":
                        S22(transaction);
                        break;

                    case "S24":
                        S24(transaction);
                        break;

                    case "S28":
                        S28(transaction);
                        break;

                    case "S30":
                        S30(transaction);
                        break;

                    case "S42":
                        S42(transaction);
                        break;

                    case "S44":
                        S44(transaction);
                        break;

                    case "S54":
                        S54(transaction);
                        break;

                    case "S64":
                        S64(transaction);
                        break;

                    case "S66":
                        S66(transaction);
                        break;

                    case "S68":
                        S68(transaction);
                        break;

                    case "S70":
                        S70(transaction);
                        break;

                    case "S74":
                        S74(transaction);
                        break;

                    //case "S86":
                    //    S86(transaction);
                    //    break;

                    //case "P83":
                    //    P83(transaction);
                    //    break;



                    case "S90":
                        S90(transaction);
                        break;

                    case "S92":
                        S92(transaction);
                        break;
                }
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void S12(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "12";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
                DateTimeSync();
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "12";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S14(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "14";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "14";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }

        public void P00()
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "00";
                msg_send.PSPrimaryMessage.PSMessage = "";
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void S16(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "16";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
                P65();
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "16";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S22(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "22";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "22";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S24(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "24";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "24";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S26(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "26";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "26";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S28(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "28";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "28";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S30(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "30";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "30";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S42(PSTransactionXClass transaction)
        {
            try
            {
                string commandId = transaction.PSSecondaryMessage.PSMessage.Substring(0, 4);
                string returnCode = transaction.PSSecondaryMessage.PSMessage.Substring(4, 1);
                if (returnCode == "0")
                {
                    OnCommandReceive?.Invoke(this, new CommandReceiveEventArgs(commandId));
                }
                else
                {
                    OnCommandReceiveError?.Invoke(this, new CommandReceiveEventArgs(commandId));
                }
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }
        private void S44(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "44";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "44";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S52(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "52";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string vehicleId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 4);
                bool set = transaction.PSPrimaryMessage.PSMessage.Substring(4, 1) == "1";
                string errorCode = transaction.PSPrimaryMessage.PSMessage.Substring(5, 6);
                string carrierId = transaction.PSPrimaryMessage.PSMessage.Substring(11, 20);

                if (set)
                {
                    OnShuttleAlarmSet?.Invoke(this, new ShuttleAlarmEventArgs(vehicleId, errorCode, carrierId));
                }
                else
                {
                    OnShuttleAlarmClear?.Invoke(this, new ShuttleAlarmEventArgs(vehicleId, errorCode, carrierId));
                }
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "52";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S54(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "54";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "54";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S62(PSTransactionXClass transaction)//帶修改成實際文件中的event，根據命令去做回傳
        {
            try
            {
                string commandId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 4);
                string vehicleId = transaction.PSPrimaryMessage.PSMessage.Substring(4, 4);
                string commandStatus = transaction.PSPrimaryMessage.PSMessage.Substring(8, 6);
                string resultCode = transaction.PSPrimaryMessage.PSMessage.Substring(14, 4);
                S62E = transaction;


                OnCommandStatusChange?.Invoke(this, new CommandStatusEventArgs(commandId, vehicleId, commandStatus, resultCode));

                

            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "62";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }



        private void S64(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "64";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string commandId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 4);
                string vehicleId = transaction.PSPrimaryMessage.PSMessage.Substring(4, 4);
                string commandStatus = transaction.PSPrimaryMessage.PSMessage.Substring(8, 6);

                OnQueryCommandStatus?.Invoke(this, new QueryCommandStatusEventArgs(commandId, vehicleId, commandStatus));
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "64";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S66(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "66";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "66";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S68(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "68";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string vehicleId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 4);
                string vehicleLocatedLayer = transaction.PSPrimaryMessage.PSMessage.Substring(4, 2);
                string vehicleStatus = transaction.PSPrimaryMessage.PSMessage.Substring(6, 1);

                OnVehicleStatusChange?.Invoke(this, new VehicleStatusEventArgs(vehicleId, vehicleLocatedLayer, vehicleStatus));
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "68";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S70(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "70";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "70";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S86(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "86";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
                //追加觸發event
                string LifterId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 2);
                string Acknowledge = transaction.PSPrimaryMessage.PSMessage.Substring(2, 1);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "86";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S72(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "72";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string areaId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 2);
                string blockStatus = transaction.PSPrimaryMessage.PSMessage.Substring(2, 1);

                if (blockStatus == "B")
                {
                    OnAreaBlock?.Invoke(this, new AreaEventArgs(areaId));
                }
                else
                {
                    OnAreaRelease?.Invoke(this, new AreaEventArgs(areaId));
                }
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "72";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S74(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "74";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "74";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S76(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "76";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                int layer = Convert.ToInt32(transaction.PSPrimaryMessage.PSMessage.Substring(0, 2));
                int areaCount = Convert.ToInt32(transaction.PSPrimaryMessage.PSMessage.Substring(2, 2));
                //System.Collections.Generic.IEnumerable<bool> serviceStatus = transaction.PSPrimaryMessage.PSMessage[4..].Select(r => r.Equals('1'));

                //OnShuttleServiceChange?.Invoke(this, new ShuttleServiceEventArgs(layer, areaCount, serviceStatus));
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "76";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S82(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "82";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string vehicleId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 4);
                //string carrierId = transaction.PSPrimaryMessage.PSMessage[4..];

                //OnUnknowCarrierOnVehicle?.Invoke(this, new UnknowCarrierOnVehicleEventArgs(vehicleId, carrierId));
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "82";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S84(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "84";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);

                string acknowledge = transaction.PSPrimaryMessage.PSMessage.Substring(0, 1);
                string ReasonCode = transaction.PSPrimaryMessage.PSMessage.Substring(1, 4);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "84";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        
        private void S90(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "90";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "90";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S92(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "92";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "92";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }
        private void S96(PSTransactionXClass transaction)
        {
            try
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "96";
                transaction.PSSecondaryMessage.PSMessage = "0";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                SecondaryMessageLog(transaction);
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "96";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }

        public void P21(string RepairDoorName)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "21";
                msg_send.PSPrimaryMessage.PSMessage = RepairDoorName.PadLeft(2, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P23(string RepairDoorName)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "23";
                msg_send.PSPrimaryMessage.PSMessage = RepairDoorName.PadLeft(2, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P27(string RepairDoorName)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "27";
                msg_send.PSPrimaryMessage.PSMessage = RepairDoorName.PadLeft(2, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P29(string RepairDoorName, string DoorStatusCode)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "29";
                msg_send.PSPrimaryMessage.PSMessage = RepairDoorName.PadLeft(2, '0') + DoorStatusCode.PadLeft(4, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P43(string CommandID)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "43";
                msg_send.PSPrimaryMessage.PSMessage = CommandID.PadLeft(4, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void P65()
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "65";
                msg_send.PSPrimaryMessage.PSMessage = null;
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P69(string VehicleID)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "69";
                msg_send.PSPrimaryMessage.PSMessage = VehicleID.PadLeft(4, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P73()
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "73";
                msg_send.PSPrimaryMessage.PSMessage = null;
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P83(PSTransactionXClass transaction)
        {
            try
            {
                string LifterId = transaction.PSPrimaryMessage.PSMessage.Substring(0, 2);
                string Destination_Layer = transaction.PSPrimaryMessage.PSMessage.Substring(2, 2);
                S84E = transaction;

                ChangeLayer?.Invoke(this, new ChangeLayerEventArgsLayer(LifterId,Destination_Layer));
            }
            catch (Exception ex)
            {
                transaction.PSSecondaryMessage.Type = "S";
                transaction.PSSecondaryMessage.Number = "84";
                transaction.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref transaction);
                Exception(ex);
            }
        }

        public void P85(string LifterID, string ChanegeLayer_Status, string ResultCode)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "85";
                msg_send.PSPrimaryMessage.PSMessage = LifterID.PadLeft(2, '0') + ChanegeLayer_Status.PadLeft(1, '0')+ResultCode.PadLeft(4,'0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void S84(string Acknowledge, string ReasonCode)
        {
            try
            {
                S84E.PSSecondaryMessage.Type = "S";
                S84E.PSSecondaryMessage.Number = "84";
                S84E.PSSecondaryMessage.PSMessage = Acknowledge.PadLeft(1, '0') + ReasonCode.PadLeft(4, '0');  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref S84E);
                SecondaryMessageLog(S84E);
            }
            catch (Exception ex)
            {
                S84E.PSSecondaryMessage.Type = "S";
                S84E.PSSecondaryMessage.Number = "84";
                S84E.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref S84E);
                Exception(ex);
            }
        }
        public void S62(string Acknowledge, string TaskNo,string commandstatus)
        {
            try
            {
                S62E.PSSecondaryMessage.Type = "S";
                S62E.PSSecondaryMessage.Number = "62";
                S62E.PSSecondaryMessage.PSMessage = Acknowledge.PadLeft(1, '0') + TaskNo.PadLeft(4, '0')+commandstatus.PadLeft(5,'0');
                _psWrapperXClass.SecondarySent(ref S62E);
                SecondaryMessageLog(S62E);
            }
            catch (Exception ex)
            {
                S62E.PSSecondaryMessage.Type = "S";
                S62E.PSSecondaryMessage.Number = "62";
                S62E.PSSecondaryMessage.PSMessage = "1";  // 0: ok  1: NG
                _psWrapperXClass.SecondarySent(ref S62E);
                Exception(ex);
            }
        }

        public void P89(string CommandID, string VehicleID)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "89";
                msg_send.PSPrimaryMessage.PSMessage = CommandID.PadLeft(4, '0') + VehicleID.PadLeft(4, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void P91(string LifterID, string LifterLocation)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "91";
                msg_send.PSPrimaryMessage.PSMessage = LifterID.PadLeft(2, '0') + LifterLocation.PadLeft(2, '0');
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void DateTimeSync()
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "15";
                msg_send.PSPrimaryMessage.PSMessage = DateTime.Now.ToString("yyyyMMddHHmmss");
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private void Exception(Exception exception)
        {
            try
            {
                _log.WriteLogFile($"Shuttle_Exception.log", LogLevel.Empty, $"{exception}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }
        }

        private void PrimaryMessageLog(PSTransactionXClass transaction)
        {
            try
            {
                _log.WriteLogFile($"Shuttle_PSMessage.log", LogLevel.Empty, $"{transaction.PSPrimaryMessage.Type}{transaction.PSPrimaryMessage.Number}:{transaction.PSPrimaryMessage.PSMessage}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }
        }

        private void SecondaryMessageLog(PSTransactionXClass transaction)
        {
            try
            {
                _log.WriteLogFile($"Shuttle_PSMessage.log", LogLevel.Empty, $"{transaction.PSSecondaryMessage.Type}{transaction.PSSecondaryMessage.Number}:{transaction.PSSecondaryMessage.PSMessage}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex}");
            }
        }

        public void CreateShuttleCommand(ShuttleCommand shuttleCommand)
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "41";
                msg_send.PSPrimaryMessage.PSMessage = $"{shuttleCommand.CommandId.PadLeft(4, '0')}{shuttleCommand.CommandType}{shuttleCommand.Priority}{shuttleCommand.Source,9}{shuttleCommand.Destination,9}CST{shuttleCommand.CarrierId.PadLeft(17, '0')}{shuttleCommand.VehicleId}";
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public void ResetAllAlarm()
        {
            try
            {
                PSTransactionXClass msg_send = new();
                msg_send.PSPrimaryMessage.Type = "P";
                msg_send.PSPrimaryMessage.Number = "53";
                msg_send.PSPrimaryMessage.PSMessage = null;
                _psWrapperXClass.PrimarySent(ref msg_send);
                PrimaryMessageLog(msg_send);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        public bool Open()
        {
            return _psWrapperXClass.Open();
        }

        public void Close()
        {
            _psWrapperXClass.Close();
        }

        public void Dispose()
        {
            _psWrapperXClass.Close();
            _psWrapperXClass.Dispose();
        }
    }
}