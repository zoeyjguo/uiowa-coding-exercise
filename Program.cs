namespace ConsoleApp
{
    /// <summary>
    /// The entry point for the LetterServices program.
    /// </summary>
    class TestProgram
    {
        static void Main(string[] args)
        {
            /*
             * Below are fields that can be edited
             */
            string currentDirectory = Directory.GetCurrentDirectory();
            string projDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;

            // If project directory cannot be found, return error
            if (string.IsNullOrEmpty(projDirectory))
            {
                throw new ArgumentNullException("Project directory is empty or null");
            }

            string dirInput = Path.Join(projDirectory, "CombinedLetters", "Input"); // dir to move files from
            string dirArchive = Path.Join(projDirectory, "CombinedLetters", "Archive"); // dir to move files to
            string dirOutput = Path.Join(projDirectory, "CombinedLetters", "Output");
            string date = "20220125"; // date of files to be combined

            LetterService ls = new();

            // Archive & get the files from the admissions/scholarship files processed on the given date
            Dictionary<string, string[]> filesFromDate = ls.ArchiveAndGetFiles(dirInput, dirArchive, date);

            // Combine letters of the same student 
            ls.CombineStudentLetters(filesFromDate, dirOutput, date);

            // Generate the text report of all the files combined 
            ls.GenerateTextReport(dirOutput, date);
        }
    }


    /// <summary>
    /// Represents a letter service for the printing and mailing department. Archives files and combines
    /// letters from the same student.
    /// </summary>
    public interface ILetterService
    {
        /// <summary>
        /// Moves all the files to another directory and returns a dictionary of the files from the given date
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        /// <param name="date">The date of the files we are looking for. </param>
        /// <returns>A dictionary separating the admission and scholarship files processed on the given date. </returns>
        public Dictionary<string, string[]> ArchiveAndGetFiles(string dirFrom, string dirTo, string date);

        /// <summary>
        /// Moves files from one directory to another. Used as a helper in ArchiveAndGetFiles.
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        public void MoveFilesFromDir(string dirFrom, string dirTo);

        /// <summary>
        /// Combines the admission and scholarship files. 
        /// </summary>
        /// <param name="files">A dictionary of the admission and scholarship files to combine. </param>
        /// <param name="outputDir">The directory the combined files are being outputted to. </param>
        /// <param name="date">The date the files were processed. </param>
        public void CombineStudentLetters(Dictionary<string, string[]> files, string outputDir, string date);

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
        /// <param name="dirToReport">The directory containing the combined files. </param>
        /// <param name="date">The date the combined files were processed. </param>
        void GenerateTextReport(string dirToReport, string date);
    }

    public class LetterService : ILetterService
    {
        /// <summary>
        /// Moves all the files to another directory and returns a dictionary of the files from the given date
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        /// <param name="date">The date of the files we are looking for. </param>
        /// <returns>A dictionary separating the admission and scholarship files processed on the given date. </returns>
        public Dictionary<string, string[]> ArchiveAndGetFiles(string dirFrom, string dirTo, string date)
        {
            string admissionFolder = Path.Join(dirFrom, "Admission");
            string dirToAdmission = Path.Join(dirTo, "Admission");

            string scholarshipFolder = Path.Join(dirFrom, "Scholarship");
            string dirToScholarship = Path.Join(dirTo, "Scholarship");

            this.MoveFilesFromDir(admissionFolder, dirToAdmission);
            this.MoveFilesFromDir(scholarshipFolder, dirToScholarship);

            string[] filesFromAdmission = Directory.GetFiles(Path.Join(dirToAdmission, date));
            string[] filesFromScholarship = Directory.GetFiles(Path.Join(dirToScholarship, date));

            Dictionary<string, string[]> filesFromDate = new Dictionary<string, string[]>();
            filesFromDate.Add("Admission", filesFromAdmission);
            filesFromDate.Add("Scholarship", filesFromScholarship);

            // return the list of files from the given date
            return filesFromDate;
        }

        /// <summary>
        /// Moves files from one directory to another. Used as a helper in ArchiveAndGetFiles.
        /// </summary>
        /// <param name="dirFrom">Directory containing the files to be moved. </param>
        /// <param name="dirTo">Directory the files are being moved to. </param>
        public void MoveFilesFromDir(string dirFrom, string dirTo)
        {
            Directory.CreateDirectory(dirTo);

            foreach (string path in Directory.GetDirectories(dirFrom))
            {
                string dirName = new DirectoryInfo(path).Name;
                string[] files = Directory.GetFiles(path);

                Directory.CreateDirectory(Path.Join(dirTo, dirName));

                foreach (string file in files)
                {
                    string dest = Path.Join(dirTo, dirName, Path.GetFileName(file));

                    // update the file if it already exists, so we assume none of the files can have the same name
                    if (File.Exists(dest))
                    {
                        File.Delete(dest);
                    }

                    File.Move(file, dest);
                }
                Directory.Delete(path);
            }
        }

        /// <summary>
        /// Combines the admission and scholarship files. 
        /// </summary>
        /// <param name="files">A dictionary of the admission and scholarship files to combine. </param>
        /// <param name="outputDir">The directory the combined files are being outputted to. </param>
        /// <param name="date">The date the files were processed. </param>
        public void CombineStudentLetters(Dictionary<string, string[]> files, string outputDir, string date)
        {

            string[] admissionsFiles = files["Admission"];
            string[] scholarshipFiles = files["Scholarship"];
            Directory.CreateDirectory(Path.Join(outputDir, date));

            foreach (string admission in admissionsFiles)
            {
                string idTxt = admission.Split("-")[1];
                foreach (string scholarship in scholarshipFiles)
                {
                    if (scholarship.Contains(idTxt))
                    {
                        string resultFile = Path.Join(outputDir, date, "combined-" + idTxt);
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
            if (File.Exists(inputFile1) && File.Exists(inputFile2))
            {
                string file1 = File.ReadAllText(inputFile1);
                string file2 = File.ReadAllText(inputFile2);

                File.WriteAllText(resultFile, file1 + file2);
            }
        }

        /// <summary>
        /// Generates a text report for the letters combined. 
        /// </summary>
        /// <param name="dirToReport">The directory containing the combined files. </param>
        /// <param name="date">The date the combined files were processed. </param>
        public void GenerateTextReport(string dirToReport, string date)
        {
            string path = Path.Join(dirToReport, date, date + "-report.txt");
            string year = date.Substring(0, 4);
            string month = date.Substring(4, 2);
            string day = date.Substring(6, 2);

            // assume that report has not been generated yet, so only combined files are in directory
            string[] files = Directory.GetFiles(Path.Join(dirToReport, date));

            using (StreamWriter writer = new(path))
            {
                writer.WriteLine(month + "/" + day + "/" + year + " Report");
                writer.WriteLine("-----------------------------\n");
                writer.WriteLine("Number of Combined Letters: " + files.Length);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string id = fileName.Split("-")[1].Split(".")[0];
                    writer.WriteLine("\t" + id);
                }

            }
        }
    }
}