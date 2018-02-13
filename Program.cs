using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;

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

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(qName);

            while(true) {
                CloudQueueMessage msg = await queue.GetMessageAsync();

                if (msg != null) {
                    Console.WriteLine(msg.AsString);
                    await queue.DeleteMessageAsync(msg);
                } else {
                    Console.WriteLine("Waiting for messages...");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
