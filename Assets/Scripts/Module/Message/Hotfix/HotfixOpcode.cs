using ETModel;
namespace ETModel
{
	[Message(HotfixOpcode.Player_Info_Base)]
	public partial class Player_Info_Base : IMessage {}

	[Message(HotfixOpcode.C2R_Login)]
	public partial class C2R_Login : IRequest {}

	[Message(HotfixOpcode.R2C_Login)]
	public partial class R2C_Login : IResponse {}

	[Message(HotfixOpcode.C2G_LoginGate)]
	public partial class C2G_LoginGate : IRequest {}

	[Message(HotfixOpcode.G2C_LoginGate)]
	public partial class G2C_LoginGate : IResponse {}

	[Message(HotfixOpcode.C2G_SendMsg)]
	public partial class C2G_SendMsg : IMessage {}

	[Message(HotfixOpcode.G2C_TestHotfixMessage)]
	public partial class G2C_TestHotfixMessage : IMessage {}

	[Message(HotfixOpcode.C2M_TestActorRequest)]
	public partial class C2M_TestActorRequest : IActorRequest {}

	[Message(HotfixOpcode.M2C_TestActorResponse)]
	public partial class M2C_TestActorResponse : IActorResponse {}

	[Message(HotfixOpcode.C2M_ReadyStartGame)]
	public partial class C2M_ReadyStartGame : IActorRequest {}

	[Message(HotfixOpcode.M2C_ReadyStartGame)]
	public partial class M2C_ReadyStartGame : IActorResponse {}

	[Message(HotfixOpcode.PlayerInfo)]
	public partial class PlayerInfo : IMessage {}

	[Message(HotfixOpcode.C2G_PlayerInfo)]
	public partial class C2G_PlayerInfo : IRequest {}

	[Message(HotfixOpcode.G2C_PlayerInfo)]
	public partial class G2C_PlayerInfo : IResponse {}

	[Message(HotfixOpcode.OneFrameMessage)]
	public partial class OneFrameMessage : IActorMessage {}

	[Message(HotfixOpcode.FrameMessage)]
	public partial class FrameMessage : IActorMessage {}

}
namespace ETModel
{
	public static partial class HotfixOpcode
	{
		 public const ushort Player_Info_Base = 10001;
		 public const ushort C2R_Login = 10002;
		 public const ushort R2C_Login = 10003;
		 public const ushort C2G_LoginGate = 10004;
		 public const ushort G2C_LoginGate = 10005;
		 public const ushort C2G_SendMsg = 10006;
		 public const ushort G2C_TestHotfixMessage = 10007;
		 public const ushort C2M_TestActorRequest = 10008;
		 public const ushort M2C_TestActorResponse = 10009;
		 public const ushort C2M_ReadyStartGame = 10010;
		 public const ushort M2C_ReadyStartGame = 10011;
		 public const ushort PlayerInfo = 10012;
		 public const ushort C2G_PlayerInfo = 10013;
		 public const ushort G2C_PlayerInfo = 10014;
		 public const ushort OneFrameMessage = 10015;
		 public const ushort FrameMessage = 10016;
	}
}
