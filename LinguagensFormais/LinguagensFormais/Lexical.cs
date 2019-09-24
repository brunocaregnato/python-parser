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
        public string Lexema { get; private set; }
        public int LastIdentationLevel { get; private set; }
        public List<TokensFound> TokensFound { get; private set; }
        
        public Lexical()
        {
            Line = 1;
            Position = 0;
            LastIdentationLevel = 0;
            TokensFound = new List<TokensFound>();
        }

        public bool LexicalAnalysis(string filePath)
        {
            var tokens = new Tokens();

            try
            {
                StreamReader sr = new StreamReader(filePath);
                {
                    string readLine;
                    TokensFound newToken;

                    while((readLine = sr.ReadLine()) != null)
                    {
                        if (readLine.Length == 0) continue;
                        if (readLine[0] == '#') continue;

                        /* Verifica identação com espaços, o python usa 4 espaços por identação */
                        int spaces = 0;
                        while (readLine[spaces] == ' ')
                        {
                            spaces++;
                            if (spaces >= readLine.Length) break;
                        }

                        if (spaces < readLine.Length)
                            if (readLine[spaces] == '#') continue;

                        /* se o módulo da divisão da quantidade de espaços por 4 não der zero quer dizer que a tabulacao esta incorreta */
                        if (spaces % 4 != 0) return false;

                        /* quantidade de identações */
                        int indentation = spaces / 4;

                        int verifyIndentation = indentation - LastIdentationLevel;

                        /* Verifica a quantidade de vezes que identou */
                        while (verifyIndentation-- > 0)
                        {
                            newToken = new TokensFound("TOKEN.INDENT", "Indentenção", 0, Line);
                            TokensFound.Add(newToken);
                        }
                        /* Verifica a quantidade de vezes que desidentou */
                        while (verifyIndentation++ < 0)
                        {
                            newToken = new TokensFound("TOKEN.DEDENT", "Desindentenção", 0, Line);
                            TokensFound.Add(newToken);
                        }
                        LastIdentationLevel = indentation;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

    }
}
