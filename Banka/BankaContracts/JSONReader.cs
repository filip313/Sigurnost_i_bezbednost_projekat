using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace BankaContracts
{
    public class JSONReader
    {
        public static List<Korisnik> listaKorisnika;

        public static List<Korisnik> ProcitajKorisnike()
        {
            string path = "..\\..\\klijenti.json";

            using (StreamReader r = new StreamReader(path))
            {
                string file = r.ReadToEnd();
                listaKorisnika = JsonConvert.DeserializeObject<List<Korisnik>>(file);

                return listaKorisnika;
            }
        }

        public static Korisnik SacuvajKorisnika(Korisnik korisnik)
        {
            string fajl;
            List<Korisnik> korisnici;

            string path = "..\\..\\klijenti.json";

            using (StreamReader r = new StreamReader(path))
            {
                string file = r.ReadToEnd();
                korisnici = JsonConvert.DeserializeObject<List<Korisnik>>(file);
                foreach(Korisnik k in korisnici)
                {
                    if(k.KorisnickoIme == korisnik.KorisnickoIme)
                    {
                        korisnici.Remove(k);
                        break;
                    }
                }
                korisnici.Add(korisnik);
                fajl = JsonConvert.SerializeObject(korisnici);
            }

            File.WriteAllText(path, fajl);
            return korisnik;
        }

        public static List<string> ProcitajSerijskiBroj()
        {
            string path = "..\\..\\serijskiBrojevi.txt";
            List<string> retList = new List<string>();

            using (StreamReader file = new StreamReader(path))
            {
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    retList.Add(ln);
                }
                file.Close();
            }
            
            return retList;
        }

        public static void SacuvajSerijskiBroj(string sb)
        {
            string path = "..\\..\\serijskiBrojevi.txt";

            using (StreamWriter outputFile = new StreamWriter(path, true))
            {
                outputFile.WriteLine(sb);
            }
        }

    }
}
