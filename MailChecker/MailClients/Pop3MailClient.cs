using System;
using System.Linq;
using OpenPop.Pop3;
using OpenPop.Mime;
using System.Collections.Generic;

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
        private readonly Pop3Client _client;
        
        public Pop3MailClient(string mail, string password, MailClientConfiguration configuration)
        {
            _client = new Pop3Client();
            _client.Connect(configuration.pop3Host, configuration.pop3Port, configuration.useSsl);
            _client.Authenticate(mail, password, OpenPop.Pop3.AuthenticationMethod.UsernameAndPassword);
        }

        public List<Pop3Mail> GetMail()
        {
            int messageCount = _client.GetMessageCount();
            var allMessages = new List<Pop3Mail>(messageCount);

            for (var i = messageCount; i > 0; i--)
            {
                allMessages.Add(new Pop3Mail() { MessageNumber = i, Message = this._client.GetMessage(i) });
            }

            return allMessages;
        }

        public List<Pop3Mail> GetMailFromAddress(string sender)
        {
            if (sender == null) throw new Exception("Sender error");
            
            int messageCount = this._client.GetMessageCount();

            var listOfMessages = new List<Pop3Mail>();

            for (int i = messageCount; i > 0; i--)
            {
                var msg = this._client.GetMessage(i);
                listOfMessages.Add(new Pop3Mail { Message = msg, MessageNumber = i });
            }

            var relMail = listOfMessages.Where(t => t.Message.Headers.From.Address == sender).ToList();
            
            return relMail;
        }
    }
}