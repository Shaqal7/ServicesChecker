# ServicesChecker - Plan Dalszego Rozwoju

## Kontekst

ServicesChecker to desktopowa aplikacja Avalonia UI (.NET 10) do monitorowania usług Windows, endpointów REST, plików logów i kontenerów Docker. Aplikacja jest w 95% ukończona (360 testów, 357 przechodzących, CI/CD na GitHub Actions). Ten plan przedstawia kierunki dalszego rozwoju - od quick-winów po duże nowe funkcjonalności.

---

## Tier 1: Szybkie poprawki (wysoka wartość, niski nakład)

### 1.1 Sanityzacja wejścia Docker CLI [SECURITY]
- **Problem**: `DockerContainerManager.cs` przekazuje `containerId` bezpośrednio do shella (`$"start {containerId}"`) - ryzyko command injection
- **Rozwiązanie**: Walidacja formatu ID kontenera (12-znakowy hex) przed przekazaniem do `Process.Start`
- **Pliki**: `Infrastructure/Services/DockerContainerManager.cs`, testy
- **Złożoność**: Mała

### 1.2 Naprawa polskich stringów błędów
- **Problem**: 2 komunikaty w `ServicesTabViewModel.cs` (linie 239, 268) są po polsku, reszta po angielsku
- **Rozwiązanie**: Ujednolicenie języka (angielski) lub ekstrakcja do zasobów `.resx`
- **Pliki**: `UI/ViewModels/ServicesTabViewModel.cs`
- **Złożoność**: Mała

### 1.3 Usunięcie martwego kodu
- **Problem**: `FileSizeConverter.cs` i `BoolToVisibilityConverter.cs` nie są nigdzie referencjonowane
- **Rozwiązanie**: Usunięcie obu plików
- **Złożoność**: Mała

### 1.4 Ekstrakcja zduplikowanego FormatBytes
- **Problem**: Logika formatowania bajtów zduplikowana 3x: `LogFileItemViewModel`, `StatisticsTabViewModel`, `FileSizeConverter`
- **Rozwiązanie**: Wyciągnięcie do wspólnej klasy utility (np. `FormatHelper` w Domain)
- **Złożoność**: Mała

### 1.5 Podpięcie interwałów odświeżania z ustawień
- **Problem**: `AppSettings` ma `ServiceRefreshIntervalSeconds` i `StatisticsRefreshIntervalSeconds`, ale ViewModele hardkodują 5000/10000/30000ms
- **Rozwiązanie**: Odczyt wartości z `ISettingsRepository` w `StartAutoRefresh()`, dodanie `ConfigurationRefreshIntervalSeconds` do `AppSettings`
- **Pliki**: 3 ViewModele + `AppSettings.cs`
- **Złożoność**: Mała

### 1.6 Naprawa wzorca Dispose
- **Problem**: `StatisticsTabViewModel.StopAutoRefresh()` ustawia `_refreshCts = null` bez `Dispose()`, w przeciwieństwie do pozostałych ViewModeli
- **Rozwiązanie**: Dodanie `_refreshCts?.Dispose()` + opcjonalnie `IDisposable` na `ViewModelBase`
- **Złożoność**: Mała

---

## Tier 2: Szybkie ulepszenia UX (umiarkowana wartość, mały-średni nakład)

### 2.1 Ekstrakcja kontrolki ErrorBanner
- **Problem**: Identyczny 20+ linijkowy blok AXAML bannera błędów zduplikowany w 3 widokach
- **Rozwiązanie**: Nowy `UserControl` ErrorBanner (analogicznie do `StatusIndicator`)
- **Złożoność**: Mała

### 2.2 Skróty klawiszowe
- `F5` - Odśwież aktywną zakładkę
- `Ctrl+N` - Dodaj nową usługę / Przeglądaj pliki logów
- `Ctrl+F` - Focus na filtr usług
- `Delete` - Usuń zaznaczony element
- `Ctrl+1/2/3` - Przełączanie zakładek
- **Złożoność**: Mała

### 2.3 Dialogi potwierdzenia dla destrukcyjnych operacji
- **Problem**: "Delete All From Disk", "Remove", "Stop container" wykonują się natychmiast
- **Rozwiązanie**: Dialog potwierdzenia przed destrukcyjnymi akcjami
- **Złożoność**: Mała

