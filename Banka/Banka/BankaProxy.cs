using BankaContracts;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Banka
{
    public class BankaProxy : ChannelFactory<IReplikatorServis>, IReplikatorServis, IDisposable
    {
        IReplikatorServis factory;

        public BankaProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
            this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new ClientCertValidator();
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

            factory = this.CreateChannel();
        }

        public void SacuvajKljuc(string kljuc, string korisnickoIme)
        {
            factory.SacuvajKljuc(kljuc, korisnickoIme);
        }

        public void SacuvajPodatke(Korisnik k)
        {
            factory.SacuvajPodatke(k);
        }
    }
}
