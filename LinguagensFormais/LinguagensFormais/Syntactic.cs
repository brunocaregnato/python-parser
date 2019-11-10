using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class Syntactic
    {
        private List<TokensFound> ListOfTokens { get; set; }
        private string Token { get; set; }
        private int index { get; set; }

        public Syntactic(List<TokensFound> listOfTokens)
        {
            ListOfTokens = listOfTokens;
            index = 0;
            Token = listOfTokens[index].Token;
        }

        private void NextToken()
        {
            Token = ListOfTokens[++index].Token;
        }

        /**
         * Realiza análise sintática dos tokens encontrados
         */
        public bool SyntacticAnalysis()
        {
            /* Se for inicio de funcao, deve estar nos padroes def nome_funcao(parametros) */
            if (Definition())
            {
                NextToken();
                if (Token.Equals("TOKEN.ID"))
                {
                    NextToken();
                    if (Parameters())
                    {
                        NextToken();
                        if (Token.Equals("TOKEN.DOIS_PONTOS"))
                        {
                            NextToken();
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
                        NextToken();
                        if (Token.Equals("TOKEN.ID"))
                        {
                            NextToken();
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
                                NextToken();
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
            return true;
        }
    }
}
