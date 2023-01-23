using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using Bunifu.Framework.UI;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using static Plutonium_Mod_Manager.RequestsManager;
using Plutonium_Mod_Manager.Properties;
using SharpCompress.Readers;
using SharpCompress.Common;

namespace Plutonium_Mod_Manager
{
    public partial class Form1 : Form
    {
        //
        // Things for the custom font
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        Font vcrFont;

        //
        // Declare settings file, images folder vars and mod types
        //
        string settingsFile = Assembly.GetEntryAssembly().Location.Replace(AppDomain.CurrentDomain.FriendlyName, "config.json");
        string modsImgFolder = Assembly.GetEntryAssembly().Location.Replace(AppDomain.CurrentDomain.FriendlyName, "ModsThumbnails");

        enum ModType
        {
            Skin = 0,
            Script = 1,
            ModPack = 2
        }

        //
        // Declaring some functions
        //
        private List<Control> GetAllControls(Control container, List<Control> list)
        {
            foreach (Control c in container.Controls)
            {

                if (c.Controls.Count > 0)
                    list = GetAllControls(c, list);
                else
                    list.Add(c);
            }

            return list;
        }

        private List<Control> GetAllControls(Control container)
        {
            return GetAllControls(container, new List<Control>());
        }

        void setCards()
        {
            //
            // Clear current cards
            //
            flowLayoutPanel1.Controls.Clear();

            //
            // Getting mod json list
            //
            JObject mods = getModList(@"https://raw.githubusercontent.com/radsialt/plutonium-t6-mods-db/main/modsDB.json");

            List<JToken> lateCards = new List<JToken>();

            //
            // Download images
            //
            foreach (var mod in mods["cards"])
            {
                string imgPath = Path.Combine(modsImgFolder, $"{(string)mod["name"]}.png");

                if ((string)mod["thumbnail"] != "none" && (string)mod["thumbnail"] != "" && (string)mod["thumbnail"] != null && !File.Exists(imgPath))
                {
                    if (!Directory.Exists(modsImgFolder)) Directory.CreateDirectory(modsImgFolder);

                    webClient.DownloadFileAsync(new Uri((string)mod["thumbnail"]), imgPath);
                }
            }

            //
            // Storing not recommended cards for later
            //
            foreach (var mod in mods["cards"])
            {
                if (!(bool)mod["recommended"])
                {
                    lateCards.Add(mod);
                    continue;
                }

                createCard(mod);
            }


            //
            // Create recommended mods card
            //
            foreach (var mod in lateCards)
            {
                createCard(mod);
            }

            //
            // Set text size and font
            //
            List<Control> allControls = GetAllControls(this);
            allControls.ForEach(k =>
            {
                k.Font = vcrFont;

                if (k.Tag != null)
                {
                    switch (k.Tag.ToString())
                    {
                        case "h1":
                            k.Font = new Font(fonts.Families[0], 15.0F);
                            break;
                    }
                }
            });
        }

