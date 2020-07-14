using System;
using System.IO;

namespace LinguagensFormais
{
    public class Program
    {
        public static string FilePath { get; set; }
        public static Lexical Lexical;
        public static Bytecode Bytecode;

        static void Main(string[] args)
        {

            var dir = "D:\\temp\\";
            Console.WriteLine("Digite o caminho a ser lido em " + dir + ":");
            FilePath = dir + Console.ReadLine();

            try
            {   
                var fileContent = File.ReadAllText(FilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Não foi possível ler o arquivo, erro: " + e);
            }

            Menu();
        }

        private static void Menu()
        {

            Console.WriteLine("----------- MENU -----------");
            Console.WriteLine("1 - Análise Léxica");
            Console.WriteLine("2 - Análise Sintática");
            Console.WriteLine("3 - Gerar Bytecode");
            var valor = Console.ReadLine();
            switch(valor)
            {
                case "1": LexicalAnalysis();
                    break;
                case "2": SyntacticalAnalysis();
                    break;
                case "3": ByteCodeAnalysis();
                    break;
                default: Console.WriteLine("Opção não disponível");
                    break;
            }

            Console.ReadLine();
        }

        private static void LexicalAnalysis()
        {
            new TokensFound();
            Lexical = new Lexical();
            if (Lexical.LexicalAnalysis(FilePath))
            {
                GenerateFile();
            }
            else
            {
                GenerateFile();
                Console.WriteLine("Houve erro na análise léxica, verifique o arquivo gerado.");
                Console.ReadLine();
                
            }
        }

        private static void SyntacticalAnalysis()
        {
            LexicalAnalysis();
            var syntactic = new Syntactic(Lexical.TokensFound);
            if (syntactic.SyntacticAnalysis())
            {
                Console.WriteLine("Análise Sintática feita com sucesso.");
            }
            else
            {
                var error = syntactic.Error();
                Console.WriteLine($"Erro na análise sintática! Após o Token: {error.Token}, Linha: {error.Line}, Coluna: {error.Column}");
            }

        }

        private static void ByteCodeAnalysis()
        {
            SyntacticalAnalysis();
            Bytecode = new Bytecode(Lexical.TokensFound);
            if (Bytecode.BytecodeAnalysis())
            {
                GenerateBytecodeFile();
            }
            else
            {
                GenerateBytecodeFile();
                Console.WriteLine("Houve erro na geração do bytecode, verifique o arquivo gerado.");
                Console.ReadLine();
            }

        }

        /**
         * Gera o arquivo Saida.lex
         */
        private static void GenerateFile()
        {
            try
            {
                string printLine, sequence, token, lexema, line, column, 
                outputPath = FilePath.Substring(0, FilePath.LastIndexOf(Path.DirectorySeparatorChar));
                using (StreamWriter outputFile = new StreamWriter(outputPath + @"\Saida.lex"))
                {
                    sequence = "| " + string.Format("{0,9}", "Sequência");
                    token = " |" + string.Format("{0,25}", "Token");
                    lexema = " |" + string.Format("{0,50}", "Lexema");
                    line = " | " + string.Format("{0,4}", "Linha");
                    column = " | " + string.Format("{0,4}", "Coluna") + " |";
                    printLine = sequence + token + lexema + line + column;
                    outputFile.WriteLine(printLine);
                    var space = new string('-', 105);
                    outputFile.WriteLine(space);
                    foreach (TokensFound rt in Lexical.TokensFound)
                    {
                        sequence = "| " + string.Format("{0,9}", rt.Sequence);
                        token = " |" + string.Format("{0,25}", rt.Token);
                        lexema = " |" + string.Format("{0,50}", rt.Lexema);
                        line = " | " + string.Format("{0,4}", rt.Line);
                        column = " | " + string.Format("{0,4}", rt.Column) + " |";
                        printLine = sequence + token + lexema + line + column;
                        outputFile.WriteLine(printLine);
                    }
                }

                Console.WriteLine("Arquivo Saida.lex gerado com sucesso!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void GenerateBytecodeFile()
        {
            try
            {
                string line, address, opname, arguments, friendlyInterpretation;
                var outputPath = FilePath.Substring(0, FilePath.LastIndexOf(Path.DirectorySeparatorChar));
                using (StreamWriter outputFile = new StreamWriter(outputPath + @"\Bytecode.txt"))
                {
                    line = "| " + string.Format("{0,5}", "Linha");
                    address = " |" + string.Format("{0,10}", "Endereço");
                    opname = " |" + string.Format("{0,25}", "Operação");
                    arguments = " |" + string.Format("{0,11}", "Argumentos");
                    friendlyInterpretation = " | " + string.Format("{0,20}", "Interpretação Humana");
                    var printLine = line + address + opname + arguments + friendlyInterpretation;
                    outputFile.WriteLine(printLine + '\n' + new string('-', 134));
                    int lineAux = 0;
                    foreach (BytecodeFound rt in Bytecode.BytecodeFounds)
                    {                                
                        if(rt.OpName.Equals("JUMP_FORWARD") || rt.OpName.Equals("RETURN_VALUE"))
                        {
                            line = "| " +  string.Format("{0,5}", " ");
                            address = " |" + string.Format("{0,10}", rt.Address);
                            opname = " |" + string.Format("{0,25}", rt.OpName);
                            arguments = " |" + string.Format("{0,11}", rt.Argument);
                            friendlyInterpretation = rt.FriendlyInterpretation != null ? " | (" + string.Format("{0,5}", rt.FriendlyInterpretation).Trim() + ")" : " | " + string.Format("{0,5}", "");
                            printLine = line + address + opname + arguments + friendlyInterpretation;
                            outputFile.WriteLine(printLine);
                            continue;
                        }

                        line = rt.Line.Equals(lineAux) ? "| " + string.Format("{0,5}", " ") : "| " + string.Format("{0,5}", rt.Line);
                        address = " |" + string.Format("{0,10}", rt.Address);
                        opname = " |" + string.Format("{0,25}", rt.OpName);
                        arguments = " |" + string.Format("{0,11}", rt.Argument);
                        friendlyInterpretation = rt.FriendlyInterpretation != null ? " | (" + string.Format("{0,5}", rt.FriendlyInterpretation).Trim() + ")" : " | " + string.Format("{0,5}", "");
                        printLine = (!rt.Line.Equals(lineAux) && lineAux > 0  ? "\n" : "") + line + address + opname + arguments + friendlyInterpretation;                        
                        outputFile.WriteLine(printLine);        
                        lineAux = rt.Line;
                    }
                }

                Console.WriteLine("Arquivo Bytecode.txt gerado com sucesso!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
