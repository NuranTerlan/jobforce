using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseJobs;
using JobExecutingViaMq.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JobExecutingViaMq.Helpers
{
    public static class MqHelpers
    {
        private static readonly IConnectionFactory ConnectionFactory;
        private static readonly NameValueCollection AppConfig;

        static MqHelpers()
        {
            AppConfig = ConfigurationManager.AppSettings;
            ConnectionFactory = new ConnectionFactory
            {
                Uri = new Uri(AppConfig.Get("mq-host") ?? string.Empty)
            };
        }

        public static void WaitMessages()
        {
            using var connection = ConnectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var queue = AppConfig.Get("listening-queue");
            try
            {
                channel.QueueDeclare(queue,
                    true,
                    autoDelete: false,
                    exclusive: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var queueMsg = JsonConvert.DeserializeObject<QueueMessage>(message);
                    Console.WriteLine($"Message which produced by {queueMsg.Name} is received from {queue}.");

                    var dllPathOfJob = AppConfig.Get(queueMsg.Message);
                    ReflectionHelpers.ExecuteJob(queueMsg.Message, dllPathOfJob);
                };
                channel.BasicConsume(queue, true, consumer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while listening the messages from the queue:\nError content: " +
                                  e.Message);
            }  
            
            Console.ReadLine();
        }
    }
}