using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Energy_5adra_Final_Version
{
    public partial class DevicesWindow : Window
    {
        public List<Device> Devices { get; set; }
        private double currentVoltage;
        private bool relayState = false; // false = OFF (circuit open), true = ON (circuit closed)
        private const int RELAY_PIN = 7; // D7 pin

        public DevicesWindow(double voltageNow)
        {
            InitializeComponent();
            currentVoltage = voltageNow;

            Devices = new List<Device>
            {
                //new Device { Name = "Device 1", ReferenceVoltage = 2.5 },
                //new Device { Name = "Device 2", ReferenceVoltage = 1.5 },
                new Device { Name = "Fan", ReferenceVoltage = 5 }
            };

            DevicesList.ItemsSource = Devices;
        }

        private void CheckVoltages_Click(object sender, RoutedEventArgs e)
        {
            //double tolerance = 0.1; // نسبة 10%

            //var matched = Devices.Where(d =>
            //    d.ReferenceVoltage <= currentVoltage
            //    ||
            //    (d.ReferenceVoltage > currentVoltage &&
            //     d.ReferenceVoltage <= currentVoltage * (1 + tolerance))
            //).ToList();

            //// Display voltage check result
            //if (matched.Any())
            //{
            //    ResultText.Text = "Devices matching condition:\n" +
            //                      string.Join("\n", matched.Select(d => $"{d.Name} ({d.ReferenceVoltage}V)"));
            //}
            //else
            //{
            //    ResultText.Text = "No device matches the condition.";
            //}

            // Toggle relay state
            ToggleRelay();
        }

        private void ToggleRelay()
        {
            try
            {
                if (MainWindow.serialPort != null && MainWindow.serialPort.IsOpen)
                {
                    // Toggle the relay state
                    relayState = !relayState;

                    // Send command to Arduino
                    // For active LOW relay:
                    // Send "RELAY_ON" to turn relay ON (circuit closed, fan runs)
                    // Send "RELAY_OFF" to turn relay OFF (circuit open, fan stops)
                    string command = relayState ? "RELAY_ON\n" : "RELAY_OFF\n";
                    MainWindow.serialPort.WriteLine(command);

                    // Update result text to show relay status
                    string relayStatus = relayState ? "ON (Fan Running)" : "OFF (Fan Stopped)";
                    ResultText.Text += $"\n\n🔌 Relay Status: {relayStatus}";
                }
                else
                {
                    MessageBox.Show("Serial port is not open. Cannot control relay.",
                                    "Connection Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error controlling relay: {ex.Message}",
                                "Relay Control Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }

    public class Device
    {
        public string Name { get; set; }
        public double ReferenceVoltage { get; set; }
    }
}