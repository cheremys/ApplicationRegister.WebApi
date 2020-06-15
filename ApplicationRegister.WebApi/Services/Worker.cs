using ApplicationRegister.WebApi.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationRegister.WebApi.Services
{

    internal class Worker : IWorker
    {
        private readonly string queueName;
        private readonly string hostName;
        private readonly int port;
        private readonly string user;
        private readonly string password;
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private EventingBasicConsumer consumer;
        private BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private IBasicProperties props;
        private bool connected;
        private readonly ILogger<Worker> logger;

        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            this.queueName = configuration.GetSection("queueName").Get<string>();
            this.hostName = configuration.GetSection("hostName").Get<string>();
            this.port = configuration.GetSection("port").Get<int>();
            this.user = configuration.GetSection("user").Get<string>();
            this.password = configuration.GetSection("password").Get<string>();
            this.logger = logger;

            connected = ConnectToQueue();
        }

        public string SendMessage(string message)
        {
            if (connected)
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: props,
                    body: messageBytes);

                channel.BasicConsume(
                    consumer: consumer,
                    queue: replyQueueName,
                    autoAck: true);

                return respQueue.Take();
            }
            else
            {
                logger.LogError("Cannot connect to queue");
                return null;
            }
        }

        public void Close()
        {
            connection?.Close();
        }

        private bool ConnectToQueue()
        {
            var result = false;

            try
            {
                ConnectionFactory factory = null;

                if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                {
                    factory = new ConnectionFactory() { HostName = hostName };
                }
                else
                {
                    factory = new ConnectionFactory()
                    {
                        HostName = hostName,
                        Port = port,
                        UserName = user,
                        Password = password
                    };
                }

                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                replyQueueName = channel.QueueDeclare().QueueName;
                consumer = new EventingBasicConsumer(channel);

                props = channel.CreateBasicProperties();
                var correlationId = Guid.NewGuid().ToString();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body.ToArray());
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        respQueue.Add(response);
                    }
                };

                result = true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
            }
            return result;
        }
    }
}
