using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Xml;
using Gopi;

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
            var qName = configuration["qName"];

            if (connectionString == null ||
                qName == null)
            {
                throw new Exception("Please define ConnectionString and qName environment variables");
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(qName);

            while (true)
            {
                CloudQueueMessage msg = await queue.GetMessageAsync();

                if (msg != null)
                {
                    Console.WriteLine(msg.AsString);
                    await queue.DeleteMessageAsync(msg);


                }
                else
                {
                    Console.WriteLine("Waiting for messages...");

                    var mailmsg = new MailMessage();
                    mailmsg.To.Add(new MailAddress("mihansen@microsoft.com"));
                    mailmsg.From = new MailAddress("mihansen@microsoft.com");

                    var sermailmsg = new SerializableMailMessage();
                    sermailmsg.Email = mailmsg;


                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.NewLineOnAttributes = true;
                    XmlWriter writer = XmlWriter.Create("email.xml", settings);

                    sermailmsg.WriteXml(writer);
                    writer.Close();
                    
                    Console.WriteLine("Email exported.");

                    Thread.Sleep(5000);
                }
            }
        }
    }
}
