﻿using System.Diagnostics;
using Microsoft.Vbe.Interop;
using System.Runtime.InteropServices;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings.EncapsulateField;
using Rubberduck.UI.Refactorings;
using Rubberduck.VBEditor;

namespace Rubberduck.UI.Command.Refactorings
{
    [ComVisible(false)]
    public class RefactorEncapsulateFieldCommand : RefactorCommandBase
    {
        private readonly RubberduckParserState _state;

        public RefactorEncapsulateFieldCommand(VBE vbe, RubberduckParserState state, IActiveCodePaneEditor editor)
            : base(vbe, editor)
        {
            _state = state;
        }

        public override bool CanExecute(object parameter)
        {
            var pane = Vbe.ActiveCodePane;
            if (pane == null || _state.Status != ParserState.Ready)
            {
                return false;
            }

            var target = _state.FindSelectedDeclaration(pane);

            var canExecute = target != null 
                && target.DeclarationType == DeclarationType.Variable
                && !target.ParentScopeDeclaration.DeclarationType.HasFlag(DeclarationType.Member);

            Debug.WriteLine("{0}.CanExecute evaluates to {1}", GetType().Name, canExecute);
            return canExecute;
        }

        public override void Execute(object parameter)
        {
            if (Vbe.ActiveCodePane == null)
            {
                return;
            }

            using (var view = new EncapsulateFieldDialog())
            {
                var factory = new EncapsulateFieldPresenterFactory(_state, Editor, view);
                var refactoring = new EncapsulateFieldRefactoring(factory, Editor);
                refactoring.Refactor();
            }
        }
    }
}