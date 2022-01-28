using BankaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replikator
{
    public class ReplikatorServis : IReplikatorServis
    {
        public void SacuvajKljuc(string kljuc, string korisnickoIme)
        {
            Manager.SecretKey.StoreKey(kljuc, korisnickoIme);
        }

        public void SacuvajPodatke(Korisnik k)
        {
            JSONReader.SacuvajKorisnika(k);
            Console.WriteLine("[" + DateTime.Now.ToString() + "] Korisnik uspesno sacuvan!");
            Console.WriteLine("\t Korisnicko ime: " + k.KorisnickoIme);
            Console.WriteLine("\t Stanje: " + k.Stanje);
            Console.WriteLine("\t PIN: " + k.Pin);
        }
    }
}
