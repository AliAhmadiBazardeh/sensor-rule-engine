# Sensor Rule Engine

A backend service for ingesting sensor readings, evaluating configurable rules, classifying readings, generating sustained-violation alerts, persisting processing results, and aggregating acceptable readings.

The implementation is intentionally focused on the requirements of the backend technical task. It uses a layered architecture, SQLite for persistence, `readings.jsonl` as the input source, and `rules.json` as startup seed data.

## Overview

The application processes sensor readings through the following pipeline:

```text
readings.jsonl
    |
    v
JSONL parsing
    |
    v
Semantic validation
    |
    v
Deduplication
    |
    v
Event-time ordering
    |
    +-----------------------------+
    |                             |
    v                             v
Stateless rule evaluation   SustainedAbove state
    |                             |
    +-------------+---------------+
                  |
                  v
             Classification
                  |
                  v
          Alert deduplication
                  |
                  v
            Alert cooldown
                  |
                  v
              Persistence
                  |
                  +----> Aggregation API
```

Rules are loaded from `data/rules.json` when the application starts. There is intentionally no rule-management CRUD API.

## Architecture

The solution is divided into four main layers/projects:

```text
src/
├── SensorRuleEngine.Domain
├── SensorRuleEngine.Application
├── SensorRuleEngine.Infrastructure
└── SensorRuleEngine.Api
```

### Domain

Contains business concepts and rule-engine behavior without dependencies on persistence or ASP.NET Core.

Responsibilities include:

* Sensor readings and rule entities
* Reading keys
* Rule operators
* Rule applicability
* Rule validation
* Reading validation
* Event-time ordering
* Deduplication primitives
* Reading classification
* `SustainedAbove` state management
* Alert cooldown and alert deduplication

### Application

Coordinates use cases and application workflows.

Responsibilities include:

* Reading ingestion
* Reading processing orchestration
* Rule loading abstractions
* Batch processing
* Persistence abstractions
* Aggregation query and service
* Processing reports

The Application layer depends on abstractions rather than concrete database implementations.

### Infrastructure

Contains external concerns and concrete implementations.

Responsibilities include:

* Entity Framework Core
* SQLite persistence
* Database entities and configurations
* Repository implementations
* EF Core migrations
* JSON rule loading
* Domain-to-persistence mapping

### API

Contains the ASP.NET Core host and HTTP/API integration.

Responsibilities include:

* Dependency injection composition
* Startup rule loading
* Startup reading processing
* Aggregation HTTP endpoint
* Scalar/OpenAPI integration

## Project Structure

```text
SensorRuleEngine/
├── data/
│   ├── readings.jsonl
│   └── rules.json
├── src/
│   ├── SensorRuleEngine.Api/
│   │   ├── Program.cs
│   │   └── Startup/
│   │       └── ReadingProcessingHostedService.cs
│   ├── SensorRuleEngine.Application/
│   │   ├── Aggregation/
│   │   ├── Ingestion/
│   │   ├── Persistence/
│   │   ├── Processing/
│   │   └── Rules/
│   ├── SensorRuleEngine.Domain/
│   │   ├── Alerting/
│   │   ├── Classification/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Readings/
│   │   ├── Rules/
│   │   └── ValueObjects/
│   └── SensorRuleEngine.Infrastructure/
│       ├── Persistence/
│       └── Rules/
├── tests/
│   ├── SensorRuleEngine.UnitTests/
│   └── SensorRuleEngine.IntegrationTests/
└── SensorRuleEngine.sln
```

## Input Data

### `readings.jsonl`

The input is a JSON Lines file. Each line represents one sensor reading:

```json
{"deviceId":"PUMP-01","metric":"temperature","ts":"2025-06-01T08:33:00Z","value":67.21,"seq":1199}
```

Required fields are:

| Field      | Meaning                      |
| ---------- | ---------------------------- |
| `deviceId` | Sensor/device identifier     |
| `metric`   | Measured metric              |
| `ts`       | UTC event timestamp          |
| `value`    | Numeric measurement          |
| `seq`      | Non-negative sequence number |

The JSONL parser accepts JSON property names case-insensitively.

### `rules.json`

Rules are seed data and are loaded during application startup.

Current rules are:

