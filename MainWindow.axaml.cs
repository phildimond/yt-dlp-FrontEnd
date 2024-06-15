using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace yt_dlp_FrontEnd;

public enum MessageTarget
{
    ProgressTextBlock,
    MainListBox
};

public class MessageData
{
    public MessageTarget Target { get; set; }
    public string Text { get; set; } = string.Empty;
}

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Execute OnTextFromAnotherThread on the thread pool
        // to demonstrate how to access the UI thread from
        // there.
        _ = Task.Run(() => OnTextFromAnotherThread(new MessageData()
        {
            Target = MessageTarget.MainListBox, Text = "Test Message"
        }));
    }

    private void SetText(MessageData messageData)
    {
        switch (messageData.Target)
        {
            case MessageTarget.MainListBox: 
                MainListBox.Items.Add(messageData.Text);
                break;
            case MessageTarget.ProgressTextBlock:
                ProgressTextBlock.Text = messageData.Text;
                break;
            default: MainListBox.Items.Add(messageData.Text);
                break;
        }
    }

    private void OnTextFromAnotherThread(MessageData messageData) => Dispatcher.UIThread.Post(() => SetText(messageData));
    
    private void ExitItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private async void DownloadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MainListBox.Items.Clear();
        ProgressTextBlock.Text = "0%";

        List<string> s = new List<string>();

        string arguments = "-f mp4 ";
        if (SubsCheckBox.IsChecked == true) arguments += "--write-subs ";
        arguments += UrlTextBox.Text;

        try
        {
            Process? p;
            using (p = Process.Start(new ProcessStartInfo
                   {
                       FileName = "yt-dlp", // File to execute
                       Arguments = arguments, // arguments to use
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
                    p.OutputDataReceived += (_, args) =>
                    {
                        if (args.Data == null) return;
                        if (args.Data.Contains('%'))
                        {
                            string[] bits = args.Data.Split(' ');
                            foreach (string bit in bits)
                            {
                                if (bit.Contains('%'))
                                {
                                    Task.Run(() => OnTextFromAnotherThread(new MessageData()
                                    {
                                        Target = MessageTarget.ProgressTextBlock, Text = bit
                                    }));
                                    break;
                                }
                            }
                        }
                        else
                            Task.Run(() => OnTextFromAnotherThread(new MessageData()
                            {
                                Target = MessageTarget.MainListBox, Text = args.Data
                            }));
                    };
                    p.ErrorDataReceived += (_, args) => { s.Add("Error = " + args.Data); };
                    p.Exited += (_, _) =>
                    {
                        Task.Run(() => OnTextFromAnotherThread(new MessageData()
                        {
                            Target = MessageTarget.MainListBox, Text = "Exit code = " + p.ExitCode
                        }));
                    };
                    p.BeginErrorReadLine();
                    p.BeginOutputReadLine();
                    await p.WaitForExitAsync();
                    if (s.Count == 0)
                    {
                        await Task.Run(() => OnTextFromAnotherThread(new MessageData()
                        {
                            Target = MessageTarget.MainListBox, Text = "Nothing in output."
                        }));
                    }
                }
                else
                {
                    await Task.Run(() => OnTextFromAnotherThread(new MessageData()
                    {
                        Target = MessageTarget.MainListBox, Text = "Command failed"
                    }));
                }
            }
        }
        catch (Exception ex)
        {
            await Task.Run(() => OnTextFromAnotherThread(new MessageData()
            {
                Target = MessageTarget.MainListBox, Text = "EXCEPTION: " + ex.Message
            }));
        }

        UrlTextBox.Clear();
        UrlTextBox.Focus();
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        UrlTextBox.Focus();
    }

}