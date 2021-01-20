using global::Avro;
using global::Avro.Specific;

namespace com.avro.bean
{

public class Endpoint : ISpecificRecord

{
    public static Schema _SCHEMA = Schema.Parse("{\"type\":\"record\",\"name\":\"com.avro.bean.Endpoint\",\"fields\":[{\"name\":\"endpoint_id\",\"type\":\"string\"},{\"name\":\"endpoint_url\",\"type\":\"string\"},{\"name\":\"http_method\",\"type\":\"string\"}]}");
        public string endpoint_id;
        public string endpoint_url;
        public string http_method;
        public virtual Schema Schema
		{
			get
			{
				return Endpoint._SCHEMA;
			}
		}        public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.endpoint_id;
			case 1: return this.endpoint_url;
			case 2: return this.http_method;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.endpoint_id = (System.String)fieldValue; break;
			case 1: this.endpoint_url = (System.String)fieldValue; break;
			case 2: this.http_method = (System.String)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
    }


}