```json
[
  {
    "id": "overheat-pump01",
    "name": "Overheating Sustained",
    "enabled": true,
    "deviceId": "PUMP-01",
    "metric": "temperature",
    "operator": "SustainedAbove",
    "threshold": 80,
    "durationSeconds": 30
  },
  {
    "id": "high-pressure-all",
    "name": "High Pressure",
    "enabled": true,
    "metric": "pressure",
    "operator": "GreaterThan",
    "threshold": 100
  },
  {
    "id": "vibration-between",
    "name": "Vibration Normal Range",
    "enabled": true,
    "deviceId": "PUMP-02",
    "metric": "vibration",
    "operator": "Between",
    "min": 10,
    "max": 20
  }
]
```

The file is validated when loaded. If the file is missing or contains invalid rules, application startup fails instead of starting with an invalid rule set.

There is no API for creating, updating, or deleting rules. Changing rule data only requires changing `rules.json` and restarting the application.

## Reading Validation

A reading is rejected when it cannot be safely processed.

The current semantic validation checks:

* `deviceId` is required and cannot be whitespace.
* `metric` is required and cannot be whitespace.
* `ts` must have a zero UTC offset.
* `seq` must be greater than or equal to zero.

Malformed JSON and parsing failures are also rejected by the ingestion layer.

Invalid readings are not evaluated and are not persisted as sensor readings.

## Deduplication

The exact reading deduplication key is:

```text
(deviceId, metric, ts, seq)
```

Deduplication uses a `HashSet<ReadingKey>` and follows a **first-wins** policy: the first valid occurrence of a key is accepted and subsequent occurrences are ignored.

Duplicates are not evaluated, classified, or counted as new stored readings.

The database also enforces a unique constraint on the same four fields. This provides persistence-level protection in addition to application-level deduplication.

## Event-Time Ordering and Out-of-Order Data

The input file is not assumed to be ordered by timestamp.

Before rule processing, valid non-duplicate readings are ordered by:

1. `Timestamp`
2. `Sequence`

The ordering is therefore:

```csharp
.OrderBy(reading => reading.Timestamp)
.ThenBy(reading => reading.Sequence)
```

This is especially important for stateful rules such as `SustainedAbove`, because their duration is based on event time rather than file order.

## Rule Evaluation

Rules are applicable only when:

* the rule is enabled,
* the rule metric matches the reading metric,
* and, when the rule has a `deviceId`, it matches the reading device.

If no enabled rule applies to a reading, the reading is classified as acceptable.

### Supported Operators

The following operators are implemented:

* `GreaterThan`
* `GreaterThanOrEqual`
* `LessThan`
* `LessThanOrEqual`
* `Equal`
* `Between`
* `SustainedAbove`

For the stateless operators, the operator describes the **violation condition**. For example, `GreaterThan` with `threshold = 100` is violated when the reading value is greater than `100`.

`Between` uses the configured `min` and `max` values according to its rule-operator implementation and validation.

## SustainedAbove

`SustainedAbove` is stateful and is handled separately from the stateless operator resolver.

A rule contains:

```text
threshold
durationSeconds
```

The current implementation uses these semantics:

* A reading is considered above the threshold only when `value > threshold`.
* An episode starts when an above-threshold reading is observed.
* Consecutive above-threshold readings extend the episode.
* A reading at or below the threshold ends the episode.
* An alert is produced only when the elapsed event-time duration reaches the configured duration.
* An unfinished episode is checked by `Complete()` at the end of the batch.
* Peak value is tracked during the episode.

Duration is measured using event timestamps, not wall-clock processing time.

### Sustained State Key

The task conceptually describes state per `(deviceId, metric)`. The implementation uses:

```text
(ruleId, deviceId, metric)
```

This is intentional. If two different `SustainedAbove` rules apply to the same device and metric, their independent thresholds and durations must not share the same state.

## Classification

Every valid, non-duplicate reading is classified as either:

```text
Acceptable
Unacceptable
```

A reading is unacceptable when at least one applicable stateless rule is violated.

The classification contains the violated rule results and their reasons.

An empty set of applicable rule results results in an `Acceptable` classification.

`SustainedAbove` is handled as an alert-producing stateful rule rather than a per-reading stateless classification result. This prevents a sustained episode from being treated as a separate alert on every individual reading.

## Alerting

Alerts are generated for completed `SustainedAbove` violations.

An alert contains:

* Rule ID
* Device ID
* Metric
* Start timestamp
* End timestamp
* Peak value

### Alert Deduplication

Generated alerts are deduplicated before persistence.

The persistence layer also enforces a unique alert key based on:

```text
(ruleId, deviceId, metric, startTimestamp, endTimestamp)
```

### Cooldown

The configured cooldown is **5 minutes**.

