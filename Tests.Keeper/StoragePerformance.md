# JSON versus CSV performance experiment

Run `StoragePerformanceTests.CompareJsonAndCsvSaveLoadPerformance` from Test Explorer, preferably in Release without a debugger and without other workloads. It is categorized as `Performance` and is nonparallel.

From the solution directory:

```powershell
dotnet test Tests.Keeper/Tests.Keeper.csproj --configuration Release --filter "FullyQualifiedName=Tests.Keeper.StoragePerformanceTests.CompareJsonAndCsvSaveLoadPerformance" --logger "console;verbosity=detailed" --logger "trx;LogFileName=storage-performance.trx" --results-directory Tests.Keeper/TestResults/StoragePerformance
```

The full text table is written to test output and to UTF-8 `Tests.Keeper/TestResults/StoragePerformance/<run-id>/report.txt`. This original report is retained outside MSTest's temporary deployment directories, even when a different `--results-directory` is used. It is also registered as a test attachment during synchronous test cleanup. The test output includes the original report's absolute path. Each run has its own report folder.

The experiment uses `JsonFiles` and `CsvFiles` from the test project, located by walking up from the test assembly directory rather than relying on the shell's working directory. Both must contain exports of the same database. All lists must load and have matching item counts; this is a basic comparability check, not field-by-field equivalence validation. SHA-256 checks verify that the source files remain unchanged.

There are 3 warm-up rounds and 20 measured rounds per operation and format, adjustable through constants in the test. JSON/CSV order alternates to reduce ordering bias. Loading and saving are timed separately using the existing production APIs. Saving uses independent copies of one canonical JSON-loaded model and new empty temporary folders for each trial, never the source fixtures. Preparation, validation, report writing, and cleanup are outside the timed intervals. Generated exports are reloaded and their list counts checked.

The table has four rows (JSON and CSV for loading and saving) and shows minimum, median, mean, and maximum milliseconds, followed by load/save comparisons based on median times. It does not assert which format must win. Fixture byte sizes, collection counts, runtime, and build configuration are included.

These are warm-cache wall-clock measurements: Windows filesystem caches are not cleared and writes are not forced to durable disk. They compare the implementations as they are: JSON uses indented serialization and includes public fields and readable Unicode, and CSV uses CsvHelper with headers, dotted nested columns and invariant value conversion. Results are not pure serializer throughput or cold-disk latency. Debug builds and running under a debugger make the measurements less representative.

The existing assembly initializer still prepares its in-memory database before this test; that work is not timed. No production saver, loader, or fixture files are changed.
