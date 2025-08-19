using System.CommandLine;
using Models;
using Models.ExporterInterfaces;
using Models.ImporterInterfaces;

namespace ConsoleUi;

public class ConvertCommand : Command
{
    private readonly IEnumerable<ICreditCardImporter> _creditCardImporters;
    private readonly IEnumerable<ICreditCardTransactionExporter> _cardTransactionExporters;

    private readonly Argument<List<FileInfo>> _inputFilesArgument = new("input file")
    {
        Description = "File to convert",
        Arity = ArgumentArity.OneOrMore,
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

        this.Arguments.Add(_inputFilesArgument);

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

        var inputFiles = parseResult.GetRequiredValue(_inputFilesArgument);
        List<Task<IList<CardTransaction>>> importTasks = [];
        foreach (FileInfo inputFile in inputFiles)
        {
            importTasks.Add(importer.Import(inputFile));
        }
        FileInfo outputFile =
            parseResult.GetValue(_outputFileOption)
            ?? new FileInfo(
                $"{Path.GetFileNameWithoutExtension(inputFiles[0].Name)}_{DateTime.UtcNow.Millisecond}_output{exporter.FileFormat}"
            );

        var exportResult = await exporter.Export(
            importTasks
                .ToAsyncEnumerable()
                .SelectManyAwait<Task<IList<CardTransaction>>, CardTransaction>(async task =>
                    (await task).ToAsyncEnumerable()
                ),
            outputFile.Open(FileMode.CreateNew, FileAccess.ReadWrite)
        );
        await exportResult.DisposeAsync();

        Console.WriteLine($"Converted Successfully, File at :\n{outputFile.FullName}");

        return (int)ExitCodes.Success;
    }
}
