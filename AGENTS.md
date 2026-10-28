# AGENTS.md --- TravelApp

Repository-level guidance for AI coding agents working on this project.

Read this file before making changes. Preserve the existing architecture
and project conventions. When a requirement or architectural choice is
not defined here, do not silently invent one; ask the human or propose
options first.

## 1. Project Overview

TravelApp is a .NET 10 .NET MAUI single-project application.

-   Language: C#
-   UI: XAML
-   Application pattern: MVVM
-   Dependency injection: built-in
    `Microsoft.Extensions.DependencyInjection`
-   Navigation: .NET MAUI Shell
-   Primary platforms: Android and iOS
-   Additional targets: Mac Catalyst and Windows
-   Source control: Git
-   AI-assisted development: Cursor + Cline
-   AI routing used by the development environment: OmniRoute through
    its OpenAI-compatible endpoint

Current project structure:

``` text
TravelApp/                 # repository root
├── AGENTS.md
├── TravelApp.slnx
├── .gitignore
└── TravelApp/             # MAUI project
    ├── TravelApp.csproj
    ├── MauiProgram.cs
    ├── App.xaml
    ├── App.xaml.cs
    ├── AppShell.xaml
    ├── AppShell.xaml.cs
    ├── MainPage.xaml
    ├── MainPage.xaml.cs
    ├── Models/
    ├── Services/
    ├── ViewModels/
    ├── Views/
    ├── Platforms/
    ├── Properties/
    └── Resources/
```

`Models/`, `Services/`, `ViewModels/`, and `Views/` are intentional
application folders. Keep new files in the folder whose responsibility
matches the file.

## 2. Technology and Targeting

The current project targets:

-   `net10.0-android` --- minimum Android version 21
-   `net10.0-ios` --- minimum iOS version 15
-   `net10.0-maccatalyst` --- minimum version 15
-   `net10.0-windows10.0.19041.0` on Windows

The project uses:

-   `<UseMaui>true</UseMaui>`
-   `<SingleProject>true</SingleProject>`
-   nullable reference types
-   implicit usings
-   XAML source generation via `MauiXamlInflator=SourceGen`

Android and iOS are the primary platforms. User-facing features should
be designed and verified for both before being considered complete.

Visual Studio 2026 is the primary build/debug environment.

Do not change target frameworks, minimum platform versions, application
ID, or MAUI configuration without human approval.

## 3. Core Architecture Principles

### 3.1 MVVM

Use MVVM for application features.

-   Views belong in `Views/`.
-   ViewModels belong in `ViewModels/`.
-   Models and provider-agnostic DTOs belong in `Models/`.
-   Application services and their interfaces belong in `Services/`.
-   ViewModels should contain presentation state, commands, and
    orchestration logic.
-   ViewModels must remain independent of concrete UI controls.
-   Do not store references to `Page`, `View`, `Button`, `Entry`, `Map`,
    or other UI elements in ViewModels.
-   Prefer bindings and commands over code-behind event handlers.
-   Use compiled bindings with `x:DataType` where practical.
-   Keep View code-behind thin. It may contain `InitializeComponent()`,
    constructor injection, and genuinely UI-specific handlers that
    delegate to the ViewModel.

The existing template `MainPage.OnCounterClicked` pattern is an example
of code that should be migrated to the project's MVVM approach when the
page is developed.

### 3.2 Dependency Injection

`MauiProgram.cs` is the application's composition root.

-   Register application services and ViewModels through
    `builder.Services` in `MauiProgram.CreateMauiApp()`.
-   Use constructor injection.
-   Do not instantiate application services with `new` inside ViewModels
    or Views.
-   Depend on service interfaces rather than concrete implementations.
-   Choose DI lifetimes deliberately based on the service's
    responsibilities and state.
-   Do not introduce another dependency-injection container without
    human approval.

Views should normally communicate with ViewModels through bindings and
commands. Views should not become a second service layer.

### 3.3 Services

Services should isolate external systems and application infrastructure
from ViewModels.

Examples include:

-   web/API access
-   persistence
-   authentication
-   location
-   mapping
-   platform capabilities

Service interfaces should expose application-oriented, provider-agnostic
contracts where appropriate.

Do not leak SDK-specific types into ViewModels when an application-level
abstraction can be used.

## 4. MAUI and XAML Rules

-   Use the existing single-project MAUI structure.
-   Keep shared application code outside `Platforms/` unless it is
    genuinely platform-specific.
-   Keep platform-specific implementations under the appropriate
    `Platforms/<Platform>/` folder.
-   Use XAML as the primary UI declaration mechanism.
-   Keep `x:Class` and C# namespace/class declarations consistent.
-   Preserve the project's XAML source-generation configuration.
-   Do not manually implement XAML loading or generated plumbing such as
    `LoadFromXaml` or `FindByName` as a workaround.
-   Prefer compiled bindings (`x:DataType`) for ViewModel-bound views.
-   Do not block the UI thread. Use asynchronous APIs and
    `async`/`await` for I/O and other asynchronous work.
