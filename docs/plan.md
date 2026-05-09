# Plan: MarkdownServer AspNetServices

Date: 2026-05-09
Branch: `main`
Workspace: `F:\GitHub\MarkdownServer\AspNetServices`

## Active Sessions

- **Current**: This Cline session — mcpserver-cline-plugin installed globally, MCP session `Cline-20260509T044943Z-use-plugin` (#240) active.
- **Prior**: HANDOFF.md (2026-05-09) — dotnet tool + global.yaml work, MCP-backed tracking.

---

## MCP State (from HANDOFF.md)

### Requirements (FR/TR)
| ID | Title | Status |
|----|-------|--------|
| FR-MDS-TOOL-001 | Publish markdown server as dotnet tool | Created |
| FR-MDS-YAML-001 | Support tree-level global.yaml defaults | Created |
| TR-MDS-TOOL-001 | dotnet tool distribution implementation | Created |
| TR-MDS-YAML-001 | global.yaml discovery and merge behavior | Created |

### Feature TODOs
| ID | Title | Status |
|----|-------|--------|
| FEATURE-TOOL-001 | Publish Markdown Server as dotnet Tool | Created |
| FEATURE-YAML-001 | Add tree-level global.yaml defaults | Created, depends on FEATURE-TOOL-001 |

### Execution TODOs
| ID | Title | Status | Next Action |
|----|-------|--------|-------------|
| EXEC-TODO-001 | Publish Markdown Server as dotnet Tool | TestReady | Implement dotnet tool flow |
| EXEC-TODO-002 | Add support for tree-level global.yaml defaults | TestReady | Depends on EXEC-TODO-001 |

### Phase
- **PHASE-001**: `MDS-PLAN-MARKDOWN-TOOL` — `Implementing`

---

## Local State (from docs/todo.yaml)

Previously created (likely via Director/McpAgent session) — larger scope refactoring plan:

| ID | Title | Priority | Status |
|----|-------|----------|--------|
| PLAN-REFAC-001 | Main refactoring plan (middleware → lifecycle) | HIGH | Active |
| REFAC-MIDDLEWARE-001 | TDD stubs for DI middleware | HIGH | Not started |
| REFAC-ANTLR-001 | Isolated ANTLR4 grammar refactor | HIGH | Not started |
| FEATURE-THEME-001 | NuGet theme system | HIGH | Not started |
| FEATURE-PDF-001 | PDF generation | HIGH | Not started |
| FEATURE-OIDC-001 | OIDC + attribute permissions | HIGH | Not started |
| FEATURE-LIFECYCLE-001 | Blazor-in-Markdown lifecycle runtime | HIGH | Not started |
| TEST-INTEGRATION-001 | Integration test suite | HIGH | Not started |
| TEST-BLAZOR-001 | Blazor template test project | MEDIUM | Not started |

---

## Execution Sequence

The handoff work (dotnet tool + global.yaml) is already in PHASE-001 with `TestReady` status. Prioritize completing that first, then consider the refactoring plan.

### Slice 1: dotnet tool entrypoint + packaging
- Implement `dotnet tool` command entrypoint in the appropriate source project.
- Update `.csproj` with tool manifest settings (`PackAsTool`, `ToolCommandName`).
- Add/update tests in `tests/MDS.AspnetServices.Tests`.
- Run tests, verify, mark `EXEC-TODO-001` complete.

### Slice 2: global.yaml front-matter defaults
- Implement `global.yaml` discovery (walk directory tree from root toward file).
- Merge global.yaml defaults with per-file front-matter (per-file wins).
- Add/update tests in `tests/MDS.AspnetServices.Tests` + `tests/MDS.TestSite`.
- Run tests, verify, mark `EXEC-TODO-002` complete.

### Slice 3 (future): Refactoring plan
- Evaluate whether middleware/lifecycle refactoring is still the goal.
- If yes, begin with REFAC-MIDDLEWARE-001 (TDD stubs first).

---

## Blockers

- Session log persistence via REST returns 500/DbUpdateException (server-side, not application-code blocker). Re-test after implementation work.

## Verification

- `dotnet test tests/MDS.AspnetServices.Tests`
- `dotnet test tests/MDS.TestSite`
- MCP session-log updates between meaningful steps