@echo off
protoc.exe --csharp_out="Scripts/Module/Message/" --proto_path="Proto/" OuterMessage.proto
echo finish... 
pause