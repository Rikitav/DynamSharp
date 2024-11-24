namespace SharpIdeMini.Compilation
{
    public class ScriptInitGlobalValues
    {
        public readonly CompilerStringWriter Environment_CompilerOut;

        public ScriptInitGlobalValues()
        {
            Environment_CompilerOut = new CompilerStringWriter(true);
        }
    }
}
