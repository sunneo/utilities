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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionParser
{
    /// <summary>
    /// option parser collection 
    /// </summary>
    public class AppOptionParserFactory:OptionParse.OptionParser
    {
        public  AppOptionValues Options = new AppOptionValues();
        public static String FileNameDelegate(String Value)
        {
            if (String.IsNullOrEmpty(Value)) return Value;
            return Value.Trim('"', '\'');
        }
        public OptionParse.RawStringParser ProcessFunction = new OptionParse.RawStringParser("/Function")
        {
            Description = "=<string>, Indicate Function To Call",
            ParseDelegator = FileNameDelegate
        };
        public OptionParse.RawStringParser ProcessOutput = new OptionParse.RawStringParser("/Output")
        {
            Description = "=<filename>, output file name",
            ParseDelegator = FileNameDelegate
        };
        public OptionParse.RawStringParser ProcessInput = new OptionParse.RawStringParser("/Input")
        {
            Description = "=<filename>, input file name",
            ParseDelegator = FileNameDelegate
        };
        public AppOptionParserFactory(params OptionParse.OptionParserUnitBase[] parsers)
            : base(parsers)
        {
            AddParsers(
                ProcessFunction,
                ProcessInput,
                ProcessOutput
            );
        }
        public new bool Parse(String[] argv)
        {
            if (!base.Parse(argv))
            {
                //   return;
                return false;
            }
            Options.Function = ProcessFunction;
            Options.Output = ProcessOutput;
            Options.Input = ProcessInput;
            Options.RawValues.AddRange(this.RawParser.Values);
            return true;
        }
        public static AppOptionParserFactory Create()
        {
            AppOptionParserFactory pthis = new AppOptionParserFactory();
            return pthis;
        }
    }
}
