using System.Collections.Generic;
using System.Linq;
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
        private readonly string _login;
        private readonly string _password;
        private readonly Pop3Client _client;

        public Pop3MailClient(string login, string password, MailClientConfiguration config)
        {
            _login = login;
            _password = password;
            _client = new Pop3Client();
            //_client.Connect("pop.rambler.ru", 995, true);
            _client.Connect(config.pop3Host, config.pop3Port, config.useSsl);
            _client.Authenticate(login, password, OpenPop.Pop3.AuthenticationMethod.UsernameAndPassword);
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
        
        public List<Pop3Mail> GetMail(string fromAddress)
        {
            int messageCount = this._client.GetMessageCount();

            var allMessages = new List<Pop3Mail>();

            for (int i = messageCount; i > 0; i--)
            {
                var msg = this._client.GetMessage(i);

                allMessages.Add(new Pop3Mail { Message = msg, MessageNumber = i });
            }

            var relevantMail = allMessages.Where(m => m.Message.Headers.From.Address == fromAddress).ToList();

            return relevantMail;
        }
    }
}