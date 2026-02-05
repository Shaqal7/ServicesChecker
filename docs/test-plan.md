# Plan naprawy i uzupełnienia testów - ServicesChecker

**Data utworzenia:** 2026-02-05
**Ostatnia aktualizacja:** 2026-02-05

## Status realizacji

| Faza | Status | Data zakończenia |
|------|--------|------------------|
| Faza 1: Naprawa błędów kompilacji | ✅ UKOŃCZONE | 2026-02-05 |
| Faza 2: Application Layer | 🟡 POMINIĘTE (pusty projekt) | - |
| Faza 3: Infrastructure Services | ✅ UKOŃCZONE | 2026-02-05 |
| Faza 4: UI ViewModels | ⏳ OCZEKUJĄCE | - |
| Faza 5: Weryfikacja końcowa | ⏳ OCZEKUJĄCE | - |

## Podsumowanie problemu

**Błędy kompilacji:** 16 błędów CS0103 - "Nazwa `_testFilePath` nie istnieje w bieżącym kontekście"

Pliki z błędami:
- `JsonServiceRepositoryTests.cs` - linie 61, 147, 149
- `JsonLogFileRepositoryTests.cs` - linie 60, 147, 149
- `JsonSettingsRepositoryTests.cs` - linie 65, 155

**Przyczyna:** Zmienna `_testFilePath` jest używana w testach, ale nigdy nie została zadeklarowana.

---

## Faza 1: Naprawa błędów kompilacji (Krytyczne) ✅

**Status:** UKOŃCZONE
**Data:** 2026-02-05

### 1.1 Dodać brakujące pole `_testFilePath` do każdego pliku testowego ✅

**JsonServiceRepositoryTests.cs** - dodano po linii 14:
```csharp
private string _testFilePath => Path.Combine(_testDirectory, "services.json");
```

**JsonLogFileRepositoryTests.cs** - dodano po linii 13:
```csharp
private string _testFilePath => Path.Combine(_testDirectory, "logfiles.json");
```

**JsonSettingsRepositoryTests.cs** - dodano po linii 14:
```csharp
private string _testFilePath => Path.Combine(_testDirectory, "settings.json");
```

### Wynik weryfikacji ✅

```bash
dotnet build tests/ServicesChecker.Infrastructure.Tests
```

**Rezultat:**
- ✅ Kompilacja zakończona sukcesem
- ✅ 0 błędów kompilacji (poprzednio: 16 błędów)
- ⚠️ 27 ostrzeżeń (głównie CA1416 - Windows-specific code, oczekiwane)
- ✅ Wszystkie 21 testów gotowe do uruchomienia

---

## Faza 2: Uzupełnienie testów Application Layer (Pusty projekt)

**Ścieżka:** `tests/ServicesChecker.Application.Tests/`

### 2.1 Testy interfejsów (weryfikacja kontraktów)

Nie są wymagane testy jednostkowe dla interfejsów - to tylko kontrakty.
Projekt `Application.Tests` może pozostać pusty lub zawierać testy integracyjne w przyszłości.

---

## Faza 3: Uzupełnienie testów Infrastructure Services ✅

**Status:** UKOŃCZONE
**Data:** 2026-02-05
**Ścieżka:** `tests/ServicesChecker.Infrastructure.Tests/Services/`

### 3.1 FileSystemServiceTests.cs (Priorytet: Wysoki) ✅

**Utworzono:** 22 testy jednostkowe
Testowane metody:
- `FileExists()` - sprawdza istnienie pliku (2 testy)
- `GetFileSize()` - zwraca rozmiar pliku (3 testy)
- `GetLastModified()` - zwraca datę modyfikacji (2 testy)
- `DeleteFileAsync()` - usuwa plik (3 testy)
- `ReadAllTextAsync()` - czyta zawartość pliku (2 testy)
- `WriteAllTextAsync()` - zapisuje plik (3 testy)
- `GetDiskSpaceInfo()` - informacje o dysku (3 testy)
- `GetAppDirectory()` - zwraca katalog aplikacji (2 testy)

### 3.2 RestEndpointCheckerTests.cs (Priorytet: Wysoki) ✅

