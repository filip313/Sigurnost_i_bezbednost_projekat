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

namespace Klijent
{
    public class ClientProxy : ChannelFactory<IBankaServis>, IBankaServis, IDisposable
    {
        IBankaServis factory;

        public ClientProxy(NetTcpBinding binding, EndpointAddress address, bool wcf) : base(binding, address)
        {
            if(!wcf)
            {
                string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

                this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
                this.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new ClientCertValidator();
                this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
                this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
            }

            factory = this.CreateChannel();
        }


        public string Registracija()
        {
            string p = factory.Registracija();

            if (p == null)
            {
                Console.WriteLine("Doslo je do greske prilikom registracije. Pokusajte ponovo kasnije!");
            }
            else
            {
                Console.WriteLine("Instaliranje sertifikata. Molimo vas sacekajte. . .");
                Console.ReadLine();
                string cltCertCN = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);
                X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
                string dekriptovano = Manager.RSA.Decrypt(p, cert.GetRSAPrivateKey().ToXmlString(true));

                string pin = dekriptovano.Substring(dekriptovano.Length - 4, 4);
                string secretKey = dekriptovano.Substring(0, dekriptovano.Length - 4);
                SecretKey.StoreKey(secretKey, cltCertCN);

                Console.WriteLine("Uspesna registracija! Vas PIN kod je: " + pin);

                return pin;
            }

            return p;
        }

        public byte[] Isplata(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            byte[] odgovor = null;

            try
            {
                odgovor = factory.Isplata(kodiranaPoruka);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sertifikat nije validan! Neuspesna isplata!");
                return null;
            }

            byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(odgovor, secretKey);

            byte[] potpis = new byte[256];
            byte[] obicnaPoruka = new byte[decoded.Length - 256];

            Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
            Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length - 256);
            string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, "banka_sign");

            if (DigitalSignature.Verify(obicnaPorukaString, potpis, certBanka))
            {
                Console.WriteLine(obicnaPorukaString);
                return obicnaPoruka;
            }
            else
            {
                Console.WriteLine("Poruka nije uspesno primljena!");
                return null;
            }
        }

        public byte[] PromenaPinKoda(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            byte[] odgovor = null;

            try
            {
                odgovor = factory.PromenaPinKoda(kodiranaPoruka);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sertifikat nije validan! Neuspesna promena PIN koda!");
                return null;
            }

            byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(odgovor, secretKey);

            byte[] potpis = new byte[256];
            byte[] obicnaPoruka = new byte[decoded.Length - 256];

            Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
            Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length - 256);
            string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, "banka_sign");

            if (DigitalSignature.Verify(obicnaPorukaString, potpis, certBanka))
            {
                Console.WriteLine(obicnaPorukaString);
                return obicnaPoruka;
            }
            else
            {
                Console.WriteLine("Poruka nije uspesno primljena!");
                return null;
            }
        }

        public bool ProveraRegistracije()
        {
            return factory.ProveraRegistracije();
        }


        public byte[] Uplata(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            byte[] odgovor = null;

            try
            {
                odgovor = factory.Uplata(kodiranaPoruka);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sertifikat nije validan! Neuspesna uplata!");
                return null;
            }

            byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(odgovor, secretKey);

            byte[] potpis = new byte[256];
            byte[] obicnaPoruka = new byte[decoded.Length - 256];

            Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
            Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length - 256);
            string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, "banka_sign");

            if (DigitalSignature.Verify(obicnaPorukaString, potpis, certBanka))
            {
                Console.WriteLine(obicnaPorukaString);
                return obicnaPoruka;
            }
            else
            {
                Console.WriteLine("Poruka nije uspesno primljena!");
                return null;
            }
        }

        public string ObnavljanjeSertifikata(string pinKod)
        {
            string p = factory.ObnavljanjeSertifikata(pinKod);

            if (p == null)
            {
                Console.WriteLine("Doslo je do greske prilikom obnove kartice. Pokusajte ponovo kasnije!");
            }
            else
            {
                Console.WriteLine("Instaliranje sertifikata. Molimo vas sacekajte. . .");
                Console.ReadLine();

                Console.WriteLine("Uspesna obnova kartice!\nVas novi PIN kod je: " + p);
            }

            return p;
        }
    }
}
