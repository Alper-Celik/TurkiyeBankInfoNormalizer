using System.Globalization;
using Models;
using Models.Currency;
using Models.ImporterInterfaces;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Importers.QNB;

// we want to avoid contuning in another thread since DbContext is not thread safe
#pragma warning disable CA2007
public class QnbCreditCardImporterXls : ICreditCardImporter
{
    private const int ECell = 5 - 1;
    IEnumerable<string> ICreditCardImporter.SupportedFileExtensions => [".xls"];

    public string ImporterName => "qnb-cc-xls-importer";

    public string BankName => _bankName;
    private static string _bankName = "QNB Bank A.Ş.";

    public Task<IList<CardTransaction>> Import(FileInfo filePath)
    {
        FileStream reader = filePath.OpenRead();
        using HSSFWorkbook cardStatementFile = new(reader);

        return Import(cardStatementFile);
    }

    public string[] SupportedFileExtensions()
    {
        return ["*.xls"];
    }

    public static Task<IList<CardTransaction>> Import(HSSFWorkbook cardStatementFile)
    {
        ISheet cardStatement = cardStatementFile.GetSheetAt(0);

        List<CardTransaction> cardTransactions = new();

        foreach (var (card, transctions) in GetCardTransactionsLists(cardStatement))
        {
            var transactions = GetCardTransactions(transctions, card);
            foreach (var transaction in transactions)
            {
                cardTransactions.Add(transaction);
            }
        }

        return Task.FromResult<IList<CardTransaction>>(cardTransactions);
    }

    private static IEnumerable<(Card, IEnumerable<string[]>)> GetCardTransactionsLists(
        ISheet cardStatementFile
    )
    {
        List<(Card, IEnumerable<string[]>)> cardTransactionsList = new();

        foreach (var i in GetCardRowNumbers(cardStatementFile))
        {
            Card card = GetQnbCardAsync(cardStatementFile.GetRow(i).GetCell(ECell).StringCellValue);
            yield return (card, GetCardTransactionRows(cardStatementFile, i));
        }
    }

    private static IEnumerable<int> GetCardRowNumbers(ISheet cardStatementFile)
    {
        List<string> eCells = ["non empty", "non empty"];
        for (int i = 1; true; i++)
        {
            var row = GetRow(cardStatementFile, i);
            var eCellValue = row.GetCell(ECell)?.StringCellValue ?? string.Empty;
            if (eCellValue.Contains("KART"))
            {
                yield return i - 1;
            }
            eCells.Add(eCellValue);
            if (eCells.TakeLast(2).Aggregate(true, ((b, c) => b && (c == string.Empty))) && i > 16)
            {
                yield break;
            }
        }
    }

    static IRow GetRow(ISheet sheet, int x) => sheet.GetRow(x - 1);

    public static IList<string[]> GetCardTransactionRows(ISheet cardStatement, int cardRow)
    {
        List<string[]> cardTransactionRows = [];
        for (
            int i = cardRow + 1;
            !string.IsNullOrEmpty(
                cardStatement
                    .GetRow(i)
                    .GetCell(GetStatementColumns(cardStatement).First())
                    .StringCellValue
            );
            i++
        )
        {
            IRow row = cardStatement.GetRow(i);
            List<string> cells = new();

            foreach (int j in GetStatementColumns(cardStatement))
            {
                cells.Add(cardStatement.GetRow(i).GetCell(j).StringCellValue);
            }

            cardTransactionRows.Add(cells.ToArray());
        }
        return cardTransactionRows;
    }

    private static IEnumerable<int> GetStatementColumns(ISheet cardStatement)
    {
        int row = GetCardRowNumbers(cardStatement).First();

        do
        {
            row--;
        } while (
            !(cardStatement.GetRow(row)?.GetCell(1)?.StringCellValue.Contains("Tarihi") ?? false)
        );

        foreach (
            var cell in cardStatement
                .GetRow(row)
                .Cells.Where(c => c.StringCellValue != string.Empty)
        )
        {
            yield return cell.ColumnIndex;
        }
    }

    private static Card GetQnbCardAsync(string cardInfo) =>
        new()
        {
            AvailableCardNumberPart = GetCardLast4Digits(cardInfo),
            Name = GetCardName(cardInfo),
            IssuedBank = _bankName,
        };

    public static IList<CardTransaction> GetCardTransactions(
        IEnumerable<string[]> statementRows,
        Card qnbCard
    )
    {
        List<CardTransaction> cardTransactions = [];

        foreach (string[] row in statementRows)
        {
            // sample date "16/06/2025"
            // so it is dd/mm/yyyy
            int[] dateParts =
            [
                .. row[0]
                    .Split("/")
                    .Select(static s => int.Parse(s, CultureInfo.InvariantCulture.NumberFormat)),
            ];
            DateOnly transactionDate = new(dateParts[2], dateParts[1], dateParts[0]);

            string comment = row[1];

            Currency currency =
                Currency.GetCurrency(
                    string.Concat(row[2].Reverse().TakeWhile(static c => c != ' ').Reverse())
                ) ?? throw new InvalidDataException();

            long amountInMinorUnit = long.Parse(
                string.Concat(
                    row[2]
                        .Reverse()
                        .SkipWhile(static c => c != ' ')
                        .Reverse()
                        .Where(static c => c is not '.' and not ',' and not ' ')
                ),
                CultureInfo.InvariantCulture.NumberFormat
            );
            // TODO : add support for reading installments

            cardTransactions.Add(
                new()
                {
                    TransactionDate = transactionDate,
                    Comment = comment,
                    Currency = currency,
                    AmountInMinorUnit = amountInMinorUnit,
                    Card = qnbCard,
                }
            );
        }
        return cardTransactions;
    }

    public static string GetCardName(string cardInfo)
    {
        return string.Concat(cardInfo.TakeWhile(static c => c != '-').SkipLast(1));
    }

    private static string GetCardLast4Digits(string cardInfo) =>
        string.Concat(cardInfo.Where(c => char.IsNumber(c)).TakeLast(4));
}
