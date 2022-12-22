using System;
using System.IO;
using System.Threading;
using MailChecker.MailClients;
using System.Collections.Generic;

namespace MailChecker.Resources;

public static class Menu
{
    public static void Head()
    {
        var menuItems = new List<string>() { "1. Mail Checker", "2. Find message by sender" };

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
    private MailClientConfiguration mailruConfig;
    
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

        mailruConfig = new MailClientConfiguration()
        {
            pop3Host = "pop.mail.ru",
            pop3Port = 995,
            useSsl = true
        };
    }
    
    public void MailChecker()
    {
        List<string> userInput = UserInput(
            new List<string>() { "--Delay(ms): " });
        
        var threadsDelay = 100;
        try
        {
            threadsDelay = int.Parse(userInput[0]);
        }
        catch
        {
            ColoredWriteLine.DarkYellow("Setting the default value for delay: 100");
        }
        //89wW37wffG2wNBmGkYYx
        Console.WriteLine();
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
            MailClientConfiguration configuration = 
                GetMailConfiguration(account.Mail);

            try
            {
                _ = new Pop3MailClient(account.Mail, account.Password, configuration);
                ColoredWriteLine.Green($"--OK: {account.Mail}");
            }
            catch
            {
                ColoredWriteLine.Red($"--Can't get access: {account.Mail}");
            }

            threadCounter -= 1;
        }
        
        
        while (true)
        {
            Thread.Sleep(100);
            if (threadCounter == 0)
            {
                ColoredWriteLine.Green("\n---------------------Done---------------------");
                break;
            }
        }

        Console.ReadKey();
    }

    public void FindMessageBySender()
    {
        List<string> userInput = UserInput(
            new List<string>() { "--Sender: ", "--Delay(ms): " });
        
        if (CheckUserInput(userInput) == false) return;
        var threadsDelay = 100;
        try
        {
            threadsDelay = int.Parse(userInput[1]);
        }
        catch
        {
            ColoredWriteLine.DarkYellow("Setting the default value for delay: 100");
        }
        
        Console.WriteLine();
        List<CsvAccountReader.MailAccount> accounts = 
            CsvAccountReader.ReadAccount(Directory.GetCurrentDirectory() + @"\accounts.csv");
        
        var threadCounter = accounts.Count;

        foreach (var account in accounts)
        {
            Thread mailThread = new Thread(_ => MailThreadFunc(account, userInput[0]));
            mailThread.Start();
            Thread.Sleep(threadsDelay);
        }

        void MailThreadFunc(CsvAccountReader.MailAccount account, string sender)
        {
            MailClientConfiguration configuration = 
                GetMailConfiguration(account.Mail);

            if (string.IsNullOrEmpty(configuration.pop3Host))
            {
                ColoredWriteLine.Red($"--Сan't get the right configuration for: {account.Mail}");
                return;
            }
            
            try
            {
                var mailObj = new Pop3MailClient(account.Mail, account.Password, configuration);
                var messages = mailObj.GetMailFromAddress(sender);
                if (messages.Count > 0)
                {
                    ColoredWriteLine.Green($"Message(s) found: {account.Mail}");
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
        
        while (true)
        {
            Thread.Sleep(100);
            if (threadCounter == 0)
            {
                ColoredWriteLine.Green("\n---------------------Done---------------------");
                break;
            }
        }

        Console.ReadKey();
    }

    private bool CheckUserInput(List<string> input)
    {
        foreach (var text in input)
        {
            if (string.IsNullOrEmpty(text)) return false;
        }

        return true;
    }
    
    private List<string> UserInput(List<string> queries)
    {
        var input = new List<string>();
        foreach (var query in queries)
        {
            ColoredWrite.DarkYellow(query);
            var answer = Console.ReadLine();
            input.Add(answer);
        }
        
        return input;
    }
    
    private MailClientConfiguration GetMailConfiguration(string login)
    {
        var provider = login.Split('@')[1];

        switch (provider)
        {
            case "rambler.ru":
                return ramblerConfig;
            case "outlook.com":
                return outLookConfig;
            case "mail.ru":
                return mailruConfig;
            default:
                return new MailClientConfiguration();
        }
    }
}