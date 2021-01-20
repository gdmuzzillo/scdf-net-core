using global::Avro;
using global::Avro.Specific;

namespace com.avro.bean {
    public class MessageDestination : ISpecificRecord
    {
        public static Schema _SCHEMA = Schema.Parse("{\"type\":\"record\",\"name\":\"com.avro.bean.MessageDestination\",\"fields\":[{\"name\":\"messageId\",\"type\":\"int\"},{\"name\":\"endpoint\",\"type\":{\"type\":\"record\",\"name\":\"com.avro.bean.Endpoint\",\"fields\":[{\"name\":\"endpoint_id\",\"type\":\"string\"},{\"name\":\"endpoint_url\",\"type\":\"string\"},{\"name\":\"http_method\",\"type\":\"string\"}]}},{\"name\":\"payload\",\"type\":{\"type\":\"record\",\"name\":\"com.avro.bean.OrderProduct\",\"fields\":[{\"name\":\"order_id\",\"type\":\"int\"},{\"name\":\"price\",\"type\":\"float\"},{\"name\":\"product_id\",\"type\":\"int\"},{\"name\":\"product_name\",\"type\":\"string\"},{\"name\":\"product_price\",\"type\":\"float\"}]}}]}");
        public int messageId;
            
        public Endpoint endpoint;

        public OrderProduct payload;
            public virtual Schema Schema
		{
			get
			{
				return MessageDestination._SCHEMA;
			}
        }
        public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.messageId;
			case 1: return this.endpoint;
			case 2: return this.payload;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.messageId = (System.Int32)fieldValue; break;
			case 1: this.endpoint = (Endpoint)fieldValue; break;
			case 2: this.payload = (OrderProduct)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
    }
    
}