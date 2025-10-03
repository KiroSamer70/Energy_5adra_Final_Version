using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;

namespace Energy_5adra_Final_Version
{
    public partial class MainWindow : Window
    {
        SerialPort serialPort;
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
            SensorChart.AxisX[0].Separator = new Separator { Step = 5 };

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
                    ResistanceText.Text = part.Replace("Resistance:", "").Trim();
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

            // التحديث الضمني يتم تلقائياً، مش لازم نعمل SensorChart.Update()
        }

        protected override void OnClosed(EventArgs e)
        {
            if (serialPort.IsOpen)
                serialPort.Close();

            base.OnClosed(e);
        }
    }
}