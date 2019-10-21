namespace NuSMV
{
    /// <summary>
    /// Utility methods for translation of NuSMV
    /// </summary>
    public class TUtil
    {
        private static string dashes = " ----------------------- ";

        internal static void AddDashedComment(string text)
        {
            string op = dashes + text + dashes;
            Writer.WriteLine(op);
        }

        internal static string tabInnerCase(NuSMV.Case innerCase)
        {
            return innerCase.ToString().Replace("\n", "\n\t");
        }

        internal static string tabInnerCase(NuSMV.UnionCases unionCases)
        {
            return unionCases.ToString().Replace("\n", "\n\t");
        }
    }
}