using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using SharpIdeMini.ApplicationInterface.ModelBase;
using SharpIdeMini.Compilation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SharpIdeMini.ApplicationInterface.MainWindow
{
    public class MainWindowViewModel : NotifyViewModelBase
    {
        // Is working fields
        private bool _scriptWorking;
        private bool _compilerWorking;
        private bool _changesMade;

        // Controls state fields
        private bool _autoCompileCheck;
        private string _editorText;
        private string _outputText;

        // Compiling fields
        private readonly IdeCompiler _compiler;
        private readonly CancellationTokenSource _cancellationSource;
        private object? _scriptReturnValue;
        private Diagnostic[] _lastErrors;

        // Commands fields
        private RelayCommand? _compileCommand;
        private RelayCommand? _executeCommand;

        // Messages
        private const string MessageBox_NullScript = "Failed to compile code inside editor.\nScript is null";
        private const string MessageBox_CompilationFailed = "Failed to compile code inside editor.\nPlease check for errors in output box";
        private const string MessageBox_NoSourceCode = "No source code was speccified";
        private const string OutputMessage_NullScript = "{ ERROR : Script instance is null, recompile code }";
        private const string OutputMessage_NoSourceCode = "{ ERROR : Script is empty, type some }";
        private const string OutputMessage_NoChanges = "{ ERROR : No changes was made, no need to recompile }";

        public bool ScriptWorking
        {
            get => _scriptWorking;
            set => SetProperty(ref _scriptWorking, value);
        }

        public bool CompilerWorking
        {
            get => _compilerWorking;
            set => SetProperty(ref _compilerWorking, value);
        }

        public bool AutoCompileCheck
        {
            get => _autoCompileCheck;
            set => SetProperty(ref _autoCompileCheck, value);
        }

        public string EditorText
        {
            get => _editorText;
            set => SetProperty(ref _editorText, value);
        }

        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        public object? ScriptReturnValue
        {
            get => _scriptReturnValue;
            set => SetProperty(ref _scriptReturnValue, value);
        }

        public RelayCommand ExecuteCommand
        {
            get => _executeCommand ??= new RelayCommand(_ =>
            {
                Task.Run(async () =>
                {
                    RunLastScript();
                    await Task.Yield();
                });
            }, _ => !_scriptWorking & !_compilerWorking);
        }

        public RelayCommand CompileCommand
        {
            get => _compileCommand ??= new RelayCommand(_ =>
            {
                Task.Run(async () =>
                {
                    if (CompileEditorCode())
                        RunLastScript();
                    await Task.Yield();
                });
            }, _ => !_compilerWorking);
        }

        public MainWindowViewModel()
        {
            _scriptWorking = false;
            _compilerWorking = false;
            _changesMade = false;

            _editorText = string.Empty;
            _outputText = string.Empty;
            _autoCompileCheck = false;

            _compiler = new IdeCompiler();
            _cancellationSource = new CancellationTokenSource();
            _scriptReturnValue = null;
        }

        private bool CompileEditorCode()
        {
            //_cancellationSource.Cancel();
            StringBuilder errorsBuilder = new StringBuilder();
            CompilerWorking = true;

            try
            {
                if (!_changesMade)
                {
                    OutputText = OutputMessage_NoChanges;
                    if (_lastErrors.Any())
                    {
                        foreach (Diagnostic diagnostic in _lastErrors)
                            errorsBuilder.AppendLine(diagnostic.ToString());
                    }

                    return false;
                }

                if (!_compiler.Compile(EditorText, _cancellationSource.Token, out _lastErrors))
                {
                    foreach (Diagnostic diagnostic in _lastErrors)
                        errorsBuilder.AppendLine(diagnostic.ToString());

                    return false;
                }

                return true;
            }
            finally
            {
                OutputText = errorsBuilder.ToString();
                CompilerWorking = false;
                _changesMade = false;
            }
        }

        private void RunLastScript()
        {
            ScriptWorking = true;

            try
            {
                if (_compiler.LastScript == null)
                {
                    OutputText = OutputMessage_NullScript;
                    return;
                }

                ScriptInitGlobalValues globals = new ScriptInitGlobalValues();
                globals.Environment_CompilerOut.Flushed += (_, _) => OutputText = globals.Environment_CompilerOut.ToString();
                ScriptState<object> scriptState = _compiler.LastScript.RunAsync(globals, _cancellationSource.Token).Result;
                ScriptReturnValue = scriptState.ReturnValue;
            }
            finally
            {
                ScriptWorking = false;
            }
        }

        protected override void OnPropertyChanged(string? propertyName, object? oldValue, object? newValue)
        {
            switch (propertyName ?? string.Empty)
            {
                case nameof(AutoCompileCheck):
                case nameof(EditorText):
                    {
                        _changesMade = true;
                        if (AutoCompileCheck)
                            CompileCommand.Execute(null);

                        break;
                    }
            }
        }
    }
}
