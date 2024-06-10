using System;
using System.Net.Mime;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CliWrap;
using CliWrap.Buffered;

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

    private async void lsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainListBox.Items.Clear();
        var result = await Cli
            .Wrap(targetFilePath:"ls")
            .WithArguments("-la")
            .ExecuteBufferedAsync();

        MainListBox.Items.Add("Output = " + result.StandardOutput);
        MainListBox.Items.Add("Error = " + result.StandardError);
        MainListBox.Items.Add("Exit code = " + result.ExitCode);
        MainListBox.Items.Add("Start time = " + result.StartTime);
        MainListBox.Items.Add("Exit time = " + result.ExitTime);
        MainListBox.Items.Add("Run time = " + result.RunTime);

        if (result.ExitCode != 0)
        {
            MainListBox.Items.Add("Command failed"); 
        }
    }
    
    private async void pwdButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainListBox.Items.Clear();
        var result = await Cli
            .Wrap(targetFilePath:"pwd")
            .ExecuteBufferedAsync();

        MainListBox.Items.Add("Output = " + result.StandardOutput);
        MainListBox.Items.Add("Error = " + result.StandardError);
        MainListBox.Items.Add("Exit code = " + result.ExitCode);
        MainListBox.Items.Add("Start time = " + result.StartTime);
        MainListBox.Items.Add("Exit time = " + result.ExitTime);
        MainListBox.Items.Add("Run time = " + result.RunTime);

        if (result.ExitCode != 0)
        {
            MainListBox.Items.Add("Command failed"); 
        }
    }
    
}