using System.IO.Abstractions;
using System.Text;

namespace ConsoleApp
{
    /// <summary>
    /// The entry point for the LetterServices program.
    /// </summary>
    class TestProgram
    {
        static void Main(string[] args)
        {
            string date = "20220125"; // date of files to be combined

            LetterService ls = new(date);

            // Archive & store files from the admissions/scholarship files processed on the given date
            ls.ArchiveAndGetFiles();

            // Combine letters of the same student 
            ls.CombineStudentLetters();

            // Generate the text report of all the files combined 
            ls.GenerateTextReport();
        }
    }


    /// <summary>
    /// Represents a letter service for the printing and mailing department. Archives files and combines
    /// letters from the same student.
    /// </summary>
    public interface ILetterService
    {
        /// <summary>
        /// Moves all the files to another directory and creates a dictionary of the files from the given date.
        /// </summary>
        public void ArchiveAndGetFiles();

        /// <summary>
        /// Moves files from one directory to another. Used as a helper in ArchiveAndGetFiles.
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        public void MoveFilesFromDir(string dirFrom, string dirTo);

        /// <summary>
        /// Combines the admission and scholarship files if they are from the same student. 
        /// </summary>
        public void CombineStudentLetters();

        /// <summary>
        /// Combine two letter files into one file. Used as a helper in CombineTwoLetters.
        /// </summary>
        /// <param name="inputFile1">File path for the first letter. </param>
        /// <param name="inputFile2">File path for the second letter. </param>
        /// <param name="resultFile">File path for the combined letter. </param>
        void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile);

        /// <summary>
        /// Generates a text report for the letters combined. 
        /// </summary>
        void GenerateTextReport();

