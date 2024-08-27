using System;
using System.Runtime.InteropServices;

namespace Base
{
    //数据转换使用
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public partial class UIT //小端模式
    {
        [FieldOffset(0)]
        private Byte b0;
        [FieldOffset(1)]
        private Byte b1;
        [FieldOffset(2)]
        private Byte b2;
        [FieldOffset(3)]
        private Byte b3;

        [FieldOffset(0)]
        private Int16 s;

        [FieldOffset(0)]
        private UInt16 us;

        [FieldOffset(0)]
        private Int32 i;

        [FieldOffset(0)]
        private UInt32 ui;

        [FieldOffset(0)]
        private float f;

        //
        #region set and get
        //

        public Byte B0 //LSB
        {
            set
            {
                b0 = value;
            }
            get
            {
                return b0;
            }
        }
        public Byte B1
        {
            set
            {
                b1 = value;
            }
            get
            {
                return b1;
            }
        }
        public Byte B2
        {
            set
            {
                b2 = value;
            }
            get
            {
                return b2;
            }
        }
        public Byte B3 //MSB
        {
            set
            {
                b3 = value;
            }
            get
            {
                return b3;
            }
        }
        public Int16 S
        {
            set
            {
                s = value;
            }
            get
            {
                return s;
            }
        }

        public UInt16 US
        {
            set
            {
                us = value;
            }
            get
            {
                return us;
            }
        }
        public Int32 I
        {
            set
            {
                i = value;
            }
            get
            {
                return i;
            }
        }
        public UInt32 UI
        {
            set
            {
                ui = value;
            }
            get
            {
                return ui;
            }
        }
        public float F
        {
            set
            {
                f = value;
            }
            get
            {
                return f;
            }
        }

        //
        #endregion
        //
    }
}
//end