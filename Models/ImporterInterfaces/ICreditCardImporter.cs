namespace Models.ImporterInterfaces;

public interface ICreditCardImporter
{
    public IEnumerable<string> SupportedFileExtensions { get; }
    public string ImporterName { get; }
    public string BankName { get; }

    Task<IList<CardTransaction>> Import(FileInfo filePath);
}
