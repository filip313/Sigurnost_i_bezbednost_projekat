using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Klijent
{
	public class Program
	{
		public static string address2 = "net.tcp://localhost:9998/BankaServis";
		public static NetTcpBinding binding2 = new NetTcpBinding();
		public static string srvCertCN = "banka";
		public static X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople,
				StoreLocation.LocalMachine, srvCertCN);

		static void Main(string[] args)
		{
			NetTcpBinding binding = new NetTcpBinding();

			string address = "net.tcp://localhost:9999/BankaServis";

			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

			binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

			EndpointAddress endpointAddress = new EndpointAddress(new Uri(address));

			ClientProxy proxyWcf = new ClientProxy(binding, endpointAddress, true);
			ClientProxy proxy = null;

			if (proxyWcf.ProveraRegistracije())
			{
				EndpointAddress endpointAddress2 = new EndpointAddress(new Uri(address2), new X509CertificateEndpointIdentity(srvCert));
				proxy = new ClientProxy(binding2, endpointAddress2, false);

				UserInterface(proxy, proxyWcf);

			}
			else
			{
				Console.WriteLine("Da li zelite da se registrujete? y/n");
				string odg = Console.ReadLine();
				if (odg == "y")
				{
					if (proxyWcf.Registracija() == null)
					{
						return;
					}
					else
					{
						EndpointAddress endpointAddress2 = new EndpointAddress(new Uri(address2), new X509CertificateEndpointIdentity(srvCert));
						proxy = new ClientProxy(binding2, endpointAddress2, false);

						UserInterface(proxy, proxyWcf);
					}
				}
				else if (odg == "n")
				{
					Console.WriteLine("Odbili ste registraciju. Program se gasi!");
					Console.ReadLine();
					return;
				}
				else
				{
					Console.WriteLine("Uneli ste nevalidan odgovor. Program se gasi!");
					Console.ReadLine();
					return;
				}
			}

			Console.ReadLine();
		}
		public static void UserInterface(ClientProxy proxy, ClientProxy proxyWcf)
		{
			string opcija;
			string username = Formatter.ParseName(WindowsIdentity.GetCurrent().Name);

			Console.WriteLine("Dobrodosli, " + username + "!");

			do
			{
				//Console.Clear();
				Console.WriteLine("Izaberite opciju: ");
				Console.WriteLine("\t1. Uplata");
				Console.WriteLine("\t2. Isplata");
				Console.WriteLine("\t3. Promena PIN koda");
				Console.WriteLine("\t4. Obnovi sertifikat");
				Console.WriteLine("\t5. Kraj");
				Console.Write("Vasa opcija: ");
				opcija = Console.ReadLine();

				switch(opcija)
                {
					case "1":
                        {
							Console.Write("Unesite PIN kod: ");
							string pinKod = Console.ReadLine();

							Console.Write("Unesite iznos za uplatu: ");
							string iznos = Console.ReadLine();

							string spakovanaPoruka = pinKod + "|" + iznos;
							byte[] spakovanaPorukaBajtovi = System.Text.Encoding.UTF8.GetBytes(spakovanaPoruka);
							
							X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "_sign");

							byte[] potpisanaPoruka = DigitalSignature.Create(spakovanaPoruka, cert);

							byte[] pripremaZaEnkripciju = new byte[256 + spakovanaPorukaBajtovi.Length];

							Buffer.BlockCopy(potpisanaPoruka, 0, pripremaZaEnkripciju, 0, 256);
							Buffer.BlockCopy(spakovanaPorukaBajtovi, 0, pripremaZaEnkripciju, 256, spakovanaPorukaBajtovi.Length);

							string secretKey = SecretKey.LoadKey(username);

							byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

							proxy.Uplata(encoded);							
						}
						break;
					case "2":
                        {
							Console.Write("Unesite PIN kod: ");
							string pinKod = Console.ReadLine();

							Console.Write("Unesite iznos za isplatu: ");
							string iznos = Console.ReadLine();

							string spakovanaPoruka = pinKod + "|" + iznos;
							byte[] spakovanaPorukaBajtovi = System.Text.Encoding.UTF8.GetBytes(spakovanaPoruka);

							X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "_sign");

							byte[] potpisanaPoruka = DigitalSignature.Create(spakovanaPoruka, cert);

							byte[] pripremaZaEnkripciju = new byte[256 + spakovanaPorukaBajtovi.Length];

							Buffer.BlockCopy(potpisanaPoruka, 0, pripremaZaEnkripciju, 0, 256);
							Buffer.BlockCopy(spakovanaPorukaBajtovi, 0, pripremaZaEnkripciju, 256, spakovanaPorukaBajtovi.Length);

							string secretKey = SecretKey.LoadKey(username);

							byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

							proxy.Isplata(encoded);
						}
						break;
					case "3":
                        {
							Console.Write("Unesite PIN kod: ");
							string pinKod = Console.ReadLine();
							string noviPin;
							int n;

							do
							{
                                Console.Write("Unesite novi PIN (4 cifre): ");
								noviPin = Console.ReadLine();

							} while (noviPin.Length != 4 || !Int32.TryParse(noviPin, out n));

							string spakovanaPoruka = pinKod + "|" + noviPin;
							byte[] spakovanaPorukaBajtovi = System.Text.Encoding.UTF8.GetBytes(spakovanaPoruka);

							X509Certificate2 cert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, username + "_sign");

							byte[] potpisanaPoruka = DigitalSignature.Create(spakovanaPoruka, cert);

							byte[] pripremaZaEnkripciju = new byte[256 + spakovanaPorukaBajtovi.Length];

							Buffer.BlockCopy(potpisanaPoruka, 0, pripremaZaEnkripciju, 0, 256);
							Buffer.BlockCopy(spakovanaPorukaBajtovi, 0, pripremaZaEnkripciju, 256, spakovanaPorukaBajtovi.Length);

							string secretKey = SecretKey.LoadKey(username);

							byte[] encoded = TripleDES_Symm_Algorithm.Encrypt(pripremaZaEnkripciju, secretKey);

							proxy.PromenaPinKoda(encoded);
						}
						break;
					case "4":
                        {
							Console.Write("Unesite PIN kod: ");
							string pinKod = Console.ReadLine();

							proxyWcf.ObnavljanjeSertifikata(pinKod);

							EndpointAddress endpointAddress2 = new EndpointAddress(new Uri(address2), new X509CertificateEndpointIdentity(srvCert));
							proxy = new ClientProxy(binding2, endpointAddress2, false);
						}
						break;
					default: break;

                }


			} while (opcija != "5");
		}
	}
}
