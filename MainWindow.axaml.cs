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
}

public enum FormatOptions
{
    None,
    VideoAndAudioRequired
}

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
    }

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        UrlTextBox.Focus();
        SubsCheckBox.IsChecked = true;
    }

    private void DisableControls()
    {
        UrlTextBox.IsEnabled = false;
        SubsCheckBox.IsEnabled = false;
        DownloadButton.IsEnabled = false;
        MainMenu.IsEnabled = false;
    }

    private void EnableControls()
    {
        UrlTextBox.IsEnabled = true;
        SubsCheckBox.IsEnabled = true;
        DownloadButton.IsEnabled = true;
        MainMenu.IsEnabled = true;
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

    // Thread swapper to send to GUI from other tasks or threads.
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

        string arguments = " "; // "-f mp4 -f 22 ";
        if (SubsCheckBox.IsChecked == true) arguments += "--write-subs ";
        arguments += UrlTextBox.Text;

        DisableControls();
        
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

        EnableControls();
        UrlTextBox.Clear();
        UrlTextBox.Focus();
    }

    private async void RetrieveFormatInfo(FormatOptions formatOptions)
    {
        MainListBox.Items.Clear();
        
        string arguments = "-F ";
        arguments += UrlTextBox.Text;

        DisableControls();
        
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
                        switch (formatOptions)
                        {
                            case FormatOptions.None:
                                break; 
                            case FormatOptions.VideoAndAudioRequired:
                                if (args.Data.Contains("video only")
                                    || args.Data.Contains("audio only")
                                    || args.Data.Contains("images"))
                                    return;
                                break;
                            default: return;
                        }
                        Task.Run(() => OnTextFromAnotherThread(new MessageData()
                        {
                            Target = MessageTarget.MainListBox, Text = args.Data
                        }));
                    };
                    p.ErrorDataReceived += (_, args) =>
                    {
                        if (args.Data == null) return;
                        Task.Run(() => OnTextFromAnotherThread(new MessageData()
                        {
                            Target = MessageTarget.MainListBox, Text = args.Data
                        }));
                    };
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

        EnableControls();
    }

    private void FormatsAllItem_OnClick(object? sender, RoutedEventArgs e)
    {
        RetrieveFormatInfo(FormatOptions.None);
    }

    private void FormatsVideoAndAudioItem_OnClick(object? sender, RoutedEventArgs e)
    {
        RetrieveFormatInfo(FormatOptions.VideoAndAudioRequired);
    }

}