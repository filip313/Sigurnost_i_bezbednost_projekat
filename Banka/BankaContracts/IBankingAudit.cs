using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankaContracts
{
    [ServiceContract]
    public interface IBankingAudit
    {
        [OperationContract]
        void PrijemPodataka(string banka, string racun, DateTime vremeDetekcije, List<string> logovi);
    }
}
