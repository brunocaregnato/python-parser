﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class Syntactic
    {
        private List<TokensFound> ListOfTokens { get; set; }
        private string Token { get; set; }
        private int Index { get; set; }

        public Syntactic(List<TokensFound> listOfTokens)
        {
            ListOfTokens = listOfTokens;
            Index = 0;
            Token = listOfTokens[Index].Token;
        }

        private void TokenAction(bool next = true)
        {
            /* Avancar um token */
            if (next)
            {
                Token = ListOfTokens[++Index].Token;
            }
            /* Voltar um token */
            else
            {
                Token = ListOfTokens[--Index].Token;
            }
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
                        if (Token.Equals("TOKEN.DOIS_PONTOS"))
                        {
                            TokenAction();
                            if (Token.Equals("TOKEN.INDENT"))
                            {

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

                }
            }


            return true;
        }

        /**
         * Verifiica se eh inicio de funcao
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
         * Verifica parametros de uma funcao
         * E -> (S)
         * S -> idS | vazio
         */
        private bool Parameters()
        {
            if(Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
            {
                while (true)
                {
                    if (IsValidType(Token))
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.ID"))
                        {
                            TokenAction();
                            if(!Token.Equals("TOKEN.VIRGULA"))
                            {
                                if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
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
                                TokenAction();
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool IsValidType(string token)
        {
            if (token.Equals("TOKEN.STRING") || token.Equals("TOKEN.INT") || token.Equals("TOKEN.FLOAT")) return true;

            return false;
        }

        private bool Source()
        {
            if (IsIf(false))
            {

            }
            return true;
        }

        /*
         * S ->  IF (Condicioes) Identacao Codigo Dedentacao E | IF (Condicioes) Identacao Codigo Dedentacao EF
         * E ->  ELSE Identacao Codigo Dedentacao | vazio 
         * EF -> ELIF S | vazio 
         */
        private bool IsIf(bool elif)
        {
            if (Token.Equals("TOKEN.IF") || elif) {
                elif = false;
                TokenAction();
                if(Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
                {
                    TokenAction();
                    if (Conditions())
                    {
                        TokenAction();
                        if (Token.Equals("TOKEN.PARENTESES_DIREITO"))
                        {
                            TokenAction();
                            if (Token.Equals("TOKEN.INDENT"))
                            {
                                TokenAction();
                                if (Source())
                                {
                                    TokenAction();
                                    if (Token.Equals("TOKEN.DEDENT") || Token.Equals("TOKEN.ELSE"))
                                    {
                                        if (Token.Equals("TOKEN.DEDENT")) TokenAction(); 
                                        if(IsElseOrElif())
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else if (Token.Equals("TOKEN.EOF"))
                                    {
                                        TokenAction(false);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsElseOrElif()
        {
            if (Token.Equals("TOKEN.ELSE"))
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
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (Token.Equals("TOKEN.ELIF"))
            {
                if(IsIf(true))
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

        /**
         * Conditions -> E | && E | || E | vazio  
         * E -> T == T | T != T | T > T | T < T | T >= T | T <= T 
         */
        private bool Conditions()
        {
            if(Expression())
            {
                while(true)
                {
                    TokenAction();
                    if (Token.Equals("TOKEN.ECOMERCIAL") || Token.Equals("TOKEN.PIPE"))
                    {
                        TokenAction();
                        if (!Expression())
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
                if(Token.Equals("TOKEN.AND") || Token.Equals("TOKEN.OR") ||
                   Token.Equals("TOKEN.MAIS") || Token.Equals("TOKEN.MENOS") ||
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
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

    }
}