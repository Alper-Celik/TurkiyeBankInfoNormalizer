using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUi.List;

public class ListCommand : Command
{
    public ListCommand([FromKeyedServices("ListSubcommands")] IEnumerable<Command> ListSubcommands)
        : base("list", "Lists Components")
    {
        foreach (Command subcommand in ListSubcommands)
        {
            this.Subcommands.Add(subcommand);
        }
    }
}
