using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Windows.Management.Deployment;

namespace nRF5DFUTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //RemoveSparsePackage();

            if (!ExecutionMode.IsRunningWithIdentity() && args.Length == 0)
            {
                string externalLocation = Application.StartupPath;
                string sparsePkgPath = Path.Combine(Application.StartupPath, "nRF5DFUTool.msix");

                // Attempt Registration
                // C:\Users\headk\AppData\Local\Packages
                if (RegisterSparsePackage(externalLocation, sparsePkgPath))
                {
                    // Registration succeded, restart the app to run with identity
                    Process.Start(Assembly.GetExecutingAssembly().Location, "-run");
                }
                else // Registration failed, run without identity
                {
                    Debug.WriteLine("Package Registation failed, running WITHOUT Identity");
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        // https://blogs.windows.com/windowsdeveloper/2019/12/17/windows-10-sdk-preview-build-19041-available-now/
        private static bool RegisterSparsePackage(string externalLocation, string sparsePkgPath)
        {
            bool registration = false;

            try
            {
                Uri externalUri = new Uri(externalLocation);
                Uri packageUri = new Uri(sparsePkgPath);

                Debug.WriteLine(String.Format("exe Location {0}", externalLocation));
                Debug.WriteLine(String.Format("msix Address {0}", sparsePkgPath));

                Debug.WriteLine(String.Format("  exe Uri {0}", externalUri));
                Debug.WriteLine(String.Format("  msix Uri {0}", packageUri));

                PackageManager packageManager = new PackageManager();

                //Declare use of an external location
                var options = new AddPackageOptions();
                options.ExternalLocationUri = externalUri;

                Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);

                ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.

                deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };

                Debug.WriteLine(String.Format("Installing package {0}", sparsePkgPath));

                Debug.WriteLine(String.Format("Waiting for package registration to complete..."));

                opCompletedEvent.WaitOne();

                if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Error)
                {
                    DeploymentResult deploymentResult = deploymentOperation.GetResults();
                    Debug.WriteLine(String.Format("Installation Error: {0}", deploymentOperation.ErrorCode));
                    Debug.WriteLine(String.Format("Detailed Error Text: {0}", deploymentResult.ErrorText));

                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Canceled)
                {
                    Debug.WriteLine("Package Registration Canceled");
                }
                else if (deploymentOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    registration = true;
                    Debug.WriteLine("Package Registration succeeded!");
                }
                else
                {
                    Debug.WriteLine("Installation status unknown");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("AddPackageSample failed, error message: {0}", ex.Message));
                Debug.WriteLine(String.Format("Full Stacktrace: {0}", ex.ToString()));

                return registration;
            }

            return registration;
        }

        private static void RemoveSparsePackage() //example of how to uninstall a Sparse Package
        {
            PackageManager packageManager = new PackageManager();
            Windows.Foundation.IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation = packageManager.RemovePackageAsync("DFUTool_1.0.0.0_x64__w5xy6scsv5yvr");
            ManualResetEvent opCompletedEvent = new ManualResetEvent(false); // this event will be signaled when the deployment operation has completed.

            deploymentOperation.Completed = (depProgress, status) => { opCompletedEvent.Set(); };

            Debug.WriteLine("Uninstalling package..");
            opCompletedEvent.WaitOne();
        }
    }
}
