using System.Windows.Forms;

namespace MaciX.UI.MyControl
{
    public class LimitedSelectionListBox : ListBox
    {
        private bool adminMode = false;  //true:选项可选，false:选项不可选

        public bool AdminMode
        {
            get { return adminMode; }
            set
            {
                adminMode = value;
                UpdateControl();
            }
        }

        public LimitedSelectionListBox()
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            UpdateControl();
        }

        protected override void WndProc(ref Message m)
        {
            if (adminMode)
            {
                base.WndProc(ref m);
            }
            else
            {
                //禁止键盘和鼠标事件
                const int WM_LBUTTONDOWN = 0x0201;
                const int WM_LBUTTONDBLCLK = 0x0203;
                const int WM_KEYDOWN = 0x0100;
                const int WM_KEYUP = 0x0101;

                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP)
                {
                    return;
                }

                base.WndProc(ref m);
            }
        }

        private void UpdateControl()
        {
            if (adminMode)
            {
                SetStyle(ControlStyles.Selectable, true);
            }
            else
            {
                SetStyle(ControlStyles.Selectable, false);
                SelectedIndex = -1;
            }
        }
    }
}
