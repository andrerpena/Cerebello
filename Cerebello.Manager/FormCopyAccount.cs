using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CerebelloWebRole.Code;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using CloudBlockBlob = Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

namespace Cerebello.Manager
{
    public partial class FormCopyAccount : Form
    {
        public FormCopyAccount()
        {
            InitializeComponent();
        }

        private static IEnumerable<T> SelectManyRecursive<T>(IEnumerable<T> items, Func<T, IEnumerable<T>> childrenGetter)
        {
            foreach (var item in items)
            {
                yield return item;

                if (childrenGetter != null)
                    foreach (var child in childrenGetter(item) ?? Enumerable.Empty<T>())
                        yield return child;
            }
        }

        private void buttonCopyStorage_Click(object sender, EventArgs e)
        {
            foreach (var control in SelectManyRecursive(this.Controls.OfType<Control>(), c => c.Controls.OfType<Control>()))
                if (control is Button)
                    this.Invoke((Action)(() => (control as Button).Enabled = false));

            var task = new Task(this.CopyBlobs);
            task.Start();
            task.ContinueWith(t =>
                {
                    foreach (var control in SelectManyRecursive(this.Controls.OfType<Control>(), c => c.Controls.OfType<Control>()))
                        if (control is Button)
                            this.Invoke((Action)(() => (control as Button).Enabled = true));
                });
        }

        private void buttoCopyDatabase_Click(object sender, EventArgs e)
        {
            foreach (var control in SelectManyRecursive(this.Controls.OfType<Control>(), c => c.Controls.OfType<Control>()))
                if (control is Button)
                    this.Invoke((Action)(() => (control as Button).Enabled = false));

            var task = new Task(this.CopyDatabase);
            task.Start();
            task.ContinueWith(t =>
            {
                foreach (var control in SelectManyRecursive(this.Controls.OfType<Control>(), c => c.Controls.OfType<Control>()))
                    if (control is Button)
                        this.Invoke((Action)(() => (control as Button).Enabled = true));
            });
        }

        private void CopyDatabase()
        {
            this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, "Copying database");

            string exportBlobPath;

            var storageKeyCerebello2 = "DW8oW7HSDj2jQ+uz4p+elMpC6B6/v/WVoJ918PPZiCMOwALIVOIY/AKLd/WvMGdWFi7vlarPvri1eS6KduowKA==";

            {
                // Set Inputs to the REST Requests in the helper class.
                var helperToExport = new ImportExportHelper
                    {
                        // Set Inputs to the REST Requests in the helper class.
                        EndPointUri = ImportExportHelper.EndPoints.EastUS,

                        // this is the destination storage account key: cerebello2
                        StorageKey = storageKeyCerebello2,

                        // this is the source database account: cerebellohq
                        DatabaseServerName = "kurptcva75.database.windows.net",
                        DatabaseName = "cerebellohq",
                        DatabaseServerUserName = "cerebello",
                        DatabaseServerPassword = "26uj27oP",
                    };

                // Export database to a blob in the destination account.
                exportBlobPath = helperToExport.DoExport(
                    string.Format(
                        @"https://cerebello2.blob.core.windows.net/db-backups/{0}-{1}.bacpac",
                        helperToExport.DatabaseName,
                        DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'-'HH'-'mm")));

                if (string.IsNullOrWhiteSpace(exportBlobPath))
                    this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, "Export database failed.");
                else
                    this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, string.Format("Database exported to: {0}.", exportBlobPath));
            }

