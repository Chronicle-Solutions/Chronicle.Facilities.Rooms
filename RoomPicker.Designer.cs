using System.Windows.Forms;
using System.Drawing;
using Chronicle.Controls;

namespace Chronicle.Facilities.Rooms
{
    partial class RoomPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabbedComboBox1 = new TabbedComboBox();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // tabbedComboBox1
            // 
            tabbedComboBox1.Location = new Point(12, 12);
            tabbedComboBox1.Name = "tabbedComboBox1";
            tabbedComboBox1.Size = new Size(312, 25);
            tabbedComboBox1.TabIndex = 0;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
            listView1.FullRowSelect = true;
            listView1.Location = new Point(12, 43);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(309, 393);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.DoubleClick += listView1_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Room Code";
            columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Room Name";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Building";
            // 
            // button1
            // 
            button1.DialogResult = DialogResult.OK;
            button1.Location = new Point(246, 444);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.DialogResult = DialogResult.Cancel;
            button2.Location = new Point(12, 444);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 3;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            // 
            // RoomPicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(333, 479);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(listView1);
            Controls.Add(tabbedComboBox1);
            Name = "RoomPicker";
            Text = "Room Picker";
            FormClosed += RoomPicker_FormClosed;
            ResumeLayout(false);
        }

        #endregion

        public TabbedComboBox tabbedComboBox1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button button1;
        private Button button2;
        public ListView listView1;
    }
}