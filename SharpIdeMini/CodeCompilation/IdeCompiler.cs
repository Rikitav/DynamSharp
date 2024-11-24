using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpIdeMini.Compilation
{
    public class IdeCompiler
    {
        private Script<object>? _lastScript;
        private readonly ScriptOptions _compileOptions;
        private const string _scriptInitCode = "Console.SetOut(Environment_CompilerOut);";

        public Script<object>? LastScript
        {
            get => _lastScript;
        }

        public IdeCompiler()
        {
            _lastScript = null;
            _compileOptions = ScriptOptions.Default
                .AddImports("System", "System.IO", "System.Collections.Generic", "System.Console", "System.Diagnostics", "System.Dynamic", "System.Linq", "System.Text", "System.Threading.Tasks")
                .AddReferences("System", "System.Core", "Microsoft.CSharp");
        }

        public bool Compile(string sourceCode, CancellationToken cancellToken, [NotNullWhen(false)] out Diagnostic[]? errors)
        {
            _lastScript = CSharpScript.Create(_scriptInitCode, _compileOptions, typeof(ScriptInitGlobalValues)).ContinueWith(sourceCode, _compileOptions);
            ImmutableArray<Diagnostic> cmplErrors = _lastScript.Compile(cancellToken);

            if (!cmplErrors.IsDefaultOrEmpty)
            {
                _lastScript = null;
                errors = cmplErrors.ToArray();
                return false;
            }

            errors = null;
            return true;
        }
    }
}
