using System;
using System.Linq;
using System.Security.Principal;
using Windows.Management.Deployment;
using Microsoft.Win32;
using Windows.ApplicationModel;

namespace EnumWSL
{
    class Program
    {
        static void Main(string[] args)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string ssdl = WindowsIdentity.GetCurrent().User.Value;
            PackageManager packageManager = new PackageManager();

            RegistryKey rk = Registry.CurrentUser;
            RegistryKey lxrk = rk.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Lxss");
            string defUID = lxrk.GetValue("DefaultDistribution").ToString();

            string[] dists = lxrk.GetSubKeyNames();
            foreach (var dist in dists)
            {
                Console.WriteLine("LxUID: {0}", dist);
                RegistryKey sub = lxrk.OpenSubKey(dist);
                string name = sub.GetValue("DistributionName").ToString();
                Console.WriteLine("DistributionName: {0}", name);
                object familyValue = sub.GetValue("PackageFamilyName");
                if (familyValue != null)
                {
                    string familyName = familyValue.ToString();
                    Console.WriteLine("PackageFamilyName: {0}", familyName);
                    Package pkg = packageManager.FindPackagesForUser(ssdl, familyName).FirstOrDefault();
                    if (pkg != null)
                    {
                        string installLocation = pkg.InstalledLocation.Path;
                        Console.WriteLine("LaunchPath: \"{0}\\Microsoft\\WindowsApps\\{1}.exe\"", localAppData, name);
                        Console.WriteLine("IconPath: \"{0}\\images\\icon.ico\"", installLocation);
                        Console.WriteLine("IsDefault: {0}", dist == defUID ? "true" : "false");
                    }
                }
                else if (name == "Legacy")
                {
                    string path = sub.GetValue("BasePath").ToString();
                    string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    Console.WriteLine("LaunchPath: \"{0}\\bash.exe\"", sys32);
                    Console.WriteLine("IconPath: \"{0}\\bash.ico\"", path);
                    Console.WriteLine("IsDefault: {0}", dist == defUID ? "true" : "false");
                }
                Console.WriteLine("");
            }
        }
    }
}
