@echo off
protoc.exe --csharp_out="Assets/Scripts/Module/Message/" --proto_path="Assets/Proto/" OuterMessage.proto
echo finish... 
pause