using DigitalRune.Windows.TextEditor;
using DigitalRune.Windows.TextEditor.Document;
using DigitalRune.Windows.TextEditor.Insight;
using Furesoft.Core.ExpressionEvaluator;
using Furesoft.Core.ExpressionEvaluator.Macros;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Nardole;

internal class MethodInsightDataProvider : AbstractInsightDataProvider
{
    private readonly ExpressionParser parser;
    private int _argumentStartOffset;   // The offset where the method arguments starts.
    private string[] _insightText;      // The insight information.

    public MethodInsightDataProvider(ExpressionParser parser)
    {
        this.parser = parser;
    }

    public override int InsightDataCount
    {
        get { return _insightText != null ? _insightText.Length : 0; }
    }

    protected override int ArgumentStartOffset
    {
        get { return _argumentStartOffset; }
    }

    public override string GetInsightData(int number)
    {
        return _insightText != null ? _insightText[number] : string.Empty;
    }

    public override void SetupDataProvider(string fileName)
    {
        // This class provides the method insight information.
        // To find out which information is requested, it simply compares the
        // word before the caret with 3 hardcoded method names.

        int offset = TextArea.Caret.Offset;
        IDocument document = TextArea.Document;
        string methodName = TextHelper.GetIdentifierAt(document, offset - 1);

        if (parser.RootScope.Macros.ContainsKey(methodName))
        {
            SetupDataProviderForMethod(methodName, offset);
        }
        else if (parser.RootScope.ImportedFunctions.ContainsKey(methodName))
        {
            SetupDataProviderForMethod(methodName, offset);
        }
        else if (parser.RootScope.Functions.ContainsKey(methodName))
        {
            SetupDataProviderForMethod(methodName, offset);
        }
        else
        {
            // Perhaps the cursor is already inside the parameter list.
            offset = TextHelper.FindOpeningBracket(document, offset - 1, '(', ')');
            if (offset >= 1)
            {
                methodName = TextHelper.GetIdentifierAt(document, offset - 1);
                SetupDataProviderForMethod(methodName, offset);
            }
        }
    }

    private void SetupDataProviderForMethod(string methodName, int argumentStartOffset)
    {
        _insightText = new string[1];

        string description = string.Empty;
        string arguments = string.Empty;
        string paramDescriptions = string.Empty;

        if (parser.RootScope.Macros.ContainsKey(methodName))
        {
            var m = parser.RootScope.Macros[methodName];
            var attributes = TypeDescriptor.GetAttributes(m.GetType());
            description = attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;

            if (m is ReflectionMacro rm)
            {
            }
            else
            {
                arguments = string.Join(", ", m.GetType().GetCustomAttributes<ParameterDescriptionAttribute>().Select(_ => _.ParameterName));
                paramDescriptions = string.Join("\n", m.GetType().GetCustomAttributes<ParameterDescriptionAttribute>().Select(_ => _.ParameterName + ": " + _.Description));
            }
        }

        _insightText[0] = $"{methodName}({arguments})\n\n"
                         + description + "\n\n" +
                         paramDescriptions;

        _argumentStartOffset = argumentStartOffset;
    }
}
