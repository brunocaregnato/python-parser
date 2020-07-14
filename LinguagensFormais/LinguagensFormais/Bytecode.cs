using System.Collections.Generic;
using System.Linq;

namespace LinguagensFormais
{
    public class Bytecode
    {
        public List<BytecodeFound> BytecodeFounds { get; private set; }
        private List<TokensFound> TokensList { get; set; }
        private OperationsName OperationsName { get; set; }
        private int Size { get; set; }
        private List<Arguments> ArgumentsList { get; set; }        
        private List<int> LineArgumentsList { get; set; }

        private Stack<int> IdentLevel { get; set; }
        private int IdentPrevious { get; set; }
        private Stack<int> WhileLevel { get; set; } 

        private int WhileCounter { get; set; }
        private Stack<int> ForLevel { get; set; }
        private int ForCounter { get; set; }

        private int BlockSize { get; set; }

        private TokensFound LastLine { get; set; }

        public Bytecode(List<TokensFound> tokensList)
        {
            TokensList = tokensList;
        }

        public bool BytecodeAnalysis()
        {
            BytecodeFounds = new List<BytecodeFound>();
            OperationsName = new OperationsName();
            ArgumentsList = new List<Arguments>();
            LineArgumentsList = new List<int>();
            IdentLevel = new Stack<int>();
            WhileLevel = new Stack<int>();
            ForLevel = new Stack<int>();

            var address = 0;
            LastLine = TokensList.Last();

            for (int line = 1; line <= LastLine.Line; line++)
            {
                foreach (var found in GetObjects(line))
                {
                    var bytecodeFound = new BytecodeFound
                    {
                        Line = found.Line > 0 ? found.Line : line,
                        Address = address,
                        OpName = found.OpName,
                        Argument = found.Argument,
                        FriendlyInterpretation = found.FriendlyInterpretation
                    };

                    BytecodeFounds.Add(bytecodeFound);
                    address += 3;
                }

            }

            int setupLoopWhile = 0, setupLoopWhileAux = 0, setupLoopFor = 0, setupLoopForAux = 0;

            //Ajusta argumentos dos jumps       
            var withoutArguments = BytecodeFounds.Where(x => x.Argument != null && x.Argument.Trim().Equals("")).ToList();
            if(withoutArguments != null)
            {
                withoutArguments.ForEach(argument =>
                {
                    var name = argument.OpName;
                
                    if (name.Contains("POP_JUMP") || name.Contains("JUMP"))
                    {
                        if(LineArgumentsList.Count > 0)
                        {
                            var line = LineArgumentsList.First();
                            var firstChar = BytecodeFounds.Where(x => x.Line.Equals(line) && !x.OpName.Contains("JUMP")).FirstOrDefault();
                            if(firstChar != null)
                            {
                                if (firstChar.OpName.Contains("POP_BLOCK"))
                                {
                                    if(firstChar.OpName.Equals("POP_BLOCK_WHILE") && !setupLoopWhile.Equals(setupLoopWhileAux)) 
                                    { 
                                        setupLoopWhileAux = setupLoopWhile;
                                        var popBlock = BytecodeFounds.Where(x => x.OpName.Equals("POP_BLOCK_WHILE")).ElementAt(--WhileCounter);
                                        argument.Argument = popBlock.Address.ToString();
                                    }
                                    else if (firstChar.OpName.Equals("POP_BLOCK_FOR") && !setupLoopFor.Equals(setupLoopForAux))
                                    {
                                        setupLoopForAux = setupLoopFor;
                                        var popBlock = BytecodeFounds.Where(x => x.OpName.Equals("POP_BLOCK_FOR")).ElementAt(--ForCounter);
                                        argument.Argument = popBlock.Address.ToString();
                                    }
                                }
                                
                                else if (name.Contains("POP_JUMP"))
                                {
                                    argument.Argument = firstChar.Address.ToString();
                                }
                                else if (name.Equals("JUMP_FORWARD"))
                                {
                                    argument.Argument = "0";
                                    argument.FriendlyInterpretation = "to " + firstChar.Address.ToString();
                                }
            
                            }
            
                            LineArgumentsList.RemoveAt(0);
                        }
                    }
                    else if (name.Equals("SETUP_LOOP_WHILE"))
                    {
                        argument.OpName = "SETUP_LOOP";
                        var popBlock = BytecodeFounds.Where(x => x.OpName.Equals("POP_BLOCK_WHILE")).ElementAt(setupLoopWhile++);
                        argument.FriendlyInterpretation = "to " + (popBlock.Address).ToString(); //sempre no do pop block
                        argument.Argument = (popBlock.Address - 2).ToString(); //sempre antes do jump_absolute
                    }
                    else if (name.Equals("SETUP_LOOP_FOR"))
                    {
                        argument.OpName = "SETUP_LOOP";
                        var popBlock = BytecodeFounds.Where(x => x.OpName.Equals("POP_BLOCK_FOR")).ElementAt(setupLoopFor++);
                        argument.FriendlyInterpretation = "to " + (popBlock.Address).ToString(); //sempre no do pop block
                        argument.Argument = (popBlock.Address - 2).ToString(); //sempre antes do jump_absolute
                    }
                    else if (name.Equals("FOR_ITER"))
                    {
                        var popBlock = BytecodeFounds.Where(x => x.OpName.Equals("POP_BLOCK_FOR")).ElementAt(setupLoopFor - 1);
                        argument.FriendlyInterpretation = "to " + (popBlock.Address).ToString(); //sempre no do pop block
                    }
                });
            }

            //ajusta os pop_block
            BytecodeFounds.Where(x => x.OpName.Contains("POP_BLOCK")).ToList().ForEach(popblock => popblock.OpName = "POP_BLOCK");

            //ajusta bloco do EOF
            if (BytecodeFounds.Where(x => x.Line.Equals(LastLine.Line)).Count().Equals(2))
            {
                BytecodeFounds.Where(x => x.Line.Equals(LastLine.Line)).ToList().ForEach(block => block.Line--);
            }

            return true;
        }

