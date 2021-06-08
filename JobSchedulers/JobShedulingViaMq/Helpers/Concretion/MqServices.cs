using System;
using System.Text;
using System.Threading;
using JobShedulingViaMq.ConfigModels;
using JobShedulingViaMq.Helpers.Abstraction;
using JobShedulingViaMq.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace JobShedulingViaMq.Helpers.Concretion
{
    public class MqServices : IMqServices
    {
        private readonly RabbitMqOptions _options;
        private readonly IConnectionFactory _connectionFactory;

        public MqServices(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
            _connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_options.Host ?? string.Empty)
            };
        }
        
        public void PublishExecutingJob(string jobName)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            try
            {
                channel.QueueDeclare(_options.Queue,
                    true,
                    autoDelete: false,
                    exclusive: false,
                    arguments: null);

                var message = new QueueMessage
                {
                    Name = _options.Producer,
                    Message = jobName
                };

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                Console.WriteLine($"\nMessage ({jobName}) publishing to {_options.Queue}..");
                channel.BasicPublish("", _options.Queue, null, body);
                Console.WriteLine($"Message ({jobName}) published to the {_options.Queue} successfully by {message.Name}.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while publishing the message to the queue:\nError content: " +
                                  e.Message);
            }
            finally
            {
                channel.Dispose();
                connection.Dispose();
            }
        }
    }
}