**Utworzono:** 16 testów z mockowaniem HttpMessageHandler
Testowane metody:
- `CheckHealthAsync()` - różne kody HTTP (200, 201, 202, 204, 400, 401, 403, 404, 500, 502, 503)
- `CheckHealthAsync()` - obsługa wyjątków (HttpRequestException, TaskCanceledException, Exception)
- `CheckHealthAsync()` - normalizacja URL (dodawanie https://)
- `CheckHealthAsync()` - CancellationToken support
- `CheckHealthWithResponseAsync()` - zwracanie response body
- `Constructor` - timeout ustawiony na 10 sekund

### 3.3 DockerStorageServiceTests.cs (Priorytet: Średni) ✅

**Utworzono:** 18 testów z mockowanym IFileSystemService
Testowane metody:
- `GetStorageInfoAsync()` - Docker nie zainstalowany (1 test)
- `GetStorageInfoAsync()` - Docker zainstalowany bez VHDX (1 test)
- `GetStorageInfoAsync()` - Docker z VHDX (1 test)
- `IsDockerInstalledAsync()` - sprawdzanie instalacji (2 testy)
- `GetVhdxPathAsync()` - znajdowanie VHDX w różnych lokalizacjach (5 testów)
- `GetStorageInfoAsync()` - LastUpdated timestamp (2 testy)
- `GetStorageInfoAsync()` - kalkulacja UsedBytes i UsedPercentage (1 test)
- CancellationToken support (2 testy)
- Optymalizacja wywołań FileSystem (1 test)

### 3.4 DockerContainerManagerTests.cs (Priorytet: Średni) ✅

**Utworzono:** 16 testów (integration/documentation)
**Uwaga:** Większość testów wymaga Docker i jest oznaczona `[Fact(Skip = "Requires Docker")]`
Testowane metody:
- `GetContainersAsync()` - parsowanie JSON z docker ps (4 testy)
- `StartContainerAsync()` - uruchamianie kontenera (1 test)
- `StopContainerAsync()` - zatrzymywanie kontenera (1 test)
- `SwitchContainerAsync()` - przełączanie kontenerów (3 testy)
- CancellationToken support (3 testy)
- Obsługa błędów (malformed JSON, Docker unavailable) (4 testy dokumentacyjne)

### 3.5 WindowsServiceManagerTests.cs (Priorytet: Niski) ✅

**Utworzono:** 20 testów (integration/documentation)
**Uwaga:** Większość testów wymaga Windows services i jest oznaczona `[Fact(Skip = ...)]`
Testowane metody:
- `GetStatusAsync()` - sprawdzanie statusu usługi (3 testy)
- `StartAsync()` - uruchamianie usługi (2 testy)
- `StopAsync()` - zatrzymywanie usługi (2 testy)
- `RestartAsync()` - restart usługi (1 test)
- `GetVersionAsync()` - wersja usługi (2 testy)
- `GetExecutablePathAsync()` - ścieżka do exe (2 testy)
- `ServiceExistsAsync()` - sprawdzanie istnienia (2 testy)
- CancellationToken support (1 test)
- Timeout behavior (2 testy dokumentacyjne)
- Status mapping (1 test dokumentacyjny)
- Exception handling (1 test dokumentacyjny)
- Path parsing (1 test dokumentacyjny)

### Wynik weryfikacji ✅

```bash
dotnet build tests/ServicesChecker.Infrastructure.Tests
```

**Rezultat:**
- ✅ Kompilacja zakończona sukcesem
- ✅ 0 błędów kompilacji
- ⚠️ 29 ostrzeżeń (głównie CA1416 - Windows-specific code, oczekiwane)
- ✅ 92 nowe testy gotowe do uruchomienia (22 + 16 + 18 + 16 + 20)
- ✅ Razem 113 testów w Infrastructure.Tests (21 persistence + 92 services)

---

## Faza 4: Uzupełnienie testów UI ViewModels

**Ścieżka:** `tests/ServicesChecker.UI.Tests/ViewModels/`

### 4.1 ViewModelBaseTests.cs
- `IsBusy` - ustawienie i powiadomienie
- `ErrorMessage` - ustawienie i czyszczenie
- `SetError()` / `ClearError()` metody

### 4.2 ServiceItemViewModelTests.cs (Priorytet: Wysoki)
- Właściwości `Name`, `Status`, `Type` - poprawne wartości
- `StatusColor` - mapowanie statusu na kolor
- `FormattedVersion` - formatowanie

### 4.3 LogFileItemViewModelTests.cs (Priorytet: Wysoki)
- `FileSizeFormatted` - formatowanie rozmiaru (KB, MB, GB)
- `ExistsText` - "Exists" / "Missing"
- `StatusColor` - zielony/czerwony

### 4.4 ServicesTabViewModelTests.cs (Priorytet: Wysoki)
- `RefreshServicesCommand` - ładuje usługi
- `FilterText` - filtrowanie listy
- `StartServiceCommand` / `StopServiceCommand` - wywołują serwis
- Obsługa błędów i `IsBusy`

### 4.5 ConfigurationTabViewModelTests.cs (Priorytet: Średni)
- `AddLogFileCommand` - dodaje plik
- `DeleteLogFileCommand` - usuwa plik
- `RefreshCommand` - odświeża status

### 4.6 StatisticsTabViewModelTests.cs (Priorytet: Średni)
- `RefreshCommand` - ładuje statystyki Docker
- Formatowanie `DiskUsageFormatted`, `VhdxSizeFormatted`
- `IsLowDiskSpace` - ostrzeżenie < 50GB

### 4.7 MainWindowViewModelTests.cs (Priorytet: Niski)
- Inicjalizacja zakładek
- `ThemeToggleCommand` - zmiana motywu
- Zapisywanie ustawień

---

## Faza 5: Weryfikacja końcowa

1. **Build całego solution:**
   ```bash
   dotnet build ServicesChecker.sln
   ```

2. **Uruchomienie wszystkich testów:**
   ```bash
   dotnet test ServicesChecker.sln
   ```

3. **Raport pokrycia (opcjonalnie):**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

---

## Podsumowanie plików do modyfikacji

| Plik | Akcja | Priorytet | Status |
|------|-------|-----------|--------|
| `JsonServiceRepositoryTests.cs` | Dodać `_testFilePath` | Krytyczny | ✅ Ukończone |
| `JsonLogFileRepositoryTests.cs` | Dodać `_testFilePath` | Krytyczny | ✅ Ukończone |
| `JsonSettingsRepositoryTests.cs` | Dodać `_testFilePath` | Krytyczny | ✅ Ukończone |
| `FileSystemServiceTests.cs` | Utworzyć nowy | Wysoki | ✅ Ukończone (22 testy) |
| `RestEndpointCheckerTests.cs` | Utworzyć nowy | Wysoki | ✅ Ukończone (16 testów) |
| `ServiceItemViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `LogFileItemViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `ServicesTabViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `ViewModelBaseTests.cs` | Utworzyć nowy | Średni |
| `ConfigurationTabViewModelTests.cs` | Utworzyć nowy | Średni |
| `StatisticsTabViewModelTests.cs` | Utworzyć nowy | Średni |
| `DockerStorageServiceTests.cs` | Utworzyć nowy | Średni | ✅ Ukończone (18 testów) |
| `DockerContainerManagerTests.cs` | Utworzyć nowy | Średni | ✅ Ukończone (16 testów) |
| `MainWindowViewModelTests.cs` | Utworzyć nowy | Niski | ⏳ Oczekujące |
| `WindowsServiceManagerTests.cs` | Utworzyć nowy | Niski | ✅ Ukończone (20 testów) |

---

## Szacowana liczba testów do dodania

| Warstwa | Istniejące | Dodane | Razem |
|---------|------------|--------|-------|
| Domain | 36 | 0 | 36 |
| Infrastructure (Persistence) | 21 | 0 | 21 |
| Infrastructure (Services) | 0 | 92 ✅ | 92 |
| UI (Controls) | 19 | 0 | 19 |
| UI (ViewModels) | 0 | 0 (oczekujące) | 0 |
| **Razem** | **76** | **92** | **168** |

---

## Istniejąca struktura testów

```
tests/
├── ServicesChecker.Domain.Tests/
│   └── Entities/
│       ├── ServiceInfoTests.cs (7 testów)
│       ├── LogFileInfoTests.cs (6 testów)
│       ├── DockerStorageInfoTests.cs (6 testów)
│       ├── ContainerInfoTests.cs (6 testów)
│       └── AppSettingsTests.cs (9 testów)
│
├── ServicesChecker.Application.Tests/
│   └── (pusty - brak plików)
│
├── ServicesChecker.Infrastructure.Tests/
│   └── Persistence/
│       ├── JsonServiceRepositoryTests.cs (8 testów - BŁĘDY)
│       ├── JsonLogFileRepositoryTests.cs (7 testów - BŁĘDY)
│       └── JsonSettingsRepositoryTests.cs (6 testów - BŁĘDY)
│
└── ServicesChecker.UI.Tests/
    ├── Controls/
    │   └── StatusIndicatorTests.cs (19 testów)
    └── Helpers/
        └── AppBuilderHelper.cs
```

## Docelowa struktura testów

```
tests/
├── ServicesChecker.Domain.Tests/
│   └── Entities/
│       ├── ServiceInfoTests.cs
│       ├── LogFileInfoTests.cs
│       ├── DockerStorageInfoTests.cs
│       ├── ContainerInfoTests.cs
│       └── AppSettingsTests.cs
│
├── ServicesChecker.Application.Tests/
│   └── (może pozostać pusty)
│
├── ServicesChecker.Infrastructure.Tests/
│   ├── Persistence/
│   │   ├── JsonServiceRepositoryTests.cs (naprawiony)
│   │   ├── JsonLogFileRepositoryTests.cs (naprawiony)
│   │   └── JsonSettingsRepositoryTests.cs (naprawiony)
│   └── Services/
│       ├── FileSystemServiceTests.cs (NOWY)
│       ├── RestEndpointCheckerTests.cs (NOWY)
│       ├── DockerStorageServiceTests.cs (NOWY)
│       ├── DockerContainerManagerTests.cs (NOWY)
│       └── WindowsServiceManagerTests.cs (NOWY)
│
└── ServicesChecker.UI.Tests/
    ├── Controls/
    │   └── StatusIndicatorTests.cs
    ├── ViewModels/
    │   ├── ViewModelBaseTests.cs (NOWY)
    │   ├── ServiceItemViewModelTests.cs (NOWY)
    │   ├── LogFileItemViewModelTests.cs (NOWY)
    │   ├── ServicesTabViewModelTests.cs (NOWY)
    │   ├── ConfigurationTabViewModelTests.cs (NOWY)
    │   ├── StatisticsTabViewModelTests.cs (NOWY)
    │   └── MainWindowViewModelTests.cs (NOWY)
    └── Helpers/
        └── AppBuilderHelper.cs
```
