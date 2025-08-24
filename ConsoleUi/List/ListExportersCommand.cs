// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using System.CommandLine;
using Models.ExporterInterfaces;

namespace ConsoleUi.List;

public class ListExportersCommand : Command
{
    private readonly IEnumerable<ICreditCardTransactionExporter> _cardTransactionExporters;

    public ListExportersCommand(
        IEnumerable<ICreditCardTransactionExporter> cardTransactionExporters
    )
        : base("exporters", "Lists Available Exporters")
    {
        _cardTransactionExporters = cardTransactionExporters;
        this.SetAction(this.List);
    }

    private int List(ParseResult parseResult)
    {
        foreach (
            ICreditCardTransactionExporter exporter in _cardTransactionExporters.OrderBy(c =>
                c.Name
            )
        )
        {
            Console.WriteLine($"{exporter.Name} : ");
            Console.WriteLine($"\tExported File Format = {exporter.FileFormat}");
            Console.WriteLine($"\tIs It a Text Based Format = {exporter.IsTextFormat}\n");
        }

        return 0;
    }
}
