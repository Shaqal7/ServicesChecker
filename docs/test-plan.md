# Plan naprawy i uzupełnienia testów - ServicesChecker

**Data utworzenia:** 2026-02-05
**Ostatnia aktualizacja:** 2026-02-05

## Status realizacji

| Faza | Status | Data zakończenia |
|------|--------|------------------|
| Faza 1: Naprawa błędów kompilacji | ✅ UKOŃCZONE | 2026-02-05 |
| Faza 2: Application Layer | 🟡 POMINIĘTE (pusty projekt) | - |
| Faza 3: Infrastructure Services | ⏳ OCZEKUJĄCE | - |
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

## Faza 3: Uzupełnienie testów Infrastructure Services

**Ścieżka:** `tests/ServicesChecker.Infrastructure.Tests/Services/`

### 3.1 FileSystemServiceTests.cs (Priorytet: Wysoki)
Testowane metody:
- `GetAppDirectory()` - zwraca poprawną ścieżkę
- `FileExists()` - sprawdza istnienie pliku
- `GetFileSize()` - zwraca rozmiar pliku
- `GetLastModified()` - zwraca datę modyfikacji
- `DeleteFile()` - usuwa plik

### 3.2 RestEndpointCheckerTests.cs (Priorytet: Wysoki)
Testowane metody z mockowaniem HttpClient:
- `CheckHealthAsync()` - zwraca `ServiceStatus.Available` dla HTTP 200
- `CheckHealthAsync()` - zwraca `ServiceStatus.Unavailable` dla HTTP 500
- `CheckHealthAsync()` - zwraca `ServiceStatus.Error` dla timeout
- `CheckHealthAsync()` - obsługuje anulowanie CancellationToken

### 3.3 DockerStorageServiceTests.cs (Priorytet: Średni)
Testowane metody:
- `GetStorageInfoAsync()` - zwraca informacje gdy Docker zainstalowany
- `GetStorageInfoAsync()` - zwraca `IsDockerInstalled = false` gdy brak Docker
- `GetDiskSpaceInfoAsync()` - kalkulacja procentów i IsLowSpace

### 3.4 DockerContainerManagerTests.cs (Priorytet: Średni)
Testowane metody:
- `GetContainersAsync()` - parsuje JSON z `docker ps`
- `StartContainerAsync()` - wywołuje `docker start`
- `StopContainerAsync()` - wywołuje `docker stop`
- Obsługa błędów gdy Docker nie zainstalowany

### 3.5 WindowsServiceManagerTests.cs (Priorytet: Niski)
Trudne do testowania jednostkowo (wymaga Windows Service API).
Opcje:
- Testy integracyjne z prawdziwymi usługami
- Mock ServiceController (wymaga abstrakcji)

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
| `FileSystemServiceTests.cs` | Utworzyć nowy | Wysoki |
| `RestEndpointCheckerTests.cs` | Utworzyć nowy | Wysoki |
| `ServiceItemViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `LogFileItemViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `ServicesTabViewModelTests.cs` | Utworzyć nowy | Wysoki |
| `ViewModelBaseTests.cs` | Utworzyć nowy | Średni |
| `ConfigurationTabViewModelTests.cs` | Utworzyć nowy | Średni |
| `StatisticsTabViewModelTests.cs` | Utworzyć nowy | Średni |
| `DockerStorageServiceTests.cs` | Utworzyć nowy | Średni |
| `DockerContainerManagerTests.cs` | Utworzyć nowy | Średni |
| `MainWindowViewModelTests.cs` | Utworzyć nowy | Niski |
| `WindowsServiceManagerTests.cs` | Utworzyć nowy | Niski |

---

## Szacowana liczba testów do dodania

| Warstwa | Istniejące | Do dodania | Razem |
|---------|------------|------------|-------|
| Domain | 36 | 0 | 36 |
| Infrastructure (Persistence) | 21 | 0 | 21 |
| Infrastructure (Services) | 0 | ~30 | ~30 |
| UI (Controls) | 19 | 0 | 19 |
| UI (ViewModels) | 0 | ~50 | ~50 |
| **Razem** | **76** | **~80** | **~156** |

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
