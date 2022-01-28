using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using BankaContracts;

namespace Manager
{
	public class ServiceCertValidator : X509CertificateValidator
	{
		public override void Validate(X509Certificate2 certificate)
		{
			/// This will take service's certificate from storage
			X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, Formatter.ParseName(WindowsIdentity.GetCurrent().Name));

			bool x = false;
			try
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

				x = retList.Contains(certificate.SerialNumber);
			}
			catch (Exception e)
			{
                //Console.WriteLine(e.Message);
			}

			if (!certificate.Issuer.Equals(srvCert.Issuer) || x)
			{
				throw new Exception("Certificate is not from the valid issuer.");
			}
		}
	}
}
