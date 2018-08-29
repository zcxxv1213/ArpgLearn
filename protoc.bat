@echo off
protoc.exe --csharp_out="Assets/Scripts/Module/Message/" --proto_path="Assets/Proto/" OuterMessage.proto
protoc.exe --csharp_out="Assets/Scripts/Module/Message/Hotfix/" --proto_path="Assets/Proto/" HotfixMessage.proto
protoc.exe --csharp_out="Assets/Scripts/Module/Message/Hotfix/" --proto_path="Assets/Proto/" HotfixMessageServer.proto
protoc.exe --csharp_out="Assets/Scripts/Module/Message/Hotfix/" --proto_path="Assets/Proto/" FrameMessage.proto
echo finish... 
pause