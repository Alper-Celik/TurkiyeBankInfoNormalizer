using System.CommandLine;
using Models.ExporterInterfaces;
using Models.ImporterInterfaces;

namespace ConsoleUi;

public class ConvertCommand : Command
{
    private readonly IEnumerable<ICreditCardImporter> _creditCardImporters;
    private readonly IEnumerable<ICreditCardTransactionExporter> _cardTransactionExporters;

    private readonly Argument<FileInfo> _inputFileArgument = new("input file")
    {
        Description = "File to convert",
    };

    private readonly Option<FileInfo> _outputFileOption = new("--output", "-o")
    {
        Description = "Output file. Extension is inferred from Exporter if not specified",
        Required = false,
    };

    private readonly Option<string> _importerName = new("--importer")
    {
        Description = "Importer to use",
        Required = true, // TODO: infer from input file
    };

    private readonly Option<string> _exporterName = new("--exporter")
    {
        Description = "Exporter to use",
        DefaultValueFactory = _ => "csv-exporter-full",
    };

    public ConvertCommand(
        IEnumerable<ICreditCardImporter> creditCardImporters,
        IEnumerable<ICreditCardTransactionExporter> cardTransactionExporters
    )
        : base(
            "convert",
            "Converts From Bank Specific Format to More Structured Format Using Importers and Exporters"
        )
    {
        _creditCardImporters = creditCardImporters;
        _cardTransactionExporters = cardTransactionExporters;

        this.Arguments.Add(_inputFileArgument);

        this.Options.Add(_outputFileOption);
        this.Options.Add(_exporterName);
        this.Options.Add(_importerName);

        this.SetAction(Convert);
    }

    private async Task<int> Convert(ParseResult parseResult, CancellationToken ct)
    {
        ICreditCardImporter? importer = null;
        foreach (ICreditCardImporter creditCardImporter in _creditCardImporters)
        {
            if (creditCardImporter.ImporterName == parseResult.GetValue(_importerName))
            {
                importer = creditCardImporter;
            }
        }

        if (importer is null)
        {
            return (int)ExitCodes.ImporterNotFound;
        }

        ICreditCardTransactionExporter? exporter = null;
        foreach (
            ICreditCardTransactionExporter creditCardTransactionExporter in _cardTransactionExporters
        )
        {
            if (creditCardTransactionExporter.Name == parseResult.GetValue(_exporterName))
            {
                exporter = creditCardTransactionExporter;
            }
        }

        if (exporter is null)
        {
            return (int)ExitCodes.ExporterNotFound;
        }

        var inputFile = parseResult.GetRequiredValue(_inputFileArgument);
        var importTask = importer.Import(inputFile);
        FileInfo outputFile =
            parseResult.GetValue(_outputFileOption)
            ?? new FileInfo(
                $"{Path.GetFileNameWithoutExtension(inputFile.Name)}_{DateTime.UtcNow.Millisecond}_output{exporter.FileFormat}"
            );

        var exportResult = await exporter.Export(
            (await importTask).ToAsyncEnumerable(),
            outputFile.Open(FileMode.CreateNew, FileAccess.ReadWrite)
        );
        await exportResult.DisposeAsync();

        Console.WriteLine($"Converted Successfully, File at :\n{outputFile.FullName}");

        return (int)ExitCodes.Success;
    }
}
