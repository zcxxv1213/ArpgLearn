namespace ETModel
{
	public interface IMessage
	{
        long Time { get; set; }
	}
	
	public interface IRequest: IMessage
	{
		int RpcId { get; set; }
	}

	public interface IResponse : IMessage
	{
		int Error { get; set; }
		string Message { get; set; }
		int RpcId { get; set; }
	}

	public class ResponseMessage : IResponse
	{
		public int Error { get; set; }
		public string Message { get; set; }
		public int RpcId { get; set; }
        public long Time { get; set; }
    }
}