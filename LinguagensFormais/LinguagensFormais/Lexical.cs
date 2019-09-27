using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LinguagensFormais
{
    public class Lexical
    {
        public int Line { get; private set; }
        public int Position { get; private set; }
        public string Lexeme { get; private set; }
        public int LastIdentationLevel { get; private set; }
        public List<TokensFound> TokensFound { get; private set; }
        private Tokens Tokens { get; set; } 

        public Lexical()
        {
            Line = 1;
            Position = 0;
            LastIdentationLevel = 0;
            TokensFound = new List<TokensFound>();
        }

        /**
         * Realiza análise léxica do arquivo
         */
        public bool LexicalAnalysis(string filePath)
        {
            Tokens = new Tokens();

            try
            {
                StreamReader streamReader = new StreamReader(filePath);
                {
                    string readLine;
                    TokensFound newToken = null;
                    bool isOnComment = false;

                    while((readLine = streamReader.ReadLine()) != null)
                    {
                        /* Arquivo sem linhas */
                        if (readLine.Length == 0) return false;

                        /* Verifica se começa com um comentário múltiplo */
                        isOnComment = VerifyMultipleComment(readLine, newToken, isOnComment);

                        if (!isOnComment)
                        {
                            /* Verifica se começa com um comentario de uma linha */
                            if (readLine[0].Equals('#'))
                            {
                                newToken = new TokensFound("TOKEN.COMENTARIO", "Comentário", 0, Line);
                                TokensFound.Add(newToken);
                                Line++;
                                continue;
                            }

                            var returnIdentation = VerifyIdentation(readLine, newToken);

                            if (returnIdentation.Equals(0)) return false;
                            else if (returnIdentation.Equals(1)) continue;

                            VerifyOperatorsAndDelimeters(readLine, newToken);
                        }
                        else Line++;
                        
                    }                
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        /**
         * Verifica se é um comentário de múltiplas linhas
         */
        private bool VerifyMultipleComment(string readLine, TokensFound newToken, bool isOnComment)
        {
            /* Verifica se começa com um comentario de multiplas linhas */
            if ((readLine[0].ToString().Equals("\'") && readLine[1].ToString().Equals("\'") && readLine[2].ToString().Equals("\'")) || isOnComment)
            {
                if (!isOnComment)
                {
                    newToken = new TokensFound("TOKEN.MULTIPLO_COMENTARIO", "Abre Múltiplo Comentário", 0, Line);
                    TokensFound.Add(newToken);
                    isOnComment = true;
                }

                /* Precisa verificar até onde vai o comentário */
                int positionOnLine = 1;
                while (positionOnLine < readLine.Length)
                {
                    /* Encontrou o fechamento do comentário */
                    if (readLine[positionOnLine].ToString().Equals("\'") && readLine[positionOnLine + 1].ToString().Equals("\'") && readLine[positionOnLine + 2].ToString().Equals("\'"))
                    {
                        isOnComment = false;
                        newToken = new TokensFound("TOKEN.MULTIPLO_COMENTARIO", "Fecha Múltiplo Comentário", positionOnLine, Line);
                        TokensFound.Add(newToken);
                        break;
                    }
                    positionOnLine++;
                }
                return isOnComment;
            }

            return false;
        }

        /**
         * Verifica se é uma identacao, caso for tambem verifica se é valida
         */
        private int VerifyIdentation(string readLine, TokensFound newToken)
        {

            /* Verifica identação com espaços, o python usa 4 espaços por identação */
            int spaces = 0;
            while (readLine[spaces].Equals(' '))
            {
                spaces++;
                if (spaces >= readLine.Length) break;
            }

            int tabulacao = 0;
            while(readLine[tabulacao].Equals('\t'))
            {
                tabulacao++;
                if (tabulacao >= readLine.Length) break;
            }

            if (spaces.Equals(0)) spaces = tabulacao * 4;

            if (spaces < readLine.Length)
                if (readLine[spaces] == '#') return 1;

            /* se o módulo da divisão da quantidade de espaços por 4 não der zero quer dizer que a tabulacao esta incorreta */
            if (spaces % 4 != 0) return 0;

            /* quantidade de identações */
            int indentation = spaces / 4;

            int verifyIndentation = indentation - LastIdentationLevel;

            /* Verifica a quantidade de vezes que identou */
            while (verifyIndentation > 0)
            {
                newToken = new TokensFound("TOKEN.INDENT", "Indentenção", 0, Line);
                TokensFound.Add(newToken);
                verifyIndentation--;
            }
            /* Verifica a quantidade de vezes que desidentou */
            while (verifyIndentation < 0)
            {
                newToken = new TokensFound("TOKEN.DEDENT", "Desindentenção", 0, Line);
                TokensFound.Add(newToken);
                verifyIndentation++;
            }
            LastIdentationLevel = indentation;

            return 2;
        }

        /**
         * Verifica os operadores e delimitadores
         */
        private void VerifyOperatorsAndDelimeters(string readLine, TokensFound newToken)
        {
            for (Position = 0; Position < readLine.Length; Position++)
            {
                char character = readLine[Position];
                Lexeme = character.ToString();

                if (character.Equals(' ')) continue;

                /* Comentario */
                if (character.Equals('#'))
                {
                    newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                    TokensFound.Add(newToken);
                    break;
                }

                /* Delimitadores que não são combinados com operadores e/ou outros delimitadores */
                if (character.Equals('(') || character.Equals(')') || character.Equals('[') || character.Equals(']') || character.Equals('{') ||
                    character.Equals('}') || character.Equals('~') || character.Equals(',') || character.Equals(':'))
                {
                    newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                    TokensFound.Add(newToken);
                    continue;
                }

                /*
                 * O caracter ";" funciona como uma quebra de linha
                 * Documentação: https://pt.stackoverflow.com/questions/329320/qual-a-fun%C3%A7%C3%A3o-do-ponto-e-v%C3%ADrgula-em-python
                 */
                if (character.Equals(';'))
                {
                    newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                    TokensFound.Add(newToken);
                    Line++;
                    continue;
                }

                /* Caso seja o operador de diferente */
                if (character.Equals('!'))
                {
                    var nextCharacter = readLine[++Position];
                    if (nextCharacter.Equals('='))
                    {
                        newToken = new TokensFound(Tokens.TokenList["!="], "!=", Position, Line);
                        TokensFound.Add(newToken);
                        continue;
                    }
                }

                /* Operadores e delimitadores que podem ser combinados com = */
                if(character.Equals('+') || character.Equals('-') || character.Equals('=') || character.Equals('|') ||
                   character.Equals('&') || character.Equals('%') || character.Equals('^') || character.Equals('@'))
                {
                    /* Verifica se não é fim de linha */
                    if(Position + 1 < readLine.Length)
                    {
                        /* Verifica se não possui um = após o primeiro caracter */
                        var nextCharacter = readLine[++Position];
                        if (nextCharacter.Equals('='))
                        {
                            newToken = new TokensFound(Tokens.TokenList[Lexeme + "="], Lexeme + "=", Position, Line);
                            TokensFound.Add(newToken);
                            continue;
                        }
                    }
                    /* Se nao possui = após o caracter, grava somente o caracter */ 
                    newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                    TokensFound.Add(newToken);
                    continue;
                }
                
                /* Operadores que podem ser combinados com eles mesmo ou com = */
                if(character.Equals('*') || character.Equals('/') || character.Equals('<') || character.Equals('>'))
                {

                }

            }
        }
    }
}
