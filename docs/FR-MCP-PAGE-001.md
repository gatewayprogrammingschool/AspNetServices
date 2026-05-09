FR-MCP-PAGE-001: Blazor in Markdown with Lifecycle Runtime

**Status:** Proposed
**Priority:** High
**Area:** PAGE

**Description:**
Markdown files shall be parsed using an improved ANTLR4 grammar and instantiated as dynamic page components supporting the full Blazor component lifecycle (OnInitializedAsync, OnParametersSetAsync, OnAfterRenderAsync, OnDispose, etc.).

This enables the 'Blazor in Markdown' pattern.

**Requirements:**
- Parse tree visitor that converts ANTLR4 output into Blazor-style components.
- Attribute-driven OIDC permissions ([Authorize], custom Permission attributes).
- Automatic theming (current theme by default).
- PDF export using current theme by default with override support.
- Full integration with AppFramework and middleware.
- All design decisions and emergent requirements shall be captured in session logs.

**Acceptance Criteria:**
- TDD used throughout.
- Requirements precede code (this FR must be baselined before implementation).
- Grammar refactor isolated in REFAC-ANTLR-001.

**Linked TR:** TR-MCP-PAGE-RUNTIME-001
**Linked TODOs:** REFAC-ANTLR-001, FEATURE-LIFECYCLE-001
