parser grammar MarkdownPageParser;

options {
  tokenVocab = MarkdownPageLexer;
}

page : content* EOF ;

content
  : heading
  | paragraph
  | listItem
  | codeBlock
  | razorDirective
  | blazorComponent
  | text
  ;

heading : H_MARKER+ text PADDED_NL ;

paragraph : text+ PADDED_NL ;

listItem : BULLET_MARKER text PADDED_NL ;

codeBlock : LISTING_MARKER language? NL (text NL)* LISTING_MARKER ;

razorDirective : RAZOR_DIRECTIVE directiveContent ;

blazorComponent : BLZ_COMPONENT IDENTIFIER htmlAttributes? ;

text : TEXT_CHARACTERS+ ;

language : IDENTIFIER ;

directiveContent : quotedText | IDENTIFIER | path ;

htmlAttributes : BRKT_LEFT (IDENTIFIER COLON IDENTIFIER)* BRKT_RIGHT ;

quotedText : DBL_QUOTE_MARKER TEXT_CHARACTERS* DBL_QUOTE_MARKER ;

path : PATH_CHARACTERS+ ;