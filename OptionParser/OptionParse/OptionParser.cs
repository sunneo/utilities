/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionParser.OptionParse
{


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
