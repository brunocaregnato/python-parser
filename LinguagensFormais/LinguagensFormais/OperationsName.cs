using System;
using System.Collections.Generic;
using System.Text;

namespace LinguagensFormais
{
    class OperationsName
    {
        public Dictionary<string, string> OperationsNameList { get; private set; }

        public OperationsName()
        {
            OperationsNameList = new Dictionary<string, string>();
            LoadOperationsName();
        }

        /**
         * OpName da linguagem Python de acordo com a documentação:
         * https://docs.python.org/3/library/dis.html
         */
        private void LoadOperationsName()
        {
            OperationsNameList.Add("TOKEN.INTEGER", "LOAD_CONST");
            OperationsNameList.Add("TOKEN.FLOAT", "LOAD_CONST");
            OperationsNameList.Add("TOKEN.STRING", "LOAD_CONST");
            OperationsNameList.Add("TOKEN.IGUAL", "STORE_FAST");
            OperationsNameList.Add("TOKEN.ID", "LOAD_FAST");
            OperationsNameList.Add("TOKEN.EOF", "RETURN_VALUE");

            /* Booleanos */
            OperationsNameList.Add("TOKEN.IF", "POP_JUMP_IF_FALSE");
            OperationsNameList.Add("TOKEN.ELIF", "POP_JUMP_IF_FALSE");
            OperationsNameList.Add("TOKEN.AND", "POP_JUMP_IF_FALSE");
            OperationsNameList.Add("TOKEN.OR", "POP_JUMP_IF_TRUE");

            /* Operadores de Comparação */
            OperationsNameList.Add("TOKEN.MAIOR", "COMPARE_OP");
            OperationsNameList.Add("TOKEN.MENOR", "COMPARE_OP");
            OperationsNameList.Add("TOKEN.IGUAL_IGUAL", "COMPARE_OP");
            OperationsNameList.Add("TOKEN.DIFERENTE", "COMPARE_OP");

            /* WHILE */
            OperationsNameList.Add("TOKEN.WHILE", "SETUP_LOOP_WHILE");

            /* FOR */
            OperationsNameList.Add("TOKEN.FOR", "SETUP_LOOP_FOR");
            OperationsNameList.Add("TOKEN.RANGE", "LOAD_GLOBAL");


            /* Operações Binárias */
            OperationsNameList.Add("TOKEN.MAIS", "BINARY_ADD");
            OperationsNameList.Add("TOKEN.MENOS", "BINARY_SUBTRACT");
            OperationsNameList.Add("TOKEN.VEZES", "BINARY_MULTIPLY");
            OperationsNameList.Add("TOKEN.BARRA", "BINARY_TRUE_DIVIDE");
            OperationsNameList.Add("TOKEN.NOME_PARAMETRO", "BINARY_POWER");
            OperationsNameList.Add("TOKEN.ARROBA", "BINARY_MATRIX_MULTIPLY");
            OperationsNameList.Add("TOKEN.BARRA_DUPLA", "BINARY_FLOOR_DIVIDE");
            OperationsNameList.Add("TOKEN.PORCENTO", "BINARY_MODULO");
            OperationsNameList.Add("TOKEN.SHIFT_LEFT", "BINARY_LSHIFT");
            OperationsNameList.Add("TOKEN.SHIFT_RIGHT", "BINARY_RSHIFT");
            OperationsNameList.Add("TOKEN.ECOMERCIAL", "BINARY_AND");
            OperationsNameList.Add("TOKEN.PIPE", "BINARY_OR");
            OperationsNameList.Add("TOKEN.CIRCUMFLEXO", "BINARY_XOR");

        }
    }
}