Cooldown state is keyed by:

```text
(ruleId, deviceId, metric)
```

The purpose is to avoid producing multiple alerts for episodes that fall within the configured cooldown window.

The cooldown is based on alert event timestamps rather than processing time.

## Persistence and Idempotency

SQLite is used as the persistence store for this task.

The application persists:

* Sensor readings
* Rule results
* Alerts

Reading records also store their final acceptable/unacceptable classification so aggregation can query acceptable readings directly.

### Idempotent Reruns

Running the same input again does not create duplicate persisted records.

Idempotency is implemented at two levels:

1. Application-level existence checks before inserting readings, rule results, and alerts.
2. Database unique constraints that protect the corresponding natural keys.

The reading key is:

```text
(deviceId, metric, timestamp, sequence)
```

The rule-result key is:

```text
(ruleId, deviceId, metric, timestamp, sequence)
```

The alert key is:

```text
(ruleId, deviceId, metric, startTimestamp, endTimestamp)
```

The current processing model is a single-file batch workflow. The persistence implementation is designed for idempotent reruns of that batch rather than concurrent distributed writers.

## Aggregation API

The API exposes acceptable-reading aggregation through:

```text
GET /api/v1/aggregation
```

Query parameters:

| Parameter       | Required | Description                |
| --------------- | -------: | -------------------------- |
| `deviceId`      |      Yes | Device to aggregate        |
| `metric`        |      Yes | Metric to aggregate        |
| `from`          |      Yes | Inclusive UTC start        |
| `to`            |      Yes | Exclusive UTC end          |
| `bucketSeconds` |      Yes | Bucket duration in seconds |

Example:

```text
/api/v1/aggregation?deviceId=PUMP-01&metric=temperature&from=2025-06-01T08:00:00Z&to=2025-06-01T09:00:00Z&bucketSeconds=600
```

The response contains buckets with:

* `bucketStart`
* `bucketEnd`
* `count`
* `average`
* `min`
* `max`

Example:

```json
[
  {
    "bucketStart": "2025-06-01T08:00:00+00:00",
    "bucketEnd": "2025-06-01T08:10:00+00:00",
    "count": 60,
    "average": 68.675466666666666666666666667,
    "min": 67.508,
    "max": 70.112
  }
]
```

### Aggregation Semantics

Only readings classified as **acceptable** are included.

The requested time range follows the half-open interval:

```text
[from, to)
```

Therefore:

* a reading exactly at `from` is included;
* a reading exactly at `to` is excluded.

Buckets are anchored at the requested `from` value rather than at a global clock boundary.

For example, with `from = 10:03:00` and a 10-minute bucket size, the buckets start at:

```text
10:03:00
10:13:00
10:23:00
...
```

Empty buckets are returned rather than omitted:

```json
{
  "count": 0,
  "average": null,
  "min": null,
  "max": null
}
```

### SQLite DateTimeOffset Trade-off

The SQLite EF Core provider used by this project does not translate the required `DateTimeOffset` comparison and ordering operations used by the aggregation query.

The repository therefore performs the following filtering in SQLite:

* `deviceId`
* `metric`
* `IsAcceptable`

It then performs timestamp filtering and ordering in application memory.

This is an intentional trade-off for the task's small input size. The supplied dataset contains roughly 2,100 persisted readings, so the approach keeps the implementation simple while avoiding provider-specific date storage changes.

For a production-scale dataset, timestamp normalization/storage and database-side range querying would need to be revisited.

## Startup Processing

The application processes `data/readings.jsonl` automatically at startup through `ReadingProcessingHostedService`.

The startup workflow is:

1. Load and validate `rules.json`.
2. Register the rules in an in-memory rule provider.
3. Start the hosted processing service.
4. Read `readings.jsonl`.
5. Parse and validate readings.
6. Deduplicate valid readings.
7. Order readings by event time.
8. Evaluate applicable rules.
9. Classify readings.
10. Generate and deduplicate alerts.
11. Apply the five-minute alert cooldown.
12. Persist readings, rule results, and alerts.
13. Log the processing report.

## Processing Report

After processing, the application logs a report containing:

* `TotalLines`
* `Parsed`
* `Invalid`
* `Duplicates`
* `Stored`
* `RulesLoaded`
* `Evaluations`
* `Acceptable`
* `Unacceptable`
* `Violations`
* `Alerts`

For the supplied input, the current implementation has produced a report in this form:

