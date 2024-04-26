using System;
using System.IO;
using System.Collections;


class TestProgram
{
    static void Main(string[] args)
    {
        string dirFrom = "CombinedLetters/Input";
        string dirTo = "CombinedLetters/Archive";
        string date = "20220125";

        ILetterService ls = new LetterService();

        ArrayList filesFromDate = ls.ArchiveAndGetFiles(dirFrom, dirTo, date);
        foreach (string file in filesFromDate)
        {
            Console.WriteLine(file);
        }
    }
}



public interface ILetterService
{

    ArrayList ArchiveAndGetFiles(string dirFrom, string dirTo, string date);

    /// <summary>
    /// Combine two letter files into one file.
    /// </summary>
    /// <param name="inputFile1">File path for the first letter. </param>
    /// <param name="inputFile2">File path for the second letter. </param>
    /// <param name="resultFile">File path for the combined letter. </param>
    void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile);

}

public class LetterService : ILetterService
{
    public ArrayList ArchiveAndGetFiles(string dirFrom, string dirTo, string date)
    {
        ArrayList filesFromDate = new ArrayList();

            var admissionScholarshipDirs = Directory.EnumerateDirectories(dirFrom);

            foreach (string currentDir in admissionScholarshipDirs)
            {
                var dateDirs = Directory.EnumerateDirectories(currentDir);

                foreach (string currDateDir in dateDirs)
                {

                    Directory.Move(currDateDir, dirTo);

                    if (string.Equals(currDateDir, date))
                    {
                        filesFromDate.Add(Directory.GetFiles(currentDir));
                    }
                }
            }

        return filesFromDate;
    }

    public void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile)
    {
        if (File.Exists(inputFile1) && File.Exists(inputFile2))
        {
            string file1 = File.ReadAllText(inputFile1);
            string file2 = File.ReadAllText(inputFile2);

            File.WriteAllText(resultFile, file1 + file2);
        }
    }
}