        void createCard(JToken mod)
        {
            Image thumbnail = Resources.none;
            string imgPath = Path.Combine(modsImgFolder, $"{(string)mod["name"]}.png");
            int cardCount = 0;

            foreach (Control card in flowLayoutPanel1.Controls)
            {
                cardCount++;
            }

            //
            // images
            //
            try
            {
                if (File.Exists(imgPath) && new FileInfo(imgPath).Length > 0)
                {
                    thumbnail = Image.FromFile(imgPath);
                }
                else
                {
                    if ((string)mod["thumbnail"] != "none" && (string)mod["thumbnail"] != "" && (string)mod["thumbnail"] != null)
                    {
                        if (!Directory.Exists(modsImgFolder)) Directory.CreateDirectory(modsImgFolder);

                        webClient.DownloadFileAsync(new Uri((string)mod["thumbnail"]), imgPath);
                    }
                }
            }
            catch (OutOfMemoryException e)
            {
                webClient.DownloadFileAsync(new Uri((string)mod["thumbnail"]), imgPath);
            }

            BunifuCards newCard = new BunifuCards();
            Label label3 = new Label();
            Label label2 = new Label();
            Label label1 = new Label();
            Label label0 = new Label();

            //
            // Card template
            //
            newCard.BackColor = Color.Transparent;
            newCard.BackgroundImageLayout = ImageLayout.Zoom;
            newCard.BorderRadius = 3;
            newCard.BorderStyle = BorderStyle.FixedSingle;
            newCard.BottomSahddow = true;
            newCard.color = (bool)mod["recommended"] ? Color.Gold : Color.Gray;
            newCard.Controls.Add(label3);
            newCard.Controls.Add(label2);
            newCard.Controls.Add(label1);
            newCard.Controls.Add(label0);
            newCard.Cursor = Cursors.Hand;
            newCard.ForeColor = Color.Transparent;
            newCard.LeftSahddow = true;
            newCard.Margin = new Padding(0);
            newCard.Name = "card" + cardCount;
            newCard.RightSahddow = true;
            newCard.ShadowDepth = 0;
            newCard.Size = new Size(395, 495);
            newCard.TabIndex = 0;
            newCard.Click += new EventHandler((sender, e) => clickCard(sender, e, (string)mod["url"], (string)mod["name"], (string)mod["game"], (string)mod["gamemode"], (ModType)Enum.ToObject(typeof(ModType), (int)mod["modtype"])));
            newCard.Paint += new PaintEventHandler((sender, e) => drawCard(sender, e, thumbnail));
            // 
            // Mod name
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, newCard.Location.Y + 230);
            label3.Name = "label3" + cardCount;
            label3.Size = new Size(287, 29);
            label3.TabIndex = 3;
            label3.Tag = "h1";
            label3.Text = (string)mod["name"];
            // 
            // Mod contributors
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, newCard.Location.Y + 270);
            label2.Name = "label2" + cardCount;
            label2.Size = new Size(94, 29);
            label2.TabIndex = 2;
            label2.Tag = "h1";
            label2.Text = (string)mod["contributors"];
            // 
            // Mod description
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, newCard.Location.Y + 330);
            label1.Name = "label1" + cardCount;
            label1.Size = new Size(350, 29);
            label1.TabIndex = 1;
            label1.Text = (string)mod["description"];
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Mod game
            // 
            label0.AutoSize = true;
            label0.Location = new Point(10, newCard.Location.Y + 450);
            label0.Name = "label0" + cardCount;
            label0.Size = new Size(287, 29);
            label0.TabIndex = 0;
            label0.Text = $"Made for: {(string)mod["game"]}, {(string)mod["gamemode"]}";
            label0.TextAlign = ContentAlignment.MiddleRight;

            //
            // Some badly done responsive thing
            //
            if (label3.Text.Contains("\n"))
            {
                label2.Location = new Point(10, label2.Location.Y + 20);
                label1.Location = new Point(10, label1.Location.Y + 20);
            }

            if (label2.Text.Contains("\n"))
            {
                label1.Location = new Point(10, label1.Location.Y + 20);
            }

            flowLayoutPanel1.Controls.Add(newCard);
        }

        /*private void Form1_Resize(object sender, EventArgs e)
        {
            bunifuVScrollBar1.Size = new Size(69, flowLayoutPanel1.Height);
        }*/

        public Form1()
        {
            InitializeComponent();

            //
            // Import custom font
            //
            byte[] fontData = Resources.vcr;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Resources.vcr.Length);
            AddFontMemResourceEx(fontPtr, (uint)Resources.vcr.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            vcrFont = new Font(fonts.Families[0], 13.0F);

            flowLayoutPanel1.Location = new Point(12, 81);

            //
            // Set cards
            //
            setCards();
        }

        //
        // Set input text value
        //
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(settingsFile))
            {
                bunifuTextBox1.Text = (string)JObject.Parse(File.ReadAllText(settingsFile))["DP"];
            }
            else
            {
                bunifuTextBox1.Text = $@"C:\Users\{System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1]}\AppData\Local\Plutonium\storage";
            }
        }

        //
        // Window borders
        //
        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.FromArgb(44, 47, 51), ButtonBorderStyle.Solid);
        }

        //
        // Saving path changes
        //
        private void bunifuTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!bunifuTextBox1.Text.Contains(@"\Plutonium\storage"))
            {
                MessageBox.Show("Provide a valid game folder");
            }
            else
            {
                JObject data = new JObject(
                    new JProperty("DP", bunifuTextBox1.Text)
                );

                File.WriteAllText(settingsFile, data.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (!dlg.SelectedPath.Contains(@"\Plutonium\storage"))
                {
                    MessageBox.Show("Provide a valid game folder");
                }
                else
                {
                    bunifuTextBox1.Text = dlg.SelectedPath;

                    JObject data = new JObject(
                        new JProperty("DP", bunifuTextBox1.Text)
                    );

                    File.WriteAllText(settingsFile, data.ToString());
                }
            }
        }

        //
        // Reload button
        //
        private void button4_Click(object sender, EventArgs e)
        {
            setCards();
        }

        private void drawCard(object sender, PaintEventArgs e, Image thumbnail)
        {
            e.Graphics.DrawImage(thumbnail, new Rectangle(0, 0, 400, 210));
        }

        //
        // Install mods by clicking card
        //
        private void clickCard(object sender_, EventArgs e_, string url, string modName, string game, string gamemode, ModType modType)
        {

            if (!bunifuTextBox1.Text.Contains(@"\Plutonium\storage"))
            {
                MessageBox.Show("Provide a valid game folder.");
                return;
            }

            string fileFullName = url.Split('/')[url.Split('/').Length - 1];
            string filePath = Path.Combine(bunifuTextBox1.Text, $"\\{game}", modName + fileFullName.Substring(fileFullName.Length - 4, 4));

            if (modType == ModType.ModPack)
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => installCompressedMod(sender, e, filePath, game, gamemode, modType));
                webClient.DownloadFileAsync(new Uri(url), filePath);  
                MessageBox.Show($"{modName} installed");
            }
            else
            {
                string extractPath = "";
                switch (modType)
                {
                    case ModType.Skin:
                        extractPath = Path.Combine(bunifuTextBox1.Text, game, "images");
                        break;
                    case ModType.Script:
                        extractPath = $@"{bunifuTextBox1.Text}\scripts\";
                        break;
                }

                if(url.EndsWith(".rar") || url.EndsWith(".zip"))
                {
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => installCompressedMod(sender, e, extractPath + modName + fileFullName.Substring(fileFullName.Length - 4, 4), game, gamemode, modType));
                }
                else
                {
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => installFiledMod(sender, e, extractPath + modName + fileFullName.Substring(fileFullName.Length - 4, 4), game, gamemode, modType));
                }

                webClient.DownloadFileAsync(new Uri(url), extractPath + modName + fileFullName.Substring(fileFullName.Length - 4, 4));
                MessageBox.Show($"{modName} installed");
            }
        }

        //
        // Functions for the install process
        //
        void installFiledMod(object sender, AsyncCompletedEventArgs e, string filePath, string game, string gamemode, ModType modType)
        {
            string extractPath = "";
            string modGamePath = Path.Combine(bunifuTextBox1.Text, game);

            using (Stream stream = File.OpenRead(filePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory)
                    {
                        extractPath = modGamePath;
                    }
                    else
                    {
                        switch (modType)
                        {
                            case ModType.Skin:
                                extractPath = $@"{modGamePath}\images\";
                                break;
                            case ModType.Script:
                                extractPath = $@"{modGamePath}\scripts\{gamemode}";
                                break;
                        }
                    }

                    reader.WriteEntryToDirectory(extractPath, new ExtractionOptions()
                    {
                        Overwrite = true
                    });
                }
            }
            File.Delete(filePath);
        }

        void installCompressedMod(object sender, AsyncCompletedEventArgs e, string filePath, string game, string gamemode, ModType modType)
        {
            string extractPath = "";
            string modGamePath = Path.Combine(bunifuTextBox1.Text, game);

            using (Stream stream = File.OpenRead(filePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (modType == ModType.ModPack)
                    {
                        reader.WriteAllToDirectory(modGamePath, new ExtractionOptions()
                        {
                            Overwrite = true
                        });
                    }
                    else
                    {
                        if (reader.Entry.IsDirectory)
                        {
                            extractPath = bunifuTextBox1.Text;
                        }
                        else
                        {
                            switch (modType)
                            {
                                case ModType.Skin:
                                    extractPath = $@"{modGamePath}\images";
                                    break;
                                case ModType.Script:
                                    extractPath = $@"{modGamePath}\scripts\{gamemode}";
                                    break;
                            }
                        }

                        reader.WriteEntryToDirectory(extractPath, new ExtractionOptions()
                        {
                            Overwrite = true
                        });
                    }
                }         
            }
            File.Delete(filePath);
        }
    }
}
