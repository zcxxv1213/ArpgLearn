using ETModel;
namespace ETModel
{
	[Message(FrameOpcode.OneFrameMessage)]
	public partial class OneFrameMessage : IActorMessage {}

	[Message(FrameOpcode.FrameMessage)]
	public partial class FrameMessage : IActorMessage {}

	[Message(FrameOpcode.LocalNCFAndJLE)]
	public partial class LocalNCFAndJLE : IActorMessage {}

//int32 CurrentHostId = 3;
	[Message(FrameOpcode.JoinEvent)]
	public partial class JoinEvent : IActorMessage {}

	[Message(FrameOpcode.JoinLeaveEvent)]
	public partial class JoinLeaveEvent : IActorMessage {}

	[Message(FrameOpcode.OnlineState)]
	public partial class OnlineState : IActorMessage {}

	[Message(FrameOpcode.InputRLE)]
	public partial class InputRLE : IActorMessage {}

	[Message(FrameOpcode.InputCoalesced)]
	public partial class InputCoalesced : IActorMessage {}

	[Message(FrameOpcode.InputHeader)]
	public partial class InputHeader : IActorMessage {}

	[Message(FrameOpcode.InputPredictionWarmValues)]
	public partial class InputPredictionWarmValues : IActorMessage {}

}
namespace ETModel
{
	public static partial class FrameOpcode
	{
		 public const ushort OneFrameMessage = 20001;
		 public const ushort FrameMessage = 20002;
		 public const ushort LocalNCFAndJLE = 20003;
		 public const ushort JoinEvent = 20004;
		 public const ushort JoinLeaveEvent = 20005;
		 public const ushort OnlineState = 20006;
		 public const ushort InputRLE = 20007;
		 public const ushort InputCoalesced = 20008;
		 public const ushort InputHeader = 20009;
		 public const ushort InputPredictionWarmValues = 20010;
	}
}