        /// <summary>
        ///  Returns the files from date. Used for testing.
        /// </summary>
        /// <returns>The dictionary containing only files processed on the specified date. </returns>
        Dictionary<string, string[]> GetFilesFromDate();
    }

    public class LetterService : ILetterService
    {
        private readonly IFileSystem fileSystem;
        private readonly string date;

        private string currentDirectory;
        private string projectDir;
        private string combinedLettersDir;
        private string dirInput;
        private string dirOutput;
        private string dirArchive;
        private Dictionary<string, string[]> filesFromDate = new Dictionary<string, string[]>();

        public LetterService(string date, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.date = date;
            currentDirectory = fileSystem.Directory.GetCurrentDirectory();

            if (currentDirectory != "C:\\")
            {
                // this is an easy way to get the directory, but not robust
                projectDir = fileSystem.Directory.GetParent(currentDirectory).Parent.Parent.FullName;
            }
            else
            {
                projectDir = currentDirectory;
            }

            combinedLettersDir = fileSystem.Path.Join(projectDir, "CombinedLetters");
            dirInput = fileSystem.Path.Join(combinedLettersDir, "Input");
            dirArchive = fileSystem.Path.Join(combinedLettersDir, "Archive");
            dirOutput = fileSystem.Path.Join(combinedLettersDir, "Output");
        }

        public LetterService(string date) : this(date, new FileSystem())
        {

        }

        /// <summary>
        /// Moves all the files to another directory and returns a dictionary of the files from the given date
        /// </summary>
        public void ArchiveAndGetFiles()
        {
            string admissionFolder = fileSystem.Path.Join(this.dirInput, "Admission");
            string dirToAdmission = fileSystem.Path.Join(this.dirArchive, "Admission");

            string scholarshipFolder = fileSystem.Path.Join(this.dirInput, "Scholarship");
            string dirToScholarship = fileSystem.Path.Join(this.dirArchive, "Scholarship");

            this.MoveFilesFromDir(admissionFolder, dirToAdmission);
            this.MoveFilesFromDir(scholarshipFolder, dirToScholarship);
            

            string[] filesFromAdmission = fileSystem.Directory.GetFiles(fileSystem.Path.Join(dirToAdmission, this.date));
            string[] filesFromScholarship = fileSystem.Directory.GetFiles(fileSystem.Path.Join(dirToScholarship, this.date));

            this.filesFromDate = new()
            {
                { "Admission", filesFromAdmission },
                { "Scholarship", filesFromScholarship }
            };
        }

        /// <summary>
        /// Moves files from one directory to another. Used as a helper in ArchiveAndGetFiles.
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        public void MoveFilesFromDir(string dirFrom, string dirTo)
        {
            fileSystem.Directory.CreateDirectory(dirTo);

            foreach (string path in fileSystem.Directory.GetDirectories(dirFrom))
            {
                string dirName = fileSystem.Path.GetFileName(path);
                string[] files = fileSystem.Directory.GetFiles(path);

                fileSystem.Directory.CreateDirectory(fileSystem.Path.Join(dirTo, dirName));

                foreach (string file in files)
                {
                    string dest = fileSystem.Path.Join(dirTo, dirName, fileSystem.Path.GetFileName(file));

                    fileSystem.File.Move(file, dest, true);
                }
                fileSystem.Directory.Delete(path);
            }
        }

        /// <summary>
        /// Combines the admission and scholarship files if they are from the same student. 
        /// </summary>
        public void CombineStudentLetters()
        {

            string[] admissionsFiles = this.filesFromDate["Admission"];
            string[] scholarshipFiles = this.filesFromDate["Scholarship"];
            fileSystem.Directory.CreateDirectory(fileSystem.Path.Join(this.dirOutput, this.date));

            foreach (string admission in admissionsFiles)
            {
                string idTxt = admission.Split("-")[1];
                foreach (string scholarship in scholarshipFiles)
                {
                    if (scholarship.Contains(idTxt))
                    {
                        string resultFile = fileSystem.Path.Join(this.dirOutput, this.date, "combined-" + idTxt);
                        this.CombineTwoLetters(admission, scholarship, resultFile);
                    }
                }
            }
        }

        /// <summary>
        /// Combine two letter files into one file. Used as a helper in CombineTwoLetters.
        /// </summary>
        /// <param name="inputFile1">File path for the first letter. </param>
        /// <param name="inputFile2">File path for the second letter. </param>
        /// <param name="resultFile">File path for the combined letter. </param>
        public void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile)
        {
            if (fileSystem.File.Exists(inputFile1) && fileSystem.File.Exists(inputFile2))
            {
                string file1 = fileSystem.File.ReadAllText(inputFile1);
                string file2 = fileSystem.File.ReadAllText(inputFile2);

                fileSystem.File.WriteAllText(resultFile, file1 + file2);
            }
        }

        /// <summary>
        /// Generates a text report for the letters combined. 
        /// </summary>
        public void GenerateTextReport()
        {
            string path = fileSystem.Path.Join(this.dirOutput, this.date, this.date + "-report.txt");
            string year = this.date.Substring(0, 4);
            string month = this.date.Substring(4, 2);
            string day = this.date.Substring(6, 2);

            // assume that report has not been generated yet, so only combined files are in directory
            string[] files = fileSystem.Directory.GetFiles(fileSystem.Path.Join(this.dirOutput, this.date));
            StringBuilder sb = new();
            sb.Append(month + "/" + day + "/" + year + " Report\n");
            sb.Append("-----------------------------\n\n");
            sb.Append("Number of Combined Letters: " + files.Length + "\n");

            foreach (string file in files)
            {
                string fileName = fileSystem.Path.GetFileName(file);
                string id = fileName.Split("-")[1].Split(".")[0];
                sb.Append("\t" + id + "\n");
            }

            fileSystem.File.AppendAllText(path, sb.ToString());
        }

        /// <summary>
        ///  Returns the files from date. Used for testing.
        /// </summary>
        /// <returns>The dictionary containing only files processed on the specified date. </returns>
        public Dictionary<string, string[]> GetFilesFromDate()
        {
            return filesFromDate;
        }
    }
}