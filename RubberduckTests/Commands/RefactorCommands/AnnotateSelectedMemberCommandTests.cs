﻿using System;
using Moq;
using NUnit.Framework;
using Rubberduck.Interaction;
using Rubberduck.Parsing.Rewriter;
using Rubberduck.Parsing.UIContext;
using Rubberduck.Parsing.VBA;
using Rubberduck.Refactorings;
using Rubberduck.Refactorings.AnnotateDeclaration;
using Rubberduck.UI.Command;
using Rubberduck.UI.Command.Refactorings;
using Rubberduck.UI.Command.Refactorings.Notifiers;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.SafeComWrappers;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using Rubberduck.VBEditor.Utility;

namespace RubberduckTests.Commands.RefactorCommands
{
    [TestFixture]
    public class AnnotateSelectedMemberCommandTests : RefactorCodePaneCommandTestBase
    {
        [Category("Commands")]
        [Test]
        public void AnnotateDeclaration_CanExecute_OutsideMember()
        {
            var vbe = SetupAllowingExecution();
            vbe.ActiveCodePane.Selection = new Selection(5,1);

            Assert.IsFalse(CanExecute(vbe));
        }

        protected override CommandBase TestCommand(
            IVBE vbe, RubberduckParserState state, 
            IRewritingManager rewritingManager,
            ISelectionService selectionService)
        {
            var factory = new Mock<IRefactoringPresenterFactory>().Object;
            var msgBox = new Mock<IMessageBox>().Object;

            var uiDispatcherMock = new Mock<IUiDispatcher>();
            uiDispatcherMock
                .Setup(m => m.Invoke(It.IsAny<Action>()))
                .Callback((Action action) => action.Invoke());
            var userInteraction = new RefactoringUserInteraction<IAnnotateDeclarationPresenter, AnnotateDeclarationModel>(factory, uiDispatcherMock.Object);

            var annotationUpdater = new AnnotationUpdater();
            var annotateDeclarationAction = new AnnotateDeclarationRefactoringAction(rewritingManager, annotationUpdater);

            var selectedDeclarationProvider = new SelectedDeclarationProvider(selectionService, state);

            var refactoring = new AnnotateDeclarationRefactoring(annotateDeclarationAction, selectedDeclarationProvider, selectionService, userInteraction);
            var notifier = new AnnotateDeclarationFailedNotifier(msgBox);

            return new AnnotateSelectedMemberCommand(refactoring, notifier, selectionService, state, state, selectedDeclarationProvider);
        }

        protected override IVBE SetupAllowingExecution()
        {
            const string input =
                @"Public Sub Foo()
myLabel: Debug.Print ""Label"";
End Sub


";
            var selection = new Selection(2,2);
            return TestVbe(input, selection, ComponentType.ClassModule);
        }
    }
}