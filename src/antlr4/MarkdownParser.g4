parser grammar MarkdownParser;

options { tokenVocab=MarkdownLexer; }

md
    : front_matter
    /* | body */
    ;

front_matter_begin
    : LINE_MARKER
    ;

front_matter_end
    : END_LINE_MARKER
    ;

fm_variable_name
    : BOL WHITE_SPACE* IDENTIFIER WHITE_SPACE*
    ;

fm_variable_value
    : quoted_text
    | fm_text
    | PIPE_MARKER PADDED_NL WHITE_SPACE+ multiline_text
    ;

front_matter_content
    : fm_variable_name COLON_MARKER fm_variable_value?
    ;

front_matter
    : front_matter_begin front_matter_content* front_matter_end
    ;

no_space_path
    : (HASH_MARKER|PATH_CHARACTERS)+
    ;

fm_text_contents
    : (WHITE_SPACE* TEXT_CHARACTERS+)+
    | (WHITE_SPACE* IDENTIFIER+)+
    ;

text_contents
    : (WHITE_SPACE* BODY_TEXT_CHARACTERS+)+
    | (WHITE_SPACE* IDENTIFIER+)+
    ;

bold_text
    : WHITE_SPACE* BOLD_MARKER italics_text BOLD_MARKER
    | WHITE_SPACE* BOLD_MARKER text_contents+ BOLD_MARKER
    ;

italics_text
    : WHITE_SPACE* ITALICS_MARKER bold_text ITALICS_MARKER
    | WHITE_SPACE* ITALICS_MARKER text_contents+ ITALICS_MARKER
    ;

text
    : (bold_text | italics_text | text_contents)+
    ;

fm_text
    : (bold_text | italics_text | fm_text_contents)+
    ;

multiline_text_line
    : text (WHITE_SPACE+|NL+) text
    ;

multiline_text
    : multiline_text_line text NL?
    ;

quoted_multiline_text
    : DBL_QUOTE_MARKER multiline_text DBL_QUOTE_MARKER
    | SNG_QUOTE_MARKER multiline_text SNG_QUOTE_MARKER
    ;

quoted_text
    : DBL_QUOTE_MARKER text DBL_QUOTE_MARKER
    | SNG_QUOTE_MARKER text SNG_QUOTE_MARKER
    ;

path_raw
    : no_space_path
    /* | WHITE_SPACE_PATH */
    ;

path
    : SNG_QUOTE_MARKER path_raw SNG_QUOTE_MARKER
    | DBL_QUOTE_MARKER path_raw DBL_QUOTE_MARKER
    | path_raw
    | EMPTY
    ;


link
    : BRKT_LEFT text BRKT_RIGHT PAREN_LEFT path PAREN_RIGHT
    | BRKT_LEFT EMPTY BRKT_RIGHT PAREN_LEFT path PAREN_RIGHT
    ;

caption
    : CAPTION_MARKER inline_content NL
    ;

image
    : BANG_MARKER BRKT_LEFT text BRKT_RIGHT PAREN_LEFT path PAREN_RIGHT
    | BANG_MARKER BRKT_LEFT EMPTY BRKT_RIGHT PAREN_LEFT path PAREN_RIGHT
    ;

include
    : CARET_MARKER BANG_MARKER INCLUDE_MARKER PAREN_LEFT path PAREN_RIGHT
    ;

bullet_line
    : BULLET_MARKER inline_content NL
    ;

number_line
    : NUMBER_MARKER inline_content NL
    ;

bullet_list
    : bullet_line+ NL
    ;

number_list
    : number_line+ NL
    ;

header_line
    : H_MARKER inline_content NL
    ;

inline_content
    : ITALICS_MARKER multiline_text ITALICS_MARKER
    | ITALICS_MARKER BOLD_MARKER multiline_text BOLD_MARKER ITALICS_MARKER
    | BOLD_MARKER multiline_text BOLD_MARKER
    | BOLD_MARKER ITALICS_MARKER multiline_text ITALICS_MARKER BOLD_MARKER
    | LABELREF_START_MARKER multiline_text LABELREF_END_MARKER
    | LABEL_START_MARKER multiline_text LABEL_END_MARKER
    | span
    | link
    | image
    | multiline_text
    ;

html_attribute
    : IDENTIFIER EQUAL_MARKER quoted_text
    ;

html_attributes
    : html_attribute*
    ;

span_set
    : span_start inline_content+ span_end
    | span_start span_end
    ;

span
    : span_set+
    ;

paragraph
    : inline_content+
    ;

terminated_paragraph
    : paragraph NL NL
    ;

paragraph_set
    : terminated_paragraph+
    ;

span_start
    : LT_MARKER SPAN_MARKER html_attributes GT_MARKER
    ;

span_end
    : LT_MARKER FORWARD_SLASH_MARKER SPAN_MARKER GT_MARKER
    ;

div_start
    : LT_MARKER DIV_MARKER html_attributes GT_MARKER
    ;

div_end
    : LT_MARKER FORWARD_SLASH_MARKER DIV_MARKER GT_MARKER
    ;

p_start
    : LT_MARKER PARA_MARKER html_attributes GT_MARKER
    ;

p_end
    : LT_MARKER FORWARD_SLASH_MARKER PARA_MARKER GT_MARKER
    ;

html_open
    : LT_MARKER IDENTIFIER html_attributes GT_MARKER
    ;

html_end
    : LT_MARKER FORWARD_SLASH_MARKER IDENTIFIER GT_MARKER
    ;


html
    : div_start html? div_end
    | html_open html? html_end
    | div_start html? div_end
    | html_open html? html_end
    | p_start html? p_end
    | p_start paragraph? p_end
    | p_start html? NL NL
    | p_start terminated_paragraph?
    ;


closed_blocks
    : PATH_START_MARKER path PATH_END_MARKER
    | FENCE_MARKER paragraph FENCE_MARKER
    | LISTING_MARKER paragraph LISTING_MARKER
    ;

quote
    : QUOTE_MARKER quotable+
    ;

terminated_quote
    : quote NL NL
    ;

newlines
    : NL+
    ;

quotable
    : terminated_quote
    | html
    | paragraph_set
    | image
    | include
    | number_list
    | bullet_list
    ;

bodyElements
    : closed_blocks
    | quote
    | quotable
    | newlines
    ;

body
    : bodyElements+
    ;
