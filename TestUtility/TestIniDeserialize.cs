using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.OptionParser.Attributes;

namespace TestUtility
{
    [TestClass]
    public class TestIniDeserialize
    {
        public class PairItem
        {
            public String Key;
            public String Value;
        }
        public class TestClass
        {
            public int intItem;
            public String stringItem;
            [FlattenArrayLengthName("PairCount"), FlattenArrayName("pairs$INDEX$","$INDEX$")]
            public PairItem[] pairs;
        }
        [TestMethod]
        public void TestTemplateDeserialize()
        {
            String iniContent = String.Join(Environment.NewLine,
                "intItem=1",
                "stringItem=2",
                "PairCount=3",
                "pairs0.Key=K1",
                "pairs0.Value=V1",
                "pairs1.Key=K2",
                "pairs1.Value=V2",
                "pairs2.Key=K3",
                "pairs2.Value=V3");
            TestClass clz= IniReader.DeserializeString<TestClass>(iniContent);
        }
        [TestMethod]
        public void TestTemplateSerialize()
        {
            TestClass clz = new TestClass();
            clz.intItem = 1;
            clz.stringItem = "2";
            clz.pairs = new PairItem[]{
                new PairItem(){ Key="K1", Value="V1"},
                new PairItem(){ Key="K2", Value="V2"},
                new PairItem(){ Key="K3", Value="V3"}
            };
            String iniContent = IniWriter.SerializeToString(clz);
        }
    }
}
