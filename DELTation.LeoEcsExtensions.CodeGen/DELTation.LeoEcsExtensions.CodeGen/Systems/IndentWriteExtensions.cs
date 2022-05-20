using System;
using System.CodeDom.Compiler;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
    public static class IndentWriteExtensions
    {
        public static Scope BeginScope(this IndentedTextWriter indentWriter, string signature)
        {
            indentWriter.WriteLine(signature);
            return indentWriter.BeginScope();
        }

        public static Scope BeginScope(this IndentedTextWriter indentWriter) =>
            new Scope(indentWriter);

        public static void OpenBrace(this IndentedTextWriter indentWriter)
        {
            indentWriter.WriteLine("{");
            indentWriter.Indent++;
        }

        public static void CloseBrace(this IndentedTextWriter indentWriter)
        {
            indentWriter.Indent--;
            indentWriter.WriteLine("}");
        }


        public struct Scope : IDisposable
        {
            private readonly IndentedTextWriter _indentedTextWriter;
            private bool _disposed;

            public Scope(IndentedTextWriter indentedTextWriter)
            {
                _indentedTextWriter = indentedTextWriter;
                _indentedTextWriter = indentedTextWriter;
                _disposed = false;
                _indentedTextWriter.OpenBrace();
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _indentedTextWriter.CloseBrace();
            }
        }
    }
}