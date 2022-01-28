using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankaContracts
{
    [ServiceContract]
    public interface IBankaServis
    {
        [OperationContract]
        bool ProveraRegistracije();
        [OperationContract]
        string Registracija();
        [OperationContract]
        byte[] Uplata(byte[] kodiranaPoruka);
        [OperationContract]
        byte[] Isplata(byte[] kodiranaPoruka);
        [OperationContract]
        byte[] PromenaPinKoda(byte[] kodiranaPoruka);
        [OperationContract]
        string ObnavljanjeSertifikata(string pinKod);

    }
}
