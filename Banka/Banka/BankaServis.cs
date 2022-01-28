using BankaContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manager;
using System.Security.Principal;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Security.Cryptography;
using System.ServiceModel;
using System.IO;
using System.Configuration;

namespace Banka
{
    public class BankaServis : IBankaServis
    {
        public bool ProveraRegistracije()
        { 
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

            string korisnickoIme = Formatter.ParseName(windowsIdentity.Name);

            List<Korisnik> listaKorisnika = JSONReader.ProcitajKorisnike();

            foreach(Korisnik k in listaKorisnika)
            {
                if(k.KorisnickoIme == korisnickoIme)
                {
                    return true;
                }
            }

            return false;
        }

        public string Registracija()
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity windowsIdentity = identity as WindowsIdentity;

            string korisnickoIme = Formatter.ParseName(windowsIdentity.Name);
            try
            {
                string pin = PinGenerator();
                Console.WriteLine("Registrovan je novi korisnik " + korisnickoIme + " sa PIN kodom: " + pin + ".");

                string cmd = "/c makecert -sv " + korisnickoIme + ".pvk -iv MainCA.pvk -n \"CN=" + korisnickoIme + "\" -pe -ic MainCA.cer " + korisnickoIme + ".cer -sr localmachine -ss My -sky exchange";
                System.Diagnostics.Process.Start("cmd.exe", cmd).WaitForExit();

                string cmd2 = "/c pvk2pfx.exe /pvk " + korisnickoIme + ".pvk /pi " + pin + " /spc " + korisnickoIme + ".cer /pfx " + korisnickoIme + ".pfx";
                System.Diagnostics.Process.Start("cmd.exe", cmd2).WaitForExit();

                string cmd3 = "/c makecert -sv " + korisnickoIme + "_sign" + ".pvk -iv MainCA.pvk -n \"CN=" + korisnickoIme + "_sign" + "\" -pe -ic MainCA.cer " + korisnickoIme + "_sign" + ".cer -sr localmachine -ss My -sky signature";
                System.Diagnostics.Process.Start("cmd.exe", cmd3).WaitForExit();

                string cmd4 = "/c pvk2pfx.exe /pvk " + korisnickoIme + "_sign" + ".pvk /pi " + pin + " /spc " + korisnickoIme + "_sign" + ".cer /pfx " + korisnickoIme + "_sign" + ".pfx";
                System.Diagnostics.Process.Start("cmd.exe", cmd4).WaitForExit();

                byte[] textData = System.Text.Encoding.UTF8.GetBytes(pin);
                SHA256Managed sha256 = new SHA256Managed();
                byte[] pinHelp = sha256.ComputeHash(textData);

                Korisnik k = new Korisnik(korisnickoIme, System.Text.Encoding.UTF8.GetString(pinHelp));

                JSONReader.SacuvajKorisnika(k);
                Program.proxy.SacuvajPodatke(k);

                string secretKey = SecretKey.GenerateKey();
                string enkripcija = secretKey + pin;
                X509Certificate2 certClient = CertManager.GetCertificateFromFile(korisnickoIme);

                string kriptovano = Manager.RSA.Encrypt(enkripcija, certClient.GetRSAPublicKey().ToXmlString(false));

                SecretKey.StoreKey(secretKey, korisnickoIme);

                Program.proxy.SacuvajKljuc(secretKey, korisnickoIme);

                Audit.IzdavanjeSuccess(korisnickoIme);

                return kriptovano;
            }
            catch(Exception e)
            {
                Console.WriteLine("Neuspesna registracija!" + e.StackTrace);
                Audit.IzdavanjeFailure(korisnickoIme, e.Message);
                return null;
            }
        }

        public string PinGenerator()
        {
            string pin = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
            pin = pin.Substring(0, 4);

            return pin;
        }

        public byte[] Uplata(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "banka_sign");

            try
            {
                List<Korisnik> sviKorisnici = JSONReader.ProcitajKorisnike();
                Korisnik korisnik = new Korisnik();

                foreach(Korisnik k in sviKorisnici)
                {
                    if(k.KorisnickoIme == korisnickoIme)
                    {
                        korisnik = k;
                        break;
                    }
                }

                byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(kodiranaPoruka, secretKey);

                byte[] potpis = new byte[256];
                byte[] obicnaPoruka = new byte[decoded.Length - 256];

                Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
                Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length-256);
                string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

