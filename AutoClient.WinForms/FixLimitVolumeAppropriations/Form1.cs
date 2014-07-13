using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseApp.Service;
using Platform.ClientInteraction;
using Platform.Web.Services;

namespace AutoClient.FixLimitVolumeAppropriations
{
    public partial class Form1 : Form
    {
        private const string newLine = "\r\n";
        private Redirector redirector;
        private string url = "http://shmelev3/Platform3.0.3.2";
        //private string url = "http://stand2/shmelev";
        //private string url = "http://kenny/shmelev";
        private string connectionString = @"Data Source=stand2\sql2012;Initial Catalog=shmelev;Integrated Security=false;User=bis;Password=bissupport;Current Language=English;MultipleActiveResultSets=True;";
        private string dir = "z:/tmp/FixLimitVolumeAppropriation";
        private Main fix;

        private Dictionary<string, int> estimateDoc;
        private int estimateTotal;
        private Dictionary<string, int> changeOperations;
        private Dictionary<string, int> processOperations;

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                WriteLine(text);
            }
        }

        public Form1()
        {
            InitializeComponent();
            redirector = new Redirector(url);
            fix = new Main(connectionString);
        }

        #region Event Handlers

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Login(redirector);
        }

        private void btnLeafStatuses_Click(object sender, EventArgs e)
        {
            LeafStatuses();
        }
        
        private void btnInfoDrafts_Click(object sender, EventArgs e)
        {
            InfoDrafts();
        }

        private void btnDeleteDrafts_Click(object sender, EventArgs e)
        {
            DeleteDrafts();
        }

        private void btnInfoDraftsInEditMode_Click(object sender, EventArgs e)
        {
            InfoDraftsInEditMode();
        }

        private void btnFinishEditing_Click(object sender, EventArgs e)
        {
            FinishEditing();
        }

        private void btnUndoChange_Click(object sender, EventArgs e)
        {
            UndoChange();
        }

        private void btnLeafsInfo_Click(object sender, EventArgs e)
        {
            LeafsInfo();
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            Change();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            Process();
        }

        #endregion

        #region Actions (Private)

        private void Login(Redirector redirector)
        {
            redirector.DoRequest<ProfileService>(service => service.Login("admin", "qwe"));
            redirector.DoRequest<ProfileService>(service => service.SetSysDimensions(new Dictionary<string, int>()
                {
                    {"Budget", 1},
                    {"PublicLegalFormation", 1},
                    {"Version", 1}
                }));
            WriteLine("login ok.");
        }

        private void LeafStatuses()
        {
            WriteLine("Список уникальных листовых статусов для каждого типа документа");
            WriteLine(fix.LeafStatuses()
               .Select(kvp => string.Format("=== {0} ==={1}{2}", kvp.Key, newLine + newLine, kvp.Value))
               .Aggregate(join(newLine)));
        }

        private void DeleteDrafts()
        {
            Dictionary<string, List<int>> ids = InfoDrafts();

            int partitionSize = 1;
            WriteLine(string.Format("Штук в пакете: {0}", partitionSize));
            WriteLine();

            foreach (string entityName in ids.Keys)
            {
                var partitions = getPartitions(ids[entityName], partitionSize);
                WriteLine(string.Format("=== Сущность: {0}, Пакетов к удалению: {1} ===", entityName, partitions.Count));
                int i = 0;

                foreach (List<int> partition in partitions)
                {
                    redirector.DoRequest<CommunicationDataService>(service => service.DeleteItem(new CommunicationContext(), fix.TargetEntities[entityName], partition.ToArray(), null));

                    WriteLine(string.Format("Пакет №{0} из {1}, элементы: {2}",
                        i,
                        partitions.Count,
                        partition.Select(a => a.ToString()).Aggregate(join(", "))
                        ));
                    i++;

                    Thread.Sleep(200);
                }
            }
        }

        private Dictionary<string, List<int>> InfoDrafts()
        {
            Dictionary<string, List<int>> ids = fix.GetDrafts();
            WriteLine("Сущность :: Элементов на черновике с родителем");
            WriteLine(ids
                .Select(kvp => string.Format("{0} :: {1}", kvp.Key, kvp.Value.Count))
                .Aggregate(join(newLine)));
            WriteLine();
            return ids;
        }

        private Dictionary<string, List<int>> InfoDraftsInEditMode()
        {
            var result = fix.GetDraftsInEditMode();

            WriteLine(string.Format("Тип документа :: количество документов на операции Редактировать"));
            WriteLine(result.Keys
                .Select(doc => string.Format("{0} :: {1}", doc, result[doc].Count))
                .Aggregate(join(newLine)));
            return result;
        }

        private void FinishEditing()
        {
            Dictionary<string, List<int>> docs = InfoDraftsInEditMode();

            foreach (string doc in docs.Keys)
            {
                WriteLine(string.Format("=== Документ: {0} ===", doc));
                foreach (int itemId in docs[doc])
                {
                    redirector.DoRequest<OperationsService>(service => service.CancelOperation(new CommunicationContext(), fix.TargetEntities[doc], itemId));
                    WriteLine(itemId.ToString());
                    Thread.Sleep(200);
                }
            }
        }

        private void UndoChange()
        {
            Dictionary<string, int> undoOperations = fix.GetUndoChangeOparations();

            Dictionary<string, List<int>> draftsParents = fix.GetDraftsParents();

            foreach (string docName in draftsParents.Keys)
            {
                WriteLine(string.Format("=== {0}, измененных документов: {1} ===", docName, draftsParents[docName].Count));
                foreach (int itemId in draftsParents[docName])
                {
                    redirector.DoRequest<OperationsService>(service => service.Exec(new CommunicationContext(), fix.TargetEntities[docName], itemId, undoOperations[docName]));
                    WriteLine(itemId.ToString());
                    Thread.Sleep(200);
                }
            }
        }

        private Dictionary<string, List<int>> LeafsInfo()
        {
            Dictionary<string, List<int>> result = fix.LeafDocuments();

            WriteLine(result
                .Select(kvp => string.Format("=== {0}, количество: {1} ==={2}{3}", 
                    kvp.Key, 
                    kvp.Value.Count,
                    newLine, 
                    kvp.Value.Select(v => v.ToString()).DefaultIfEmpty().Aggregate(join(", "))
                ))
                .Aggregate(join(newLine)));
            return result;
        }


        private void Change()
        {
            changeOperations = fix.GetChangeOparations();
            Dictionary<string, List<int>> documents = LeafsInfo();

            WriteLine(newLine);
            estimateDoc = documents.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            estimateTotal = estimateDoc.Sum(kvp => kvp.Value);

            foreach (string docName in documents.Keys)
            {
                if (docName == "LimitBudgetAllocations") //
                {
                    Exec_DocType(docName, documents[docName], changeOperations[docName]);
                    //var t = new Thread(() => Change_DocType(docName, documents[docName], changeOperations[docName]));
                    //t.Start();
                }
            }
        }

        private void Process()
        {
            processOperations = fix.GetProcessOparations();
            Dictionary<string, List<int>> documents = InfoDrafts();

            WriteLine(newLine);
            estimateDoc = documents.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
            estimateTotal = estimateDoc.Sum(kvp => kvp.Value);

            foreach (string docName in documents.Keys)
            {
                if (docName == "PublicInstitutionEstimate") //
                {
                    Exec_DocType(docName, documents[docName], processOperations[docName]);
                }
            }
        }


        #endregion

        #region Tools (Private)

        private void Exec_DocType(string docName, List<int> items, int entityOpId)
        {
            SetText(newLine);
            SetText(string.Format("=== {0}, документов: {1} ===", docName, items.Count));
            foreach (List<int> list in getPartitions(items, (int)Math.Ceiling((double)items.Count / 5)))
            {
                var t = new Thread(() => Exec_Doc(docName, list, entityOpId));
                t.Start();
            }
        }

        private void Exec_Doc(string docName, List<int> items, int entityOpId)
        {
            foreach (var itemId in items)
            {
                var watch = new Stopwatch();
                watch.Start();
                redirector.DoRequest<DeveloperService>(service => service.ExecWithoutControls(new CommunicationContext(), fix.TargetEntities[docName], itemId, entityOpId));
                watch.Stop();

                SetText(string.Format("{0} завершен,  длительность {4}, {1} осталось {2}, всего осталось {3}",
                    itemId.ToString(),
                    docName,
                    --estimateDoc[docName],
                    --estimateTotal,
                    watch.ElapsedMilliseconds
                    ));
            }
        }

        private List<List<T>> getPartitions<T>(List<T> list, int partitionSize)
        {
            List<List<T>> result = new List<List<T>>();
            int partitionIndex = -1;
            int itemIndex = partitionSize;
            foreach (T item in list)
            {
                if (itemIndex >= partitionSize - 1)
                {
                    partitionIndex++;
                    itemIndex = 0;
                    result.Add(new List<T>());
                }

                result[partitionIndex].Add(item);
                itemIndex++;
            }

            return result;
        }

        private void WriteLine(string line = "")
        {
            var msg = line + newLine;
            Debug.WriteLine(msg);
            textBox1.Text += msg;
            textBox1.SelectionStart = textBox1.TextLength;
            textBox1.ScrollToCaret();
            textBox1.Invalidate();
            textBox1.Update();
        }

        private Func<string, string, string> join(string delimeter)
        {
            return (a, b) => string.Format("{0}{1}{2}", a, delimeter, b);
        }

        #endregion


    }
}
