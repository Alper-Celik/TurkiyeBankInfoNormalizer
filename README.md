<!--
SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>

SPDX-License-Identifier: Apache-2.0
-->

# Türkiye Bank Info Normalizer

[![codecov](https://codecov.io/gh/Alper-Celik/TurkiyeBankInfoNormalizer/graph/badge.svg?token=zgFqShKXLG)](https://codecov.io/gh/Alper-Celik/TurkiyeBankInfoNormalizer)

A .NET project for importing, normalizing, and exporting Turkish bank data, The 
project is modular, supporting various importers and exporters for different data formats.

## Features
- Import bank data from multiple sources
- Export data in various formats (e.g., CSV)
- Extensible via dependency injection
- Console UI for command-line operations

## Project Structure
- `ConsoleUi/` - Command-line interface for interacting with the application
- `DependencyInjection/` - Dependency injection setup and service registration
- `Exporters/` - Data exporters (e.g., CSV)
- `Importers/` - Data importers for different banks
- `Models/` - Core data models (Card, CardTransaction, etc.)

## License
This project follows [REUSE specification](https://reuse.software/)
for license information

See the `LICENSES/` directory for used licenses

Most of the code is licensed under apache 2.0 license except some build files are licensed under mit license

See file headers and .license files for licenses of files

