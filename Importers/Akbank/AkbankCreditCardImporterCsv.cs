using System.Globalization;
using System.Text;
using BankDataDb.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Importers.Akbank;

// we want to avoid contuning in another thread since DbContext is not thread safe
#pragma warning disable CA2007
public class AkbankCreditCardImporterCsv : IBankImporter
{
    public async Task<(IList<CardTransaction>, IDbContextTransaction)> Import(
        BankDataContext context,
        FileInfo filePath
    )
    {
#pragma warning disable CA1849 //  File.ReadLinesAsync returns AsyncIEnumerable which requires to change everywhere it touches it isn't worth the hassle
        IEnumerable<string> data = File.ReadLines(
            filePath.FullName,
            Encoding.GetEncoding("windows-1254") // windows-turkish since akbank seems to encode it in it for some reason
        );
#pragma warning restore CA1849
        IDbContextTransaction dbTransaction = await context.Database.BeginTransactionAsync();

        try
        {
            Bank akbank = await GetAkbankBankAsync(context);
            Card cardFromStatement = await GetAkbankCardAsync(data.First(), akbank, context);
            IList<CardTransaction> cardTransactions = GetCardTransactions( // TODO: add duplicate item prevantion in case of
                GetCardTransactionLines(data), //               importing current months statement multiple times
                cardFromStatement
            );

            await context.CardTransactions.AddRangeAsync(cardTransactions);
            _ = await context.SaveChangesAsync();
            return (cardTransactions, dbTransaction);
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync();
            dbTransaction.Dispose();
            throw;
        }
    }

    public static async Task<Bank> GetAkbankBankAsync(BankDataContext context)
    {
        Bank? akbank = context.Banks.FirstOrDefault(static b => b.Name == "Akbank");
        if (akbank is null)
        {
            akbank = new Bank() { Name = "Akbank" };
            _ = await context.Banks.AddAsync(akbank);
            _ = await context.SaveChangesAsync();
        }
        return akbank;
    }

    public static async Task<Card> GetAkbankCardAsync(
        string cardLine,
        Bank akbank,
        BankDataContext context
    )
    {
        short cardLast4Digits = GetCardLast4Digits(cardLine);
        Card? cardFromStatement = context.Cards.FirstOrDefault(c => c.Id == cardLast4Digits);
        if (cardFromStatement is null)
        {
            cardFromStatement = new()
            {
                Id = cardLast4Digits,
                Name = GetCardName(cardLine),
                IssuedBank = akbank,
            };
            _ = await context.AddAsync(cardFromStatement);
            _ = await context.SaveChangesAsync();
        }

        return cardFromStatement;
    }

    public static IList<CardTransaction> GetCardTransactions(
        IEnumerable<string> cardTransactionLines,
        Card cardFromStatement
    )
    {
        List<CardTransaction> cardTransactions = [];
        string cardTransactionCategory = "";
        foreach (string cardTransactionLine in cardTransactionLines)
        {
            CardTransaction? transaction = GetCardTransaction(
                cardTransactionLine,
                cardFromStatement
            );
            // TODO: add transaction category to the model
            if (transaction is null)
            {
                // read category lines ex.:
                // ";      SUPERMARKET;0,00 TL;0 TL / 0;"
                // note: 0TL part is just empty data it doesn't mean all SUPERMARKET transactions costed 0 TL
                cardTransactionCategory = string.Concat(
                    cardTransactionLine.Split(";")[1].SkipWhile(static c => c == ' ')
                );
                continue;
            }
            cardTransactions.Add(transaction);
        }
        return cardTransactions;
    }

    public static IEnumerable<string> GetCardTransactionLines(IEnumerable<string> lines)
    {
        return lines
            .SkipWhile(static l => !l.StartsWith("Tarih", false, CultureInfo.InvariantCulture))
            .Skip(1) // skip until and the "Tarih;Açıklama;Tutar;Chip Para / Mil;" line
            .TakeWhile(static l => l.Contains(';', StringComparison.InvariantCulture)); // last lines doesn't contain semicolons
    }

    // parses transaction info csv line and returns CardTransaction
    // example csv lines:
    // - "8.07.2025;[Redacted]             [Redacted(city)]         TR;65,00 TL;0 TL / 0;"
    // - "17.06.2025;Chip-Para ile Ödeme;-133,60 TL;-133,60 TL / 0;"
    //
    // returns null if first column is null like in sector columns ex. ";   TURISM AND ENTERTAINMENT;0,00 TL;0 TL / 0;"
    // schema of the line is : Tarih|Açıklama|Tutar|Chip Para / Mil
    public static CardTransaction? GetCardTransaction(string line, Card card)
    {
        string[] columns = line.Split(";");

        if (string.IsNullOrEmpty(columns[0]))
        {
            return null;
        }

        int[] dateParts = [.. columns[0].Split(".").Select(int.Parse)];
        DateOnly transactionDate = new(day: dateParts[0], month: dateParts[1], year: dateParts[2]);

        string comment = columns[1];

        // if it has country code it is in the last part
        // like in "******    *****       TR"
        Country? country = Country.GetCountry(
            string.Concat(comment.Reverse().TakeWhile(static c => c != ' ').Reverse())
        );

        long amountInMinorUnit = long.Parse(
            string.Concat(
                columns[2]
                    .TakeWhile(static c => c != ' ')
                    .Where(static c => c is not '.' and not ',')
            ),
            CultureInfo.InvariantCulture
        );

        Currency currency =
            Currency.GetCurrency(
                string.Concat(columns[2].Reverse().TakeWhile(static c => c != ' ').Reverse())
            )
            ?? new Currency
            {
                CurrencyCode = "TRY",
                Symbol = "TL",
                MinorUnitFractions = 2,
            };

        return new()
        {
            TransactionDate = transactionDate,
            Comment = comment,
            AmountInMinorUnit = amountInMinorUnit,
            CurrencyCode = currency.CurrencyCode,
            Currency = currency,
            Country = country,
            CountryAlpha3Code = country?.Alpha3Code,
            Card = card,
        };
    }

    // gets card name and last 4 digits of card number from
    // first line of csv data
    //
    // example data "Kart Türü / No:;Some Axes Card / **** **** **** 1234;"
    public static string GetCardName(string data)
    {
        string[] cardNameAndNo = data.Split(";")[1].Split("/");
        string cardName = cardNameAndNo[0];
        cardName = string.Concat(cardName.Take(cardName.Length - 1));
        return cardName;
    }

    public static short GetCardLast4Digits(string data)
    {
        string[] cardNameAndNo = data.Split(";")[1].Split("/");
        string last4Digits = string.Concat(cardNameAndNo[1].Skip(16).Take(4));
        return short.Parse(last4Digits, CultureInfo.InvariantCulture);
    }

    public string[] SupportedFileExtensions()
    {
        return [".csv"];
    }
}
