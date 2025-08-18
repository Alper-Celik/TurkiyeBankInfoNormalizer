using System.Globalization;
using CsvHelper;
using Models;
using Models.ExporterInterfaces;

namespace Exporters.Csv;

public class FullCsvExporter : ICreditCardTransactionExporter
{
    public string Name => "csv-exporter-full";
    public string FileFormat => ".csv";
    public bool IsTextFormat => true;

    public async Task<Stream> Export(IAsyncEnumerable<CardTransaction> transactions, Stream? stream)
    {
        stream ??= new MemoryStream();
        await using var output = new StreamWriter(stream);
        await using var csv = new CsvWriter(output, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(transactions);

        return stream;
    }
}
