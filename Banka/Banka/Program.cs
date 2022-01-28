using BankaContracts;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace Banka
{
	public class Program
	{
		public static BankaProxy proxy = null;
		public static BankaProxyAudit proxyAudit = null;
		static void Main(string[] args)
		{
			//1
			NetTcpBinding binding = new NetTcpBinding();
			string address = "net.tcp://localhost:9999/BankaServis";

			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
			binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

			ServiceHost host = new ServiceHost(typeof(BankaServis));
			host.AddServiceEndpoint(typeof(IBankaServis), binding, address);

			ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
			newAudit.AuditLogLocation = AuditLogLocation.Application;
			newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
			host.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
			host.Description.Behaviors.Add(newAudit);

			//2
			NetTcpBinding binding2 = new NetTcpBinding();
			string address2 = "net.tcp://localhost:9998/BankaServis";

			binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
			binding2.OpenTimeout = new TimeSpan(0, 10, 0);
			binding2.CloseTimeout = new TimeSpan(0, 10, 0);
			binding2.SendTimeout = new TimeSpan(0, 10, 0);
			binding2.ReceiveTimeout = new TimeSpan(0, 10, 0);

			ServiceHost host2 = new ServiceHost(typeof(BankaServis));
			host2.AddServiceEndpoint(typeof(IBankaServis), binding2, address2);
			host2.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.Custom;
			host2.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = new ServiceCertValidator();
			host2.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
			host2.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "banka");
			
			newAudit.AuditLogLocation = AuditLogLocation.Application;
			newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
			host2.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
			host2.Description.Behaviors.Add(newAudit);

			//3
			NetTcpBinding binding3 = new NetTcpBinding();
			string address3 = "net.tcp://localhost:9997/ReplikatorServis";

			binding3.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
			binding3.Security.Transport.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

			string srvCertCN = "replikator";
			X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople,
				StoreLocation.LocalMachine, srvCertCN);

			EndpointAddress endpointAddress3 = new EndpointAddress(new Uri(address3), new X509CertificateEndpointIdentity(srvCert));
			proxy = new BankaProxy(binding3, endpointAddress3);

			//4
			NetTcpBinding binding4 = new NetTcpBinding();
			string address4 = "net.tcp://localhost:9996/BankingAuditServis";

			binding4.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
			binding4.Security.Transport.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

			string auditCertCN = "audit";
			X509Certificate2 auditCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople,
				StoreLocation.LocalMachine, auditCertCN);

			EndpointAddress endpointAddress4 = new EndpointAddress(new Uri(address4), new X509CertificateEndpointIdentity(auditCert));
			proxyAudit = new BankaProxyAudit(binding4, endpointAddress4);

			host.Open();
			host2.Open();

			Console.ReadLine();

		}
	}
}
