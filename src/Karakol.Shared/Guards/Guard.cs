namespace Karakol.Shared.Guards;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} cannot be empty.", name);
        }

        return value;
    }

    public static int AtLeast(int value, int minimum, string name)
    {
        if (value < minimum)
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be at least {minimum}.");
        }

        return value;
    }
}
