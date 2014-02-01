﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection.Emit;
using Dlight.LexicalAnalysis;
using Dlight.SyntacticAnalysis;
using Dlight.CilTranslate;

namespace Dlight
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[0];
            Func<string, string> lambda = x => x + fileName;
            Root root = new Root();
            root.Append(CompileFile(fileName));
            RootTranslator trans = new RootTranslator();
            RegisterEmbed(trans);
            root.PreProcess(trans);
            root.CheckSemantic();
            root.CheckDataType();
            Console.WriteLine(root);
            if (root.ErrorCount == 0)
            {
                root.Translate();
                trans.Save(fileName.Replace(".txt", ""));
            }
        }

        static Element CompileFile(string fileName)
        {
            string text = File.ReadAllText(fileName);
            Lexer lexer = new Lexer();
            List<Token> token = lexer.Lex(text, fileName);
            Parser parser = new Parser();
            return parser.Parse(token, fileName.Replace(".txt", ""));
        }

        static void RegisterEmbed(RootTranslator trans)
        {
            trans.CreateExturn(typeof(DlightObject.Integer32));
            trans.CreateExturn(typeof(DlightObject.Binary64));
        }
    }
}
