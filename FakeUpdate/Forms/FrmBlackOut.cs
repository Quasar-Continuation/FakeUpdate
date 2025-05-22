using FakeUpdate.Keyboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FakeUpdate.Forms
{
    public partial class FrmBlackOut : Form
    {
        public FrmBlackOut()
        {
            InitializeComponent();
            
            this.TopMost = true;
            
            Cursor.Hide();
            
            HookInputEvents(this.Controls);
            
            this.BackColor = Color.Black;
            
            this.FormBorderStyle = FormBorderStyle.None;
            
            this.WindowState = FormWindowState.Maximized;
        }

        private void HookInputEvents(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ctrl.KeyDown += BlockInput;
                ctrl.KeyPress += BlockInput;
                ctrl.KeyUp += BlockInput;
                ctrl.PreviewKeyDown += BlockInput;
                ctrl.MouseDown += BlockInput;
                ctrl.MouseUp += BlockInput;
                ctrl.MouseMove += BlockInput;
                ctrl.MouseWheel += BlockInput;
                ctrl.MouseClick += BlockInput;
                ctrl.MouseDoubleClick += BlockInput;

                if (ctrl.HasChildren)
                    HookInputEvents(ctrl.Controls);
            }
        }

        private void BlockInput(object sender, EventArgs e)
        {
            if (e is KeyEventArgs ke) ke.Handled = true;
            if (e is KeyPressEventArgs kpe) kpe.Handled = true;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            const int WM_CHAR = 0x0102;
            const int WM_SYSKEYDOWN = 0x0104;
            const int WM_SYSKEYUP = 0x0105;
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP = 0x0202;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_RBUTTONUP = 0x0205;
            const int WM_MBUTTONDOWN = 0x0207;
            const int WM_MBUTTONUP = 0x0208;
            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_XBUTTONDOWN = 0x020B;
            const int WM_XBUTTONUP = 0x020C;

            switch (m.Msg)
            {
                case WM_KEYDOWN:
                case WM_KEYUP:
                case WM_CHAR:
                case WM_SYSKEYDOWN:
                case WM_SYSKEYUP:
                case WM_MOUSEMOVE:
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MOUSEWHEEL:
                case WM_XBUTTONDOWN:
                case WM_XBUTTONUP:
                    return;
            }

            base.WndProc(ref m);
        }
    }
}