            if (!string.IsNullOrWhiteSpace(exportBlobPath))
            {
                var helperToImport = new ImportExportHelper
                    {
                        // Set Inputs to the REST Requests in the helper class.
                        EndPointUri = ImportExportHelper.EndPoints.EastUS,

                        // this is the destination storage account key: cerebello2
                        StorageKey = storageKeyCerebello2,

                        DatabaseServerName = "kurptcva75.database.windows.net",
                        DatabaseServerUserName = "cerebello",
                        DatabaseServerPassword = "26uj27oP",
                        DatabaseName = "cerebello2",
                    };

                // Proceed with Import operation using bacpac created by Export if the Export operation succeeded
                var importSucceeded = helperToImport.DoImport(exportBlobPath);

                this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, importSucceeded ? "Import database succeded." : "Import database failed.");
            }
        }

        private void CopyBlobs()
        {
            this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, "Copying blobs");

            var startTime = DateTime.Now;
            var copyingItems = 0;
            var failedItems = 0;

            var srcAccount = GetStorageAccountFromConfiguration("StorageConnectionString_cerebellohq");
            var dstAccount = GetStorageAccountFromConfiguration("StorageConnectionString_cerebello2");

            var srcBlobClient = srcAccount.CreateCloudBlobClient();
            var dstBlobClient = dstAccount.CreateCloudBlobClient();

            var queueOfPendingOps = new Queue<Action>(1000);

            foreach (var srcCloudBlobContainer in srcBlobClient.ListContainers())
            {
                var dstCloudBlobContainer = dstBlobClient
                    .GetContainerReference(srcCloudBlobContainer.Name);

                dstCloudBlobContainer.CreateIfNotExists();

                foreach (var srcBlob in srcCloudBlobContainer.ListBlobs(useFlatBlobListing: true))
                {
                    if (srcBlob is CloudBlockBlob)
                    {
                        var srcBlockBlock = (CloudBlockBlob)srcBlob;
                        var dstBlockBlock = dstCloudBlobContainer
                            .GetBlockBlobReference(srcBlockBlock.Name);

                        // Assuming the source blob container ACL is "Private", let's create a Shared Access Signature with
                        // Expiry Time = Current Time (UTC) + 7 Days - 7 days is the maximum time allowed for copy operation to finish.
                        // Permission = Read so that copy service can read the blob from source
                        var sas = srcBlockBlock.GetSharedAccessSignature(
                            new SharedAccessBlobPolicy
                                {
                                    SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                                    Permissions = SharedAccessBlobPermissions.Read,
                                });

                        // Create a SAS URI for the blob
                        var srcBlockBlobSasUri = string.Format("{0}{1}", srcBlockBlock.Uri, sas);

                        var asyncResult = dstBlockBlock.BeginStartCopyFromBlob(
                            new Uri(srcBlockBlobSasUri),
                            null,
                            null);

                        queueOfPendingOps.Enqueue(
                            () =>
                            {
                                bool ok = false;
                                try
                                {
                                    dstBlockBlock.EndStartCopyFromBlob(asyncResult);
                                    ok = true;
                                }
                                catch
                                {
                                }

                                if (ok)
                                {
                                    Interlocked.Increment(ref copyingItems);
                                    this.Invoke((Action)(() => this.labelCopied.Text = string.Format("Copying: {0} blobs", copyingItems)));
                                }
                                else
                                {
                                    Interlocked.Increment(ref failedItems);
                                    this.Invoke((Action)(() => this.labelFailed.Text = string.Format("Failed: {0} blobs", failedItems)));
                                    this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, string.Format("Failed blob copy: {0}", dstBlockBlock.Uri));
                                }
                            });
                    }
                    else if (srcBlob is CloudPageBlob)
                    {
                        throw new NotImplementedException();
                    }
                    else if (srcBlob is CloudBlobDirectory)
                    {
                        // Nothing to do... directories are virtual things in azure blob storage.
                    }

                    // Don't let the queue of pending operations get too large...
                    // otherwise this could cause copy operations to time-out.
                    if (queueOfPendingOps.Count > 3)
                        while (queueOfPendingOps.Count > 1)
                            queueOfPendingOps.Dequeue()();
                }
            }

            while (queueOfPendingOps.Count > 0)
                queueOfPendingOps.Dequeue()();

            var endTime = DateTime.Now;

            this.Invoke((Func<object, int>)this.listBoxItems.Items.Add, string.Format("Todos os {1} itens foram copiados em {0}.", endTime - startTime, copyingItems));
        }

        private static CloudStorageAccount GetStorageAccountFromConfiguration(string name)
        {
            var storageAccountStr = StringHelper.FirstNonEmpty(
                () => CloudConfigurationManager.GetSetting(name),
                () => ConfigurationManager.ConnectionStrings[name].ConnectionString,
                () => ConfigurationManager.AppSettings[name],
                () => { throw new Exception("No storage connection string found."); });

            storageAccountStr = Regex.Replace(storageAccountStr, @"\s+", m => m.Value.Contains("\n") ? "" : m.Value);

            var storageAccount = CloudStorageAccount.Parse(storageAccountStr);
            return storageAccount;
        }
    }
}
