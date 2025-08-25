// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using Models;
using Models.Country;
using Models.Currency;
using Models.ImporterInterfaces;

namespace Importers.Akbank;

public class AkbankCreditCardImporterCsv : ICreditCardImporter
{
    public string ImporterName => "akbank-cc-csv-importer";
    public string BankName => "Akbank T.A.Ş.";
    public IEnumerable<string> SupportedFileExtensions => [".csv"];

    public AkbankCreditCardImporterCsv()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public async Task<IList<CardTransaction>> Import(FileInfo filePath)
    {
        var data = File.ReadLinesAsync(
            filePath.FullName,
            Encoding.GetEncoding("windows-1254") // windows-turkish since akbank seems to encode it in it for some reason
        );

        // ReSharper disable once PossibleMultipleEnumeration
        // it is fine at most we are iterating twice
        Card cardFromStatement = GetAkbankCardAsync(await data.FirstAsync());
        IList<CardTransaction> cardTransactions = await GetCardTransactions(
            // ReSharper disable once PossibleMultipleEnumeration
            GetCardTransactionLines(data),
            cardFromStatement
        );

        return cardTransactions;
    }

    public Card GetAkbankCardAsync(string cardLine)
    {
        Card card = new()
        {
            AvailableCardNumberPart = GetCardLast4Digits(cardLine),
            Name = GetCardName(cardLine),
            IssuedBank = BankName,
        };

        return card;
    }

    public static async Task<IList<CardTransaction>> GetCardTransactions(
        IAsyncEnumerable<string> cardTransactionLines,
        Card cardFromStatement
    )
    {
        List<CardTransaction> cardTransactions = [];
        string? cardTransactionCategory = null;
        await foreach (string cardTransactionLine in cardTransactionLines)
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

            transaction.Category = cardTransactionCategory;
            cardTransactions.Add(transaction);
        }
        return cardTransactions;
    }

    public static IAsyncEnumerable<string> GetCardTransactionLines(IAsyncEnumerable<string> lines)
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
        Country country =
            Country.GetCountry(
                string.Concat(comment.Reverse().TakeWhile(static c => c != ' ').Reverse())
            ) ?? Country.GetCountry("TR")!; // Assume TR since it is a Turkey bank TODO: infer from currency

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
            Inflow = (amountInMinorUnit < 0) ? (amountInMinorUnit * -1) / 100m : 0m,
            Outflow = (amountInMinorUnit > 0) ? amountInMinorUnit / 100m : 0m,
            Currency = currency,
            Country = country,
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

    public static string? GetCardLast4Digits(string data)
    {
        string[] cardNameAndNo = data.Split(";")[1].Split("/");
        string last4Digits = string.Concat(cardNameAndNo[1].Skip(16).Take(4));
        return last4Digits;
    }
}
