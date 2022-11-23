using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailChecker.MailClients;

namespace MailChecker.Resources;

public static class Menu
{
    public static void Head()
    {
        var menuItems = new List<string>() { "1. Mail Checker", "2. Find message by sender", "3. Exit" };

        var menuFunctions = new MenuFunctions();
        var row = Console.CursorTop;
        var col = Console.CursorLeft;

        var index = 0;
        while (true)
        {
            Draw(menuItems, row, col, index);
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.DownArrow:
                    if (index < menuItems.Count - 1)
                        index++;
                    break;
                case ConsoleKey.UpArrow:
                    if (index > 0)
                        index--;
                    break;
                case ConsoleKey.Enter:
                    switch (index)
                    {
                        case 0:
                            menuFunctions.MailChecker();
                            return;
                        case 1:
                            menuFunctions.FindMessageBySender();
                            return;
                        case 2:
                            return;
                    }
                    break;
            }
        }
    }

    private static void Draw(IReadOnlyList<string> items, int row, int col, int index)
    {
        const string logo = @"/-------------------------------------------------------\" + "\n" +
                            @"|  _                      _                             |" + "\n" +
                            @"| | |                    | |                            |" + "\n" +
                            @"| | |__  _   _  __      _| |_ _ __ ___  _______   ___   |" + "\n" +
                            @"| | '_ \| | | | \ \ /\ / | __| '_ ` _ \|_  / _ \ / _ \  |" + "\n" +
                            @"| | |_) | |_| |  \ V  V /| |_| | | | | |/ | (_) | (_) | |" + "\n" +
                            @"| |_.__/ \__, |   \_/\_/  \__|_| |_| |_/___\___/ \___/  |" + "\n" +
                            @"|         __/ |                                         |" + "\n" +
                            @"|        |___/                                          |" + "\n" +
                            @"\-------------------------------------------------------/" + "\n\n";


        Console.SetCursorPosition(col, row);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(logo);
        Console.ForegroundColor = ConsoleColor.White;
        for (var i = 0; i < items.Count; i++)
        {
            if (i == index)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }
            Console.WriteLine(items[i]);
            Console.ResetColor();
        }
        Console.WriteLine();
    }
}

public class MenuFunctions
{
    private MailClientConfiguration outLookConfig;
    private MailClientConfiguration ramblerConfig;
    
    public MenuFunctions()
    {
        ramblerConfig = new MailClientConfiguration()
        {
            pop3Host = "pop.rambler.ru",
            pop3Port = 995,
            useSsl = true
        };
        
        outLookConfig = new MailClientConfiguration()
        {
            pop3Host = "outlook.office365.com",
            pop3Port = 995,
            useSsl = true
        };
    }
    
    public void MailChecker()
    {
        var accounts = CsvAccountReader.ReadAccount(Directory.GetCurrentDirectory() + @"\accounts.csv");
        
        foreach (var account in accounts)
        {
            MailClientConfiguration configuration = GetMailConfiguration(account.Mail);
            var mailType = account.Mail.Split('@')[1];
            try
            {
                _ = new Pop3MailClient(account.Mail, account.Password, configuration);
                ColoredWriteLine.Green($"--OK: {account.Mail}");
            }
            catch
            {
                ColoredWriteLine.Red($"--Can't get access: {account.Mail}");
            }
        }
        
        ColoredWriteLine.Green("\n---------------------Done---------------------");
    }

    public void FindMessageBySender()
    {
        var sender = "";
        var threadsDelay = 0;
        
        try
        {
            ColoredWrite.DarkYellow("--Sender: ");
            sender = Console.ReadLine();
            ColoredWrite.DarkYellow("--Delay(ms): ");
            threadsDelay = int.Parse(Console.ReadLine()!);

            if (string.IsNullOrEmpty(sender))
            {
                ColoredWriteLine.Red("\n--Invalid sender input");
                Thread.Sleep(5000);
                return;
            }
        }
        catch (Exception e)
        {
            ColoredWriteLine.Red("\n--Invalid delay input");
            Thread.Sleep(5000);
            return;
        }
        
        var accounts = CsvAccountReader.ReadAccount(Directory.GetCurrentDirectory() + @"\accounts.csv");
        var threadCounter = accounts.Count;

        foreach (var account in accounts)
        {
            Thread mailThread = new Thread(_ => MailThreadFunc(account));
            mailThread.Start();
            Thread.Sleep(threadsDelay);
        }

        void MailThreadFunc(CsvAccountReader.MailAccount account)
        {
            MailClientConfiguration configuration = GetMailConfiguration(account.Mail);

            if (string.IsNullOrEmpty(configuration.pop3Host))
            {
                ColoredWriteLine.Red($"--Сan't get the right configuration for: {account.Mail}");
                return;
            }
            
            try
            {
                var mailObj = new Pop3MailClient(account.Mail, account.Password, configuration, sender);
                var messages = mailObj.GetMailFromAddress();
                if (messages.Count > 0)
                {
                    ColoredWriteLine.Green($"Message found: {account.Mail}");
                    threadCounter -= 1;
                    return; 
                }
                ColoredWriteLine.Red($"No message found: {account.Mail}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ColoredWriteLine.DarkYellow($"--Can't connect to the address: {account.Mail}");
            }
            
            threadCounter -= 1;
        }

        var cnt = 0;
        while (true)
        {
            Thread.Sleep(100);
            if (threadCounter == 0 && cnt == 0)
            {
                ColoredWriteLine.Green("\n---------------------Done---------------------");
                cnt++;
            }
        }
    }

    private MailClientConfiguration GetMailConfiguration(string login)
    {
        var provider = login.Split('@')[1];
        MailClientConfiguration configuration = new();
        switch (provider)
        {
            case "rambler.ru":
                return ramblerConfig;
                break;
            case "outlook.com":
                return outLookConfig;
                break;
            default:
                return new MailClientConfiguration();
        }
    }
}