                X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, korisnickoIme + "_sign");

                if (DigitalSignature.Verify(obicnaPorukaString, potpis, cert))
                {
                    string pinKod = obicnaPorukaString.Split('|')[0];
                    string iznos = obicnaPorukaString.Split('|')[1];

                    byte[] textData = System.Text.Encoding.UTF8.GetBytes(pinKod);
                    SHA256Managed sha256 = new SHA256Managed();
                    byte[] hesovanPin = sha256.ComputeHash(textData);
                    
                    if(korisnik.Pin == System.Text.Encoding.UTF8.GetString(hesovanPin))
                    {
                        korisnik.Stanje += Double.Parse(iznos);
                        JSONReader.SacuvajKorisnika(korisnik);
                        Program.proxy.SacuvajPodatke(korisnik);

                        string retSign = "Uspesna uplata. Vase trenutno stanje je: " + korisnik.Stanje + " dinara.";
                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        Audit.UplataSuccess(korisnickoIme, iznos);

                        return encoded;
                    }
                    else
                    {
                        string retSign = "Uplata nije uspesna, jer PIN nije validan!";
                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        Audit.UplataFailure(korisnickoIme, retSign);

                        return encoded;
                    }
                }
                else
                {
                    string retSign = "Uplata nije uspesna, jer potpis nije validan!";
                    byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                    byte[] sign = DigitalSignature.Create(retSign, certBanka);

                    byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                    Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                    Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                    byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                    Audit.UplataFailure(korisnickoIme, retSign);
                    
                    return encoded;
                }
            }
            catch(Exception e)
            {
                string retSign = "Nije moguce izvrsiti uplatu, pokusajte kasnije! Poruka: " + e.Message;
                byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                byte[] sign = DigitalSignature.Create(retSign, certBanka);

                byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                Audit.UplataFailure(korisnickoIme, retSign);

                return encoded;
            }
        }

        public byte[] Isplata(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "banka_sign");
            
            try
            {
                List<Korisnik> sviKorisnici = JSONReader.ProcitajKorisnike();
                Korisnik korisnik = new Korisnik();

                foreach (Korisnik k in sviKorisnici)
                {
                    if (k.KorisnickoIme == korisnickoIme)
                    {
                        korisnik = k;
                        break;
                    }
                }

                byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(kodiranaPoruka, secretKey);

                byte[] potpis = new byte[256];
                byte[] obicnaPoruka = new byte[decoded.Length - 256];

                Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
                Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length - 256);
                string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

                X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, korisnickoIme + "_sign");

                if (DigitalSignature.Verify(obicnaPorukaString, potpis, cert))
                {
                    string pinKod = obicnaPorukaString.Split('|')[0];
                    string iznos = obicnaPorukaString.Split('|')[1];

                    byte[] textData = System.Text.Encoding.UTF8.GetBytes(pinKod);
                    SHA256Managed sha256 = new SHA256Managed();
                    byte[] hesovanPin = sha256.ComputeHash(textData);

                    if (korisnik.Pin == System.Text.Encoding.UTF8.GetString(hesovanPin))
                    {
                        string retSign;

                        if(korisnik.Stanje >= Double.Parse(iznos))
                        {
                            korisnik.Stanje -= Double.Parse(iznos);
                            JSONReader.SacuvajKorisnika(korisnik);
                            retSign = "Uspesna isplata. Vase trenutno stanje je: " + korisnik.Stanje + " dinara.";
                            Program.proxy.SacuvajPodatke(korisnik);
                            Audit.IsplataSuccess(korisnickoIme, iznos);
                            Task.Run(() => ProveraLogova(korisnickoIme));
                        }
                        else
                        {
                            retSign = "Nije uspesna isplata, jer nemate dovoljno novca na racunu!";
                            Audit.IsplataFailure(korisnickoIme, retSign);
                        }

                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        return encoded;
                    }
                    else
                    {
                        string retSign = "Isplata nije uspesna, jer PIN nije validan!";
                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        Audit.IsplataFailure(korisnickoIme, retSign);

                        return encoded;
                    }
                }
                else
                {
                    string retSign = "Isplata nije uspesna, jer potpis nije validan!";
                    byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                    byte[] sign = DigitalSignature.Create(retSign, certBanka);

                    byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                    Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                    Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                    byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                    Audit.IsplataFailure(korisnickoIme, retSign);

                    return encoded;
                }
            }
            catch (Exception e)
            {
                string retSign = "Nije moguce izvrsiti isplatu, pokusajte kasnije!" + e.Message;
                byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                byte[] sign = DigitalSignature.Create(retSign, certBanka);

                byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                Audit.IsplataFailure(korisnickoIme, retSign);

                return encoded;
            }
        }

        public byte[] PromenaPinKoda(byte[] kodiranaPoruka)
        {
            string korisnickoIme = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);

            string secretKey = SecretKey.LoadKey(korisnickoIme);

            X509Certificate2 certBanka = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "banka_sign");

            try
            {
                List<Korisnik> sviKorisnici = JSONReader.ProcitajKorisnike();
                Korisnik korisnik = new Korisnik();

                foreach (Korisnik k in sviKorisnici)
                {
                    if (k.KorisnickoIme == korisnickoIme)
                    {
                        korisnik = k;
                        break;
                    }
                }

                byte[] decoded = TripleDES_Symm_Algorithm.Decrypt(kodiranaPoruka, secretKey);

                byte[] potpis = new byte[256];
                byte[] obicnaPoruka = new byte[decoded.Length - 256];

                Buffer.BlockCopy(decoded, 0, potpis, 0, 256);
                Buffer.BlockCopy(decoded, 256, obicnaPoruka, 0, decoded.Length - 256);
                string obicnaPorukaString = System.Text.Encoding.UTF8.GetString(obicnaPoruka);

                X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, korisnickoIme + "_sign");

                if (DigitalSignature.Verify(obicnaPorukaString, potpis, cert))
                {
                    string pinKod = obicnaPorukaString.Split('|')[0];
                    string noviPin = obicnaPorukaString.Split('|')[1];

                    byte[] textData = System.Text.Encoding.UTF8.GetBytes(pinKod);
                    SHA256Managed sha256 = new SHA256Managed();
                    byte[] hesovanPin = sha256.ComputeHash(textData);

                    if (korisnik.Pin == System.Text.Encoding.UTF8.GetString(hesovanPin))
                    {
                        byte[] textData2 = System.Text.Encoding.UTF8.GetBytes(noviPin);
                        byte[] hesovanNoviPin = sha256.ComputeHash(textData2);
                        korisnik.Pin = Encoding.UTF8.GetString(hesovanNoviPin);

                        JSONReader.SacuvajKorisnika(korisnik);
                        Program.proxy.SacuvajPodatke(korisnik);

                        string retSign = "Uspesna promena PIN-a!";

                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        Audit.PromenaPinaSuccess(korisnickoIme);

                        return encoded;
                    }
                    else
                    {
                        string retSign = "Promena PIN-a nije moguca, jer stari PIN nije validan!";
                        byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                        byte[] sign = DigitalSignature.Create(retSign, certBanka);

                        byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                        Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                        Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                        byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                        Audit.PromenaPinaFailure(korisnickoIme, retSign);

                        return encoded;
                    }
                }
                else
                {
                    string retSign = "Promena PIN-a nije moguca, jer potpis nije validan!";
                    byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                    byte[] sign = DigitalSignature.Create(retSign, certBanka);

                    byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                    Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                    Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                    byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                    Audit.PromenaPinaFailure(korisnickoIme, retSign);

                    return encoded;
                }
            }
            catch (Exception e)
            {
                string retSign = "Promena PIN-a nije moguca, pokusajte kasnije!" + e.Message;
                byte[] retEnc = Encoding.UTF8.GetBytes(retSign);

                byte[] sign = DigitalSignature.Create(retSign, certBanka);

                byte[] pripremaZaEnkripciju = new byte[256 + retEnc.Length];

                Buffer.BlockCopy(sign, 0, pripremaZaEnkripciju, 0, 256);
                Buffer.BlockCopy(retEnc, 0, pripremaZaEnkripciju, 256, retEnc.Length);

                byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

                Audit.PromenaPinaFailure(korisnickoIme, retSign);

                return encoded;
            }
        }

        public string ObnavljanjeSertifikata(string pinKod)
        {
            string korisnickoIme = Formatter.ParseName(ServiceSecurityContext.Current.PrimaryIdentity.Name);
            byte[] textData = System.Text.Encoding.UTF8.GetBytes(pinKod);
            SHA256Managed sha256 = new SHA256Managed();
            byte[] hesovanPin = sha256.ComputeHash(textData);

            List<Korisnik> sviKorisnici = JSONReader.ProcitajKorisnike();
            Korisnik korisnik = new Korisnik();

            foreach (Korisnik k in sviKorisnici)
            {
                if (k.KorisnickoIme == korisnickoIme)
                {
                    korisnik = k;
                    break;
                }
            }

            if(korisnik.Pin == Encoding.UTF8.GetString(hesovanPin))
            {
                X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, korisnickoIme);

                JSONReader.SacuvajSerijskiBroj(cert.SerialNumber);

                string cmd = "/c certmgr.exe -del -n " + korisnickoIme + "_sign" + " -c -s -r localMachine my";
                //Process.Start("cmd.exe", cmd).WaitForExit();
                cmd = "/c certmgr.exe -del -n " + korisnickoIme + "_sign" + " -c -s -r localMachine trustedPeople";
                Process.Start("cmd.exe", cmd).WaitForExit();
                cmd = "/c certmgr.exe -del -n " + korisnickoIme + " -c -s -r localMachine trustedPeople";
                Process.Start("cmd.exe", cmd).WaitForExit();
                cmd = "/c certmgr.exe -del -n " + korisnickoIme + " -c -s -r localMachine my";
                //Process.Start("cmd.exe", cmd).WaitForExit();

                File.Delete(korisnickoIme + ".pvk");
                File.Delete(korisnickoIme + "_sign.pvk");
                File.Delete(korisnickoIme + ".pfx");
                File.Delete(korisnickoIme + "_sign.pfx");
                File.Delete(korisnickoIme + ".cer");
                File.Delete(korisnickoIme + "_sign.cer");

                string pin = PinGenerator();
                Console.WriteLine("Obnovljena je kartica, novi PIN je: " + pin);

                cmd = "/c makecert -sv " + korisnickoIme + ".pvk -iv MainCA.pvk -n \"CN=" + korisnickoIme + "\" -pe -ic MainCA.cer " + korisnickoIme + ".cer -sr localmachine -ss My -sky exchange";
                Process.Start("cmd.exe", cmd).WaitForExit();
                string cmd2 = "/c pvk2pfx.exe /pvk " + korisnickoIme + ".pvk /pi " + pin + " /spc " + korisnickoIme + ".cer /pfx " + korisnickoIme + ".pfx";
                Process.Start("cmd.exe", cmd2).WaitForExit();
                string cmd3 = "/c makecert -sv " + korisnickoIme + "_sign" + ".pvk -iv MainCA.pvk -n \"CN=" + korisnickoIme + "_sign" + "\" -pe -ic MainCA.cer " + korisnickoIme + "_sign" + ".cer -sr localmachine -ss My -sky signature";
                Process.Start("cmd.exe", cmd3).WaitForExit();
                string cmd4 = "/c pvk2pfx.exe /pvk " + korisnickoIme + "_sign" + ".pvk /pi " + pin + " /spc " + korisnickoIme + "_sign" + ".cer /pfx " + korisnickoIme + "_sign" + ".pfx";
                Process.Start("cmd.exe", cmd4).WaitForExit();

                textData = Encoding.UTF8.GetBytes(pin);
                byte[] hPin = sha256.ComputeHash(textData);
                string noviPin = Encoding.UTF8.GetString(hPin);

                korisnik.Pin = noviPin;
                JSONReader.SacuvajKorisnika(korisnik);
                Program.proxy.SacuvajPodatke(korisnik);

                Audit.ObnovaSuccess(korisnickoIme);
                
                return pin;
            }
            else
            {
                Audit.ObnovaFailure(korisnickoIme, "PIN kod nije odgovarajuci!");
                return null;
            }

        }

        public void ProveraLogova(string korisnickoIme)
        {
            int vreme = Int32.Parse(ConfigurationManager.AppSettings["Vreme"]);
            int brojPokusaja = Int32.Parse(ConfigurationManager.AppSettings["BrojIsplata"]);

            EventLogEntryCollection events = Audit.customLog.Entries;
            List<EventLogEntry> listEntry = new List<EventLogEntry>();
            
            foreach(EventLogEntry e in events)
            {
                if(e.EventID == (int)AuditEventTypes.IsplataSuccess)
                {
                    string user = e.Message.Split('[')[1].Split(']')[0];

                    if (user == korisnickoIme)
                    {
                        listEntry.Add(e);
                    }
                }
            }

            if(listEntry.Count >= brojPokusaja)
            {
                listEntry = listEntry.OrderByDescending(a => a.TimeGenerated).ToList();

                DateTime dt = listEntry[0].TimeGenerated;
                DateTime dt2 = listEntry[brojPokusaja - 1].TimeGenerated;

                TimeSpan t = dt.Subtract(dt2);
                double sekunde = t.TotalSeconds;

                List<string> logovi = new List<string>();

                if(vreme >= sekunde)
                {
                    for(int i = 0; i < brojPokusaja; i++)
                    {
                        logovi.Add(listEntry[i].Message);
                    }

                    Program.proxyAudit.PrijemPodataka("banka", korisnickoIme, dt, logovi);
                }
            }
        }
    }
}
