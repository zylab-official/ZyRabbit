# Logging

ZyRabbit captures [structual log entries](https://messagetemplates.org/) through [`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/). By default it sends all logs to [NullLogger<>](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.abstractions.nulllogger-1), which basically means that all logs will be skipped. However this behaviour can be easilly override if approproate implementations of interfaces ILogger<>, ILogger and ILoggerFactory will be registered in DI container.
