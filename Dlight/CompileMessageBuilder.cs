﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace AbstractSyntax
{
    public static class CompileMessageBuilder
    {
        private static Dictionary<string, string> messageBase;
        private static XNamespace ns;

        static CompileMessageBuilder()
        {
            messageBase = new Dictionary<string, string>();
            ns = "CompileMessage.xsd";
            foreach(var file in Directory.EnumerateFiles(@"./msg/", "*.xml"))
            {
                var element = XElement.Load(file);
                if(element.Name != ns + "compile-message")
                {
                    continue;
                }
                AppendBase(element);
            }
        }

        private static void AppendBase(XElement element)
        {
            foreach(var v in element.Descendants(ns + "msg"))
            {
                var key = (string)v.Attribute("key");
                var msg = (string)v;
                messageBase.Add(key, msg);
            }
        }

        public static string Build(CompileMessageManager manager)
        {
            var builder = new StringBuilder();
            foreach(var v in manager)
            {
                builder.AppendLine(Build(v));
            }
            builder.Append(manager);
            return builder.ToString();
        }

        public static string Build(CompileMessage message)
        {
            var builder = new StringBuilder();
            builder.Append(GetPrefix(message.Type)).Append(": ");
            builder.Append(message.Position).Append(": ");
            var msg = messageBase[message.Key];
            var current = 0;
            var match = Regex.Match(msg, @"\{.*?\}");
            while(match.Success)
            {
                builder.Append(msg.Substring(current, match.Index - current));
                current = match.Index + match.Length;
                builder.Append(GetValue(match.Value.Trim('{', '}'), message.Target));
                match = match.NextMatch();
            }
            builder.Append(msg.Substring(current, msg.Length - current));
            return builder.ToString();
        }

        private static string GetPrefix(CompileMessageType type)
        {
            switch (type)
            {
                case CompileMessageType.Info: return "Info";
                case CompileMessageType.Error: return "Error";
                case CompileMessageType.Warning: return "Warning";
            }
            throw new Exception();
        }

        private static string GetValue(string exp, object target)
        {
            object current = target;
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach(var s in exp.Split('.'))
            {
                var type = current.GetType();
                var prop = type.GetProperty(s, bf);
                if(prop != null && prop.CanRead)
                {
                    current = prop.GetValue(current);
                    continue;
                }
                var field = type.GetField(s, bf);
                if(field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }
                throw new CompileMessageBuildExcepsion(exp, target);
            }
            return current.ToString();
        }
    }

    public class CompileMessageBuildExcepsion : ApplicationException
    {
        public string Exp { get; set; }
        public object Target { get; set; }

        public CompileMessageBuildExcepsion(string exp, object target)
            : base(BuildMessage(exp, target))
        {
            Exp = exp;
            Target = target;
        }

        private static string BuildMessage(string exp, object target)
        {
            var builder = new StringBuilder();
            object current = target;
            const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var s in exp.Split('.'))
            {
                var type = current.GetType();
                if (target == current)
                {
                    builder.Append(type.Name);
                }
                else
                {
                    builder.Append("<").Append(type.Name).Append(">");
                }
                builder.Append(".").Append(s);
                var prop = type.GetProperty(s, bf);
                if (prop != null && prop.CanRead)
                {
                    current = prop.GetValue(current);
                    continue;
                }
                var field = type.GetField(s, bf);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }
                builder.Append(" is not found");
                return builder.ToString();
            }
            throw new Exception();
        }
    }
}
