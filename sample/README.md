# ZyRabbit Sample

## Projects
This sample contains three C# projects:
1. ZyRabbit.Messages.Sample is simple library with classes that will be used for communication between Console and Web applications.
1. ZyRabbit.AspNet.Sample is a simple web application that is configured to use Microsoft.Extensions.DependencyInjection container and Microsoft.Extensions.Logging.Console for logging.
1. ZyRabbit.ConsoleApp.Sample is a simple console application that is configured to use built-in ZyRabbit DI container and Serilog for logging.

## Pub-Sub Workflow
Pub-Sub communication workflow between web and console apps looks as follow:
1. User sends a HTTP requrest to "http://localhost:5000/values" endpoint
1. Web app receive user request
1. Web app publishes a message(ValuesRequested) to RabbitMQ with a single number that represents amount of values to receive.
1. Console app consumes the message from RabbitMQ and publishes a message(ValuesCalculated) to RabbitMQ that contains created values
1. Web app consumes the message with created values from RabbitMQ
1. Web app sends back HTTP response to the user with received values

## RPC Workflow
RPC communication workflow between web and console apps looks as follow:
1. User sends a HTTP requrest to "http://localhost:5000/values/42" endpoint
1. Web app receive user request
1. Web app publishes a message(ValueRequest) to RabbitMQ with a single number that represents value to receive.
1. Console app consumes the message from RabbitMQ and publishes back a message(ValueResponse) to RabbitMQ that contains created value
1. Web app consumes the message with received value from RabbitMQ
1. Web app sends back HTTP response to the user with received value