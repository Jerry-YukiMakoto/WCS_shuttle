using System;
using System.Windows;

using Mirle.IASC;
using Mirle.Logger;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShuttleController? _shuttleController;
        private ShuttleCommand? _shuttleCommand;
        private Log _log = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void _shuttleController_OnAreaBlock(object sender, AreaEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Area Block Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Area Block Recieve\n"; });
        }
        private void _shuttleController_OnAreaRelease(object sender, AreaEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Area Release Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Area Release Recieve\n"; });
        }
        private void _shuttleController_OnCommandReceive(object sender, CommandReceiveEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Command Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Command Recieve\n"; });
        }
        private void _shuttleController_OnCommandReceiveError(object sender, CommandReceiveEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Command Receive Error");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Command Receive Error\n"; });
        }
        private void _shuttleController_OnCommandStatusChange(object sender, CommandStatusEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Command Status Change Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Command Status Change Recieve\n"; });
        }
        private void _shuttleController_OnLayerChange(object sender, LayerChangeEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Layer Change Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Layer Change Recieve\n"; });
        }
        private void _shuttleController_OnQueryCommandStatus(object sender, QueryCommandStatusEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Query Command Status Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Query Command Status Recieve\n"; });
        }
        private void _shuttleController_OnShuttleAlarmClear(object sender, ShuttleAlarmEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Shuttle Alarm Clear Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Shuttle Alarm Clear Recieve\n"; });
        }
        private void _shuttleController_OnShuttleAlarmSet(object sender, ShuttleAlarmEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Shuttle Alarm Set Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Shuttle Alarm Set Recieve\n"; });
        }
        private void _shuttleController_OnShuttleServiceChange(object sender, ShuttleServiceEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Shuttle Service Change Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Shuttle Service Change Recieve\n"; });
        }
        private void _shuttleController_OnUnknowCarrierOnVehicle(object sender, UnknowCarrierOnVehicleEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Unknow Carrier On Vehicle Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Unknow Carrier On Vehicle Recieve\n"; });
        }
        private void _shuttleController_OnVehicleStatusChange(object sender, VehicleStatusEventArgs e)
        {
            _log.WriteLogFile($"Log.log", $"Vehicle Status Change Recieve");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Vehicle Status Change Recieve\n"; });
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _shuttleController = new ShuttleController(IP.Text, Convert.ToInt32(Port.Text));
                _shuttleController.OnAreaBlock += _shuttleController_OnAreaBlock;
                _shuttleController.OnAreaRelease += _shuttleController_OnAreaRelease;
                _shuttleController.OnCommandReceive += _shuttleController_OnCommandReceive;
                _shuttleController.OnCommandReceiveError += _shuttleController_OnCommandReceiveError;
                _shuttleController.OnCommandStatusChange += _shuttleController_OnCommandStatusChange;
                _shuttleController.OnLayerChange += _shuttleController_OnLayerChange;
                _shuttleController.OnQueryCommandStatus += _shuttleController_OnQueryCommandStatus;
                _shuttleController.OnShuttleAlarmClear += _shuttleController_OnShuttleAlarmClear;
                _shuttleController.OnShuttleAlarmSet += _shuttleController_OnShuttleAlarmSet;
                _shuttleController.OnShuttleServiceChange += _shuttleController_OnShuttleServiceChange;
                _shuttleController.OnUnknowCarrierOnVehicle += _shuttleController_OnUnknowCarrierOnVehicle;
                _shuttleController.OnVehicleStatusChange += _shuttleController_OnVehicleStatusChange;
                _shuttleController.Open();
                _log.WriteLogFile($"Log.log", $"Open");
                Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Open\n"; });
            }
            catch (Exception)
            {
                _log.WriteLogFile($"Log.log", $"Open Parameter Error");
                Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] Open Parameter Error\n"; });
            }
        }
        private void P41_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _shuttleCommand = new ShuttleCommand(CommandID.Text, CommandType.Text, Convert.ToInt32(Priority.Text), Source.Text, Destination.Text, CarrierID.Text, VehicleID.Text);
                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                _log.WriteLogFile($"Log.log", $"P41 Send");
                Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P41 Send\n"; });
            }
            catch
            {
                _log.WriteLogFile($"Log.log", $"P41 Parameter Error");
                Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P41 Parameter Error\n"; });
            }
        }
        private void P53_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.ResetAllAlarm();
            _log.WriteLogFile($"Log.log", $"P53 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P53 Send\n"; });
        }
        private void P69_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P69(VehicleID.Text);
            _log.WriteLogFile($"Log.log", $"P69 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P69 Send\n"; });
        }
        private void P43_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P43(CommandID.Text);
            _log.WriteLogFile($"Log.log", $"P43 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P43 Send\n"; });
        }
        private void P83_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P83(CommandID.Text, SourceLayer.Text, DestinationLayer.Text);
            _log.WriteLogFile($"Log.log", $"P83 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P83 Send\n"; });
        }
        private void P91_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P91(LifterID.Text, LifterLocation.Text);
            _log.WriteLogFile($"Log.log", $"P91 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P91 Send\n"; });
        }
        private void P89_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P89(CommandID.Text, VehicleID.Text);
            _log.WriteLogFile($"Log.log", $"P89 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P89 Send\n"; });
        }
        private void P21_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P21(RepairDoorName.Text);
            _log.WriteLogFile($"Log.log", $"P21 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P21 Send\n"; });
        }
        private void P29_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P29(RepairDoorName.Text, DoorStatusCode.Text);
            _log.WriteLogFile($"Log.log", $"P29 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P29 Send\n"; });
        }
        private void P27_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P27(RepairDoorName.Text);
            _log.WriteLogFile($"Log.log", $"P27 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P27 Send\n"; });
        }
        private void P23_Click(object sender, RoutedEventArgs e)
        {
            _shuttleController?.P23(RepairDoorName.Text);
            _log.WriteLogFile($"Log.log", $"P23 Send");
            Dispatcher.BeginInvoke(() => { Log.Text += $"[{DateTime.Now}] P23 Send\n"; });
        }
    }
}