        private List<BytecodeFound> GetObjects(int line)
        {
            var tokens = TokensList.FindAll(x => x.Line.Equals(line));
            var list = new List<BytecodeFound>();
            bool hasPlaceJumpForward = false;
            int index = 0;

            while (tokens[index].Token.Equals("TOKEN.INDENT"))
            {
                index++;
                IdentLevel.Push(line);
                if (tokens[index].Token.Equals("TOKEN.ELSE")) return list;
                
            }
            while (tokens[index].Token.Equals("TOKEN.DEDENT"))
            {
                index++;
                if (IdentLevel.Count > 0) IdentLevel.Pop();

                string nextToken;
                if(line + 1 <= LastLine.Line)
                {
                    if ((nextToken = TokensList.FindAll(x => x.Line.Equals(line + 1)).FirstOrDefault().Token.ToString()) != null)
                    {
                        if(nextToken.Equals("TOKEN.DEDENT") && IdentLevel.Count > 0) IdentLevel.Pop();
                    }
                }                    
                
                if (tokens[index].Token.Equals("TOKEN.ELSE") || tokens[index].Token.Equals("TOKEN.DEF")) return list;                                

                int actualLine = tokens[index].Line;
                InsertIntoLineArgumentsList(actualLine, IdentLevel.Count);

                if (tokens[index].Token.Equals("TOKEN.IF") || tokens[index].Token.Equals("TOKEN.ELIF"))
                {
                    InsertIntoLineArgumentsList(actualLine, IdentLevel.Count, tokens[index].Token);
                    if (!hasPlaceJumpForward)
                    {
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_FORWARD",
                            Argument = ""
                            
                        });

                        hasPlaceJumpForward = true;
                    }
                    else
                    {
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_ABSOLUTE",
                            Argument = ""
                        });
                    }
                    
                }
                else if (WhileLevel.Count > 0 && WhileLevel.Count != WhileCounter)
                {
                    InsertIntoLineArgumentsList(actualLine, IdentLevel.Count);

                    if(WhileLevel.Count == 1) { 
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_ABSOLUTE",
                            Argument = "",
                            Line = line - 1
                        });
                    }

                    list.Add(new BytecodeFound
                    {
                        OpName = "POP_BLOCK_WHILE",
                        Line = line - 1
                    });

                    WhileLevel.Pop();
                    WhileCounter++;
                }
                else if (ForLevel.Count > 0 && ForLevel.Count != ForCounter)
                {
                    InsertIntoLineArgumentsList(actualLine, IdentLevel.Count);

                    if (ForLevel.Count == 1)
                    {
                        list.Add(new BytecodeFound
                        {
                            OpName = "JUMP_ABSOLUTE",
                            Argument = "",
                            Line = line - 1
                        });
                    }

                    list.Add(new BytecodeFound
                    {
                        OpName = "POP_BLOCK_FOR",
                        Line = line - 1
                    });

                    ForLevel.Pop();
                    ForCounter++;
                }
                else if(!tokens[index].Token.Equals("TOKEN.DEDENT") && !tokens[index].Token.Equals("TOKEN.EOF"))
                {
                    InsertIntoLineArgumentsList(actualLine, IdentLevel.Count);
                    list.Add(new BytecodeFound
                    {
                        OpName = "JUMP_FORWARD",
                        Argument = ""
                    });
                }
                else if(!tokens[index - 1].Token.Equals("TOKEN.DEDENT") && !tokens[index].Token.Equals("TOKEN.EOF"))
                {
                    InsertIntoLineArgumentsList(actualLine, IdentLevel.Count);
                    list.Add(new BytecodeFound
                    {
                        OpName = "JUMP_ABSOLUTE",
                        Argument = ""
                    });
                }
            }
            if (tokens[index].Token.Equals("TOKEN.ELSE") || tokens[index].Token.Equals("TOKEN.DEF"))
            {
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.ID") && tokens[index + 1].Token.Equals("TOKEN.IGUAL"))
            {
                IsAssign(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.IF") || tokens[index].Token.Equals("TOKEN.ELIF"))
            {
                IsIf(tokens).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.WHILE"))
            {
                IsWhile(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.FOR"))
            {
                IsFor(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.ID") && tokens[index + 1].Token.Equals("TOKEN.PARENTESES_ESQUERDO"))
            {
                IsFunction(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.RETURN"))
            {
                IsReturn(tokens, index).ForEach(bytecode => list.Add(bytecode));
                return list;
            }
            else if (tokens[index].Token.Equals("TOKEN.EOF"))
            {
                var endProgram = new BytecodeFound()
                {
                    Argument = "0",
                    OpName = "LOAD_CONST",
                    FriendlyInterpretation = "None"
                };

                list.Add(endProgram);
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                list.Add(CreateBytecodeFoundObject(opName));
                return list;
            }

            return list;
        }

        private List<BytecodeFound> IsAssign(List<TokensFound> tokens, int indented = 0, bool isReturn = false)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;

            for (int index = isReturn ? indented : indented + 2; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetAssignBytecodeFound(tokens, index);
                if (list != null)
                {
                    list.ForEach(bytecode =>
                    {
                        Size++;
                        bytecodeFoundList.Add(bytecode);
                    });
                }
                else
                {
                    Size++;
                }
            }

            if (isReturn) return bytecodeFoundList;

            GetAssignBytecodeFound(tokens, indented + 1).ForEach(bytecode => bytecodeFoundList.Add(bytecode));

            if (tokens[tokens.Count - 1].Token.Equals("TOKEN.EOF"))
            {
                var endProgram = new BytecodeFound()
                {
                    Argument = "0",
                    OpName = "LOAD_CONST",
                    FriendlyInterpretation = "None"
                };

                bytecodeFoundList.Add(endProgram);
                OperationsName.OperationsNameList.TryGetValue(tokens[tokens.Count - 1].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
            }

            return bytecodeFoundList;
        }

        private List<BytecodeFound> GetAssignBytecodeFound(List<TokensFound> tokens, int index)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            var bytecodeFound = new BytecodeFound();

            if (IsType(tokens[index].Token))
            {
                while (!IsOutOfBounds(index + 1, tokens) && IsBinaryOperation(tokens[index + 1].Token))
                {
                    if (!IsOutOfBounds(index + 2, tokens) && IsType(tokens[index + 2].Token)) //se tem const op const, ja retorna o resultado
                    {
                        if (tokens[index].Token.Equals("TOKEN.STRING") &&
                            tokens[index + 1].Token.Equals("TOKEN.MAIS") &&
                            tokens[index + 2].Token.Equals("TOKEN.STRING"))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateStringOperation(tokens[index].Lexema, tokens[index + 2].Lexema);
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateStringOperation(bytecodeFound.FriendlyInterpretation, tokens[index + 2].Lexema);
                                Size += 2;
                                index += 2;
                            }
                        }
                        else if (tokens[index].Token.Equals("TOKEN.INTEGER") &&
                                 tokens[index + 2].Token.Equals("TOKEN.INTEGER"))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateIntOperation(
                                    int.Parse(tokens[index].Lexema),
                                    int.Parse(tokens[index + 2].Lexema),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateIntOperation(
                                    int.Parse(bytecodeFound.FriendlyInterpretation),
                                    int.Parse(tokens[index + 2].Lexema),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 2;
                                index += 2;
                            }
                        }
                        else if ((tokens[index].Token.Equals("TOKEN.FLOAT") || tokens[index].Token.Equals("TOKEN.INTEGER")) &&
                                 (tokens[index + 2].Token.Equals("TOKEN.FLOAT") || tokens[index + 2].Token.Equals("TOKEN.INTEGER")))
                        {
                            if (bytecodeFound.FriendlyInterpretation == null)
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateFloatOperation(
                                    float.Parse(tokens[index].Lexema.Replace(".", ",")),
                                    float.Parse(tokens[index + 2].Lexema.Replace(".", ",")),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 3;
                                index += 2;
                            }
                            else
                            {
                                bytecodeFound.FriendlyInterpretation = CalculateFloatOperation(
                                    float.Parse(bytecodeFound.FriendlyInterpretation.Replace(".", ",")),
                                    float.Parse(tokens[index + 2].Lexema.Replace(".", ",")),
                                    tokens[index + 1].Lexema).ToString();
                                Size += 2;
                                index += 2;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFound.OpName = opName;                        
                        if (bytecodeFound.FriendlyInterpretation == null)
                        {
                            bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                        }
                        bytecodeFound.Argument = GetArgument(opName, bytecodeFound.FriendlyInterpretation);
                        bytecodeFoundList.Add(bytecodeFound);
                        return bytecodeFoundList;
                    }
                }

                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName2);
                bytecodeFound.OpName = opName2;
                if (bytecodeFound.FriendlyInterpretation == null)
                {
                    bytecodeFound.FriendlyInterpretation = tokens[index].Lexema;
                }
                bytecodeFound.Argument = GetArgument(opName2, bytecodeFound.FriendlyInterpretation);
                bytecodeFoundList.Add(bytecodeFound);
                return bytecodeFoundList;
            }

            if (tokens[index].Token.Equals("TOKEN.IGUAL"))
            {
                var indexAux = index - 1;
                if (indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[indexAux].Lexema));
                    return bytecodeFoundList;
                }
            }

            if (tokens[index].Token.Equals("TOKEN.ID"))
            {
                var indexAux = index - 1;
                if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.IGUAL")) || tokens[indexAux].Token.Equals("TOKEN.RETURN") || (!IsOutOfBounds(index + 1, tokens) && tokens[index + 1].Token.Equals("TOKEN.IGUAL")))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if ((indexAux >= 0 && tokens[indexAux].Token.Equals("TOKEN.MAIS")) || (!IsOutOfBounds(index + 1, tokens) && tokens[index + 1].Token.Equals("TOKEN.MAIS")))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(index >= 0 ? tokens[indexAux].Token : tokens[index + 1].Token, out opName);                    
                    
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

                    return bytecodeFoundList;
                }
            }

            if (IsBinaryOperation(tokens[index].Token))
            {
                if (tokens[index + 1].Token.Equals("TOKEN.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName2);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName2, tokens[index + 1].Lexema));
                }

                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
                return bytecodeFoundList;
            }

            return null;
        }

        private List<BytecodeFound> IsIf(List<TokensFound> tokens)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;

            for (int index = 1; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetBytecodeFound(tokens, index);
                if (list != null)
                {
                    list.ForEach(bytecode =>
                    {
                        Size++;
                        bytecodeFoundList.Add(bytecode);
                    });
                }
                else
                {
                    Size++;
                }
            }

            OperationsName.OperationsNameList.TryGetValue("TOKEN.IF", out string opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

            return bytecodeFoundList;
        }
        
        private List<BytecodeFound> IsWhile(List<TokensFound> tokens, int indented = 0)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            Size = 1;
            WhileLevel.Push(tokens[indented].Line);

            OperationsName.OperationsNameList.TryGetValue(tokens[indented].Token, out string opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

            for (int index = indented + 2; index < tokens.Count; index += Size)
            {
                Size = 0;
                var list = GetBytecodeFound(tokens, index);
                if (list != null)
                {
                    list.ForEach(bytecode =>
                    {
                        Size++;
                        bytecodeFoundList.Add(bytecode);
                    });
                }
                else
                {
                    Size++;
                }
            }

            bytecodeFoundList.Add(CreateBytecodeFoundObject("POP_JUMP_IF_FALSE"));

            return bytecodeFoundList;
        }

        private List<BytecodeFound> IsFor(List<TokensFound> tokens, int indented = 0)
        {
            var bytecodeFoundList = new List<BytecodeFound>();
            ForLevel.Push(tokens[indented].Line);

            OperationsName.OperationsNameList.TryGetValue(tokens[indented].Token, out string opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));

            OperationsName.OperationsNameList.TryGetValue(tokens[indented + 3].Token, out opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[indented + 3].Lexema));

            OperationsName.OperationsNameList.TryGetValue(tokens[indented + 5].Token, out opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[indented + 5].Lexema));
                        
            var bytecode = new BytecodeFound()
            {
                OpName = "CALL_FUNCTION",
                FriendlyInterpretation = "1 positional, 0 keyword pair",
                Argument = ""
            };
            bytecodeFoundList.Add(bytecode);

            bytecode = new BytecodeFound()
            {
                OpName = "GET_ITER",
                Argument = ""
            };
            bytecodeFoundList.Add(bytecode);

            bytecode = new BytecodeFound()
            {
                OpName = "FOR_ITER",
                Argument = ""
            };
            bytecodeFoundList.Add(bytecode);

            OperationsName.OperationsNameList.TryGetValue(tokens[indented + 1].Token, out opName);
            bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[indented + 1].Lexema));

            return bytecodeFoundList;
        }

        private List<BytecodeFound> IsFunction(List<TokensFound> tokens, int indented = 0)
        {
            var bytecodeFoundList = new List<BytecodeFound>();

            var bytecode = new BytecodeFound()
            {
                OpName = "LOAD_GLOBAL",
                FriendlyInterpretation = tokens[indented].Lexema,
                Argument = ""
            };
            bytecodeFoundList.Add(bytecode);

            //possui parametros
            int parameters = 0;
            if(!tokens[indented + 2].Token.Equals("TOKEN.PARENTESES_DIREITO"))
            {
                var index = indented + 2;
                while (!IsOutOfBounds(index, tokens) && !tokens[index].Token.Equals("TOKEN.DOIS_PONTOS"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    index += 2;
                    parameters++;
                }
            }

            bytecode = new BytecodeFound()
            {
                OpName = "CALL_FUNCTION",
                FriendlyInterpretation = parameters.ToString() + " positional, 0 keyword pair"
            };
            bytecodeFoundList.Add(bytecode);

            bytecode = new BytecodeFound()
            {
                OpName = "POP_TOP"                
            };
            bytecodeFoundList.Add(bytecode);


            return bytecodeFoundList;
        }

        private List<BytecodeFound> IsReturn(List<TokensFound> tokens, int indented = 0)
        {
            var bytecodeFoundList = IsAssign(tokens, indented + 1, true);
            bytecodeFoundList.Add(new BytecodeFound()
            {
                OpName = "RETURN_VALUE"
            });


            return bytecodeFoundList;
        }

        private List<BytecodeFound> GetBytecodeFound(List<TokensFound> tokens, int index)
        {
            var bytecodeFoundList = new List<BytecodeFound>();

            if (tokens[index].Token.Equals("TOKEN.ID"))
            {
                if (IsBinaryOperation(tokens[index + 1].Token) || IsConditionalExpression(tokens[index + 1].Token))
                {
                    if (tokens[index + 2].Token.Equals("TOKEN.ID") || IsType(tokens[index + 2].Token))
                    {
                        OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                        OperationsName.OperationsNameList.TryGetValue(tokens[index + 2].Token, out opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 2].Lexema));
                        OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out opName);
                        bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, IsConditionalExpression(tokens[index + 1].Token) ? tokens[index + 1].Lexema : null));
                        return bytecodeFoundList;
                    }
                }
            }
            else if (IsConditionalExpression(tokens[index].Token))
            {
                if (IsType(tokens[index + 1].Token))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
                else if (tokens[index + 1].Token.Equals("TOKENS.ID"))
                {
                    OperationsName.OperationsNameList.TryGetValue(tokens[index + 1].Token, out string opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index + 1].Lexema));
                    OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out opName);
                    bytecodeFoundList.Add(CreateBytecodeFoundObject(opName, tokens[index].Lexema));
                    return bytecodeFoundList;
                }
            }
            else if (IsAndOr(tokens[index].Token))
            {
                OperationsName.OperationsNameList.TryGetValue(tokens[index].Token, out string opName);
                bytecodeFoundList.Add(CreateBytecodeFoundObject(opName));
                return bytecodeFoundList;
            }

            return null;
        }

        private bool IsBinaryOperation(string token)
        {
            return token.Equals("TOKEN.MAIS") || token.Equals("TOKEN.MENOS") || token.Equals("TOKEN.VEZES") ||
                   token.Equals("TOKEN.BARRA") || token.Equals("TOKEN.NOME_PARAMETRO") || token.Equals("TOKEN.ARROBA") ||
                   token.Equals("TOKEN.BARRA_DUPLA") || token.Equals("TOKEN.PORCENTO") || token.Equals("TOKEN.SHIFT_LEFT") ||
                   token.Equals("TOKEN.SHIFT_RIGHT") || token.Equals("TOKEN.ECOMERCIAL") || token.Equals("TOKEN.PIPE") ||
                   token.Equals("TOKEN.CIRCUMFLEXO");
        }

        private bool IsType(string token)
        {
            return token.Equals("TOKEN.STRING") || token.Equals("TOKEN.FLOAT") || token.Equals("TOKEN.INTEGER");
        }

        private bool IsConditionalExpression(string token)
        {
            return token.Equals("TOKEN.IGUAL_IGUAL") || token.Equals("TOKEN.DIFERENTE") ||
                   token.Equals("TOKEN.MENOR_IGUAL") || token.Equals("TOKEN.MAIOR_IGUAL") ||
                   token.Equals("TOKEN.MAIOR") || token.Equals("TOKEN.MENOR");
        }

        private bool IsAndOr(string token)
        {
            return token.Equals("TOKEN.AND") || token.Equals("TOKEN.OR");
        }

        private string CalculateStringOperation(string to1, string to2)
        {
            return "\"" + to1.Replace("\"", "") + to2.Replace("\"", "") + "\"";
        }

        private int CalculateIntOperation(int to1, int to2, string operation)
        {
            switch (operation)
            {
                case "+": return to1 + to2;
                case "-": return to1 - to2;
                case "*": return to1 * to2;
                case "/": return to1 / to2;
                case "%": return to1 % to2;
                case "&": return to1 & to2;
                case "|": return to1 | to2;
                case "<<": return to1 << to2;
                case ">>": return to1 >> to2;
                default:
                    break;
            }

            return 0;
        }

        private float CalculateFloatOperation(float to1, float to2, string operation)
        {
            switch (operation)
            {
                case "+": return to1 + to2;
                case "-": return to1 - to2;
                case "*": return to1 * to2;
                case "/": return to1 / to2;
                case "%": return to1 % to2;
                default:
                    break;
            }

            return 0;
        }

        private bool IsOutOfBounds(int index, List<TokensFound> tokens)
        {
            return !(index <= tokens.Count - 1);
        }

        private void InsertIntoLineArgumentsList(int line, int identLevel, string token = "")
        {
            if(identLevel == 1)
            {
                LineArgumentsList.Add(line);
                IdentPrevious = identLevel;
                BlockSize++;
            }
            else
            {
                if (token.Equals("TOKEN.ELIF"))
                {
                   LineArgumentsList.Sort();
                   LineArgumentsList.Insert(0, line);        
                }
                else if (identLevel == 0)
                {
                    if (LineArgumentsList.Count % 2 == 0 && identLevel != IdentPrevious)
                    {
                        LineArgumentsList.Insert(0, line);
                        IdentPrevious = identLevel;
                    }
                    else
                    {
                        LineArgumentsList.Add(line);
                        IdentPrevious = identLevel;
                    }

                    BlockSize++;
                }
                else
                {
                    LineArgumentsList.Insert(BlockSize - identLevel, line);
                    if (identLevel != IdentPrevious) IdentPrevious = identLevel;
                    else BlockSize += 2;
                }
            }          
        }

        private BytecodeFound CreateBytecodeFoundObject(string opName, string friendlyInterpretation = null)
        {
            var argument = GetArgument(opName, friendlyInterpretation);
            return new BytecodeFound
            {
                OpName = opName,
                Argument = argument,
                FriendlyInterpretation = friendlyInterpretation
            };
        }

        private string GetArgument(string opName, string friendlyInterpretation)
        {
            string value = "";
            if (friendlyInterpretation == null) return value;

            Arguments argument;
            if (opName.Contains("FAST"))
            {
                argument = ArgumentsList.Where((Arguments x) => x.OpName.Contains("FAST") && x.Value.Equals(friendlyInterpretation)).LastOrDefault();
            }
            else
            {
                argument = ArgumentsList.Where((Arguments x) => x.OpName.Equals(opName) && x.Value.Equals(friendlyInterpretation)).LastOrDefault();
            }

            if (argument != null)
            {
                return argument.Argument;
            }

            Arguments lastValue;            
            if (opName.Contains("FAST"))
            {
                lastValue = ArgumentsList.Where(x => x.OpName.Contains("FAST")).LastOrDefault();
                value = lastValue == null ? "0" : (int.Parse(lastValue.Argument) + 1).ToString();
            }
            else
            {
                if (opName.Contains("CONST"))
                {
                    lastValue = ArgumentsList.Where(x => x.OpName.Equals(opName)).LastOrDefault();
                    value = lastValue == null ? "1" : (int.Parse(lastValue.Argument) + 1).ToString();
                }
                else if (opName.Contains("COMPARE"))
                {
                    lastValue = ArgumentsList.Where(x => x.OpName.Equals(opName)).LastOrDefault();
                    value = lastValue == null ? "0" : (int.Parse(lastValue.Argument) + 1).ToString();
                }
            }

            argument = new Arguments
            {
                OpName = opName,
                Value = friendlyInterpretation,
                Argument = value
            };

            ArgumentsList.Add(argument);

            return value;
        }
    }

    class Arguments
    {
        public string OpName { get; set; }
        public string Value { get; set; }
        public string Argument { get; set; }
    }
}
