using System.Text;

namespace Karakol.Parsing.Input;

public sealed record ReadOptions(int? MaxLines = null, Encoding? Encoding = null, int BufferSize = 81920);