### 2.4 Operacje grupowe (Start All / Stop All / Restart All)
- **Problem**: Start/Stop wymaga kliknięcia każdej usługi osobno
- **Rozwiązanie**: Przyciski na toolbarze + opcja "na filtrowanych" usługach, z per-service `IsBusy` locking
- **Złożoność**: Mała-Średnia

### 2.5 In-place updates dla zakładki Configuration
- **Problem**: `ConfigurationTabViewModel.LoadLogFilesAsync()` przebudowuje całą kolekcję co 10s (flickering, utrata zaznaczenia)
- **Rozwiązanie**: Analogiczny refactoring jak dla Services tab (in-place property updates)
- **Pliki**: `ConfigurationTabViewModel.cs`, `LogFileItemViewModel.cs`
- **Złożoność**: Mała-Średnia

---

## Tier 3: Infrastruktura fundamentalna (wysoka wartość długoterminowa)

### 3.1 Strukturalne logowanie (Serilog)
- **Problem**: Zero logowania - błędy są albo wyświetlane użytkownikowi, albo połykane
- **Rozwiązanie**: `Microsoft.Extensions.Logging` + Serilog z zapisem do pliku `app.log`
- **Wpływ**: Inject `ILogger<T>` do każdego serwisu i ViewModelu, zamiana cichych `catch` na `LogWarning/LogError`
- **Złożoność**: Średnia
- **Zależności**: Brak (ale umożliwia wiele kolejnych feature'ów)

### 3.2 Mechanizm retry (Polly)
- **Problem**: REST checks, Docker CLI i GitHub update - jeden błąd = natychmiastowy "Unavailable/Error"
- **Rozwiązanie**: Polly retry policies z exponential backoff dla HTTP klientów i Docker CLI
- **Złożoność**: Średnia
- **Zależności**: 3.1 (logowanie, żeby retry były obserwowalne)

### 3.3 Scentralizowana obsługa błędów z Correlation IDs
- **Problem**: Niespójna obsługa błędów - różne wzorce w różnych miejscach
- **Rozwiązanie**: `OperationResult<T>` pattern + ID korelacji w logach i komunikatach
- **Złożoność**: Średnia
- **Zależności**: 3.1

---

## Tier 4: Istotne funkcjonalności UX (wysoka wartość, średni nakład)

### 4.1 Ikona w zasobniku systemowym (System Tray)
- **Opis**: Minimalizacja do tray, kontekstowe menu (Show/Hide/Exit), kolor ikony wg stanu usług (zielony/czerwony)
- **Technologia**: Avalonia `TrayIcon` w `App.axaml`
- **Złożoność**: Średnia

### 4.2 Powiadomienia o zmianie statusu
- **Opis**: Toast/balloniki gdy usługa zmieni stan (Running→Stopped, Available→Unavailable)
- **Implementacja**: Śledzenie poprzedniego statusu w `ServiceItemViewModel`, Avalonia `WindowNotificationManager`
- **Złożoność**: Średnia
- **Zależności**: 4.1 (balloniki w tray gdy zminimalizowane), 3.1 (logowanie zdarzeń)

### 4.3 Konfigurowalne interwały odświeżania z UI
- **Opis**: Panel ustawień (flyout z ikony zębatki w headerze) - suwaki/inputy dla interwałów
- **Złożoność**: Średnia
- **Zależności**: 1.5 (podpięcie interwałów z ustawień)

### 4.4 Import/Export konfiguracji
- **Opis**: Export 3 plików JSON do ZIP, import z potwierdzeniem nadpisania
- **Nowy interfejs**: `IConfigurationExportService` w Application
- **Złożoność**: Średnia

---

## Tier 5: Duże nowe funkcjonalności (transformacyjne)

### 5.1 Dashboard / Zakładka podsumowania
- **Opis**: Nowa zakładka (domyślna) z widokiem "na pierwszy rzut oka": ile usług działa/nie działa, REST up/down, status Docker, miejsce na dysku
- **Layout**: Karty z kolorowymi wskaźnikami, kliknięcie przenosi do właściwej zakładki
- **Warstwy**: UI (nowy `DashboardTabViewModel` + `DashboardTabView`), aktualizacja `MainWindow.axaml`
- **Złożoność**: Średnia-Duża

### 5.2 Grupowanie usług / Kategorie
- **Opis**: Definiowanie grup (np. "Production Web", "Database", "Background Workers") ze zwijalnymi sekcjami w DataGrid
- **Zmiany Domain**: `string? GroupName` w `ServiceInfo`
- **UI**: `CollectionViewSource` grouping, UI do zarządzania grupami
- **Złożoność**: Średnia-Duża
- **Zależności**: 2.4 (batch ops per-group)

### 5.3 Śledzenie czasu odpowiedzi REST (basic → advanced)
- **Basic**: Dodanie `ResponseTimeMs` do `ServiceInfo`, `Stopwatch` w `RestEndpointChecker`, nowa kolumna w DataGrid (zielony <200ms, żółty 200-1000ms, czerwony >1000ms)
- **Advanced**: Historia z wykresem (sparkline), wymaga 5.5 (baza danych)
- **Uwaga**: `CheckHealthWithResponseAsync` już istnieje ale nie jest używany - gotowa infrastruktura
- **Złożoność**: Średnia (basic) / Duża (z historią)

### 5.4 Przeglądarka plików logów (tail -f)
- **Opis**: Wbudowany podgląd ostatnich N linii pliku logu z auto-odświeżaniem (FileSystemWatcher)
- **Nowy interfejs**: `ILogFileReaderService` z efektywnym czytaniem od końca pliku
- **UI**: Split-pane w zakładce Configuration lub pop-up okno
- **Dodatkowe**: Wyszukiwanie tekstu w podglądzie, podświetlanie składni logów
- **Złożoność**: Duża

### 5.5 Baza danych historii (LiteDB)
- **Opis**: Embedded NoSQL baza do przechowywania historii statusów, czasu odpowiedzi, audit log operacji
- **Technologia**: LiteDB (single-file, in-process, pasuje do portable deployment)
- **Nowe encje**: `HistoryEntry`, `AuditLogEntry`
- **Złożoność**: Duża
- **Umożliwia**: Zaawansowaną wersję 5.3, dashboard z trendami, raportowanie

### 5.6 Zaplanowane operacje
- **Opis**: Harmonogram start/stop usług (np. "Wyłącz dev services o 18:00", "Uruchom DB o 8:00 w dni robocze")
- **Technologia**: `System.Threading.Timer` lub Quartz.NET
- **Złożoność**: Duża
- **Zależności**: 3.1 (logowanie), 4.2 (powiadomienia)

---

## Tier 6: Strategiczne / Długoterminowe

### 6.1 Cachowanie odczytów repozytorium
- Warstwa cache w pamięci nad JSON repozytoriami, redukcja I/O dysku podczas auto-refresh

### 6.2 Cross-platform
- Abstrakcja platformy (`IPlatformService`), `NullWindowsServiceManager` na Linux/macOS
- Docker i REST monitoring działają cross-platform już teraz
- Avalonia jest cross-platform - potrzeba tylko abstrakcji Windows-specific

### 6.3 Pełna lokalizacja (PL/EN)
- System zasobów `.resx` dla wszystkich stringów UI
- Przełącznik języka w ustawieniach

---

## Podsumowanie priorytetów

| Tier | Charakter | Elementy | Szacowany nakład |
|------|-----------|----------|-----------------|
| **1** | Quick wins & security | 6 elementów | 1-2 dni |
| **2** | UX polish | 5 elementów | 2-3 dni |
| **3** | Fundamenty infrastruktury | 3 elementy | 3-5 dni |
| **4** | Istotne UX features | 4 elementy | 4-6 dni |
| **5** | Duże nowe funkcjonalności | 6 elementów | 2-4 tygodnie |
| **6** | Strategiczne | 3 elementy | Długoterminowo |

## Rekomendacja

Zacznij od **Tier 1** (security fix + cleanup + quick wins), potem **Tier 2** (UX polish), następnie **Tier 3** (logowanie jako fundament). Z Tier 5 najbardziej wartościowe dla codziennego użytkowania to **5.1 Dashboard** i **5.3 Response time tracking** (basic).
