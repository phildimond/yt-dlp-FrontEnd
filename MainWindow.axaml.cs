using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace yt_dlp_FrontEnd;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private async void DownloadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainListBox.Items.Clear();

        List<string> s = new List<string>();

        string userName = System.Environment.UserName;
        MainListBox.Items.Add("Username = " + userName);
        
        try
        {
            Process? p = null;
            using (p = Process.Start(new ProcessStartInfo
                   {
                       FileName = "yt-dlp", // File to execute
                       Arguments = "-f mp4 " + UrlTextBox.Text, // arguments to use
                       WorkingDirectory = "/home/phillip/Videos",
                       UseShellExecute = false, // use process creation semantics
                       RedirectStandardOutput = true, // redirect standard output to this Process object
                       RedirectStandardError = true, // redirect standard error to this Process object
                       CreateNoWindow = true, // if this is a terminal app, don't show it
                       WindowStyle = ProcessWindowStyle.Hidden // if this is a terminal app, don't show it
                   }))
            {
                // Wait for the process to finish executing
                if (p != null)
                {
                    p.OutputDataReceived += (o, args) => { s.Add("Output = " + args.Data);
                        MainListBox.Items.Add("Output = " + args.Data);
                    };
                    p.ErrorDataReceived += (o, args) => { s.Add("Error = " + args.Data); };
                    p.Exited += (o, args) => { MainListBox.Items.Add("Exit code = " + p.ExitCode); };
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                    await p.WaitForExitAsync();
                    if (s.Count == 0)
                    {
                        MainListBox.Items.Add("Nothing in output.");
                    }
                    else
                    {
                        foreach (string str in s)
                        {
                            MainListBox.Items.Add(str);
                        }
                    }
                }
                else
                {
                    MainListBox.Items.Add("Command failed");
                }
            }
        } catch (Exception ex) { MainListBox.Items.Add("EXCEPTION: " + ex.Message); }
    } 

    private void BashButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}