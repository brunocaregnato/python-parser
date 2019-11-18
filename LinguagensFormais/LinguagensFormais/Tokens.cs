using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class Tokens
    {
        public Dictionary<string, string> TokenList { get; private set; }

        public Tokens()
        {
            TokenList = new Dictionary<string, string>();
            LoadTokens();
        }

        /**
         * Tokens da linguagem Python de acordo com a documentação:
         * https://docs.python.org/3/reference/lexical_analysis.html
         */
        private void LoadTokens()
        {
            //Identacao
            TokenList.Add("indent", "TOKEN.INDENT");
            TokenList.Add("dedent", "TOKEN.DEDENT");

            //Tipos
            TokenList.Add("string", "TOKEN.STRING");
            TokenList.Add("float", "TOKEN.FLOAT");
            TokenList.Add("integer", "TOKEN.INTEGER");

            //Palavras reservadas
            TokenList.Add("identf", "TOKEN.ID");
            TokenList.Add("and", "TOKEN.AND");
            TokenList.Add("del", "TOKEN.DEL");
            TokenList.Add("from", "TOKEN.FROM");
            TokenList.Add("not", "TOKEN.NOT");
            TokenList.Add("while", "TOKEN.WHILE");
            TokenList.Add("as", "TOKEN.AS");
            TokenList.Add("elif", "TOKEN.ELIF");
            TokenList.Add("global", "TOKEN.GLOBAL");
            TokenList.Add("or", "TOKEN.OR");
            TokenList.Add("with", "TOKEN.WITH");
            TokenList.Add("assert", "TOKEN.ASSERT");
            TokenList.Add("else", "TOKEN.ELSE");
            TokenList.Add("if", "TOKEN.IF");
            TokenList.Add("pass", "TOKEN.PASS");
            TokenList.Add("yield", "TOKEN.YIELD");
            TokenList.Add("break", "TOKEN.BREAK");
            TokenList.Add("except", "TOKEN.EXCEPT");
            TokenList.Add("import", "TOKEN.IMPORT");
            TokenList.Add("print", "TOKEN.PRINT");
            TokenList.Add("class", "TOKEN.CLASS");
            TokenList.Add("exec", "TOKEN.EXEC");
            TokenList.Add("in", "TOKEN.IN");
            TokenList.Add("raise", "TOKEN.RAISE");
            TokenList.Add("continue", "TOKEN.CONTINUE");
            TokenList.Add("finally", "TOKEN.FINALLY");
            TokenList.Add("is", "TOKEN.IS");
            TokenList.Add("return", "TOKEN.RETURN");
            TokenList.Add("def", "TOKEN.DEF");
            TokenList.Add("for", "TOKEN.FOR");
            TokenList.Add("lambda", "TOKEN.LAMBDA");
            TokenList.Add("try", "TOKEN.TRY");
            TokenList.Add("none", "TOKEN.NONE");
            TokenList.Add("nonlocal", "TOKEN.NONLOCAL");

            //Operadores
            TokenList.Add("+", "TOKEN.MAIS");
            TokenList.Add("-", "TOKEN.MENOS");
            TokenList.Add("*", "TOKEN.VEZES");
            TokenList.Add("**", "TOKEN.NOME_PARAMETRO");
            TokenList.Add("/", "TOKEN.BARRA");
            TokenList.Add("//", "TOKEN.BARRA_DUPLA");
            TokenList.Add("%", "TOKEN.PORCENTO");
            TokenList.Add("<<", "TOKEN.SHIFT_LEFT");
            TokenList.Add(">>", "TOKEN.SHIFT_RIGHT");
            TokenList.Add("&", "TOKEN.ECOMERCIAL");
            TokenList.Add("|", "TOKEN.PIPE");
            TokenList.Add("^", "TOKEN.CIRCUMFLEXO");
            TokenList.Add("~", "TOKEN.TIL");
            TokenList.Add("<", "TOKEN.MENOR");
            TokenList.Add(">", "TOKEN.MAIOR");
            TokenList.Add("<=", "TOKEN.MENOR_IGUAL");
            TokenList.Add(">=", "TOKEN.MAIOR_IGUAL");
            TokenList.Add("==", "TOKEN.IGUAL_IGUAL");
            TokenList.Add("!=", "TOKEN.DIFERENTE");
            
            //Delimitadores
            TokenList.Add("(", "TOKEN.PARENTESES_ESQUERDO");
            TokenList.Add(")", "TOKEN.PARENTESES_DIREITO");
            TokenList.Add("[", "TOKEN.COLCHETES_ESQUERDO");
            TokenList.Add("]", "TOKEN.COLCHETES_DIREITO");
            TokenList.Add("{", "TOKEN.CHAVES_ESQUERDA");
            TokenList.Add("}", "TOKEN.CHAVES_DIREITA");
            TokenList.Add(",", "TOKEN.VIRGULA");
            TokenList.Add(":", "TOKEN.DOIS_PONTOS");
            TokenList.Add(".", "TOKEN.PONTO");
            TokenList.Add(";", "TOKEN.PONTO_VIRGULA");
            TokenList.Add("@", "TOKEN.ARROBA"); //o arroba é tanto operador como delimitador 
            TokenList.Add("=", "TOKEN.IGUAL");
            TokenList.Add("+=", "TOKEN.MAIS_IGUAL");
            TokenList.Add("-=", "TOKEN.MENOS_IGUAL");
            TokenList.Add("*=", "TOKEN.VEZES_IGUAL");
            TokenList.Add("/=", "TOKEN.BARRA_IGUAL");
            TokenList.Add("//=", "TOKEN.BARRA_DUPLA_IGUAL");
            TokenList.Add("%=", "TOKEN.PORCENTO_IGUAL");
            TokenList.Add("@=", "TOKEN.ARROBA_IGUAL");
            TokenList.Add("&=", "TOKEN.ECOMERCIAL_IGUAL");
            TokenList.Add("|=", "TOKEN.PIPE_IGUAL");
            TokenList.Add("^=", "TOKEN.CIRCUMFLEXO_IGUAL");
            TokenList.Add(">>=", "TOKEN.SHIFT_RIGHT_IGUAL");
            TokenList.Add("<<=", "TOKEN.SHIFT_LEFT_IGUAL");
            TokenList.Add("**=", "TOKEN.DUPLO_ASTERISCO_IGUAL");

            //Caracteres ASCII  
            TokenList.Add("'", "TOKEN.ASPAS_SIMPLES");
            TokenList.Add("#", "TOKEN.COMENTARIO");
            TokenList.Add("'''", "TOKEN.MULTIPLO_COMENTARIO");
            TokenList.Add("$", "TOKEN.EOF");
        }
    }
}
