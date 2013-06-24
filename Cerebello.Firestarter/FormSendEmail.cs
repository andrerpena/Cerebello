using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml.Serialization;
using Cerebello.Firestarter.Parser;
using Cerebello.Model;
using CerebelloWebRole.Code;

namespace Cerebello.Firestarter
{
    public partial class FormSendEmail : Form
    {
        private FormSendEmailConfigData configData;
        private EmailData[] emailData;

        private FilterObject filterObject;

        public class EmailData
        {
            private CerebelloEntities db;

            public EmailData(CerebelloEntities db, User u)
            {
                this.db = db;
                this.ObjectData = u;
                this.EmailAddress = StringHelper.FirstNonEmpty(u.Person.Email, u.Practice.Email);
            }

            public string EmailAddress { get; set; }

            [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
            public User ObjectData { get; set; }

            public string CreateAccountToken
            {
                get
                {
                    var user = this.ObjectData as User;
                    if (user == null)
                        return "";

                    var tokenName = string.Format("Practice={0}&UserName={1}", user.Practice.UrlIdentifier, user.UserName);
                    var token = this.db.GLB_Token.FirstOrDefault(tok => tok.Name == tokenName && tok.Type == "VerifyPracticeAndEmail");
                    if (token != null)
                    {
                        var tokenId = new TokenId(token.Id, token.Value);
                        return tokenId.ToString();
                    }

                    return "";
                }
            }

            public string BaseUrl
            {
                get { return "https://www.cerebello.com.br"; }
            }

            [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
            public Consts Constants
            {
                get { return new Consts(); }
            }

            public override string ToString()
            {
                return string.Format("{0}", this.EmailAddress);
            }
        }

        public class Consts
        {
            [Description("Gets the current operator name. This configuration must be placed in the 'Uncommited.config' file.")]
            public string OperatorName
            {
                get { return ConfigurationManager.AppSettings["OperatorName"]; }
            }
        }

        public FormSendEmail(FormSendEmailConfigData configData, EmailData[] emailData)
        {
            this.InitializeComponent();

            this.configData = configData;
            this.emailData = emailData;

            this.checkedListBoxClients.Items.AddRange(emailData);

            this.propertyGridFilter.SelectedObject = this.filterObject = new FilterObject();

            this.toolStripComboBoxModels.Items.AddRange(configData.EmailModels.Items.ToArray());

            this.textBoxSubject.Enter += textBox_Enter_RecordLastFocus;
            this.textBoxTextContent.Enter += textBox_Enter_RecordLastFocus;
            this.textBoxHtmlContent.Enter += textBox_Enter_RecordLastFocus;
        }

        private TextBox lastFocused;

        void textBox_Enter_RecordLastFocus(object sender, EventArgs e)
        {
            this.lastFocused = (TextBox)sender;
        }

        public FormSendEmailConfigData ConfigData
        {
            get { return this.configData; }
        }

        private void checkedListBoxClients_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.propertyGridEmailData.SelectedObject = null;

            if (checkedListBoxClients.SelectedIndex >= 0)
            {
                var currentObj = checkedListBoxClients.Items[checkedListBoxClients.SelectedIndex];
                if (currentObj != null)
                {
                    this.propertyGridEmailData.SelectedObject = currentObj;
                }
            }
        }

        private void toolStripButtonFilterGo_Click(object sender, EventArgs e)
        {
            var filteredItems = this.checkedListBoxClients.Items.Cast<EmailData>()
                .Select(ed => this.filterObject.Filter(ed))
                .ToArray();

            for (int it = 0; it < filteredItems.Length; it++)
                this.checkedListBoxClients.SetItemChecked(it, filteredItems[it]);
        }

        public class FilterObject
        {
            public int? PracticeId { get; set; }

            public bool Filter(EmailData emailData)
            {
                var dynObj = (dynamic)emailData.ObjectData;
                var results = new List<bool>(50);

                if (this.PracticeId != null)
                    results.Add(this.PracticeId == dynObj.PracticeId);

                return results.All(b => b);
            }
        }

        private void toolStripComboBoxModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            var model = (FormSendEmailConfigData.EmailModelData)this.toolStripComboBoxModels.SelectedItem;
            this.textBoxHtmlContent.Text = model.HtmlContent;
            this.textBoxTextContent.Text = model.TextContent;
            this.textBoxSubject.Text = model.Title;
        }

