lexer grammar MarkdownPageLexer;

tokens { SKIP_TOKEN }

@header {
using Antlr4.Runtime.Misc;
}

// Standard Markdown tokens from MarkdownLexer.g4
EMPTY : ;

BOL : NL -> anchor ;

WHITE_SPACE : [ \u000B\t\r\f] -> skip ;

NL : '\r'? '\n' ;

PADDED_NL : WHITE_SPACE* NL ;

LINE_MARKER : '---' ;

END_LINE_MARKER : BOL '---' ;

STAR_MARKER : '*' ;

EQUAL_MARKER : '=' ;

DASH_MARKER : '-' ;

UNDERLINE_MARKER : '_' ;

QUOTE_MARKER : '> ' ;

PIPE_MARKER : '|' ;

BRACE_OPEN : '{' ;

BRACE_CLOSE : '}' ;

COL