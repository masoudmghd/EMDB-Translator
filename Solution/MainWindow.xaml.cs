using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using EMDB_Translator.Model;

namespace EMDB_Translator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string path = @"C:\Program Files (x86)\EMDB\languages\";
        string source = "English.lng";
        string version = "";
        string name = "";

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            path = @"C:\Program Files (x86)\EMDB\languages\";
            source = "English.lng";

            if (!File.Exists($"{path}{source}"))
            {
                path = @"C:\Program Files\EMDB\languages\";
                if (!File.Exists($"{path}{source}"))
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        dialog.Description = "Please Select EMDB Folder...";
                        DialogResult result = dialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return;
                        }
                        path = $"{dialog.SelectedPath}\\languages\\";
                    }
                    if (!File.Exists($"{path}{source}"))
                    {
                        System.Windows.MessageBox.Show("Wrong Folder!");
                        return;
                    }
                }
            }

            var fileList = Directory.GetFiles(path);
            List<string> langList = new List<string>();

            foreach (var item in fileList)
            {
                var tmp = item.Replace(path, "");
                langList.Add(tmp.Replace(".lng", ""));
            }


            source = $"{path}{source}";

            cmbLang.ItemsSource = langList;
            cmbLang.Text = "Select Your Language...";
            //cmbLang.SelectedItem = "English";

            //var dest = $"{path}{cmbLang.SelectedItem}.lng";
            Process(source, null);
            btnLoad.IsEnabled = false;
            btnSave.IsEnabled = true;
            cmbLang.IsEnabled = true;
            txtVersion.IsEnabled = true;
            txtDestVersion.IsEnabled = true;
            textName.IsEnabled = true;
        }

        private List<TranslationModel> ParseValues(string[] values, bool dest)
        {
            List<TranslationModel> translations = new List<TranslationModel>();
            string[] itemSplited;
            string[] idSplited = new string[] { "" };
            foreach (var item in values)
            {
                if (item.Contains("=") & !item.Contains("'='"))
                {
                    itemSplited = item.Split('=');
                    if (itemSplited[0].Contains("->"))
                    {
                        idSplited = itemSplited[0].Split('>');
                        itemSplited[0] = idSplited[1];
                    }
                    if (idSplited[0] != "REMOVED-")
                    {
                        translations.Add(new TranslationModel()
                        {
                            Id = itemSplited[0],
                            Source = itemSplited[1],
                            Info = idSplited[0]
                        }
                        );
                    }
                    idSplited[0] = null;
                }
                else if (item.Contains("[") || item == "")
                {
                    translations.Add(new TranslationModel()
                    {
                        Source = item
                    }
                    );
                }
                else if (item.Contains("Version: "))
                {
                    var versionTmp = item.Replace("; Version: ", "");
                    if (dest == true)
                    {
                        txtDestVersion.Text = versionTmp;
                    }
                    else
                    {
                        txtVersion.Text = versionTmp;
                        version = versionTmp;
                    }
                }
                else if (item.Contains("Translated by: "))
                {
                    if (dest == true)
                    {
                        name = item.Replace("; Translated by: ", "");
                        textName.Text = name;
                    }
                }
            }
            return translations;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string language = cmbLang.SelectedItem.ToString();
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            var items = datagridmain.Items.OfType<TranslationModel>();
            var exportList = new List<string>();

            string exportPath = $"{cmbLang.SelectedItem}.lng";

            if (!(textName.Text == name) & !(textName.Text == "Your Name"))
            {
                name = textName.Text;
            }

            exportList.Add("; -------------------------------------------------------------------------------");
            exportList.Add("; EMDB Language file.");
            exportList.Add($"; Language: {language}");
            exportList.Add($"; Version: {version}");
            exportList.Add($"; Date: {date}");
            exportList.Add($"; Translated by: {name}");
            exportList.Add("; -------------------------------------------------------------------------------");
            exportList.Add("");
            foreach (var item in items)
            {
                if (item.Id == null)
                {
                    exportList.Add(item.Source);
                }
                else if (item.Translation != null)
                {
                    exportList.Add($"{item.Id}={item.Translation}");
                }

            }
            File.WriteAllLines(exportPath, exportList);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dest = $"{path}{cmbLang.SelectedItem}.lng";
            Process(source, dest);
        }

        private void Process(string source, string dest)
        {
            var sourceValues = File.ReadAllLines(source);
            var sourceTranslations = ParseValues(sourceValues, false);

            if (dest != null)
            {
                if (!File.Exists(dest))
                {
                    System.Windows.MessageBox.Show("Destination language is not available...");
                    return;
                }
                var destValues = File.ReadAllLines(dest);
                var destTranslations = ParseValues(destValues, true);

                foreach (var sourceItem in sourceTranslations)
                {
                    foreach (var destItem in destTranslations)
                    {
                        if (sourceItem.Id != null)
                        {
                            if (sourceItem.Id == destItem.Id)
                            {
                                sourceItem.Translation = destItem.Source;
                            }
                        }
                    }
                }
            }
            datagridmain.AutoGenerateColumns = false;
            datagridmain.ItemsSource = sourceTranslations;
        }
    }
}
