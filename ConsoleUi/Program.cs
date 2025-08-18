using System.CommandLine;
using ConsoleUi.List;
using DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection servicesBuilder = new ServiceCollection();

servicesBuilder.RegisterCreditCardImporters();
servicesBuilder.RegisterCreditCardExporters();
servicesBuilder.AddKeyedSingleton<Command, ListCommand>("RootSubcommands");

// register list subcommands
servicesBuilder.AddKeyedScoped<Command, ListExportersCommand>("ListSubcommands");
servicesBuilder.AddKeyedScoped<Command, ListImportersCommand>("ListSubcommands");

ServiceProvider services = servicesBuilder.BuildServiceProvider();

RootCommand rootCommand = new("A program to work with Türkiye bank data");
foreach (var subcommand in services.GetKeyedServices<Command>("RootSubcommands"))
{
    rootCommand.Add(subcommand);
}

ParseResult parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
