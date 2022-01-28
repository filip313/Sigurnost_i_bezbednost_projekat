using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankaContracts
{
    [ServiceContract]
    public interface IReplikatorServis
    {
        [OperationContract]
        void SacuvajPodatke(Korisnik k);
        [OperationContract]
        void SacuvajKljuc(string kljuc, string korisnickoIme);
    }
}
