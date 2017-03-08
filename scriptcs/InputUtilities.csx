public static class InputUtilities {
    public static int GetUserInputInteger(string prompt) {
        int result = -1;
        Console.WriteLine(prompt);
        var input = Console.ReadLine();
        if (!int.TryParse(input, out result))
            throw new ArgumentException(input);
        return result;
    }
}