        private void toolStripButtonAddModel_Click(object sender, EventArgs e)
        {
            var newName = this.toolStripComboBoxModels.Text;

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Must define a name for the new model.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var namesDic = this.toolStripComboBoxModels.Items
                .Cast<FormSendEmailConfigData.EmailModelData>()
                .Where(m => m.Name != null)
                .ToDictionary(m => m.Name);

            FormSendEmailConfigData.EmailModelData model;
            if (namesDic.ContainsKey(newName))
            {
                if (MessageBox.Show(
                    string.Format("Replace the model {0}", newName),
                    "Replace model?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                model = namesDic[newName];
            }
            else
            {
                if (this.configData == null)
                    this.configData = new FormSendEmailConfigData();
                if (this.configData.EmailModels == null)
                    this.configData.EmailModels = new FormSendEmailConfigData.EmailModelsData();
                if (this.configData.EmailModels.Items == null)
                    this.configData.EmailModels.Items = new List<FormSendEmailConfigData.EmailModelData>();

                model = new FormSendEmailConfigData.EmailModelData();
                model.Name = newName;
                this.configData.EmailModels.Items.Add(model);
                this.toolStripComboBoxModels.Items.Add(model);
            }

            model.HtmlContent = this.textBoxHtmlContent.Text;
            model.TextContent = this.textBoxTextContent.Text;
            model.Title = this.textBoxSubject.Text;
        }

        private void toolStripButtonRemoveModel_Click(object sender, EventArgs e)
        {
            var delName = this.toolStripComboBoxModels.Text;

            if (string.IsNullOrWhiteSpace(delName))
            {
                MessageBox.Show("Must indicate the name of the model to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var namesDic = this.toolStripComboBoxModels.Items
                .Cast<FormSendEmailConfigData.EmailModelData>()
                .ToDictionary(m => m.Name);

            if (namesDic.ContainsKey(delName))
            {
                if (MessageBox.Show(
                    string.Format("Delete the model {0}", delName),
                    "Delete model?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                var model = namesDic[delName];
                this.configData.EmailModels.Items.Remove(model);
                this.toolStripComboBoxModels.Items.Remove(model);
            }
        }

        private void toolStripButtonPasteProperty_Click(object sender, EventArgs e)
        {
            var gridItem = this.propertyGridEmailData.SelectedGridItem;
            var path = gridItem.GetHierarchy().Select(gi => gi.Label).ToArray();

            if (path.Length > 2)
                this.lastFocused.SelectedText = "{{" + string.Join(".", path.Skip(2)) + "}}";
        }

        private void toolStripButtonSendEmail_Click(object sender, EventArgs e)
        {
            var model = new FormSendEmailConfigData.EmailModelData();
            model.HtmlContent = this.textBoxHtmlContent.Text;
            model.TextContent = this.textBoxTextContent.Text;
            model.Title = this.textBoxSubject.Text;

            bool ok = false;
            int emailsSent = 0;
            try
            {
                using (DebugConfig.SetDebug(this.toolStripButtonDEBUG.Checked))
                {
                    var checkedIndices = this.checkedListBoxClients.CheckedIndices.Cast<int>().ToArray();
                    for (int it = 0; it < checkedIndices.Length; it++)
                    {
                        var checkedItemIndex = checkedIndices[it];
                        var checkedItem = this.checkedListBoxClients.Items[checkedItemIndex];
                        var user = ((EmailData)checkedItem).ObjectData as User;
                        if (user != null)
                        {
                            var toAddress = new MailAddress(user.Person.Email, user.Person.FullName);
                            var subject = ProcessTemplate(model.Title, checkedItem, false);
                            var bodyText = ProcessTemplate(model.TextContent, checkedItem, false);
                            var bodyHtml = ProcessTemplate(model.HtmlContent, checkedItem, true);
                            var emailMessage = EmailHelper.CreateEmailMessage(toAddress, subject, bodyText, bodyHtml);
                            EmailHelper.SendEmail(emailMessage);

                            emailsSent++;
                            if (!this.toolStripButtonDEBUG.Checked)
                                this.checkedListBoxClients.SetItemChecked(checkedItemIndex, false);
                        }
                    }
                }

                ok = true;
            }
            catch { }

            if (ok) MessageBox.Show("E-mails were sent.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else MessageBox.Show(string.Format("An error has occurred. Sent: {0}", emailsSent), "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string ProcessTemplate(string template, object data, bool htmlEncode)
        {
            var result = Regex.Replace(template, @"{{(.*?)}}", m => MatchEval(m, data, htmlEncode));
            return result;
        }

        private static string MatchEval(Match m, object data, bool htmlEncode)
        {
            var parser = new SimpleParser(m.Groups[1].Value) { GlobalType = data.GetType() };
            var valueExecutor = parser.Read<TemplateParser.ValueBuilder, TemplateParser.INode>();
            var type = valueExecutor.Compile(parser.GlobalType);
            var value = valueExecutor.Execute(data);
            var result = value.ToString();
            if (htmlEncode)
                result = HttpUtility.HtmlEncode(result);

            return result;
        }

        private void toolStripButtonDEBUG_Click(object sender, EventArgs e)
        {
            this.toolStripButtonDEBUG.Checked = !this.toolStripButtonDEBUG.Checked;
        }
    }

    internal static class PropertyGridExtensions
    {
        public static GridItemCollection GetAllGridEntries(this PropertyGrid grid)
        {
            var view = grid.GetPropertyGridView();
            return (GridItemCollection)view.GetType().InvokeMember("GetAllGridEntries", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, view, null);
        }

        public static Control GetPropertyGridView(this PropertyGrid grid)
        {
            if (grid == null)
                throw new ArgumentNullException("grid");

            var view = grid.Controls.Cast<Control>().Single(c => c.GetType().Name == "PropertyGridView");

            return view;
        }

        public static void UpdatePropertyGrid(this PropertyGrid pg)
        {
            var expandedGridItemPaths = new HashSet<string>(
                pg
                    .GetAllGridEntries()
                    .Cast<GridItem>()
                    .Where(gi1 => gi1.Expanded)
                    .Select(gi1 => string.Join("/", gi1.GetHierarchy().Select(gi2 => gi2.Label))));

            var allItems = pg.GetAllGridEntries()
                .Cast<GridItem>()
                .ToList();

            var selectedGridItemIndex = allItems.IndexOf(pg.SelectedGridItem);

            var selectedGridItemPath = string.Join("/", pg.SelectedGridItem.GetHierarchy().Select(gi2 => gi2.Label));

            var view = pg.GetPropertyGridView();
            var scrollBar = (ScrollBar)view.GetType().GetField("scrollBar", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
            var scroll = scrollBar.Value;

            var oldSelected = pg.SelectedObject;
            pg.SelectedObject = null;
            pg.SelectedObject = oldSelected;

            var newExpandableGridItems = new Queue<GridItem>(
                pg
                    .GetAllGridEntries()
                    .Cast<GridItem>()
                    .Where(gi => gi.Expandable || gi.GridItems.Count > 0));

            bool anySelected = false;

            while (newExpandableGridItems.Count > 0)
            {
                var gi = newExpandableGridItems.Dequeue();

                var path = string.Join("/", gi.GetHierarchy().Select(gi1 => gi1.Label));

                if (expandedGridItemPaths.Contains(path))
                    gi.Expanded = true;

                if (path == selectedGridItemPath)
                {
                    gi.Select();
                    anySelected = true;
                }

                foreach (var child in gi.GridItems.Cast<GridItem>())
                    newExpandableGridItems.Enqueue(child);
            }

            if (!anySelected)
            {
                var allItems2 = pg.GetAllGridEntries()
                    .Cast<GridItem>()
                    .ToList();

                var index = Math.Min(selectedGridItemIndex, allItems2.Count);
                allItems2[index].Select();
            }

            scrollBar.Value = scroll;
        }
    }

    internal static class ObjectExtensions
    {
        public static IEnumerable<T> GetHierarchy<T>(this T item, Func<T, T> parentGetter) where T : class
        {
            var stack = new Stack<T>();
            var current = item;

            while (current != null)
            {
                stack.Push(current);
                current = parentGetter(current);
            }

            return stack;
        }
        public static IEnumerable<T> GetHierarchy<T>(this T item) where T : class
        {
            var stack = new Stack<T>();
            dynamic current = item;

            while (current != null)
            {
                stack.Push(current);
                current = current.Parent as T;
            }

            return stack;
        }
    }

    public class FormSendEmailConfigData
    {
        public class EmailModelData
        {
            [XmlAttribute("name")]
            [Browsable(false)]
            public string Name { get; set; }

            [XmlElement("title")]
            public string Title { get; set; }

            [XmlElement("textContent")]
            [Browsable(false)]
            public string TextContent { get; set; }

            [XmlElement("htmlContent")]
            [Browsable(false)]
            public string HtmlContent { get; set; }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class EmailModelsData
        {
            [XmlElement("emailModel")]
            public List<EmailModelData> Items { get; set; }
        }

        [XmlElement("emailModels")]
        public EmailModelsData EmailModels { get; set; }
    }
}
