using ConsoleApp;
using System.IO.Abstractions.TestingHelpers;

namespace TestLetterService
{

    [TestClass]

    public class Test_LetterService
    {

        readonly string dirInput = @"c:\CombinedLetters\Input";
        readonly string dirArchive = @"c:\CombinedLetters\Archive";
        readonly string dirOutput = @"c:\CombinedLetters\Output";

        [TestMethod]
        public void TestArchiveAndGetFiles()
        {
            // Arrange
            MockFileSystem fs = new(new Dictionary<string, MockFileData>
            {
                { @"c:\CombinedLetters", new MockDirectoryData() },
                { @"c:\CombinedLetters\Archive", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-11111111.txt", new MockFileData("admission-11111111.txt") },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-88888888.txt", new MockFileData("admission-88888888.txt") },
                { @"c:\CombinedLetters\Input\Admission\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20240426\admission-22222222.txt", new MockFileData("admission-22222222.txt") },
                { @"c:\CombinedLetters\Input\Scholarship", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-11111111.txt", new MockFileData("scholarship-11111111.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-88888888.txt", new MockFileData("scholarship-88888888.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20240426\scholarship-22222222.txt", new MockFileData("scholarship-22222222.txt") },
                { @"c:\CombinedLetters\Output", new MockDirectoryData() },

            });

            // Act
            LetterService ls = new("20220125", fs);
            ls.ArchiveAndGetFiles();

            // Assert
            string[] dirsInInput = fs.Directory.GetDirectories(dirInput);
            Array.Sort(dirsInInput);

            CollectionAssert.AreEqual(dirsInInput,
                new string[2] { fs.Path.Join(dirInput, "Admission"), fs.Path.Join(dirInput, "Scholarship") });
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Admission")), 
                Array.Empty<string>());
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Scholarship")), 
                Array.Empty<string>());

            string[] dirsInArchive = fs.Directory.GetDirectories(dirArchive);
            Array.Sort(dirsInArchive);
            CollectionAssert.AreEqual(dirsInArchive,
                new string[2] { fs.Path.Join(dirArchive, "Admission"), fs.Path.Join(dirArchive, "Scholarship") });

            string[] dirsInArchiveAdmission = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Admission"));
            Array.Sort(dirsInArchiveAdmission);
            CollectionAssert.AreEqual(dirsInArchiveAdmission,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125"),
                fs.Path.Join(dirArchive, "Admission", "20240426") });

