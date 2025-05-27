using System.Diagnostics;
using SignalRWorker;


var inPipe = args[0];
var outPipe = args[1];
var token = args.Length > 2 ? args[2] : "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImVuZy5qd2VsbGJyaXR0b0BnbWFpbC5jb20iLCJMaWNlbmNpYWRvIjoiMTAwIiwibmFtZWlkIjoiMyIsImp0aSI6Ijc0MWY2MGZkLWMxMGQtNGU3My04MGEyLWU0NTAxOTE2MjIxOSIsImV4cCI6MTc0ODk3ODg3NCwiaXNzIjoiSW5mbyBXIFNvZnR3YXJlIiwiYXVkIjoiT0xJTVBPIn0.NcTxIPGH-boa1WoOH-twNGXNVf3zrl0rzcuuz0GtxsU"; // Recebe o token como terceiro argumento

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService(_ => new Worker(inPipe, outPipe, token));

var host = builder.Build();
host.Run();
