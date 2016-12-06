using System;
using System.Collections.Generic;

namespace LeanCode.CQRS.MvcValidation
{
    public abstract class DictionaryTranslator<TCommand> : ICommandResultTranslator<TCommand>
        where TCommand : ICommand
    {
        private readonly Dictionary<string, string> propertyMap = new Dictionary<string, string>();
        private readonly Dictionary<int, string> codeMappings = new Dictionary<int, string>();
        private readonly Dictionary<Type, string> exceptionMappings = new Dictionary<Type, string>();

        string ICommandResultTranslator<TCommand>.TranslateProperty(string name)
        {
            string newName;
            propertyMap.TryGetValue(name, out newName);
            return newName;
        }

        bool ICommandResultTranslator<TCommand>.CanHandle(TCommand command, Exception ex)
        {
            return exceptionMappings.ContainsKey(ex.GetType());
        }

        string ICommandResultTranslator<TCommand>.Translate(TCommand command, Exception ex)
        {
            return exceptionMappings[ex.GetType()];
        }

        string ICommandResultTranslator<TCommand>.Translate(TCommand command, int code)
        {
            string msg;
            codeMappings.TryGetValue(code, out msg);
            return msg;
        }

        protected PropertyMap MapProprety(string from)
        {
            return new PropertyMap(from, propertyMap);
        }

        protected CodeMap MapCode(params int[] codes)
        {
            return new CodeMap(codes, codeMappings);
        }

        protected ExceptionMap MapException<TException>()
            where TException : Exception
        {
            return new ExceptionMap(typeof(TException), exceptionMappings);
        }

        public struct PropertyMap
        {
            private readonly string from;
            private readonly Dictionary<string, string> destMap;

            internal PropertyMap(string from, Dictionary<string, string> destMap)
            {
                this.from = from;
                this.destMap = destMap;
            }

            public void To(string to)
            {
                destMap.Add(from, to);
            }
        }

        public struct CodeMap
        {
            private readonly int[] codes;
            private readonly Dictionary<int, string> destMap;

            internal CodeMap(int[] codes, Dictionary<int, string> destMap)
            {
                this.codes = codes;
                this.destMap = destMap;
            }

            public void To(string to)
            {
                foreach (var c in codes)
                {
                    destMap.Add(c, to);
                }
            }
        }

        public struct ExceptionMap
        {
            private readonly Type from;
            private readonly Dictionary<Type, string> destMap;

            internal ExceptionMap(Type from, Dictionary<Type, string> destMap)
            {
                this.from = from;
                this.destMap = destMap;
            }

            public void To(string to)
            {
                destMap.Add(from, to);
            }
        }
    }
}
