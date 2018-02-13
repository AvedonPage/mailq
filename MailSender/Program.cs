using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Collections.Generic;
using MailLib;
using System.Xml;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace MailSender
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["ConnectionString"];
            var qName = configuration["MailQueueName"];

            if (connectionString == null ||
                qName == null)
            {
                throw new Exception("Please define ConnectionString and qName environment variables");
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(qName);

            await queue.CreateIfNotExistsAsync();

            Console.WriteLine("FROM:");
            string toAddress = Console.ReadLine();

            Console.WriteLine("TO:");
            string fromAddress = Console.ReadLine();

            Console.WriteLine("Subject:");
            string subject = Console.ReadLine();

            MailMessage msg = new MailMessage();

            msg.To.Add(new MailAddress(toAddress));
            msg.From = new MailAddress(fromAddress);
            msg.Subject = subject;

            Console.WriteLine("Message body (CTRL+Z to finish):");
            string line;
            List<string> msgBody = new List<string>();
            do
            {
                line = Console.ReadLine();
                if (line != null)
                {
                    msgBody.Add(line);
                }
            } while (line != null);

            msg.Body = string.Join(Environment.NewLine, msgBody);

            SerializableMailMessage sermsg = new SerializableMailMessage();
            sermsg.Email = msg;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            var sw = new StringWriter();
            var xw = XmlWriter.Create(sw);
            sermsg.WriteXml(xw);
            xw.Flush();

            var xmlEmail = sw.ToString();

            await queue.AddMessageAsync(new CloudQueueMessage(xmlEmail));

            Console.WriteLine("Email exported:");

        }
    }
}
