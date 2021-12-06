using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TaskManager.Commands;

namespace TaskManager.View_Models
{
    public class MainViewModel : BaseViewModel
    {
        public MainWindow MainWindows { get; set; }

        private PerformanceCounter _PerformanceCounter1;
        public PerformanceCounter PerformanceCounter1
        {
            get { return _PerformanceCounter1; }
            set { _PerformanceCounter1 = value; OnPropertyChanged(); }
        }

        private PerformanceCounter _PerformanceCounter2;
        public PerformanceCounter PerformanceCounter2
        {
            get { return _PerformanceCounter2; }
            set { _PerformanceCounter2 = value; OnPropertyChanged(); }
        }

        private DispatcherTimer timer;
        private DispatcherTimer timer2;
        
        public MainViewModel()
        {

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);

            timer2 = new DispatcherTimer();
            timer2.Tick += DataGetS;
            timer2.Interval = TimeSpan.FromSeconds(2);
            timer2.Start();

            Thread tempData = new Thread(() =>
            {
                MainWindows.GetButton.Click += GetAllProcess;
                MainWindows.EndButton.Click += EndProcess;
                MainWindows.SeachButton.Click += SearchProcess;
                MainWindows.CreateButton.Click += CreateProcess;
                MainWindows.AddBlackListButton.Click += AddBlackList;
            });

            timer.Start();
            tempData.Start();
        }


        private void GetAllProcess(object sender, EventArgs e)
        {
            timer.Stop();
            timer2.Stop();
            try
            {
                MainWindows.proceslistview.ItemsSource = null;
                MainWindows.proceslistview.Items.Clear();

                if (MainWindows.blackListBox.Items.Count != 0)
                {
                    foreach (Process item in Process.GetProcesses())
                    {
                        foreach (var item2 in MainWindows.blackListBox.Items)
                        {
                            if (item2.ToString() == item.ProcessName)
                            {
                                item.Kill();
                                timer2.Start();
                            }
                        }
                    }
                }

                timer.Start();
                timer2.Start();
            }
            catch (Exception)
            {
            }
        }

        private void EndProcess(object sender, EventArgs e)
        {
            try
            {
                if (MainWindows.proceslistview.SelectedItem != null)
                {
                    Process item = MainWindows.proceslistview.SelectedItem as Process;

                    foreach (var process in Process.GetProcesses())
                    {
                        if (item.Id == process.Id)
                        {
                            if (!process.WaitForExit(1000))
                            {
                                if (!process.HasExited) process.Kill();
                            }
                        }
                    }
                }

                MainWindows.proceslistview.Items.RemoveAt(MainWindows.proceslistview.SelectedIndex);
                if (MainWindows.proceslistview.Items.Count == 0)
                    timer2.Start();
            }
            catch (Exception)
            {
            }
        }

        private void SearchProcess(object sender, EventArgs e)
        {
            MainWindows.proceslistview.ItemsSource = null;
            MainWindows.proceslistview.Items.Clear();

            if (string.IsNullOrEmpty(MainWindows.SearchTextBox.Text) == false)
            {
                foreach (var process in Process.GetProcesses())
                {
                    if (process.ProcessName == MainWindows.SearchTextBox.Text)
                        MainWindows.proceslistview.Items.Add(process);
                }
            }
            timer2.Stop();
        }

        private void CreateProcess(object sender, EventArgs e)
        {
            if (MainWindows.blackListBox.Items.Count > 0)
            {
                foreach (var item in MainWindows.blackListBox.Items)
                {
                    if (item.ToString() == MainWindows.SearchTextBox.Text)
                    {
                        MessageBox.Show("Enter process BLOCKED", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            Process p = new Process();
            p.StartInfo.FileName = MainWindows.SearchTextBox.Text;
            p.Start();
            MainWindows.proceslistview.Items.Add(p);
            timer2.Start();
        }

        private void AddBlackList(object sender, EventArgs e)
        {
            timer2.Stop();
            MainWindows.blackListBox.Items.Add(MainWindows.SearchTextBox.Text);
            timer2.Start();
        }


        private void DataGetS(object sender, EventArgs e)
        {
            if (MainWindows.blackListBox.Items.Count != 0)
            {
                foreach (var item in MainWindows.blackListBox.Items)
                {
                    var processes = Process.GetProcesses().Where(p => p.ProcessName == item.ToString());
                    if (processes.Count() > 0)
                    {
                        foreach (Process item2 in processes)
                        {
                            Process process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == item.ToString());
                            if (item2.ProcessName == process.ProcessName)
                                item2.Kill();
                        }
                    }

                    foreach (Process item1 in Process.GetProcesses())
                    {
                        foreach (var item2 in MainWindows.blackListBox.Items)
                        {
                            if (item2.ToString() == item1.ProcessName)
                                if (!item1.WaitForExit(300))
                                    item1.Kill();
                        }
                    }

                }
            }
            MainWindows.proceslistview.ItemsSource = null;
            MainWindows.proceslistview.Items.Clear();
            foreach (var item in Process.GetProcesses())
                MainWindows.proceslistview.Items.Add(item);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            PerformanceCounter1 = new PerformanceCounter("Processor", "% Idle Time", "_Total");
            PerformanceCounter2 = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            float cpu = PerformanceCounter1.NextValue();
            MainWindows.CPU_textbox.Text = string.Format("{0:0.00} %", PerformanceCounter1.NextValue());
            MainWindows.RAM_textbox.Text = string.Format("{0:0.00} %", PerformanceCounter2.NextValue());
        }
    } 
}