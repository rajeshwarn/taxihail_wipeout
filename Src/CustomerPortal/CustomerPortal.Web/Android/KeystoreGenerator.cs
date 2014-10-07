using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CustomerPortal.Web.Entities;
using MongoRepository;

namespace CustomerPortal.Web.Android
{
    public class KeystoreGenerator
    {
        private readonly MongoRepository<Company> repository;

        public KeystoreGenerator()
        {
            repository = new MongoRepository<Company>();
        }

        public void Generate(string companyId, string companyPath)
        {
            var keytoolPath = FindKeytoolPath();

            var company = repository.GetById(companyId);
            var signingKeyAlias = company.CompanySettings.FirstOrDefault(x => x.Key == "AndroidSigningKeyAlias") != null 
                    ? company.CompanySettings.First(x => x.Key == "AndroidSigningKeyAlias").Value 
                    : null;
            var signingKeyStorePass = company.CompanySettings.FirstOrDefault(x => x.Key == "AndroidSigningKeyPassStorePass") != null
                    ? company.CompanySettings.First(x => x.Key == "AndroidSigningKeyPassStorePass").Value 
                    : null;

            if (signingKeyAlias == null || signingKeyStorePass == null)
            {
                throw new Exception("Settings \"AndroidSigningKeyAlias\" and \"AndroidSigningKeyPassStorePass\" are required to generate Android Keystore");
            }

            //generate key store file
            var keystoreFile = Path.Combine(companyPath, "public.keystore");
            if (File.Exists(keystoreFile))
            {
                File.Copy(keystoreFile, keystoreFile + "."+ DateTime.Now.Ticks.ToString() );
                File.Delete(keystoreFile);
            }

            var command = string.Format(@" -genkey -v -keystore ""{0}"" -alias {1} -keyalg RSA -keysize 2048 -validity 10000 -storepass {2} -dname ""cn={3}"" -keypass {2}", 
                            keystoreFile, 
                            signingKeyAlias, 
                            signingKeyStorePass, 
                            company.CompanyName);

            var generateKeyTool = new ProcessStartInfo
            {
                FileName = keytoolPath,
                Arguments = command
            };

            using (var exeProcess = Process.Start(generateKeyTool))
            {
                exeProcess.WaitForExit();
            }

            //genete md5/sha1 fingerprint for google map key
            var commandFingerprints = String.Format(@"-v -list -alias {0} -keystore ""{1}"" -storepass {2} -keypass {2}",
                                       signingKeyAlias,
                                       keystoreFile,
                                       signingKeyStorePass);

            var generateFingerprints = new ProcessStartInfo
            {
                FileName = keytoolPath,
                Arguments = commandFingerprints,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using (var exeProcess = Process.Start(generateFingerprints))
            {
                exeProcess.WaitForExit();
                var result = exeProcess.StandardOutput.ReadToEnd();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during google map key generation");
                }

                var md5 = result.Substring(result.IndexOf("MD5:") + 6, 47).Trim();
                var sha1 = result.Substring(result.IndexOf("SHA1:") + 6, 59).Trim();

                company.GooglePlayCredentials.KeystoreMD5Signature = md5;
                company.GooglePlayCredentials.KeystoreSHA1Signature = sha1;
            }

            repository.Update(company);
        }

        private string FindKeytoolPath()
        {
            try
            {
                // try to find the x64 version 
                var p = new Process
                {
                    StartInfo =
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        FileName = "cmd.exe"
                    }
                };
                p.Start();
                p.StandardInput.WriteLine("where /R \"c:\\Program Files\\Java\" keytool");
                p.StandardInput.WriteLine("exit");

                var lines = new List<string>();
                while (!p.StandardOutput.EndOfStream)
                {
                    var line = p.StandardOutput.ReadLine();
                    lines.Add(line);
                }

                var keytoolPath = lines.LastOrDefault(x => x.EndsWith("keytool.exe"));
                if (keytoolPath == null)
                {
                    // try to find the x86 version
                    p = new Process
                    {
                        StartInfo =
                        {
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            FileName = "cmd.exe"
                        }
                    };
                    p.Start();
                    p.StandardInput.WriteLine("where /R \"c:\\Program Files (x86)\\Java\" keytool");
                    p.StandardInput.WriteLine("exit");

                    while (!p.StandardOutput.EndOfStream)
                    {
                        var line = p.StandardOutput.ReadLine();
                        lines.Add(line);
                    }

                    keytoolPath = lines.Last(x => x.EndsWith("keytool.exe"));
                }
                Debug.WriteLine(keytoolPath);
                return keytoolPath;
            }
            catch (Exception)
            {
                throw new Exception("Keytool not found, please install JRE 6.");
            }
        }
    }
}