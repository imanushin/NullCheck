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
        public void PassOnEmptyTest()
        {
            const string test = @"";

            VerifyCSharpDiagnostic(test);
        }
        
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
        }

        [TestMethod]
        public void FailOnMethodWithoutNullChecks()
        {
            const string test = @"
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
            public TypeName()
            {
            }

            public void SetValue(string value)
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
                Message = string.Format(Resources.AnalyzerMessageFormat, "SetValue", "TypeName", "value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 17, 41)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SuccessOnMethodWithNullChecks()
        {
            const string test = @"
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
            public TypeName()
            {
            }

            public void SetValue([NotNull] string value)
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

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void SuccessOnMethodWithNullChecksIgnore()
        {
            const string test = @"
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
            public TypeName()
            {
            }

            public void SetValue([CanBeNull] string value)
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

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void SuccessOnMethodWithOutParameters()
        {
            const string test = @"
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
            public TypeName()
            {
            }

            public void SetValue(int value)
            {
                Value = value.ToString();
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

            VerifyCSharpDiagnostic(test);
        }



        [TestMethod]
        public void SuccessOnMethodWithNotReferenceType()
        {
            const string test = @"
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
            public TypeName()
            {
            }

            public void SetValue(out string value)
            {
                value = Value;
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

            VerifyCSharpDiagnostic(test);
        }


        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NullCheckAnalyzer();
        }
    }
}