# Engineering Insights Platform

An extensible platform engine for engineering governance. Built with .NET 10, EF Core, and Azure Functions, featuring a cross git repository .NET dependency graph generator.

## MVP

An Azure Functions app that scans repositories, extracts '.csproj' package consumption and published package identity, stores the results in SQLite and emits a Mermaid (.mmd) graph, which can be rendered by open source tooling.

- A weekly timer trigger performs an organization-wide sweep.
- An HTTP POST endpoint refreshes a single repository on demand.
- Two HTTP GET endpoints export the dependency graph of all packages, or of a single package, as a Mermaid (.mmd) file.
- Repository data comes from a deterministic mock Git client (mocked); no real platform is called yet.

## Example output

The result of `GET /api/export-dependency-graph` on the mock data. Arrows point from a package to the packages that use it: platform packages (`RB.Platform.*`) are used by everyone, the shared `Client` and `Contracts` packages by their own applications and other teams, and the applications (`Api`, `App`, `Worker`) are not used by anything.

```mermaid
graph TD
    n0["RB.Lending.Api"]
    n1["RB.Lending.App"]
    n2["RB.Lending.Client"]
    n3["RB.Lending.Contracts"]
    n4["RB.Lending.Worker"]
    n5["RB.Payments.Api"]
    n6["RB.Payments.App"]
    n7["RB.Payments.Client"]
    n8["RB.Payments.Contracts"]
    n9["RB.Payments.Worker"]
    n10["RB.Platform.Configuration"]
    n11["RB.Platform.Core"]
    n12["RB.Platform.Logging"]
    n13["RB.Platform.Messaging"]
    n14["RB.Platform.Security"]
    n15["RB.Platform.Telemetry"]
    n16["RB.Savings.Api"]
    n17["RB.Savings.App"]
    n18["RB.Savings.Client"]
    n19["RB.Savings.Contracts"]
    n20["RB.Savings.Worker"]
    n10 --> n16
    n10 --> n19
    n11 --> n0
    n11 --> n10
    n11 --> n12
    n11 --> n13
    n11 --> n14
    n11 --> n15
    n11 --> n16
    n11 --> n17
    n11 --> n18
    n11 --> n2
    n11 --> n4
    n11 --> n5
    n12 --> n0
    n12 --> n18
    n12 --> n19
    n12 --> n9
    n13 --> n19
    n13 --> n2
    n13 --> n20
    n13 --> n3
    n13 --> n7
    n14 --> n1
    n14 --> n18
    n14 --> n20
    n14 --> n6
    n14 --> n7
    n14 --> n8
    n14 --> n9
    n15 --> n20
    n18 --> n0
    n18 --> n16
    n18 --> n17
    n18 --> n20
    n18 --> n6
    n18 --> n9
    n19 --> n16
    n19 --> n17
    n19 --> n18
    n19 --> n20
    n2 --> n0
    n2 --> n1
    n2 --> n17
    n2 --> n4
    n3 --> n0
    n3 --> n1
    n3 --> n19
    n3 --> n2
    n3 --> n4
    n3 --> n6
    n3 --> n9
    n7 --> n17
    n7 --> n2
    n7 --> n20
    n7 --> n5
    n7 --> n6
    n7 --> n9
    n8 --> n2
    n8 --> n20
    n8 --> n5
    n8 --> n6
    n8 --> n7
    n8 --> n9
```

The same for a single package, `GET /api/export-dependency-graph-for-package/RB.Payments.Contracts`: only that package, everything it depends on and everything that depends on it will be shown.

