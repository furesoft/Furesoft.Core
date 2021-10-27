using DigitalRune.Windows.TextEditor;
using DigitalRune.Windows.TextEditor.Completion;
using ExpressionEvaluator_App.Properties;
using Furesoft.Core.ExpressionEvaluator;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nardole;

internal class CodeCompletionDataProvider : AbstractCompletionDataProvider
{
    private readonly ImageList _imageList;
    private ExpressionParser evaluator;

    public CodeCompletionDataProvider(ExpressionParser evaluator)
    {
        this.evaluator = evaluator;

        _imageList = new ImageList();
        _imageList.Images.Add(Resources.TextFile);
        _imageList.Images.Add(Resources.field);
        _imageList.Images.Add(Resources.method);
        _imageList.Images.Add(Resources.macro);
    }

    public override ImageList ImageList
    {
        get { return _imageList; }
    }

    public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
    {
        // This class provides the data for the Code-Completion-Window.
        // Some random variables and methods are returned as completion data.

        List<ICompletionData> completionData = new List<ICompletionData>();

        foreach (var mod in evaluator.Modules)
        {
            completionData.Add(new DefaultCompletionData(mod.Key, "", 0));
        }

        foreach (var k in new[] { "in", "set", "valueset", "INFINITY", "module", "alias", "use" })
        {
            completionData.Add(new DefaultCompletionData(k, "", 0));
        }

        foreach (var set in evaluator.RootScope.SetDefinitions)
        {
            completionData.Add(new DefaultCompletionData(set.Key, "", 0));
        }

        FillFromScope(completionData);

        return completionData.ToArray();
    }

    public override CompletionDataProviderKeyResult ProcessKey(char key)
    {
        return key == '\t' ? CompletionDataProviderKeyResult.InsertionKey : CompletionDataProviderKeyResult.NormalKey;
    }

    private void FillFromScope(List<ICompletionData> completionData)
    {
        foreach (var variable in evaluator.RootScope.Variables)
        {
            completionData.Add(new DefaultCompletionData(variable.Key, "", 1));
        }

        foreach (var macro in evaluator.RootScope.Macros)
        {
            completionData.Add(new DefaultCompletionData(macro.Key, "", 3));
        }

        foreach (var func in evaluator.RootScope.Functions)
        {
            completionData.Add(new DefaultCompletionData(Umangle(func.Key), "", 2));
        }

        foreach (var func in evaluator.RootScope.ImportedFunctions)
        {
            completionData.Add(new DefaultCompletionData(Umangle(func.Key), "", 2));
        }
    }

    private string Umangle(string name)
    {
        return name.Split(":")[0];
    }
}
