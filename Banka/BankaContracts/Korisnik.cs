using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BankaContracts
{
    [DataContract]
    public class Korisnik
    {
        [DataMember]
        public string KorisnickoIme { get; set; }
        [DataMember]
        public string Pin { get; set; }
        [DataMember]
        public double Stanje { get; set; }

        public Korisnik()
        {

        }

        public Korisnik(string username, string pinkod)
        {
            this.KorisnickoIme = username;
            this.Pin = pinkod;
            this.Stanje = 0;
        }
    }
}
