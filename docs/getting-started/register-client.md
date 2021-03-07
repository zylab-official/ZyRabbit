# Registering the Client

Depending on the scenario, there are a few different ways to register the client.

### Register from factory

The easiest way to register a new client is by calling `ZyRabbitFactory.CreateSingleton`.
If no arguments are provided, the local configuration as defined in `ZyRabbitConfiguration.Local` will be used (`guest` user on `localhost:5672` with virtual host `/`).

```csharp
var raw = ZyRabbitFactory.CreateSingleton();
```

### Microsoft.Extensions.DependencyInjection Registration
If the application with Microsoft.Extensions.DependencyInjection, the dependencies and client can be registered by using the `AddZyRabbit` extension for `IServiceCollection`.
The package [`ZyRabbit.DependencyInjection.ServiceCollection`](https://www.nuget.org/packages/ZyRabbit.DependencyInjection.ServiceCollection) contains modules and extension methods for registering `ZyRabbit`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddZyRabbit(); // optional overrides here, too.
}
```

### Autofac Registration
The package [`ZyRabbit.DependencyInjection.Autofac`](https://www.nuget.org/packages/ZyRabbit.DependencyInjection.Autofac) contains modules and extension methods for registering `ZyRabbit`.

```csharp
var builder = new ContainerBuilder();
builder.RegisterZyRabbit();
var container = builder.Build();
```