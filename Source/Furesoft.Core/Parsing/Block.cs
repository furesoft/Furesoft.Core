namespace Backlang.Codeanalysis.Parsing.AST;

public class Block : SyntaxNode
{
    public Block(List<SyntaxNode> body)
    {
        Body = body;
    }

    public Block()
    {
        Body = new List<SyntaxNode>();
    }

    public List<SyntaxNode> Body { get; set; }

    public override string ToString()
    {
        return string.Join("\n", Body);
    }
}