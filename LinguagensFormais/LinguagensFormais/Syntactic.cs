using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class Syntactic
    {
        private List<TokensFound> ListOfTokens { get; set; }
        private string Token { get; set; }
        private int Id { get; set; }

        public Syntactic(List<TokensFound> listOfTokens)
        {
            ListOfTokens = listOfTokens;
            Id = 0;
            Token = listOfTokens[Id].Token;
        }

        /**
         * Realiza análise sintática dos tokens encontrados
         */
        public bool SyntacticAnalysis()
        {
            /* Se for inicio de funcao, deve estar nos padroes def nome_funcao(parametros) */
            if (Definition())
            {
                TokenAction();
                if (Token.Equals("TOKEN.ID"))
                {
                    TokenAction();
                    if (Parameters())
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.INDENT"))
                        {
                            TokenAction();
                            if (Source())
                            {
                                if (Token.Equals("TOKEN.EOF"))
                                {
                                    return true;
                                }
                                else if (Token.Equals("TOKEN.DEDENT"))
                                {
                                    TokenAction();
                                    if (SyntacticAnalysis())
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            /* No python nao eh necessario comecar com uma funcao, pode-se colocar codigo direto */
            else
            {
                if (Source())
                {
                    if (Token.Equals("TOKEN.EOF"))
                    {
                        return true;
                    }
                    else if (Token.Equals("TOKEN.DEDENT"))
                    {
                        TokenAction();
                        if (SyntacticAnalysis())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (Token.Equals("TOKEN.INDENT"))
                    {
                        TokenAction();
                        if (SyntacticAnalysis())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        /**
         * Devolve o objeto TokenFound que deu erro
         */
        public TokensFound Error()
        {
            return ListOfTokens[--Id];
        }

        /**
         * Realiza a operacao de ir pro proximo token e voltar pro token anterior
         */
        private void TokenAction(bool next = true)
        {
            /* Avancar um token */
            if (next)
            {
                Token = ListOfTokens[++Id].Token;
            }
            /* Voltar um token */
            else
            {
                Token = ListOfTokens[--Id].Token;
            }
        }

        /**
         * Verifiica se possui o prefixo def antes de uma funcao
         */
        private bool Definition()
        {
            if(Token.Equals("TOKEN.DEF"))
            {
                return true;
            }

            return false;
        }

        /**
         * Verifica se eh um tipo valido
         */
        private bool IsValidType(string token)
        {
            if (token.Equals("TOKEN.STRING") || token.Equals("TOKEN.INTEGER") || token.Equals("TOKEN.FLOAT"))
            {
                return true;
            }

            return false;
        }

        /**
         * Verifica o codigo do programa em si
         */
        private bool Source()
        {
            if (IsIf(false))
            {
                TokenAction();
                if(Source())
                {
                    return true;
                }
                return false;
            }
            if (isWhile())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }
            if (isFor())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }
            if (isFunction())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }
            if (isReceivingValues())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }
            if (IsComment())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }
            if (IsReturn())
            {
                TokenAction();
                if (Token.Equals("TOKEN.STRING") || Source())
                {
                    return true;
                }
                return false;
            }
            if (Definition())
            {
                TokenAction();
                if (Source())
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        /**
         * Se for comentario, apenas verifica se eh o token e vai pro proximo
         */
        private bool IsComment()
        {
            if (Token.Equals("TOKEN.MULTIPLO_COMENTARIO"))
            {
                TokenAction();
                return true;
            }

            return false;
        }

        private bool IsReturn()
        {
            if (Token.Equals("TOKEN.RETURN"))
            {
                return true;
            }

            return false;
        }

        /**
         * Verifica parametros de uma funcao
         * E -> (S)
         * S -> idS | vazio
         */
        private bool Parameters()
        {
            if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
            {
                while (true)
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.ID"))
                    {
                        TokenAction();
                        if (!Token.Equals("TOKEN.VIRGULA"))
                        {
                            if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                            {
                                TokenAction();
                                if (Token.Equals("TOKEN.DOIS_PONTOS"))
                                {
                                    return true;
                                }                            
                            }
                        }
                    }                    
                    /* Funcao sem parametros */
                    else if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                    {
                        TokenAction();

                        if (Token.Equals("TOKEN.DOIS_PONTOS"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se eh uma funcao
         */
        private bool isFunction()
        {
            if (Token.Equals("TOKEN.ID"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
                {
                    TokenAction();
                    while (true)
                    {
                        if (Token.Equals("TOKEN.ID")) {
                            TokenAction();
                            if (Token.Equals("TOKEN.VIRGULA"))
                            {
                                TokenAction();
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.DOIS_PONTOS"))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    TokenAction(false);
                }
            }

            return false;
        }

        /**
         * Verifica se eh um if
         * S  -> IF (Condicoes) Identacao Codigo Dedentacao E | IF (Condicoes) Identacao Codigo Dedentacao EF | S'
         * S' -> IF Condicoes Identacao Codigo Dedentacao E | IF Condicoes Identacao Codigo Dedentacao EF | S'
         * E  -> ELSE Identacao Codigo Dedentacao | vazio 
         * EF -> ELIF S | vazio 
         */
        private bool IsIf(bool elif)
        {
            var hasParenteses = false;
            if (Token.Equals("TOKEN.IF") || elif) {
                TokenAction();

                if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
                {
                    hasParenteses = true;
                    TokenAction();
                }

                if (Conditions())
                {
                    TokenAction();
                    if(hasParenteses) //se abriu parenteses, precisa fechar
                    {
                        if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                        {
                            TokenAction();
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (Token.Equals("TOKEN.DOIS_PONTOS"))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.INDENT"))
                        {
                            TokenAction();
                            if (Source())
                            {
                                if (Token.Equals("TOKEN.DEDENT") || Token.Equals("TOKEN.ELSE"))
                                {
                                    if (Token.Equals("TOKEN.DEDENT")) TokenAction(); 
                                    if(IsElseOrElif())
                                    {
                                        return true;
                                    }
                                }
                                else if (Token.Equals("TOKEN.EOF"))
                                {
                                    TokenAction(false);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se o que veio depois do if eh um elif ou else
         */
        private bool IsElseOrElif()
        {
            if (Token.Equals("TOKEN.ELSE"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.DOIS_PONTOS"))
                {
                    TokenAction(); 
                    if (Token.Equals("TOKEN.INDENT"))
                    {
                        TokenAction();
                        if (Source())
                        {
                            TokenAction();
                            if (Token.Equals("TOKEN.DEDENT"))
                            {
                                return true;
                            }
                            else if (Token.Equals("TOKEN.EOF"))
                            {
                                TokenAction(false);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            else if (Token.Equals("TOKEN.ELIF"))
            {
                if(IsIf(true))
                {
                    return true;
                }
                return false;
            }

            TokenAction(false);
            return true;
        }

        /**
         * Verifica se eh uma condicao 
         * Conditions -> E | and E | or E | vazio  
         * E -> T == T | T != T | T > T | T < T | T >= T | T <= T 
         */
        private bool Conditions()
        {
            if(ConditionalExpression())
            {
                while(true)
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.AND") || Token.Equals("TOKEN.OR"))
                    {
                        TokenAction();
                        if (ConditionalExpression())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        TokenAction(false);
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se eh uma expressao booleana
         * E -> E == E | E != E | E <= E | E >= E | E < E | E > E 
         */
        private bool ConditionalExpression()
        {
            if (Expression())
            {
                TokenAction();
                if (Token.Equals("TOKEN.IGUAL_IGUAL") || Token.Equals("TOKEN.DIFERENTE") ||
                   Token.Equals("TOKEN.MENOR_IGUAL") || Token.Equals("TOKEN.MAIOR_IGUAL") ||
                   Token.Equals("TOKEN.MAIOR") || Token.Equals("TOKEN.MENOR"))
                {
                    TokenAction();
                    if (Expression())
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /**
         * Verifica se eh uma expressao
         * E -> E E' | id | ( E ) | TYPE em que E' = Exp_Attrib_1
         * E' -> + E | - E | * E | / E | ^ E | and E | or E 
        */
        private bool Expression()
        {
            //Valida se eh um tipo ou se eh um identificador
            if(IsValidType(Token) || Token.Equals("TOKEN.ID"))
            {
                TokenAction();
                //Valida acao em cima do token anterior
                if(Token.Equals("TOKEN.MAIS") || Token.Equals("TOKEN.MENOS") ||
                   Token.Equals("TOKEN.VEZES") || Token.Equals("TOKEN.BARRA") ||
                   Token.Equals("TOKEN.CIRCUMFLEXO"))
                {
                    TokenAction();
                    if(Expression())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    TokenAction(false);
                    return true;
                }
            }
            else 
            {
                if (OpenAndCloseParenthesesInExpression())
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * Verifica o abre e fecha parenteses em uma expressao
         */
        private bool OpenAndCloseParenthesesInExpression()
        {
            if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
            {
                TokenAction();
                if (Expression())
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se eh um while
         * E -> While(S) Indent T Dedent  | While S Indent T Dedent | Z
         * Z -> While(S) Indent T Dedent ELSE T | While S Indent T Dedent ELSE T
         * S -> Condition
         * T -> Source
         */
        private bool isWhile()
        {
            var hasParenteses = false;
            if (Token.Equals("TOKEN.WHILE"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
                {
                    hasParenteses = true;
                    TokenAction();
                }
                if (Conditions())
                {
                    TokenAction();
                    if (hasParenteses) //se abriu parenteses, precisa fechar
                    {
                        if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                        {
                            TokenAction();
                        }
                        else
                        {
                            return false;
                        }
                    }
                    if (Token.Equals("TOKEN.DOIS_PONTOS"))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.INDENT"))
                        {
                            TokenAction();
                            if (Source())
                            {   
                                if (Token.Equals("TOKEN.DEDENT"))
                                {
                                    TokenAction();
                                    if (Token.Equals("TOKEN.ELSE"))
                                    {
                                        TokenAction();
                                        if (Token.Equals("TOKEN.DOIS_PONTOS"))
                                        {
                                            TokenAction();
                                            if (Token.Equals("TOKEN.INDENT"))
                                            {
                                                TokenAction();
                                                if (Source())
                                                {
                                                    if (Token.Equals("TOKEN.DEDENT"))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        
                                        }
                                    }
                                    else
                                    {
                                        TokenAction(false);
                                        return true;    
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se eh um for
         * E -> FOR id in range(S) Indent T Dedent | B
         * B -> FOR id in STRING Indent T Dedent
         * S -> int, int
         * T -> Source
         */
        private bool isFor()
        {
            if (Token.Equals("TOKEN.FOR"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.ID"))
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.IN"))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.ID") || Token.Equals("TOKEN.STRING")) //pode iterar sob uma string, exemplo: for a in "teste":
                        {
                            bool isString = Token.Equals("TOKEN.STRING");
                            TokenAction();

                            if (isString || (!isString && rangeParameters()))
                            {
                                if (!isString)
                                {
                                    TokenAction();
                                }

                                if (Token.Equals("TOKEN.DOIS_PONTOS"))
                                {
                                    TokenAction();
                                    if (Token.Equals("TOKEN.INDENT"))
                                    {
                                        TokenAction();
                                        if (Source())
                                        {
                                            if (Token.Equals("TOKEN.DEDENT"))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /**
         * Verifica se o range esta correto
         * Exemplos: range(5), range(0,10)
         */
        private bool rangeParameters()
        {
            if (Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.INTEGER"))
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.VIRGULA"))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.INTEGER"))
                        {
                            TokenAction();
                            if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                            {
                                return true;
                            }
                        }
                    }
                    else if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /**
         * Verifica se a variavel esta recebendo alguma expressao
         * E -> ID = Expression | ID += Expression | ID -= Expression |  ID *= Expression | ID /= Expression | ID = Function() |
         *      ID //= Expression | ID %= Expression | ID @= Expression | ID &= Expression | ID ^= Expression | ID >>= Expression |
         *      ID <<= Expression | ID **= Expression
         */
        private bool isReceivingValues()
        {
            if (Token.Equals("TOKEN.ID"))
            {
                TokenAction();
                if (Token.Equals("TOKEN.IGUAL") || Token.Equals("TOKEN.MAIS_IGUAL") ||
                    Token.Equals("TOKEN.MAIS") || Token.Equals("TOKEN.MENOS") ||
                    Token.Equals("TOKEN.VEZES") || Token.Equals("TOKEN.BARRA") || 
                    Token.Equals("TOKEN.MENOS_IGUAL") || Token.Equals("TOKEN.VEZES_IGUAL") ||
                    Token.Equals("TOKEN.BARRA_IGUAL") || Token.Equals("TOKEN.PORCENTO_IGUAL") ||
                    Token.Equals("TOKEN.ARROBA_IGUAL") || Token.Equals("TOKEN.ECOMERCIAL_IGUAL") ||
                    Token.Equals("TOKEN.CIRCUMFLEXO_IGUAL") || Token.Equals("TOKEN.SHIFT_RIGHT_IGUAL") ||
                    Token.Equals("TOKEN.SHIFT_LEFT_IGUAL") || Token.Equals("TOKEN.DUPLO_ASTERISCO_IGUAL"))
                {
                    TokenAction();
                    if (isFunction() || Expression())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
