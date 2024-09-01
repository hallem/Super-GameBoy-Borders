using System.Text;

namespace Super_GameBoy_Borders;

public class ConsoleFileOutput : TextWriter
{
    private readonly StreamWriter writer;
    private readonly TextWriter console;

    public ConsoleFileOutput()
    {
        this.console = Console.Out;
        this.writer = new StreamWriter("Output.txt", false, Encoding.Default);
        this.writer.AutoFlush = true;
    }

    public override void WriteLine(string? value)
    {
        Console.SetOut(console);
        Console.WriteLine(value);
        this.writer.WriteLine(value);
        Console.SetOut(this);
    }

    public override Encoding Encoding => Encoding.Default;

    public override void Flush()
    {
        this.writer.Flush();
    }

    public override void Close()
    {
        this.writer.Close();
    }

    public new void Dispose()
    {
        this.writer.Flush();
        this.writer.Close();
        this.writer.Dispose();
        base.Dispose();
    }
}
