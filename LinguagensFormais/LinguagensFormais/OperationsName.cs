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
            OperationsNameList.Add("TOKEN.MAIS", "BINARY_ADD");
            OperationsNameList.Add("TOKEN.IGUAL", "STORE_FAST");
            OperationsNameList.Add("TOKEN.ID", "LOAD_FAST");
            OperationsNameList.Add("TOKEN.EOF", "RETURN_VALUE");
        }
    }
}
