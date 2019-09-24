using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class Tokens
    {
        public Dictionary<string, string> TokenDictionary { get; private set; }

        public Tokens()
        {
            TokenDictionary = new Dictionary<string, string>();
            LoadTokens();
        }

        /**
         * Tokens da linguagem Python de acordo com a documentação:
         * https://docs.python.org/3/reference/lexical_analysis.html
         */
        private void LoadTokens()
        {
            //Identacao
            TokenDictionary.Add("indent", "TOKEN.INDENT");
            TokenDictionary.Add("dedent", "TOKEN.DEDENT");

            //Tipos
            TokenDictionary.Add("string", "TOKEN.STRING");
            TokenDictionary.Add("float", "TOKEN.FLOAT");
            TokenDictionary.Add("integer", "TOKEN.INTEGER");

            //Palavras reservadas
            TokenDictionary.Add("identf", "TOKEN.ID");
            TokenDictionary.Add("and", "TOKEN.AND");
            TokenDictionary.Add("del", "TOKEN.DEL");
            TokenDictionary.Add("from", "TOKEN.FROM");
            TokenDictionary.Add("not", "TOKEN.NOT");
            TokenDictionary.Add("while", "TOKEN.WHILE");
            TokenDictionary.Add("as", "TOKEN.AS");
            TokenDictionary.Add("elif", "TOKEN.ELIF");
            TokenDictionary.Add("global", "TOKEN.GLOBAL");
            TokenDictionary.Add("or", "TOKEN.OR");
            TokenDictionary.Add("with", "TOKEN.WITH");
            TokenDictionary.Add("assert", "TOKEN.ASSERT");
            TokenDictionary.Add("else", "TOKEN.ELSE");
            TokenDictionary.Add("if", "TOKEN.IF");
            TokenDictionary.Add("pass", "TOKEN.PASS");
            TokenDictionary.Add("yield", "TOKEN.YIELD");
            TokenDictionary.Add("break", "TOKEN.BREAK");
            TokenDictionary.Add("except", "TOKEN.EXCEPT");
            TokenDictionary.Add("import", "TOKEN.IMPORT");
            TokenDictionary.Add("print", "TOKEN.PRINT");
            TokenDictionary.Add("class", "TOKEN.CLASS");
            TokenDictionary.Add("exec", "TOKEN.EXEC");
            TokenDictionary.Add("in", "TOKEN.IN");
            TokenDictionary.Add("raise", "TOKEN.RAISE");
            TokenDictionary.Add("continue", "TOKEN.CONTINUE");
            TokenDictionary.Add("finally", "TOKEN.FINALLY");
            TokenDictionary.Add("is", "TOKEN.IS");
            TokenDictionary.Add("return", "TOKEN.RETURN");
            TokenDictionary.Add("def", "TOKEN.DEF");
            TokenDictionary.Add("for", "TOKEN.FOR");
            TokenDictionary.Add("lambda", "TOKEN.LAMBDA");
            TokenDictionary.Add("try", "TOKEN.TRY");
            TokenDictionary.Add("none", "TOKEN.NONE");
            TokenDictionary.Add("nonlocal", "TOKEN.NONLOCAL");

            //Operadores e Delimitadores
            TokenDictionary.Add("(", "TOKEN.LEFTPAR");
            TokenDictionary.Add(")", "TOKEN.RIGHTPAR");
            TokenDictionary.Add(",", "TOKEN.COMMA");
            TokenDictionary.Add(".", "TOKEN.DOT");
            TokenDictionary.Add("'", "TOKEN.BACKQUOTE");
            TokenDictionary.Add("{", "TOKEN.LEFTBRAC");
            TokenDictionary.Add("}", "TOKEN.RIGHTBRAC");
            TokenDictionary.Add("~", "TOKEN.TILDE");
            TokenDictionary.Add("@", "TOKEN.AT");
            TokenDictionary.Add("!=", "TOKEN.NOTEQUAL");
            TokenDictionary.Add("[", "TOKEN.LEFTSQB");
            TokenDictionary.Add("]", "TOKEN.RIGHTSQB");
            TokenDictionary.Add(":", "TOKEN.COLON");
            TokenDictionary.Add(";", "TOKEN.SEMICOLON");
            TokenDictionary.Add("+", "TOKEN.PLUS");
            TokenDictionary.Add("+=", "TOKEN.PLUSEQUAL");
            TokenDictionary.Add("-", "TOKEN.MINUS");
            TokenDictionary.Add("-=", "TOKEN.MINEQUAL");
            TokenDictionary.Add("*", "TOKEN.STAR");
            TokenDictionary.Add("*=", "TOKEN.STAREQUAL");
            TokenDictionary.Add("**", "TOKEN.DOUBLESTAR");
            TokenDictionary.Add("**=", "TOKEN.DOUBLESTAREQUAL");
            TokenDictionary.Add("/", "TOKEN.SLASH");
            TokenDictionary.Add("/=", "TOKEN.SLASHEQUAL");
            TokenDictionary.Add("//", "TOKEN.DOUBLESLASH");
            TokenDictionary.Add("//=", "TOKEN.DOUBLESLASHEQUAL");
            TokenDictionary.Add("|", "TOKEN.VERTBAR");
            TokenDictionary.Add("|=", "TOKEN.VERTBAREQUAL");
            TokenDictionary.Add("&", "TOKEN.AMPER");
            TokenDictionary.Add("&=", "TOKEN.AMPEREQUAL");
            TokenDictionary.Add("<", "TOKEN.LESS");
            TokenDictionary.Add("<=", "TOKEN.LESSEQUAL");
            TokenDictionary.Add("<<", "TOKEN.SHIFTLEFT");
            TokenDictionary.Add("<<=", "TOKEN.SHIFTLEFTEQUAL");
            TokenDictionary.Add(">", "TOKEN.GREATER");
            TokenDictionary.Add(">=", "TOKEN.GREATEREQUAL");
            TokenDictionary.Add(">>", "TOKEN.SHIFTRIGHT");
            TokenDictionary.Add(">>=", "TOKEN.SHIFTRIGHTEQUAL");
            TokenDictionary.Add("=", "TOKEN.EQUAL");
            TokenDictionary.Add("==", "TOKEN.EQUALEQUAL");
            TokenDictionary.Add("%", "TOKEN.PERCENT");
            TokenDictionary.Add("%=", "TOKEN.PERCENTEQUAL");
            TokenDictionary.Add("^", "TOKEN.CIRCUMFLEX");
            TokenDictionary.Add("^=", "TOKEN.CIRCUMFLEXEQUAL");
        }
    }
}
