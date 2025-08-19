// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

namespace Models.ExporterInterfaces;

public interface ICreditCardTransactionExporter
{
    public string Name { get; }
    public string FileFormat { get; }
    public bool IsTextFormat { get; }

    public Task<Stream> Export(IAsyncEnumerable<CardTransaction> transactions, Stream? stream);
}
