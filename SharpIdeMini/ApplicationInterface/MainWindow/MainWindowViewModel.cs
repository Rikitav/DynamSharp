using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using SharpIdeMini.ApplicationInterface.ModelBase;
using SharpIdeMini.Compilation;
using System;
using System.IO;
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
        private RelayCommand? _readFileContent;
        private RelayCommand? _saveContentFile;

        // File dialogs
        private readonly OpenFileDialog _openFile = new OpenFileDialog()
        {
            Filter = "C# source code|*.cs",
            CheckFileExists = true,
            Multiselect = false
        };

        private readonly SaveFileDialog _saveFile = new SaveFileDialog()
        {
            Filter = "C# source code|*.cs",
            CheckFileExists = false
        };

        // Messages
        private const string OutputMessage_NullScript = "{ ENVIRONMENT_ERROR : Script instance is null, recompile code }";
        private const string OutputMessage_NoChanges = "{ ENVIRONMENT_ERROR : No changes was made, no need to recompile }";

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
            });
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

        public RelayCommand ReadFileContent
        {
            get => _readFileContent ??= new RelayCommand(_ => ReadContentFromFile());
        }

        public RelayCommand SaveContentFile
        {
            get => _saveContentFile ??= new RelayCommand(_ => SaveEditorContentToFile());
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

        private void ReadContentFromFile()
        {
            try
            {
                if (!(_openFile.ShowDialog() ?? false))
                {
                    MessageBox.Show("No file selected");
                    return;
                }

                EditorText = File.ReadAllText(_openFile.FileName);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed to read file contents");
            }
        }

        private void SaveEditorContentToFile()
        {
            try
            {
                if (!(_saveFile.ShowDialog() ?? false))
                {
                    MessageBox.Show("No file selected");
                    return;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed to save script to file\n\n" + exc.ToString());
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
