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
using Utilities.OptionParser.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities.OptionParser
{
    public class AppFunctionHandler
    {
        
        public void PrintUsage()
        {
            foreach (var kv in HandlerMap)
            {
                Console.WriteLine("/Function=" + kv.Key);
                Action<AppOptionValues> Handler = kv.Value;
                DescriptionAttribute[] customAttrs = (DescriptionAttribute[])Handler.Method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                ArgumentDescAttribute[] customArgDesc = (ArgumentDescAttribute[])Handler.Method.GetCustomAttributes(typeof(ArgumentDescAttribute), true);


                if (customAttrs.Length == 0)
                {
                    Object handlerTarget = Handler.Target;
                    Type handlerTargetType = handlerTarget.GetType();
                    FieldInfo handlerTargetMethod = handlerTargetType.GetField("handler", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance);
                    Object handlerTargetHandlerValue = handlerTargetMethod.GetValue(handlerTarget);
                    PropertyInfo prop = handlerTargetHandlerValue.GetType().GetProperty("Method");


                    if (prop != null)
                    {
                        System.Reflection.MethodInfo trueMethod = (System.Reflection.MethodInfo)prop.GetValue(handlerTargetHandlerValue);
                        customAttrs = (DescriptionAttribute[])trueMethod.GetCustomAttributes(typeof(DescriptionAttribute), true);
                        customArgDesc = (ArgumentDescAttribute[])trueMethod.GetCustomAttributes(typeof(ArgumentDescAttribute), true);
                    }
                }
                
                if (customAttrs.Length > 0)
                {
                    foreach (DescriptionAttribute desc in customAttrs)
                    {
                        Console.WriteLine("  "+desc.Description);
                    }
                }
                else
                {
                    customAttrs = (DescriptionAttribute[])Handler.Method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                }
                
                if (customArgDesc.Length > 0)
                {
                    foreach (ArgumentDescAttribute desc in customArgDesc)
                    {
                        foreach (String descStr in desc.Description)
                        {
                            Console.WriteLine("    " + descStr);
                        }
                    }
                }
                Console.WriteLine();
            }
        }
        /// <summary>
        /// add function handler
        /// </summary>
        /// <param name="functionName">function name</param>
        /// <param name="Handler">handler which deal with function</param>
        public void AddHandler(String functionName, Action<AppOptionValues> Handler) 
        {
            HandlerMap[functionName] =Handler;
        }
        /// <summary>
        /// handle given function if function name is given
        /// </summary>
        /// <param name="Values">option value collection</param>
        /// <returns></returns>
        public bool Handle(AppOptionValues Values)
        {
            if (!String.IsNullOrEmpty(Values.Function))
            {
                if (HandlerMap.ContainsKey(Values.Function))
                {
                    try
                    {
                        HandlerMap[Values.Function](Values);
                    }
                    catch (Exception ee)
                    {
                        Console.Error.WriteLine(ee.ToString());
                    }
                    return true;
                }
            }
            return false;
        }
        public AppFunctionHandler()
        {
        }
        Dictionary<String, Action<AppOptionValues>> HandlerMap = new Dictionary<string, Action<AppOptionValues>>();
    }
}
