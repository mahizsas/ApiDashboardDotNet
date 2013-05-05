using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ApiDashboard.Service
{
	public class ApiEventReader
	{
		private const string ExchangeName = "gcs.System";
		private static ApiEventReader _instance;
		private static List<Action<ApiWebRequestCompletedEvent>> _actionsToPerformOnRead;
		private static readonly object Lock = new object();

		public static void Init()
		{
			lock (Lock)
			{
				if (_instance != null)
					throw new ApplicationException("Cannot Init ApiEventReader more than once!");

				_instance = new ApiEventReader();
				ThreadPool.QueueUserWorkItem(InitReader);
			}
		}

		private ApiEventReader()
		{
			_actionsToPerformOnRead = new List<Action<ApiWebRequestCompletedEvent>>();
		}

		public static void RegisterActionToPerformOnRead(Action<ApiWebRequestCompletedEvent> actionToPerform)
		{
			lock (Lock)
			{
				_actionsToPerformOnRead.Add(actionToPerform);
			}
		}

		private static void InitReader(object state)
		{
			var factory = new ConnectionFactory
			{
				//production information left out of repo
				HostName = "",
				UserName = "",
				Password = "",
				RequestedHeartbeat = 10,
				VirtualHost = "/",
				Protocol = Protocols.FromEnvironment(),
				Port = AmqpTcpEndpoint.UseDefaultPort
			};


			using (var conn = factory.CreateConnection())
			using (var channel = conn.CreateModel())
			{

				channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, true, false, null);

				var queueName = String.Empty;	//queue will be given random name, since it's non-durable we don't need to have a known queue name
				var durable = false;
				var exclusive = true;
				var autoDelete = true;
				var noAck = true;	//these are non-critical and don't need ack.  Avoiding ack saves server and client some work


				//durable (true) -> the queue will be persisted across a rabbitMQ node reboot
				//exclusive (true) -> the queue can only be accessed via the current connection
				//auto-delete (true) -> the queue will be deleted once the client disconnects
				var queue = channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
				var routingKey = "ApiWebRequestCompletedEvent";

				channel.QueueBind(queueName, ExchangeName, routingKey);

				var consumer = new QueueingBasicConsumer(channel);
				channel.BasicConsume(queueName, noAck, consumer);

				BasicDeliverEventArgs envelope;

				while (true)
				{
					object _ = null;
					if (!consumer.Queue.Dequeue(500, out _)) //release poll at 500 ms
						continue;

					envelope = _ as BasicDeliverEventArgs;
					var message = FromJson(envelope.Body);

					lock (Lock)
					{
						_actionsToPerformOnRead.ForEach(x => x(message));
					}
				}

			}
		}

		private static ApiWebRequestCompletedEvent FromJson(byte[] bytes)
		{
			using (var memoryStream = new MemoryStream(bytes))
			{
				var serializer = new DataContractJsonSerializer(typeof(ApiWebRequestCompletedEvent));
				return (ApiWebRequestCompletedEvent)serializer.ReadObject(memoryStream);
			}
		}
	}
}
