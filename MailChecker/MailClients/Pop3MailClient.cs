using System;
using System.Collections.Generic;
using System.Linq;
using MailChecker.Resources;
using OpenPop.Pop3;
using OpenPop.Mime;

namespace MailChecker.MailClients
{
    public struct MailClientConfiguration
    {
        public string pop3Host { get; set; }
        public int pop3Port { get; set; }
        public bool useSsl { get; set; } 
    }
    public struct Pop3Mail
    {
        public int MessageNumber { get; set; }
        public Message Message { get; set; }
    }
    
    public class Pop3MailClient
    {
        private readonly string _mail;
        private readonly string _sender;
        private readonly Pop3Client _client;
        
        public Pop3MailClient(string mail, string password, MailClientConfiguration configuration)
        {
            _mail = mail;
            _client = new Pop3Client();
            _client.Connect(configuration.pop3Host, configuration.pop3Port, configuration.useSsl);
            _client.Authenticate(mail, password, OpenPop.Pop3.AuthenticationMethod.UsernameAndPassword);
        }
        
        public Pop3MailClient(string mail, string password, MailClientConfiguration configuration, string sender)
        {
            _mail = mail;
            _sender = sender;
            _client = new Pop3Client();
            _client.Connect(configuration.pop3Host, configuration.pop3Port, configuration.useSsl);
            _client.Authenticate(mail, password, OpenPop.Pop3.AuthenticationMethod.UsernameAndPassword);
        }

        public List<Pop3Mail> GetMail()
        {
            var messageCount = _client.GetMessageCount();
            var allMessages = new List<Pop3Mail>(messageCount);

            for (var i = messageCount; i > 0; i--)
            {
                allMessages.Add(new Pop3Mail() { MessageNumber = i, Message = this._client.GetMessage(i) });
            }

            return allMessages;
        }

        public List<Pop3Mail> GetMailFromAddress()
        {
            if (_sender == null) throw new Exception("Sender error");
            
            int messageCount = this._client.GetMessageCount();

            var allMessages = new List<Pop3Mail>();

            for (int i = messageCount; i > 0; i--)
            {
                var msg = this._client.GetMessage(i);

                allMessages.Add(new Pop3Mail { Message = msg, MessageNumber = i });
            }

            var relevantMail = allMessages.Where(m => m.Message.Headers.From.Address == _sender).ToList();
            
            return relevantMail;
        }
    }
}