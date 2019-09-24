using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LinguagensFormais
{
    public class TokensFound
    {
        public int Sequence { get; private set; }
        public string Token { get; private set; }
        public string Lexema { get; private set; }
        public int Column { get; private set; }
        public int Line { get; private set; }

        private static int _newSequence;

        /*
         * Reseta sequencia para refazer analise lexica
         */
        public TokensFound()
        {
            _newSequence = 0;
        }

        /*
         * Grava cada token encotrado e incrementa a sequencia
         */
        public TokensFound(string token, string lexema, int column, int line)
        {
            Sequence = Interlocked.Increment(ref _newSequence);
            Token = token;
            Lexema = lexema;
            Column = column;
            Line = line;
        }
    }
}