            string[] dirsInArchiveScholarship = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Scholarship"));
            Array.Sort(dirsInArchiveScholarship);
            CollectionAssert.AreEqual(dirsInArchiveScholarship,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125"),
                fs.Path.Join(dirArchive, "Scholarship", "20240426") });

            string[] filesInArchiveAdmissionTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20220125"));
            Array.Sort(filesInArchiveAdmissionTargetDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });

            string[] filesInArchiveAdmissionWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20240426"));
            Array.Sort(filesInArchiveAdmissionWrongDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionWrongDate,
            new string[1] { fs.Path.Join(dirArchive, "Admission", "20240426", "admission-22222222.txt") });

            string[] filesInArchiveScholarshipTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20220125"));
            Array.Sort(filesInArchiveScholarshipTargetDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });

            string[] filesInArchiveScholarshipWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20240426"));
            Array.Sort(filesInArchiveScholarshipWrongDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipWrongDate,
                new string[1] { fs.Path.Join(dirArchive, "Scholarship", "20240426", "scholarship-22222222.txt") });

            CollectionAssert.AreEqual(fs.Directory.GetDirectories(dirOutput), Array.Empty<string>());

            Dictionary<string, string[]> fileHashmap = ls.GetFilesFromDate();
            Array.Sort(fileHashmap["Admission"]);
            Array.Sort(fileHashmap["Scholarship"]);

            CollectionAssert.AreEqual(fileHashmap["Admission"], new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });
            CollectionAssert.AreEqual(fileHashmap["Scholarship"], new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });
        }

        [TestMethod]
        public void TestCombineStudentLetters()
        {
            // Arrange
            MockFileSystem fs = new(new Dictionary<string, MockFileData>
            {
                { @"c:\CombinedLetters", new MockDirectoryData() },
                { @"c:\CombinedLetters\Archive", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-11111111.txt", new MockFileData("admission-11111111.txt") },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-88888888.txt", new MockFileData("admission-88888888.txt") },
                { @"c:\CombinedLetters\Input\Admission\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20240426\admission-22222222.txt", new MockFileData("admission-22222222.txt") },
                { @"c:\CombinedLetters\Input\Scholarship", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-11111111.txt", new MockFileData("scholarship-11111111.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-88888888.txt", new MockFileData("scholarship-88888888.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20240426\scholarship-22222222.txt", new MockFileData("scholarship-22222222.txt") },
                { @"c:\CombinedLetters\Output", new MockDirectoryData() },

            });

            LetterService ls = new("20220125", fs);
            ls.ArchiveAndGetFiles();

            // Act
            ls.CombineStudentLetters();

            // Assert
            string[] dirsInInput = fs.Directory.GetDirectories(dirInput);
            Array.Sort(dirsInInput);

            CollectionAssert.AreEqual(dirsInInput,
                new string[2] { fs.Path.Join(dirInput, "Admission"), fs.Path.Join(dirInput, "Scholarship") });
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Admission")),
                Array.Empty<string>());
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Scholarship")),
                Array.Empty<string>());

            string[] dirsInArchive = fs.Directory.GetDirectories(dirArchive);
            Array.Sort(dirsInArchive);
            CollectionAssert.AreEqual(dirsInArchive,
                new string[2] { fs.Path.Join(dirArchive, "Admission"), fs.Path.Join(dirArchive, "Scholarship") });

            string[] dirsInArchiveAdmission = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Admission"));
            Array.Sort(dirsInArchiveAdmission);
            CollectionAssert.AreEqual(dirsInArchiveAdmission,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125"),
                fs.Path.Join(dirArchive, "Admission", "20240426") });

            string[] dirsInArchiveScholarship = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Scholarship"));
            Array.Sort(dirsInArchiveScholarship);
            CollectionAssert.AreEqual(dirsInArchiveScholarship,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125"),
                fs.Path.Join(dirArchive, "Scholarship", "20240426") });

            string[] filesInArchiveAdmissionTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20220125"));
            Array.Sort(filesInArchiveAdmissionTargetDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });

            string[] filesInArchiveAdmissionWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20240426"));
            Array.Sort(filesInArchiveAdmissionWrongDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionWrongDate,
            new string[1] { fs.Path.Join(dirArchive, "Admission", "20240426", "admission-22222222.txt") });

            string[] filesInArchiveScholarshipTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20220125"));
            Array.Sort(filesInArchiveScholarshipTargetDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });

            string[] filesInArchiveScholarshipWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20240426"));
            Array.Sort(filesInArchiveScholarshipWrongDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipWrongDate,
                new string[1] { fs.Path.Join(dirArchive, "Scholarship", "20240426", "scholarship-22222222.txt") });

            Dictionary<string, string[]> fileHashmap = ls.GetFilesFromDate();
            Array.Sort(fileHashmap["Admission"]);
            Array.Sort(fileHashmap["Scholarship"]);

            CollectionAssert.AreEqual(fileHashmap["Admission"], new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });
            CollectionAssert.AreEqual(fileHashmap["Scholarship"], new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });

            CollectionAssert.AreEqual(fs.Directory.GetDirectories(dirOutput),
                new string[1] { fs.Path.Join(dirOutput, "20220125") });

            string[] filesInOutput = fs.Directory.GetFiles(fs.Path.Join(dirOutput, "20220125"));
            Array.Sort(filesInOutput);
            CollectionAssert.AreEqual(filesInOutput, new string[2] { fs.Path.Join(dirOutput, "20220125", "combined-11111111.txt"),
            fs.Path.Join(dirOutput, "20220125", "combined-88888888.txt")});

            Assert.AreEqual(fs.File.ReadAllText(fs.Path.Join(dirOutput, "20220125", "combined-11111111.txt")),
                "admission-11111111.txtscholarship-11111111.txt");
            Assert.AreEqual(fs.File.ReadAllText(fs.Path.Join(dirOutput, "20220125", "combined-88888888.txt")),
                "admission-88888888.txtscholarship-88888888.txt");
        }

        [TestMethod]
        public void TestGenerateTextReport()
        {
            // Arrange
            MockFileSystem fs = new(new Dictionary<string, MockFileData>
            {
                { @"c:\CombinedLetters", new MockDirectoryData() },
                { @"c:\CombinedLetters\Archive", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-11111111.txt", new MockFileData("admission-11111111.txt") },
                { @"c:\CombinedLetters\Input\Admission\20220125\admission-88888888.txt", new MockFileData("admission-88888888.txt") },
                { @"c:\CombinedLetters\Input\Admission\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Admission\20240426\admission-22222222.txt", new MockFileData("admission-22222222.txt") },
                { @"c:\CombinedLetters\Input\Scholarship", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-11111111.txt", new MockFileData("scholarship-11111111.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20220125\scholarship-88888888.txt", new MockFileData("scholarship-88888888.txt") },
                { @"c:\CombinedLetters\Input\Scholarship\20240426", new MockDirectoryData() },
                { @"c:\CombinedLetters\Input\Scholarship\20240426\scholarship-22222222.txt", new MockFileData("scholarship-22222222.txt") },
                { @"c:\CombinedLetters\Output", new MockDirectoryData() },

            });

            LetterService ls = new("20220125", fs);
            ls.ArchiveAndGetFiles();
            ls.CombineStudentLetters();

            // Act
            ls.GenerateTextReport();

            // Assert
            string[] dirsInInput = fs.Directory.GetDirectories(dirInput);
            Array.Sort(dirsInInput);

            CollectionAssert.AreEqual(dirsInInput,
                new string[2] { fs.Path.Join(dirInput, "Admission"), fs.Path.Join(dirInput, "Scholarship") });
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Admission")),
                Array.Empty<string>());
            CollectionAssert.AreEqual(fs.Directory.GetDirectories(fs.Path.Join(dirInput, "Scholarship")),
                Array.Empty<string>());

            string[] dirsInArchive = fs.Directory.GetDirectories(dirArchive);
            Array.Sort(dirsInArchive);
            CollectionAssert.AreEqual(dirsInArchive,
                new string[2] { fs.Path.Join(dirArchive, "Admission"), fs.Path.Join(dirArchive, "Scholarship") });

            string[] dirsInArchiveAdmission = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Admission"));
            Array.Sort(dirsInArchiveAdmission);
            CollectionAssert.AreEqual(dirsInArchiveAdmission,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125"),
                fs.Path.Join(dirArchive, "Admission", "20240426") });

            string[] dirsInArchiveScholarship = fs.Directory.GetDirectories(fs.Path.Join(dirArchive, "Scholarship"));
            Array.Sort(dirsInArchiveScholarship);
            CollectionAssert.AreEqual(dirsInArchiveScholarship,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125"),
                fs.Path.Join(dirArchive, "Scholarship", "20240426") });

            string[] filesInArchiveAdmissionTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20220125"));
            Array.Sort(filesInArchiveAdmissionTargetDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });

            string[] filesInArchiveAdmissionWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Admission", "20240426"));
            Array.Sort(filesInArchiveAdmissionWrongDate);
            CollectionAssert.AreEqual(filesInArchiveAdmissionWrongDate,
            new string[1] { fs.Path.Join(dirArchive, "Admission", "20240426", "admission-22222222.txt") });

            string[] filesInArchiveScholarshipTargetDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20220125"));
            Array.Sort(filesInArchiveScholarshipTargetDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipTargetDate,
                new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });

            string[] filesInArchiveScholarshipWrongDate = fs.Directory.GetFiles(fs.Path.Join(dirArchive, "Scholarship", "20240426"));
            Array.Sort(filesInArchiveScholarshipWrongDate);
            CollectionAssert.AreEqual(filesInArchiveScholarshipWrongDate,
                new string[1] { fs.Path.Join(dirArchive, "Scholarship", "20240426", "scholarship-22222222.txt") });

            Dictionary<string, string[]> fileHashmap = ls.GetFilesFromDate();
            Array.Sort(fileHashmap["Admission"]);
            Array.Sort(fileHashmap["Scholarship"]);

            CollectionAssert.AreEqual(fileHashmap["Admission"], new string[2] { fs.Path.Join(dirArchive, "Admission", "20220125", "admission-11111111.txt"),
                fs.Path.Join(dirArchive, "Admission", "20220125", "admission-88888888.txt") });
            CollectionAssert.AreEqual(fileHashmap["Scholarship"], new string[2] { fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-11111111.txt"),
                fs.Path.Join(dirArchive, "Scholarship", "20220125", "scholarship-88888888.txt") });

            CollectionAssert.AreEqual(fs.Directory.GetDirectories(dirOutput),
                new string[1] { fs.Path.Join(dirOutput, "20220125") });

            string[] filesInOutput = fs.Directory.GetFiles(fs.Path.Join(dirOutput, "20220125"));
            Array.Sort(filesInOutput);
            CollectionAssert.AreEqual(filesInOutput, new string[3] { fs.Path.Join(dirOutput, "20220125", "20220125-report.txt"),
            fs.Path.Join(dirOutput, "20220125", "combined-11111111.txt"),
            fs.Path.Join(dirOutput, "20220125", "combined-88888888.txt")});

            Assert.AreEqual(fs.File.ReadAllText(fs.Path.Join(dirOutput, "20220125", "combined-11111111.txt")),
                "admission-11111111.txtscholarship-11111111.txt");
            Assert.AreEqual(fs.File.ReadAllText(fs.Path.Join(dirOutput, "20220125", "combined-88888888.txt")),
                "admission-88888888.txtscholarship-88888888.txt");
            Assert.AreEqual(fs.File.ReadAllText(fs.Path.Join(dirOutput, "20220125", "20220125-report.txt")),
                "01/25/2022 Report\n" +
                "-----------------------------\n\n" +
                "Number of Combined Letters: 2\n" +
                "\t11111111\n" +
                "\t88888888\n");

        }
    }
}