// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

namespace Models.ImporterInterfaces;

public interface ICreditCardImporter
{
    public IEnumerable<string> SupportedFileExtensions { get; }
    public string ImporterName { get; }
    public string BankName { get; }

    Task<IList<CardTransaction>> Import(FileInfo filePath);
}
