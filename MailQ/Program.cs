using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Xml;
using MailLib;
using System.IO;

namespace MailQ
{

    public class Program
    {
        public static async Task Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["ConnectionString"];
            var qName = configuration["MailQueueName"];
            var smtpServer = configuration["smtpServer"];

            if (connectionString == null ||
                qName == null ||
                smtpServer == null)
            {
                throw new Exception("Please define ConnectionString and qName environment variables");
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(qName);

            await queue.CreateIfNotExistsAsync();

            while (true)
            {
                CloudQueueMessage msg = await queue.GetMessageAsync();

                if (msg != null)
                {

                    MailMessage emailmsg = null;

                    try {
                        StringReader sr = new StringReader(msg.AsString);
                        XmlReader xr = XmlReader.Create(sr);

                        SerializableMailMessage sermsg = new SerializableMailMessage();
                        sermsg.ReadXml(xr);
                        emailmsg = sermsg.Email;

                        Console.WriteLine("TO: " + emailmsg.To[0].Address);
                        Console.WriteLine("Subject: " + emailmsg.Subject);
                         
                        Console.WriteLine(emailmsg.Body);

                    } catch {
                        Console.WriteLine("Unable to desrialize email message: ");
                        Console.WriteLine("----");
                        Console.WriteLine(msg.AsString);
                        Console.WriteLine("----");
                        Console.WriteLine("Discarding email");
                    }

                    if (emailmsg != null) {
                        try {
                            SmtpClient smtpClient = new SmtpClient(smtpServer);
                            smtpClient.Send(emailmsg);
                        } catch {
                            Console.WriteLine("Unable to connect to smtp server");
                            throw;
                        }
                    } 

                    // Get the email off the queue
                    await queue.DeleteMessageAsync(msg);

                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
