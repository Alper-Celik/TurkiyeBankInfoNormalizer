namespace Models.ExporterInterfaces;

public interface ICreditCardTransactionExporter
{
    public string Name { get; }
    public string FileFormat { get; }
    public bool IsTextFormat { get; }

    public Task<Stream> Export(IAsyncEnumerable<CardTransaction> transactions, Stream? stream);
}
