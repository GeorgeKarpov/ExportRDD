using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using ExpPt1;

namespace Refact
{
    public static class Utils
    {
        public static void ShowErrList(string errPath, string caption, string message, Icon icon, bool modal)
        {
            System.Windows.Forms.Form frmErrors = new System.Windows.Forms.Form
            {
                Text = caption
            };
            ErrCntrl err = new ErrCntrl();

            err.BtnLoad.Visible = false;
            err.ListView.Dock = System.Windows.Forms.DockStyle.Fill;
            err.Dock = System.Windows.Forms.DockStyle.Fill;
            ErrLogger.Configure(logDirTmp: errPath);
            err.LoadList();
            PicturePanel panel = new PicturePanel
            {
                BackgroundImage = icon.ToBitmap(),
                Dock = System.Windows.Forms.DockStyle.Bottom
            };
            frmErrors.Controls.Add(panel);
            frmErrors.Controls.Add(err);
            System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
            lbl.Dock = System.Windows.Forms.DockStyle.Top;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Bold);
            if (icon == SystemIcons.Error)
            {
                lbl.ForeColor = Color.Red;
            }
            else
            {
                lbl.ForeColor = Color.Black;
            }
            lbl.Text = message;
            frmErrors.Controls.Add(lbl);
            err.BringToFront();
            if (modal)
            {
                Application.ShowModalDialog(null, frmErrors, true);
            }
            else
            {
                Application.ShowModelessDialog(null, frmErrors, true);
            }
        }
    }

    internal class PicturePanel : System.Windows.Forms.Panel
    {
        System.Windows.Forms.Button btnOK;
        public PicturePanel()
        {
            this.Height = 34;
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnOK = new System.Windows.Forms.Button
            {
                Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right),
                DialogResult = System.Windows.Forms.DialogResult.OK
            };
            this.btnOK.Location = new Point(this.Width - btnOK.Width - 20 , 4);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 27);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            //this.Controls.Add(btnOK);
        }
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set
            {
                base.BackgroundImage = value;
                if (value != null) this.AutoScrollMinSize = value.Size;
            }
        }
    }
}