```text
Sensor readings processing completed.
TotalLines: 2150
Parsed: 2143
Invalid: 9
Duplicates: 38
Stored: 2103
RulesLoaded: 3
Evaluations: 632
Acceptable: 2103
Unacceptable: 0
Violations: 0
Alerts: 0
```

The sample input currently does not produce a sustained alert or stateless violation under the configured rules.

## Running the Application

### Prerequisites

* .NET 8 SDK

Check the installed SDK:

```bash
dotnet --version
```

### Restore

From the repository root:

```bash
dotnet restore
```

### Database Migration

The project uses EF Core migrations.

Apply the database migrations with:

```bash
dotnet ef database update \
  --project src/SensorRuleEngine.Infrastructure \
  --startup-project src/SensorRuleEngine.Api
```

### Run

```bash
dotnet run --project src/SensorRuleEngine.Api
```

The API is exposed by the ASP.NET Core application. In the Development environment, Scalar is available for interactive API exploration.

The aggregation endpoint can also be called directly, for example:

```bash
curl "http://localhost:5011/api/v1/aggregation?deviceId=PUMP-01&metric=temperature&from=2025-06-01T08:00:00Z&to=2025-06-01T09:00:00Z&bucketSeconds=600"
```

The exact local port can vary according to the ASP.NET Core launch configuration.

## Running Tests

Run all tests from the repository root:

```bash
dotnet test
```

The test suite contains both unit and integration tests.

Current test coverage includes:

* Stateless rule operators
* Rule applicability
* Rule validation
* Rule loading
* Reading parsing
* Reading validation
* Reading deduplication
* Event-time ordering
* `SustainedAbove` state transitions
* Alert deduplication
* Alert cooldown
* Reading classification
* Reading processing
* Aggregation statistics
* Aggregation boundaries
* Empty aggregation buckets
* API validation
* API aggregation behavior
* Persistence repositories
* Database uniqueness constraints
* EF Core migrations
* Idempotent batch persistence
* Startup processing/orchestration

The latest local test run completed with:

```text
Unit Tests:        120 passed
Integration Tests:  18 passed
Total:             138 passed
```

## API Documentation

In the Development environment, the application exposes the generated OpenAPI document and Scalar API reference.

The interactive Scalar UI is available at:

```text
/scalar
```

The aggregation endpoint is documented under the `Aggregation` tag.

## Design Decisions and Trade-offs

### Rules are startup configuration

Rules are not persisted or managed through an API. They are treated as startup configuration in `rules.json`.

This matches the task requirement and keeps rule-management concerns out of the application.

### Batch processing instead of a message broker

The task explicitly allows file-based ingestion and does not require queues or brokers. The implementation therefore processes the supplied JSONL file as a batch.

### Stateful rule isolation

`SustainedAbove` maintains state separately for each `(ruleId, deviceId, metric)` combination. This avoids state collisions between multiple stateful rules targeting the same device and metric.

### Event time instead of processing time

Sustained duration and alert cooldown are based on event timestamps. This makes the behavior deterministic and allows out-of-order input to be normalized before stateful processing.

### Application-level idempotency plus database constraints

The application checks existing records before insertion, while database uniqueness constraints provide a second line of defense.

### SQLite

SQLite keeps the project self-contained and easy to run locally without requiring an external database server. Its `DateTimeOffset` query limitations are explicitly handled in the aggregation repository.

### In-memory aggregation bucketing

The aggregation service receives the acceptable readings for the requested device/metric and constructs the requested buckets in memory. This keeps bucket generation and empty-bucket behavior independent of database-specific aggregation functions.

## Extensibility

The design intentionally keeps domain behavior independent from infrastructure details.

Examples of extension points include:

* Adding another rule operator through `IRuleOperator`.
* Adding another input source behind the ingestion/application abstractions.
* Replacing SQLite repositories without changing domain rule behavior.
* Adding additional aggregation strategies behind the application aggregation service/repository abstractions.
* Replacing the in-memory rule provider with another configuration source if rule-management requirements change.

The current task does not introduce a message broker, Kubernetes deployment, authentication, UI, or rule-management API.

## Error Handling

Malformed or semantically invalid input records are rejected individually and do not terminate the complete ingestion process.

Invalid rules are treated differently: because the application cannot safely determine its intended rule set when `rules.json` contains invalid rules, startup fails instead of silently ignoring an invalid configuration.

## AI Disclosure

AI assistance was used during development for code discussion, architecture review, debugging, test design, documentation, and implementation guidance.

The final implementation, decisions, tests, and verification were reviewed and executed in the project environment.