```mermaid
graph TD
    n0["RB.Lending.Api"]
    n1["RB.Lending.App"]
    n2["RB.Lending.Client"]
    n3["RB.Lending.Worker"]
    n4["RB.Payments.Api"]
    n5["RB.Payments.App"]
    n6["RB.Payments.Client"]
    n7["RB.Payments.Contracts"]
    n8["RB.Payments.Worker"]
    n9["RB.Platform.Core"]
    n10["RB.Platform.Security"]
    n11["RB.Savings.App"]
    n12["RB.Savings.Worker"]
    n10 --> n1
    n10 --> n12
    n10 --> n5
    n10 --> n6
    n10 --> n7
    n10 --> n8
    n2 --> n0
    n2 --> n1
    n2 --> n11
    n2 --> n3
    n6 --> n11
    n6 --> n12
    n6 --> n2
    n6 --> n4
    n6 --> n5
    n6 --> n8
    n7 --> n12
    n7 --> n2
    n7 --> n4
    n7 --> n5
    n7 --> n6
    n7 --> n8
    n9 --> n0
    n9 --> n10
    n9 --> n11
    n9 --> n2
    n9 --> n3
    n9 --> n4
```

## Interpretation of the problem

Engineering knowledge is fragmented across repositories, pipelines and platform APIs. Before leaders can answer questions about reliability, risk or trends, they need a trustworthy map of who owns what and how components are coupled. Ownership and dependencies are the foundation for all later operational insights.

## Scope and rationale

The scope for this MVP is limited to repository inventory, team ownership and cross-repository dependencies. The initial focus of this MVP is on .NET applications and packages only.
This is the mandatory first step: failure trends or deployment frequency cannot be attributed accurately without knowing owners and coupling. Other focus areas (workflows, security) are deliberately deferred.

## Architecture and data flow

```mermaid
flowchart LR
    subgraph Worker["EngineeringInsights.WorkerApp (Azure Functions)"]
        F1["ImportAllRepositoriesPeriodically<br/>timer, Sundays at midnight"]
        F2["ImportRepository<br/>HTTP POST"]
        F3["ExportDependencyGraph<br/>HTTP GET, returns .mmd"]
        F4["ExportDependencyGraphForPackage<br/>HTTP GET, returns .mmd"]
    end

    subgraph Dal["EngineeringInsights.Dal"]
        DB[("SQLite<br/>ComponentConfigurations<br/>PackageDependencies")]
    end

    subgraph Domain["EngineeringInsights.DependencyAnalyzer"]
        IMPORT["ImportRepositoryService"]
        PARSER["CsProjParserService"]
        DIAGRAM["MermaidDiagramGeneratorService<br/>(IDiagramGeneratorService)"]
    end

    subgraph Git["EngineeringInsights.GitRepositoryClient"]
        CLIENT["IGitRepositoryClient"]
        MOCK["GitRepositoryMockClient<br/>deterministic mock data"]
    end

    F1 -->|import all| IMPORT
    F2 -->|import one| IMPORT
    F3 -->|full graph| DIAGRAM
    F4 -->|graph of one package| DIAGRAM

    IMPORT -->|"repositories, .csproj files, owner team"| CLIENT
    CLIENT -. implemented by .-> MOCK
    IMPORT -->|parse .csproj| PARSER
    IMPORT -->|replace rows of the repository| DB
    DIAGRAM -->|read packages and references| DB
```

### Data flow

1. A trigger starts an import: the timer imports all repositories, the HTTP POST a single `repositoryName`.
2. The import service retrieves the `.csproj` files and the owning team of a repository through `IGitRepositoryClient`.
3. The parser extracts consumed packages (`PackageReference`) and the published package identity (`PackageId`, defaulting to the project file name).
4. The dependencies are stored per repository, with owner team, in SQLite. The rows of the repository are replaced in a single save.
5. An export request makes the diagram generator read the packages and references from the database and build the Mermaid graph, which the function returns as a downloadable `dependencies.mmd` file.

### Projects

| Project | Responsibility |
| --- | --- |
| `EngineeringInsights.WorkerApp` | Azure Functions host with the triggers |
| `EngineeringInsights.DependencyAnalyzer` | Import service, `.csproj` parsing and diagram generation (Mermaid) |
| `EngineeringInsights.DependencyAnalyzer.Tests` | Unit tests of the domain project |
| `EngineeringInsights.Dal` | EF Core `DbContext`, migrations and table models |
| `EngineeringInsights.GitRepositoryClient` | `IGitRepositoryClient` interface and the mock implementation |

## Decisions, alternatives and trade-offs

