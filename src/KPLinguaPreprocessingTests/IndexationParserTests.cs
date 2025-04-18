using KPLinguaPreprocessing;
using KPLinguaPreprocessing.Models;
using System.Text.RegularExpressions;

namespace KPLinguaPreprocessingTests
{
    [TestClass]
    public sealed class IndexationParserTests
    {
        private IndexationParser parser;

        [TestInitialize]
        public void Setup()
        {
            parser = new IndexationParser();
        }

        [TestMethod]
        public void ReadKpl_ShouldReadAllLines()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var expectedLines = new List<string> { "line 1", "line 2" };
            File.WriteAllLines(tempFile, expectedLines);

            // Act
            var lines = parser.ReadKpl(tempFile);

            // Assert
            CollectionAssert.AreEqual(expectedLines, lines);
        }

        [TestMethod]
        public void WriteKpl_ShouldWriteAllLines()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var lines = new List<string> { "first line", "second line" };

            // Act
            parser.WriteKpl(lines, tempFile);
            var readLines = File.ReadAllLines(tempFile).ToList();

            // Assert
            CollectionAssert.AreEqual(lines, readLines);
        }

        [TestMethod]
        public void GetKpl_ShouldWriteToTempFile_AndReturnPath()
        {
            // Arrange
            var lines = new List<string> { "abc", "123" };

            // Act
            var path = parser.GetKpl(lines);

            // Assert
            Assert.IsTrue(File.Exists(path));
            var writtenLines = File.ReadAllLines(path);
            CollectionAssert.AreEqual(lines, writtenLines);
        }

        [TestMethod]
        public void BuildRuleWithVariables_ShouldReturnCorrectReplacements()
        {
            // Arrange
            string input = "a -> $i+1$";
            var builder = new KplIteratorBuilder(new Dictionary<string, Variable>());
            var (expressions, replaced) = parser.BuildRuleWithVariables(input, builder);

            // Assert
            Assert.AreEqual(1, expressions.Count);
            Assert.AreEqual("a -> @0@", replaced);
        }

        [TestMethod]
        public void BuildRuleWithVariablesUsingBaseList_ShouldReturnCorrectData()
        {
            // Arrange
            string input = "a -> $2*i$";
            var builder = new KplIteratorBuilder(new Dictionary<string, Variable>());

            // Act
            var (expressions, replaced) = parser.BuildRuleWithVariablesUsingBaseList(input, builder);

            // Assert
            Assert.AreEqual(1, expressions.Count);
            Assert.AreEqual("a -> @0@", replaced);
        }

        [TestMethod]
        public void Execute_ShouldReturnModifiedFilePath()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, new[] { "a -> b : 0<=i<=2" });

            // Act
            var outputFile = parser.Execute(tempFile);

            // Assert
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod]
        public void Execute_ShouldExpandIndexationAndDefine()
        {
            string input = @"
#define bits=3
a_$i$ -> a_$i$ (Add) . :0<=i<=bits
termA {@a_$i$:0<=i<=bits@} (Term) .
";
            var tempInput = Path.GetTempFileName();
            File.WriteAllText(tempInput, input.Trim());

            string outputFile = parser.Execute(tempInput);
            string output = File.ReadAllText(outputFile).Trim();

            string expected = @"
a_0 -> a_0 (Add) .
a_1 -> a_1 (Add) .
a_2 -> a_2 (Add) .
a_3 -> a_3 (Add) .
termA {a_0,a_1,a_2,a_3} (Term) .
".Trim();
            Assert.AreEqual(Normalize(expected), Normalize(output));
        }

        [TestMethod]
        [DynamicData(nameof(GetIndexationTestCases), DynamicDataSourceType.Method)]
        public void Execute_ShouldExpandIndexationAndDefine(string input, string expected)
        {
            var tempInput = Path.GetTempFileName();
            File.WriteAllText(tempInput, input.Trim());

            string outputFile = parser.Execute(tempInput);
            string output = File.ReadAllText(outputFile).Trim();

            Assert.AreEqual(NormalizeLogicalGuards(expected), NormalizeLogicalGuards(output));
        }

        private string Normalize(string text)
        {
            return string.Join("\n", text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.TrimEnd()))
                .Trim();
        }

        private static string NormalizeLogicalGuards(string text)
        {
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static IEnumerable<object[]> GetIndexationTestCases()
        {
            yield return new object[]
            {
                @"@&,(=x$i$ & =y$i$) : 0<=i<=2@ : a -> b .",
                @"(=x0 & =y0) & (=x1 & =y1) & (=x2 & =y2) : a -> b ."
            };

            yield return new object[]
            {
                @"@|,(=p$i$ & =q$i$) : 0<=i<=3@ : r -> s .",
                @"(=p0 & =q0) | (=p1 & =q1) | (=p2 & =q2) | (=p3 & =q3) : r -> s ."
            };

            yield return new object[]
            {
                @"@&,(=a$i$ & =b$i$) : 0<=i<=1@ | =c : d$j$ -> e$j$ : 2<=j<=4 .",
                @"(=a0 & =b0) & (=a1 & =b1) | =c : d2 -> e2 .
(=a0 & =b0) & (=a1 & =b1) | =c : d3 -> e3 .
(=a0 & =b0) & (=a1 & =b1) | =c : d4 -> e4 ."
            };

            yield return new object[]
            {
                @"@&,(=x$i$) & >y : 0<=i<=2@ : a->b.",
                @"(=x0) & >y & (=x1) & >y & (=x2) & >y : a->b."
            };

            yield return new object[]
            {
                @"@&,(=x$i$) & >y & <z$i+1$ | =a$i+2$ : 0<=i<=2@ : a->b.",
                @"(=x0) & >y & <z1 | =a2 & (=x1) & >y & <z2 | =a3 & (=x2) & >y & <z3 | =a4 : a->b."
            };
        }
    }
}
