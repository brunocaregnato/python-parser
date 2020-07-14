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
        public Stack<int> IdentationLevel { get; private set; }
        public List<TokensFound> TokensFound { get; private set; }
        private Tokens Tokens { get; set; }

        public Lexical()
        {
            Line = 1;
            Position = 0;
            IdentationLevel = new Stack<int>();
            IdentationLevel.Push(0);
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

                    while ((readLine = streamReader.ReadLine()) != null)
                    {
                        Position = 0;

                        /* Linha de espaçamento */
                        readLine = readLine.TrimEnd();

                        if (readLine.Length.Equals(0)) continue;    

                        /* Verifica se começa com um comentário múltiplo */
                        var lineAux = Line;
                        isOnComment = VerifyMultipleComment(readLine, isOnComment);

                        /* Se esta num comentario, entao ignora o que tem dentro */
                        if (!isOnComment)
                        {
                            if (!lineAux.Equals(Line)) continue;

                            /* Verifica se começa com um comentario de uma linha */
                            if (readLine[0].Equals('#'))
                            {
                                newToken = new TokensFound("TOKEN.COMENTARIO", "Comentário", 0, Line);
                                TokensFound.Add(newToken);
                                Line++;
                                continue;
                            }

                            var returnIdentation = VerifyIdentation(readLine);

                            if (returnIdentation.Equals(0)) return false;
                            else if (returnIdentation.Equals(1)) continue;

                            for (Position = IdentationLevel.Peek(); Position < readLine.Length; Position++)
                            {

                                isOnComment = VerifyMultipleComment(readLine, isOnComment);

                                /* Se esta num comentario, entao ignora o que tem dentro */
                                if (!isOnComment)
                                {
                                    var returnOpeDelim = VerifyOperatorsAndDelimeters(readLine);
                                    if (returnOpeDelim.Equals(1)) continue;
                                    else if (returnOpeDelim.Equals(2)) break;


                                    switch (VerifyTypes(readLine))
                                    {
                                        case 0: return false;
                                        case 1: continue;
                                    }

                                    if (VerifyKeywordsAndIds(readLine)) continue;
                                }
                                else break;
                            }
                        }

                        Line++;
                    }

                    while (IdentationLevel.Count > 1)
                    {
                        newToken = new TokensFound("TOKEN.DEDENT", "Desindentenção", 0, Line);
                        TokensFound.Add(newToken);
                        IdentationLevel.Pop();
                    }

                    /* Adiciona token de final do arquivo quando termina de encontrar outros tokens */
                    newToken = new TokensFound("TOKEN.EOF", "EOF", TokensFound[TokensFound.Count - 1].Column + 1, TokensFound[TokensFound.Count - 1].Line);
                    TokensFound.Add(newToken);
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
        private bool VerifyMultipleComment(string readLine, bool isOnComment)
        {
            /* Verifica se começa com um comentario de multiplas linhas */
            if ((readLine[Position].ToString().Equals("\'") && readLine[Position + 1].ToString().Equals("\'") && readLine[Position + 2].ToString().Equals("\'")) || isOnComment)
            {
                if (!isOnComment)
                {
                    var newToken = new TokensFound("TOKEN.MULTIPLO_COMENTARIO", "Abre Múltiplo Comentário", 0, Line);
                    TokensFound.Add(newToken);
                    isOnComment = true;
                    Position += 2;
                }

                /* Precisa verificar até onde vai o comentário */
                int positionOnLine = Position;
                while (positionOnLine < readLine.Length)
                {
                    /* Encontrou o fechamento do comentário */
                    if (readLine[positionOnLine].ToString().Equals("\'") && readLine[positionOnLine + 1].ToString().Equals("\'") && readLine[positionOnLine + 2].ToString().Equals("\'"))
                    {
                        isOnComment = false;
                        var newToken = new TokensFound("TOKEN.MULTIPLO_COMENTARIO", "Fecha Múltiplo Comentário", positionOnLine, Line);
                        TokensFound.Add(newToken);
                        if (positionOnLine + 3 >= readLine.Length) Line++;
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
        private int VerifyIdentation(string readLine)
        {

            /* Verifica identação com espaços */
            int spaces = 0;
            while (readLine[spaces].Equals(' '))
            {
                spaces++;
                if (spaces >= readLine.Length) break;
            }

            /* Verifica identação com tabulação */
            int tabulacao = 0;
            while (readLine[tabulacao].Equals('\t'))
            {
                tabulacao++;
                if (tabulacao >= readLine.Length) break;
            }

            if (spaces.Equals(0)) spaces = tabulacao * 4;

            if (spaces < readLine.Length)
                if (readLine[spaces].Equals('#')) return 1;

            /* Verifica se identou */
            if (spaces > IdentationLevel.Peek())
            {
                var newToken = new TokensFound("TOKEN.INDENT", "Indentenção", 0, Line);
                TokensFound.Add(newToken);
                IdentationLevel.Push(spaces);
            }
            /* Verifica se desidentou */
            else if (spaces < IdentationLevel.Peek())
            {
                while (spaces != IdentationLevel.Peek())
                {
                    IdentationLevel.Pop();
                    var newToken = new TokensFound("TOKEN.DEDENT", "Desindentenção", 0, Line);
                    TokensFound.Add(newToken);
                }
            }

            return 2;
        }

        /**
         * Verifica os operadores e delimitadores
         */
        private int VerifyOperatorsAndDelimeters(string readLine)
        {
            TokensFound newToken = null;
            char character = readLine[Position];
            Lexeme = character.ToString();

            if (character.Equals(' ')) return 1;

            /* Comentario */
            if (character.Equals('#'))
            {
                newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                TokensFound.Add(newToken);
                return 2;
            }

            /* Delimitadores que não são combinados com operadores e/ou outros delimitadores */
            if (character.Equals('(') || character.Equals(')') || character.Equals('[') || character.Equals(']') || character.Equals('{') ||
                character.Equals('}') || character.Equals('~') || character.Equals(',') || character.Equals(':'))
            {
                newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                TokensFound.Add(newToken);
                return 1;
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
                return 1;
            }

            /* Caso seja o operador de diferente */
            if (character.Equals('!'))
            {
                Position++;
                if (Position + 1 > readLine.Length)
                {
                    Position--;
                    return 1;
                }
                var nextCharacter = readLine[Position];
                if (nextCharacter.Equals('='))
                {
                    newToken = new TokensFound(Tokens.TokenList["!="], "!=", Position, Line);
                    TokensFound.Add(newToken);
                    return 1;
                }
            }

            /* Operadores e delimitadores que podem ser combinados com = */
            if (character.Equals('+') || character.Equals('-') || character.Equals('=') || character.Equals('|') ||
               character.Equals('&') || character.Equals('%') || character.Equals('^') || character.Equals('@'))
            {
                /* Verifica se não é fim de linha */
                if (Position + 1 < readLine.Length)
                {
                    /* Verifica se não possui um = após o primeiro caracter */
                    if (readLine[Position + 1].Equals('='))
                    {
                        newToken = new TokensFound(Tokens.TokenList[Lexeme + "="], Lexeme + "=", Position, Line);
                        TokensFound.Add(newToken);
                        Position++;
                        return 1;
                    }
                }

                /* Se nao possui = após o caracter, grava somente o caracter */
                newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                TokensFound.Add(newToken);
                return 1;
            }

            /* Operadores que podem ser combinados com eles mesmo ou com = */
            if (character.Equals('*') || character.Equals('/') || character.Equals('<') || character.Equals('>'))
            {
                /* Verifica se não é fim de linha */
                if (Position + 1 < readLine.Length)
                {
                    /* Verifica se não possui um = após o primeiro caracter */
                    var nextCharacter = readLine[Position + 1];
                    if (nextCharacter.Equals('='))
                    {
                        newToken = new TokensFound(Tokens.TokenList[Lexeme + "="], Lexeme + "=", Position, Line);
                        TokensFound.Add(newToken);
                        Position++;
                        return 1;
                    }
                    else if (nextCharacter.Equals(character))
                    {
                        Position++;
                        if(Position + 1 < readLine.Length)
                        {
                            nextCharacter = readLine[Position + 1];
                            if (nextCharacter.Equals("="))
                            {
                                newToken = new TokensFound(Tokens.TokenList[Lexeme + Lexeme + "="], Lexeme + Lexeme + "=", Position - 1, Line);
                                TokensFound.Add(newToken);
                                Position++;
                                return 1;
                            }
                            newToken = new TokensFound(Tokens.TokenList[Lexeme + Lexeme], Lexeme + Lexeme, Position - 1, Line);
                            TokensFound.Add(newToken);
                            return 1;
                        }                        
                    }
                }
                /* Se nao possui = após o caracter ou o proprio caracter, grava somente o caracter */
                newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, Position, Line);
                TokensFound.Add(newToken);
                return 1;
            }
            return 0;
        }

        /**
         * Verifica palavras reservadas e identificadores
         */
        private bool VerifyKeywordsAndIds(string readLine)
        {
            var character = readLine[Position];

            if ((character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') || character.Equals('_')) // ID ou palavra reservada
            {
                int initialPosition = Position;
                if (Position + 1 < readLine.Length)
                {
                    var nextCharacter = readLine[Position + 1];
                    while ((nextCharacter >= 'a' && nextCharacter <= 'z') ||
                            (nextCharacter >= 'A' && nextCharacter <= 'Z') ||
                            (nextCharacter >= '0' && nextCharacter <= '9') || nextCharacter.Equals('_'))
                    {
                        Lexeme += nextCharacter.ToString();
                        Position++;
                        if (Position + 1 < readLine.Length)
                        {
                            nextCharacter = readLine[Position + 1];
                        }
                        else
                        {
                            nextCharacter = ' ';
                        }
                    }
                }
                TokensFound newToken = null;
                if (Tokens.TokenList.ContainsKey(Lexeme))
                {
                    newToken = new TokensFound(Tokens.TokenList[Lexeme], Lexeme, initialPosition, Line);
                }
                else
                {
                    newToken = new TokensFound("TOKEN.ID", Lexeme, initialPosition, Line);
                }

                TokensFound.Add(newToken);
                return true;
            }

            return false;
        }

        /**
         * Verifica tipos (float, string e int)
         */
        private int VerifyTypes(string readLine)
        {
            var character = readLine[Position];
            /* int e float */
            if (character >= '1' && character <= '9')
            {
                var startPosition = Position;
                var type = "INTEGER";
                while (Position + 1 < readLine.Length)
                {
                    Position++;
                    var nextCharacter = readLine[Position];
                    if (nextCharacter >= '0' && nextCharacter <= '9')
                    {
                        Lexeme += nextCharacter.ToString();
                    }
                    else if (nextCharacter.Equals('.')) //se encontrou . então é float
                    {
                        if (type.Equals("FLOAT"))
                        {
                            break;
                        }
                        else
                        {
                            Lexeme += nextCharacter.ToString();
                            type = "FLOAT";
                        }
                    }
                    else
                    {
                        Position--;
                        break;
                    }
                }
                var newToken = new TokensFound("TOKEN." + type, Lexeme, startPosition, Line);
                TokensFound.Add(newToken);
            }
            /* string */
            else if (character.Equals('"') || character.Equals('\''))
            {
                var startPosition = Position;
                while (Position + 1 < readLine.Length)
                {
                    Position++;
                    var nextCharacter = readLine[Position];
                    Lexeme += nextCharacter.ToString();
                    if (nextCharacter.Equals(character)) break;

                }
                if (Lexeme.LastIndexOf(character).Equals(0)) //string nao foi fechado, retorna erro
                    return 0;

                var newToken = new TokensFound("TOKEN.STRING", Lexeme, startPosition, Line);
                TokensFound.Add(newToken);
                return 1;
            }
            return 2;
        }
    }
}