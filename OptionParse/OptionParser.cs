using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.OptionParse
{
    /// <summary>
    /// Base Class of OptionParser
    /// 
    /// </summary>
    public class OptionParserUnitBase
    {
        /// <summary>
        /// option name
        /// </summary>
        public String Key;

        /// <summary>
        /// description, be used to printing usage
        /// </summary>
        public String Description;

        /// <summary>
        /// a virtual function caller which is about to parse string value
        /// </summary>
        /// <param name="Value"></param>
        public virtual void Parse(String Value)
        {

        }
    }
    /// <summary>
    /// A Reflection-Oriented TryParse bridge
    /// </summary>
    /// <example>
    /// bool boolVal = DynamicTryParse<bool>.Parse("true");
    /// int intVal = DynamicTryParse<int>.Parse("1048576");
    /// 
    /// class KVPair{
    ///    public String Key;
    ///    public String Value;
    ///    public static bool TryParse(String value,out KVPair output){
    ///       String[] splits=value.Split('=');
    ///       if(splits.Length==2) {
    ///          KVPair ret = new KVPair();
    ///          ret.Key = splits[0];
    ///          ret.Value = splits[1];
    ///          output=ret;
    ///          return true;
    ///       }
    ///       output=null;
    ///       return false;
    ///    }
    /// }
    /// KVPair kvpair = DynamicTryParse<KVPair>.Parse("A=100");
    /// 
    /// </example>
    /// <typeparam name="T">DataType which implements static method TryParse</typeparam>
    public class DynamicTryParse<T>
    {
        private delegate bool TryParseInternal(String value, out T outputValue);
        /// <summary>
        /// Use TryParseInternal to convert TryParse interface into given datatype
        /// and invoke to write parsed value.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="outputValue"></param>
        /// <returns></returns>
        public static bool TryParse(String s, out T outputValue)
        {
            outputValue = default(T);
            var method = typeof(T).GetMethod("TryParse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                TryParseInternal tryparse = (TryParseInternal)Delegate.CreateDelegate(typeof(TryParseInternal), method);
                return tryparse(s, out outputValue);
            }
            return false;
        }
        /// <summary>
        /// parse given string into specified data-typed value.
        /// </summary>
        /// <param name="s">input string to convert</param>
        /// <returns>converted value, when parse failed it will return default value of data type</returns>
        public static T Parse(String s)
        {
            T ret = default(T);
            TryParse(s, out ret);
            return ret;
        }
    }
    /// <summary>
    /// OptionParserUnit template
    /// an option value parser which can be implicitly converted to a value.
    /// 
    /// </summary>
    /// <typeparam name="T">data type</typeparam>
    public class OptionParserUnit<T> : OptionParserUnitBase
    {
        public Func<String, T> ParseDelegator;
        public T Value;
        public String RawValue;
        public static implicit operator T(OptionParserUnit<T> pthis)
        {
            return pthis.Value;
        }
        /// <summary>
        /// override implemetation which invokes ParseDelegator to assign parsed value to Value field.
        /// while keep original string input to RawValue.
        /// </summary>
        /// <param name="Value"></param>
        public override void Parse(String Value)
        {
            base.Parse(Value);
            this.RawValue = Value;

            if (ParseDelegator != null)
            {
                this.Value = ParseDelegator(Value);
            }
        }
        /// <summary>
        /// constructor for option parser
        /// </summary>
        /// <param name="key">option name</param>
        /// <param name="ParseDelegator">Converter to translate string to value</param>
        public OptionParserUnit(String key, Func<String, T> ParseDelegator = null)
        {
            this.Key = key;
            this.ParseDelegator = ParseDelegator;
            Value = default(T);
        }

    }
    /// <summary>
    /// Raw string parser, which stores value to Value field of string type.
    /// </summary>
    public class RawStringParser : OptionParserUnit<String>
    {
        public RawStringParser(String key = "")
            : base(key, null)
        {

        }
        public override void Parse(string Value)
        {
            base.Parse(Value);
            this.Value = Value;
        }
    }

    /// <summary>
    /// OptionParser will accepts varies of parser, and use them to parse arguments.
    /// </summary>
    public class OptionParser : IEnumerable<OptionParserUnitBase>
    {
        public Dictionary<String, OptionParserUnitBase> Parsers = new Dictionary<string, OptionParserUnitBase>();
        public List<OptionParserUnitBase> ParserList = new List<OptionParserUnitBase>();
        public RawStringParser RawParser = new RawStringParser();
        public void AddParsers(params OptionParserUnitBase[] parsers)
        {
            foreach (OptionParserUnitBase parser in parsers)
            {
                ParserList.Add(parser);
                if (!String.IsNullOrEmpty(parser.Key))
                    Parsers[parser.Key] = parser;
            }
        }

        public bool Parse(String[] args)
        {
            bool ret = false;
            foreach (String s in args)
            {
                int idxOfAssign = s.IndexOf('=');
                if (idxOfAssign > -1)
                {
                    String key = s.Substring(0, idxOfAssign);
                    String value = s.Substring(idxOfAssign + 1);
                    if (Parsers.ContainsKey(key))
                    {
                        Parsers[key].Parse(value);
                        ret = true;
                    }
                }
                else
                {
                    if (RawParser != null)
                    {
                        RawParser.Parse(s);
                        ret = true;
                    }
                }
            }
            return ret;
        }
        public OptionParser()
        {

        }
        public OptionParser(params OptionParserUnitBase[] parsers)
        {
            AddParsers(parsers);
        }
        /// <summary>
        /// print usages of each given parser
        /// </summary>
        /// <param name="writer">null = output to Console.Out</param>
        public void PrintUsage(TextWriter writer = null)
        {
            if (writer == null) writer = Console.Out;
            foreach (var parser in this)
            {
                if (!String.IsNullOrEmpty(parser.Key))
                {
                    writer.WriteLine(parser.Key + parser.Description);
                }
            }
        }

        public IEnumerator<OptionParserUnitBase> GetEnumerator()
        {
            return ParserList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ParserList.GetEnumerator();
        }
    }
}
