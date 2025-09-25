using System;
using System.Net;
using System.Threading.Tasks;
using MyChat;

var server = new ChatServer(IPAddress.Any, 7777);
Console.WriteLine("Starting server on port 7777...");
await server.StartAsync(); 