-   Use MAUI/.NET APIs before reaching for platform-specific APIs when
    the framework already provides the required capability.
-   Put shared colors, styles, templates, and other reusable resources
    in `Resources/Styles/` or the appropriate resource folder.
-   Avoid duplicating global styles inline on individual pages.
-   Follow current .NET MAUI APIs and avoid obsolete APIs when
    implementing new code.

## 5. Navigation

`AppShell` is the navigation host.

-   Prefer Shell routes and `Shell.Current.GoToAsync(...)` for
    application navigation.
-   Do not introduce a second primary navigation architecture.
-   Do not use `Navigation.PushAsync`/`PushModalAsync` as the
    application's primary navigation mechanism unless there is a
    specific approved reason.
-   Keep navigation decisions out of reusable service classes.
-   If navigation requires platform-specific behavior, isolate that
    behavior appropriately rather than scattering platform checks
    through ViewModels.

## 6. Android and iOS Platform Code

Platform-specific code belongs under `Platforms/Android/`,
`Platforms/iOS/`, `Platforms/MacCatalyst/`, and `Platforms/Windows/`.

-   Keep platform-specific APIs out of shared Models and ViewModels.
-   Prefer platform-specific implementations or partial classes over
    large `#if ANDROID` / `#if IOS` blocks in shared files.
-   Use conditional compilation only where it materially improves the
    implementation.
-   Modify platform entry points only when the requested feature
    actually requires it.
-   Keep platform entry points, manifests, and permission declarations
    minimally changed and intentional.
-   Preserve `MauiProgram.CreateMauiApp()` wiring.
-   For map/location functionality, add permissions only when the
    feature requires them.
-   Verify Android and iOS behavior for user-facing features.

## 7. Map Provider Architecture --- Guardrails Only

The application is expected to support Google Maps, AMap, and Naver
Maps.

The concrete map architecture has not yet been decided.

Until a design is approved:

-   Do not add Google, AMap, or Naver SDK packages/bindings.
-   Do not choose a specific map control or SDK architecture
    unilaterally.
-   Do not put provider-specific SDK types into ViewModels or shared
    application models.
-   The eventual design should provide a provider abstraction so
    application code can work with provider-agnostic models and
    operations.
-   Provider-specific implementations should be isolated behind that
    abstraction.
-   Provider selection should be configurable or otherwise swappable
    rather than hard-coded throughout the application.
-   Before implementing the map subsystem, present the proposed
    interface, provider-selection mechanism, SDK/binding strategy, and
    platform approach for human approval.

Do not treat an example interface name such as `IMapService` as an
already-approved design.

## 8. Security, API Keys, and Configuration

-   Never hard-code API keys, access tokens, passwords, connection
    strings containing secrets, or other credentials in source code.
-   Never commit real secrets to Git.
-   Do not place real secrets in `.cs`, `.xaml`, `.csproj`, manifests,
    comments, or committed configuration files.
-   Do not expose upstream AI provider credentials in the TravelApp
    application.
-   OmniRoute is part of the development/agent environment. Do not
    assume OmniRoute is an application runtime dependency unless an
    explicit application feature requires it.
-   The configuration and credential mechanism for Google Maps, AMap,
    Naver Maps, and future backend services is not yet finalized, except
    for the Android live-search key reuse recorded in section 13.1.
-   Do not invent a secrets/configuration mechanism without human
    approval.
-   When implementing credentials, consider platform/provider
    restrictions and the security model of the relevant API rather than
    blindly treating every client-side map key as a server secret.
-   If a feature requires a new credential, explain where it must live,
    how it is protected, and what restrictions should be applied before
    implementation.

## 9. NuGet and External Dependencies

Do not add, remove, or replace NuGet packages without human approval.

When proposing a package, provide:

1.  package name
2.  exact version
3.  purpose
4.  why the existing framework/project cannot reasonably provide the
    functionality
5.  important compatibility or licensing considerations where relevant

Rules:

-   Avoid floating package versions.
-   Keep MAUI framework package versions aligned with the installed MAUI
    workload and existing project configuration.
-   Prefer current, maintained .NET/MAUI-compatible packages.
-   Avoid obsolete Xamarin-era packages unless a specific compatibility
    requirement has been reviewed.
-   Do not add duplicate or overlapping libraries without a clear
    reason.
-   Do not add map-provider packages until the map architecture is
    approved.
-   Do not add an MVVM toolkit until that decision is approved.

After an approved `.csproj` dependency change, restore and build the
affected target.

## 10. File Modification Rules for AI Agents

AI agents may modify files only when the current task explicitly
authorizes implementation.

Before making architectural changes, identify the affected files and
explain important assumptions.

Do not modify these generated/build/runtime folders directly:

``` text
.git/
.vs/
bin/
obj/
```

Do not edit generated files to fix problems. Fix the
source/configuration that generates them.

Do not casually rename or delete existing project folders, platform
files, or resource folders.

