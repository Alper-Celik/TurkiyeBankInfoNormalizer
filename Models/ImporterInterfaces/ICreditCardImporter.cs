namespace Models.ImporterInterfaces;

public interface ICreditCardImporter
{
    string[] SupportedFileExtensions();

    Task<IList<CardTransaction>> Import(FileInfo filePath);
}
