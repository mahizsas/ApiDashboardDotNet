using System;
using System.Runtime.Serialization;

namespace ApiDashboard.Service
{
	[DataContract]
	public class ApiWebRequestCompletedEvent
	{
		public ApiWebRequestCompletedEvent()
		{
		}

		public ApiWebRequestCompletedEvent(string apiKey, int? publisherId, int? merchantId)
		{
			ApiKey = apiKey;
			PublisherId = publisherId;
			MerchantId = merchantId;
		}

		[DataMember]
		public bool OutputCache { get; set; }

		[DataMember]
		public string Route { get; private set; }

		[DataMember]
		public int Cache_LongTermLookup { get; private set; }

		[DataMember]
		public int Cache_LongTermMisses { get; private set; }

		[DataMember]
		public int Cache_LongTermHits { get; private set; }

		[DataMember]
		public int Cache_VolatileLookup { get; private set; }

		[DataMember]
		public int Cache_VolatileMisses { get; private set; }

		[DataMember]
		public int Cache_VolatileHits { get; private set; }

		[DataMember]
		public DateTime StartDateTimeUtc { get; private set; }

		[DataMember]
		public DateTime EndDateTimeUtc { get; private set; }

		[DataMember]
		public bool? Success { get; private set; }

		[DataMember]
		public string ApiTarget { get; private set; }

		[DataMember]
		public int? ApiVersion { get; private set; }

		[DataMember]
		public string ApiKey { get; private set; }

		[DataMember]
		public int? PublisherId { get; private set; }

		[DataMember]
		public int? ContractSourceId { get; private set; }

		[DataMember]
		public int? MerchantId { get; private set; }

		[DataMember]
		public int? AdminLoginId { get; private set; }
	}
}