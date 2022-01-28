using BankaContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankingAudit
{
    public class BankingAuditServis : IBankingAudit
    {
        public void PrijemPodataka(string banka, string racun, DateTime vremeDetekcije, List<string> logovi)
        {
            string resultString;

            string fileInput = "Source name: " + banka + "\n";
            fileInput += "Naziv racuna: " + racun + "\n";
            fileInput += "Vreme detekcije: " + vremeDetekcije.ToString() + "\n";

            foreach (string s in logovi)
            {
                resultString = Regex.Match(s, @"\d+").Value;
                
                fileInput += "Iznos: " + resultString + "\n";
                fileInput += "--------------------------------------------\n";
            }

            Console.WriteLine(fileInput);

            string path = "..\\..\\bankingAudit.txt";

            using (StreamWriter outputFile = new StreamWriter(path, true))
            {
                outputFile.WriteLine(fileInput);
            }
        }
    }
}
