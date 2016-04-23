using System.Diagnostics;
using Microsoft.Vbe.Interop;
using System.Runtime.InteropServices;
using Antlr4.Runtime;
using Rubberduck.Common;
using Rubberduck.Parsing;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings.ExtractMethod;
using Rubberduck.SmartIndenter;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.VBEInterfaces.RubberduckCodePane;

namespace Rubberduck.UI.Command.Refactorings
{
    [ComVisible(false)]
    public class RefactorExtractMethodCommand : RefactorCommandBase
    {
        private readonly RubberduckParserState _state;
        private readonly IIndenter _indenter;

        public RefactorExtractMethodCommand(VBE vbe, RubberduckParserState state, IActiveCodePaneEditor editor, IIndenter indenter)
            : base (vbe, editor)
        {
            _state = state;
            _indenter = indenter;
        }

        public override bool CanExecute(object parameter)
        {
            if (Vbe.ActiveCodePane == null || _state.Status != ParserState.Ready)
            {
                return false;
            }

            var selection = Vbe.ActiveCodePane.GetSelection();
            var code = Vbe.ActiveCodePane.CodeModule.Lines[selection.Selection.StartLine, selection.Selection.LineCount];

            var parentProcedure = _state.AllDeclarations.FindSelectedDeclaration(selection, DeclarationExtensions.ProcedureTypes, d => ((ParserRuleContext)d.Context.Parent).GetSelection());
            var canExecute = parentProcedure != null
                && selection.Selection.StartColumn != selection.Selection.EndColumn
                && selection.Selection.LineCount > 0
                && !string.IsNullOrWhiteSpace(code);

            Debug.WriteLine("{0}.CanExecute evaluates to {1}", GetType().Name, canExecute);
            return canExecute;
        }

        public override void Execute(object parameter)
        {
            var factory = new ExtractMethodPresenterFactory(Editor, _state.AllDeclarations, _indenter);
            var refactoring = new ExtractMethodRefactoring(factory, Editor);
            refactoring.InvalidSelection += HandleInvalidSelection;
            refactoring.Refactor();
        }
    }
}