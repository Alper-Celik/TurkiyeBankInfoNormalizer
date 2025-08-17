using Importers.Models;

namespace Importers;

public interface ICreditCardImporter
{
    string[] SupportedFileExtensions();

    Task<IList<CardTransaction>> Import(FileInfo filePath);
}