When adding a View:

-   add both the `.xaml` and `.xaml.cs` files
-   use a consistent `x:Class` and namespace
-   keep code-behind thin
-   use normal MAUI XAML build behavior
-   do not add unnecessary project-file overrides

When modifying an existing file:

-   make the smallest reasonable change
-   preserve unrelated code
-   do not reformat an entire file unless requested
-   do not silently refactor unrelated architecture
-   explain significant changes in the final response

Never modify `.gitignore` merely to make a build or generated file
disappear. If an ignore rule genuinely needs to change, explain why and
obtain approval when the change affects repository behavior.

## 11. Build, Validation, and Testing

Visual Studio 2026 is the primary build and debugging environment.

After non-trivial implementation changes:

-   build the affected target
-   fix compilation and XAML source-generation errors
-   report warnings that may affect correctness
-   verify Android and iOS for user-facing features where practical

CLI builds may be used for validation, for example:

``` powershell
dotnet build TravelApp\TravelApp.csproj
```

Do not treat a successful compile as proof that a platform-specific
feature works correctly.

There is currently no test project or selected test framework.

-   Do not create a test project or select xUnit/NUnit/MSTest without
    human approval.
-   When testing infrastructure is approved, update this document with
    the agreed testing conventions.

## 12. Working Style for Cline and Other AI Agents

1.  Inspect before editing.
2.  Make focused, minimal changes.
3.  Preserve existing working code unless the task requires changing it.
4.  Do not make architectural decisions silently.
5.  Ask before adding packages or committing to deferred architecture.
6.  Prefer simple, maintainable implementations over unnecessary
    abstraction.
7.  Keep provider-specific concerns behind clear boundaries.
8.  Build after meaningful changes.
9.  Report what changed, what was verified, and any remaining issues.
10. If the requested change conflicts with this document, stop and
    explain the conflict rather than silently overriding the rule.

For ambiguous tasks, first provide a proposed approach and affected
files before making broad changes.

## 13. Deferred Architectural Decisions

The following decisions are intentionally open and must not be assumed:

-   concrete Google Maps / AMap / Naver Maps architecture
-   map SDK/control and native-binding strategy
-   exact map abstraction/interface shape
-   provider-selection and runtime/build-time strategy
-   MVVM toolkit versus framework `INotifyPropertyChanged`
    implementation
-   HTTP/REST client strategy beyond the current per-service
    `HttpClient` (see section 13.1)
-   secrets and configuration-loading mechanism, other than the Android
    live-search key reuse in section 13.1
-   release logging strategy
-   test framework and test project
-   CI/CD pipeline
-   centralized package/version management
-   localization strategy
-   application-wide state-management architecture
-   authentication architecture, if/when required
-   backend/API architecture, if/when required

When one of these decisions becomes necessary, stop before implementing
the irreversible part and present the options and recommendation to the
human.

### 13.1 Decisions Finalized in Step 13.7

The following decisions were approved and implemented in Step 13.7
(live map search). They apply to the live destination search feature
and do not settle the other open items above.

-   Live destination search uses Google Places API (New) Text Search
    (`places:searchText`), implemented by
    `GooglePlacesDestinationSearchService` behind the provider-agnostic
    `IDestinationSearchService`.
-   HTTP client: each service currently owns its own `HttpClient`
    instance. This records the existing implementation, not a
    project-wide HTTP-client architecture.
-   Credentials: Android live search reuses the existing
    `com.google.android.geo.API_KEY` manifest meta-data entry, read
    through `IGoogleApiCredentialProvider`. Its value comes from
    `GoogleMaps.Local.props`, which must remain local, ignored, and
    untracked. Never write the key into source, documentation, or logs.
-   Provider selection (`MapProviderResolver`): mainland China (`CN`) →
    AMap identity; South Korea (`KR`) → NAVER Maps; all other locations
    → Google Maps.
-   China fallback: AMap rendering is not implemented in `MapHostView`.
    The live-search page preserves the AMap identity, shows a pinned
    Google fallback map, and displays the status
    `Provider not supported by MapHostView`.
-   Current limitations: live-search credential retrieval is
    Android-only. iOS live search and NAVER iOS rendering are not
    implemented.
-   Validation: the Step 13.7 Android build succeeded and all six
    Step 13.7 Android device acceptance tests passed. iOS was not built
    or tested for Step 13.7.

## 14. Current Baseline

At the time this document was created:

-   the project is a .NET 10 MAUI template-based application
-   Android and iOS are the primary targets
-   `Models/`, `Services/`, `ViewModels/`, and `Views/` are established
    folders
-   no application services are registered in `MauiProgram.cs`
-   no MVVM toolkit has been selected
-   no map SDK/provider package has been selected
-   no test project exists
-   no application-specific secrets/configuration mechanism has been
    selected

Treat this baseline as the starting point. Update this document when an
architectural decision is deliberately made and approved. Decisions
recorded in section 13.1 supersede the corresponding baseline items.
