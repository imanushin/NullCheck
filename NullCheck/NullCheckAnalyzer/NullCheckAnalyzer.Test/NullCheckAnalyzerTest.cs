using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace NullCheckAnalyzer.Test
{
    [TestClass]
    public sealed class NullCheckAnalyzerTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            const string test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void FailOnCtorWithoutNullChecks()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        internal sealed class TypeName
        {
            public TypeName(string value)
            {
                Value = value;
            }
            
            public string Value
            {
                get;
                private set;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = NullCheckAnalyzer.ParameterIsNullId,
                Message = string.Format(Resources.AnalyzerMessageFormat, ".ctor", "TypeName", "value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 36)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            const string fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        internal sealed class TypeName
        {
            public TypeName([NotNull]string value)
            {
                Value = value;
            }
            
            public string Value
            {
                get;
                private set;
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new NullCheckAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NullCheckAnalyzer();
        }
    }
}