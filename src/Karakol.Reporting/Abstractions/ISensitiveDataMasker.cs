namespace Karakol.Reporting.Abstractions;

public interface ISensitiveDataMasker
{
    string Mask(string input);
}
