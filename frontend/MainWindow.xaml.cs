﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Runtime;

namespace dotnetAnima {
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private static Process executableProcess;
    public MainWindow() {
        InitializeComponent();
        StartExecutable();
        MainFrame.Navigate(new AnimaHomePage());
        this.Closing += CloseWindow;
    }

    static void StartExecutable() {
        try {
            Console.WriteLine(Directory.GetCurrentDirectory());
            string executablePath = "..\\backend\\ANIMA\\anima.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo(executablePath);
            startInfo.WindowStyle =
                ProcessWindowStyle.Hidden;  // hide the backend terminal window
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = "..\\backend\\ANIMA\\";

            executableProcess = Process.Start(startInfo);
            Console.WriteLine("Executable started successfully.");
        } catch (Exception ex) {
            Console.WriteLine($"Error starting executable: {ex.Message}");
        }
    }

    static void CloseWindow(object sender,
                            System.ComponentModel.CancelEventArgs e) {
        if (executableProcess != null && !executableProcess.HasExited) {
            executableProcess.Kill();
            executableProcess.WaitForExit();  // Ensure the process is
                                              // terminated before continuing
        }
    }
}
}