Key choices:

- Start with repository dependencies as a solid foundation for gathering more information on the teams and their responsibilities.
- An abstracted git client interface so a real GitHub or Azure DevOps implementation can replace the mock. Files are streamed and the `.csproj` filter is passed to the client, so a real implementation can push it down to the API.
- Scheduled sweeps are combined with on-demand refresh instead of full event-driven collection.
- A flat relational schema over a document or graph based database, in SQLite for a zero-setup MVP.
- A thin WorkerApp: the functions only call two domain entry points (`IImportRepositoryService` and `IDiagramGeneratorService`), wired through dependency injection. The diagram interface is format agnostic, Mermaid is just the first implementation.
- Importing a repository is all or nothing: everything is read first and the stored rows are replaced in a single save, so a failing import keeps the previous data.
- The graph only contains packages published from our own repositories (external packages such as `Newtonsoft.Json` are left out), and arrows point from a package to the packages that use it, which puts the shared platform packages at the top and the applications at the bottom.
- The graph of a single package shows its transitive dependencies and dependents, but not packages that merely share a dependency, otherwise nearly everything would be connected through packages like `RB.Platform.Core`.
- The mock git client is deterministic and generates three tiers (platform packages, shared team packages and applications), including repositories without an owner team, as the owner cannot always be determined.
- No authentication was setup on the Function App, as it is beyond the scope of the MVP

## Evolution beyond the MVP

- Replace the mock git client with a real platform client
- move to a production database
- add event-driven ingestion (webhooks)
- The WorkerApp function app is a plug-and-play engine: new modules (e.g. a `WorkflowAnalyzer` or `SecurityScanner`) can be added as separate domain projects and make use of the same database and shared services.

## How to run

Prerequisites:

- .NET 10 SDK
- Azure Functions Core Tools v4 (`func`)
- Node.js, for Azurite
- VS Code with the extensions recommended by the workspace (Azure Functions, REST Client) and the C# extension for debugging

The local Azure Functions configuration uses `UseDevelopmentStorage=true`, so Azurite is required when running the Function App locally.

Install Azurite once if needed:

```bash
npm install -g azurite
```

Start Azurite from a terminal:

```bash
azurite
```

Then open the repository in VS Code, select **Run and Attach to .NET Functions** in the Run and Debug view, and start debugging. The project builds first, starts the Azure Functions host on its default port `7071`, and applies EF Core migrations automatically. The SQLite database is created at `src/engineering-insights.db` and is ignored by Git.

To run the unit tests:

```bash
dotnet test src/Domain/EngineeringInsights.DependencyAnalyzer.Tests
```

## Testing the endpoints

[EngineeringInsights.WorkerApp.http](src/EngineeringInsights.WorkerApp/EngineeringInsights.WorkerApp.http) contains ready-made requests for the Function App. Install the [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension (recommended by the workspace), start the Function App, then click **Send Request** above a request in the file.

| Function | Request | Purpose |
| --- | --- | --- |
| `ImportAllRepositoriesPeriodicallyFunction` | Timer, Sundays at midnight (`0 0 0 * * 0`); can be fired manually through the local admin endpoint (`POST /admin/functions/import-all-repositories-periodically`, also in the `.http` file) | Imports all repositories via the Git client, updating the database |
| `ImportRepositoryFunction` | `POST /api/import-repository/{repositoryName}` | On-demand import of a single repository. Returns `404` for an unknown repository |
| `ExportDependencyGraphFunction` | `GET /api/export-dependency-graph` | Reads the dependencies from SQLite and returns them as a Mermaid `graph TD` text. Only packages published by a `.csproj` in our own repositories are included; external packages such as `Newtonsoft.Json` are left out. Arrows point from a package to the packages that use it, so shared packages end up at the top and applications at the bottom |
| `ExportDependencyGraphForPackageFunction` | `GET /api/export-dependency-graph-for-package/{packageId}` | Same Mermaid output, limited to one package: everything it depends on and everything that depends on it, transitively. Returns `404` for a package that is not published by one of our repositories |
