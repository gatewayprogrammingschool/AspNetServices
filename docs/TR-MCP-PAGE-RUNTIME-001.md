TR-MCP-PAGE-RUNTIME-001: ANTLR4 Grammar and Parse-Tree Visitor for Blazor in Markdown

**Status:** Proposed
**Priority:** High
**Subarea:** RUNTIME

**Description:**
Create a clean, maintainable ANTLR4 grammar optimized for parsing Markdown into a rich parse tree that can drive dynamic page component instantiation with Blazor-compatible lifecycle.

**Technical Details:**
- Grammar must support custom tags, lifecycle directives, permission attributes, and theme references.
- Implement a visitor pattern that walks the parse tree and instantiates PageBase-derived components.
- The grammar refactor must be isolated (REFAC-ANTLR-001) and completed before Lifecycle implementation.
- Must support both server-side and Blazor rendering paths.
- Requirements must be fully baselined before any lexer/parser code is written (Requirements Precede Code rule).

**Linked FR:** FR-MCP-PAGE-001
**Linked TODO:** REFAC-ANTLR-001
