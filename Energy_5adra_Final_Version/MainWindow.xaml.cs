using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveChartsSeparator = LiveCharts.Wpf.Separator;

namespace Energy_5adra_Final_Version
{
    public partial class MainWindow : Window
    {
        public static SerialPort serialPort; // Made public static to access from DevicesWindow
        public static bool relayModule = true; // true = ON, false = OFF (stopped)
        int time = 0;
        List<string> timeLabels = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            serialPort = new SerialPort("COM3", 9600); // عدل حسب منفذ Arduino
            serialPort.DataReceived += SerialPort_DataReceived;

            SensorChart.Series = new SeriesCollection
            {
                new LineSeries { Title = "Voltage", Values = new ChartValues<double>(), LineSmoothness = 0, PointGeometry = null },
                new LineSeries { Title = "Current", Values = new ChartValues<double>(), LineSmoothness = 0, PointGeometry = null },
                new LineSeries { Title = "Power", Values = new ChartValues<double>(), LineSmoothness = 0, PointGeometry = null }
            };

            SensorChart.AxisX[0].Labels = timeLabels;
            SensorChart.AxisX[0].Separator = new LiveChartsSeparator { Step = 5 };

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر فتح المنفذ التسلسلي: " + ex.Message);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                Dispatcher.Invoke(() => UpdateUI(data));
            }
            catch { }
        }

        private void UpdateUI(string data)
        {
            string[] parts = data.Split('\t');
            double voltage = 0, current = 0, power = 0;

            foreach (string part in parts)
            {
                if (part.StartsWith("Volt:"))
                {
                    VoltageText.Text = part.Replace("Volt:", "").Trim();
                    double.TryParse(VoltageText.Text.Split(' ')[0], out voltage);
                }
                else if (part.StartsWith("Current:"))
                {
                    CurrentText.Text = part.Replace("Current:", "").Trim();
                    double.TryParse(CurrentText.Text.Split(' ')[0], out current);
                }
                else if (part.StartsWith("Resistance:"))
                {
                    string resistanceValue = part.Replace("Resistance:", "").Trim();
                    ResistanceText.Text = resistanceValue.Replace("??", "ohm");
                }
                else if (part.StartsWith("Power:"))
                {
                    PowerText.Text = part.Replace("Power:", "").Trim();
                    double.TryParse(PowerText.Text.Split(' ')[0], out power);
                }
            }

            var voltageSeries = SensorChart.Series[0].Values;
            var currentSeries = SensorChart.Series[1].Values;
            var powerSeries = SensorChart.Series[2].Values;

            voltageSeries.Add(voltage);
            currentSeries.Add(current);
            powerSeries.Add(power);

            time++;
            timeLabels.Add(time.ToString());

            int maxPoints = 50;
            if (voltageSeries.Count > maxPoints)
            {
                voltageSeries.RemoveAt(0);
                currentSeries.RemoveAt(0);
                powerSeries.RemoveAt(0);
                timeLabels.RemoveAt(0);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();

            base.OnClosed(e);
        }

        private void OpenDevicesWindow_Click(object sender, RoutedEventArgs e)
        {
            double currentVoltage = 0;
            double.TryParse(VoltageText.Text.Split(' ')[0], out currentVoltage);

            DevicesWindow win = new DevicesWindow(currentVoltage);
            win.Show();
        }

        private void SimulateFault_Click(object sender, RoutedEventArgs e)
        {
            relayModule = false;

            // Send relay-off command to Arduino to turn off the relay module
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.WriteLine("RELAY_OFF\n");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending relay-off command: " + ex.Message);
                }
            }

            Window faultWindow = new Window
            {
                Title = "⚠ FAULT DETECTED",
                Width = 420,
                Height = 220,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1e1e1e")),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };

            panel.Children.Add(new TextBlock
            {
                Text = "⚠ WARNING!!",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e74c3c")),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "The device has an issue and we stopped it\nimmediately for your safety.",
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "Relay Module: OFF",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#e74c3c")),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            faultWindow.Content = panel;
            faultWindow.ShowDialog();
        }
    }
}