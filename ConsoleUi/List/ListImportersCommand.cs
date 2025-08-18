using System.CommandLine;
using System.Text.Json;
using Models.ImporterInterfaces;

namespace ConsoleUi.List;

public class ListImportersCommand : Command
{
    private readonly IEnumerable<ICreditCardImporter> _creditCardImporters;

    public ListImportersCommand(IEnumerable<ICreditCardImporter> creditCardImporters)
        : base("importers", "Lists Available Importers")
    {
        _creditCardImporters = creditCardImporters;
        this.SetAction(List);
    }

    private int List(ParseResult parseResult)
    {
        foreach (ICreditCardImporter importer in _creditCardImporters)
        {
            Console.WriteLine($"{importer.ImporterName} :");
            Console.WriteLine($"\tSupported Bank = {importer.BankName}");
            Console.WriteLine(
                $"\tSupported File Formats = {JsonSerializer.Serialize(importer.SupportedFileExtensions)}"
            );
        }

        return 0;
    }
}
