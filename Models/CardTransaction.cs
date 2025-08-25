// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Models;

public record CardTransaction
{
    [Format("o")]
    public required DateOnly TransactionDate { get; set; }

    [Format("o")]
    public TimeOnly? TransactionTime { get; set; }

    public decimal Inflow { get; set; }

    public decimal Outflow { get; set; }

    public required string Comment { get; set; }

    [HeaderPrefix("Currency.")]
    public required Currency.Currency Currency { get; set; }

    [HeaderPrefix("Country.")]
    public Country.Country? Country { get; set; }

    [HeaderPrefix("Card.")]
    public required Card Card { get; set; }

    public string? Category { get; set; }
}
