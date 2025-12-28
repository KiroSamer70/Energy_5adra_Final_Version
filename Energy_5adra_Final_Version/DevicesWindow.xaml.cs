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

        public DevicesWindow(double voltageNow)
        {
            InitializeComponent();
            currentVoltage = voltageNow;

            Devices = new List<Device>
            {
                //new Device { Name = "Device 1", ReferenceVoltage = 2.5 },
                //new Device { Name = "Device 2", ReferenceVoltage = 1.5 },
                new Device { Name = "Device 3", ReferenceVoltage = 0.25 }
            };

            DevicesList.ItemsSource = Devices;
        }

        private void CheckVoltages_Click(object sender, RoutedEventArgs e)
        {
            double tolerance = 0.1; // نسبة 10%

            var matched = Devices.Where(d =>
                d.ReferenceVoltage <= currentVoltage
                ||
                (d.ReferenceVoltage > currentVoltage &&
                 d.ReferenceVoltage <= currentVoltage * (1 + tolerance))
            ).ToList();

            if (matched.Any())
            {
                ResultText.Text = "Devices matching condition:\n" +
                                  string.Join("\n", matched.Select(d => $"{d.Name} ({d.ReferenceVoltage}V)"));
            }
            else
            {
                ResultText.Text = "No device matches the condition.";
            }
        }
    }

    public class Device
    {
        public string Name { get; set; }
        public double ReferenceVoltage { get; set; }
    }
}