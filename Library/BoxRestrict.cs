using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

//Tong Ziyun 20230417
//Alvin 20230505
//Alvin 20230523

//Junzhe 20230906
//修改了KeyPress_RationalNumber，工单输入力矩上下限使用

namespace Library
{
    public class BoxRestrict
    {
        #region 数字

        /// <summary>
        /// 只允许输入整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Integer(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字、负号和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '-') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //负数符号只能出现在首位
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.Length > 0))
            {
                e.Handled = true;
                return;
            }

            //负号只能输入一次
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.IndexOf('-') != -1))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，则只能输入删除键
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为负号，则不允许输入0
            if ((e.KeyChar == '0') && (((TextBox)sender).Text == "-"))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入整数
        /// 限制长度5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Integer_len5(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字、负号和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '-') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //负数符号只能出现在首位
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.Length > 0))
            {
                e.Handled = true;
                return;
            }

            //负号只能输入一次
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.IndexOf('-') != -1))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，则只能输入删除键
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为负号，则不允许输入0
            if ((e.KeyChar == '0') && (((TextBox)sender).Text == "-"))
            {
                e.Handled = true;
                return;
            }

            //长度限制5
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 4)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入整数
        /// 限制长度10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Integer_len10(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字、负号和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '-') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //负数符号只能出现在首位
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.Length > 0))
            {
                e.Handled = true;
                return;
            }

            //负号只能输入一次
            if ((e.KeyChar == '-') && (((TextBox)sender).Text.IndexOf('-') != -1))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，则只能输入删除键
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为负号，则不允许输入0
            if ((e.KeyChar == '0') && (((TextBox)sender).Text == "-"))
            {
                e.Handled = true;
                return;
            }

            //长度限制5
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 10)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }
            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len2(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制2
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 2)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len3(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制3
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 3)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len4(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制4
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 4)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len5(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制4
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 5)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len6(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制6
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 6)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制7
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len7(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制7
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 7)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入正整数
        /// 长度限制10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_IntegerPositive_len10(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            // 如果第一位为0，且输入的不是删除键，则不允许输入
            if ((e.KeyChar != 8) && (((TextBox)sender).Text == "0"))
            {
                e.Handled = true;
                return;
            }

            //长度限制10
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 10)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入有理数和逗号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_RationalNumber(object sender, KeyPressEventArgs e)
        {
            //获取字符串
            string text = ((TextBox)sender).Text;

            //只允许输入数字,负号,正号,e,E,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '-') && (e.KeyChar != '+') && (e.KeyChar != 'e') && (e.KeyChar != 'E') && (e.KeyChar != '.') && (e.KeyChar != 8) && (e.KeyChar != ','))
            {
                e.Handled = true;
                return;
            }

            //负数、正号符号只能出现在首位或者e/E后
            if ((e.KeyChar == '-' || e.KeyChar == '+') && text.Length > 0 && !(text.EndsWith("e") || text.EndsWith("E")))
            {
                e.Handled = true;
                return;
            }

            //,只能出现一次
            if ((e.KeyChar == ',') && (text.IndexOf(",", StringComparison.CurrentCultureIgnoreCase) != -1))
            {
                e.Handled = true;
                return;
            }

            //e/E只能出现一次
            if ((e.KeyChar == 'E' || e.KeyChar == 'e') && (text.IndexOf("e", StringComparison.CurrentCultureIgnoreCase) != -1))
            {
                e.Handled = true;
                return;
            }

            //第一位不能为小数点和e,E
            if (((e.KeyChar == '.') || (e.KeyChar == 'e') || (e.KeyChar == 'E')) && (text.Length == 0))
            {
                e.Handled = true;
                return;
            }

            //负号、正号后不能为小数点
            if ((e.KeyChar == '.') && (text == "-" || (text == "+")))
            {
                e.Handled = true;
                return;
            }

            //小数点只能输入一次,e后不能有小数点（例如 23e2.3）
            //if ((e.KeyChar == '.') && (text.IndexOf('.') != -1 || text.IndexOf("e", StringComparison.CurrentCultureIgnoreCase) != -1))
            //{
            //    e.Handled = true;
            //    return;
            //}

            //负数、正号后第一位是0,或第一位是0，第二位必须为小数点
            if ((e.KeyChar != '.') && (e.KeyChar != 8) && (text == "-0" || text == "0" || text == "+0"))
            {
                e.Handled = true;
                return;
            }

            //e\E\e+\e-\E+\E-后不能为0
            if (e.KeyChar == '0' && (text.EndsWith("e") || text.EndsWith("E") || text.EndsWith("e-") || text.EndsWith("E-") || text.EndsWith("e+") || text.EndsWith("E+")))
            {
                e.Handled = true;
                return;
            }

            //判断输入的是否是实数
            //if ((e.KeyChar != '-') && (e.KeyChar != '+') && (e.KeyChar != '.') && (e.KeyChar != 8) && (e.KeyChar != 'e') && (e.KeyChar != 'E') && (e.KeyChar != ','))
            //{
            //    text += e.KeyChar;
            //    if (!Regex_IsNumber(text))
            //    {
            //        e.Handled = true;
            //        return;
            //    }
            //}
        }

        /// <summary>
        /// 只允许输入十六进制数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_HEX(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,A-F,空格和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && ((e.KeyChar < 'A') || (e.KeyChar > 'F')) && ((e.KeyChar < 'a') || (e.KeyChar > 'f')) && (e.KeyChar != ' ') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //空格符号不能出现在首位且不能连续出现两个空格
            if ((e.KeyChar == ' ') && (((TextBox)sender).Text.Length < 2 || ((TextBox)sender).Text.EndsWith(" ") || ((TextBox)sender).Text.LastIndexOfAny(new char[] { ' ' }) == ((TextBox)sender).Text.Length - 2))
            {
                e.Handled = true;
                return;
            }

            //长度为2时只能输入空格
            if ((e.KeyChar != ' ' && e.KeyChar != 8) && ((TextBox)sender).Text.Length == 2)
            {
                e.Handled = true;
                return;
            }

            //输入两个数后必须输入空格
            if ((e.KeyChar != ' ' && e.KeyChar != 8) && !((TextBox)sender).Text.EndsWith(" ") && ((TextBox)sender).Text.Trim().LastIndexOfAny(new char[] { ' ' }) == ((TextBox)sender).Text.Length - 3)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入十六进制数
        /// 长度为2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_HEX_len2(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字,A-F,空格和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && ((e.KeyChar < 'A') || (e.KeyChar > 'F')) && ((e.KeyChar < 'a') || (e.KeyChar > 'f')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //长度限制2
            if ((e.KeyChar != 8) && ((TextBox)sender).Text.Length >= 2)
            {
                e.Handled = true;
                return;
            }
        }

        #endregion

        #region 电话号码

        /// <summary>
        /// 输入电话号码
        /// 只允许输入数字，且首位数字为1，长度不超过11位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_MobilePhone(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //首位只能出现1
            if ((e.KeyChar != '1') && (((TextBox)sender).Text.Length == 0))
            {
                e.Handled = true;
                return;
            }

            //长度为11时，只能输入删除键
            if ((e.KeyChar != 8) && (((TextBox)sender).Text.Length >= 11))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 输入座机号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_TelePhone(object sender, KeyPressEventArgs e)
        {

        }

        #endregion

        #region 时间日期

        /// <summary>
        /// 只允许输入时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Time(object sender, KeyPressEventArgs e)
        {
            string data = ((TextBox)sender).Text;

            //只允许输入数字,负号,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != ':') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //符号不能出现在首位和：后
            if ((e.KeyChar == ':') && (data.Length == 0 || data.EndsWith(":") || data.Split(':').Length == 3))
            {
                e.Handled = true;
                return;
            }

            //长度为8时，只能输入删除键
            if ((e.KeyChar != 8) && (data.Length == 8))
            {
                e.Handled = true;
                return;
            }

            //控制数字输入
            if ((e.KeyChar != ':') && (e.KeyChar != 8))
            {
                data += e.KeyChar;
                string[] timeString = data.Split(':');

                if (timeString.Length == 1)
                {
                    //时不大于24
                    if (int.Parse(timeString[0]) >= 24)
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    //分秒不大于60
                    if (int.Parse(timeString[timeString.Length - 1]) > 60)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 只允许输入日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Date(object sender, KeyPressEventArgs e)
        {
            //获取文本框字符串
            string text = ((TextBox)sender).Text;

            //只允许输入数字,负号,小数点和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9')) && (e.KeyChar != '-') && (e.KeyChar != '/') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //符号不能出现在首位且不能出现在分隔符后
            if (((e.KeyChar == '-') || (e.KeyChar == '/')) && (text.Length == 0 || text.EndsWith("-") || text.EndsWith("/")))
            {
                e.Handled = true;
                return;
            }

            //分隔符唯一
            if ((e.KeyChar == '-') && text.Contains("/"))
            {
                e.Handled = true;
                return;
            }

            if ((e.KeyChar == '/') && text.Contains("-"))
            {
                e.Handled = true;
                return;
            }

            //有分割符号时，最大长度只能到10
            if (text.Contains("-") || (text.Contains("/")))
            {
                //长度为10时，只能输入删除键
                if ((e.KeyChar != 8) && (text.Length == 10))
                {
                    e.Handled = true;
                    return;
                }
            }
            else//没有分隔符时，最大长度只能到8
            {
                //长度为8时，只能输入删除键
                if ((e.KeyChar != 8) && (text.Length == 8))
                {
                    e.Handled = true;
                    return;
                }
            }


            //判断输入的年月日是否是合理的
            if ((e.KeyChar != '/') && (e.KeyChar != '-') && (e.KeyChar != 8))
            {
                text += e.KeyChar;
                string[] data = text.Split(new char[] { '/', '-' });

                if (data.Length == 1)//对年进行监控
                {
                    if (data[0].Length > 4)//年份长度不超过四位
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else if (data.Length == 2)
                {
                    if (int.Parse(data[1]) > 12)//月份不超过12
                    {
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    if (!Regex_IsDateTime(text))
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        #endregion

        #region 文本文字等

        /// <summary>
        /// 一般命名规则
        /// 只允许输入数字、字母和下划线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_UserName(object sender, KeyPressEventArgs e)
        {
            //只允许输入数字、字母、下划线和删除键
            if (((e.KeyChar < '0') || (e.KeyChar > '9'))
                && ((e.KeyChar < 'A') || (e.KeyChar > 'Z'))
                && ((e.KeyChar < 'a') || (e.KeyChar > 'z'))
                && (e.KeyChar != '_') && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }

            //方法二
            Regex regex = new Regex(@"[a-zA-Z0-9_]");
            if ((regex.IsMatch(e.KeyChar.ToString()) == false) && e.KeyChar != 8)
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 文件的命名，不允许输入特殊字符
        /// \/:*?"<>|
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_FileName(object sender, KeyPressEventArgs e)
        {
            //不可以有以下特殊字符
            // \/:*?"<>|
            // \\
            // \|
            // ""
            Regex meRgx = new Regex(@"[\\/:*?""<>\|]");
            if (meRgx.IsMatch(e.KeyChar.ToString()))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 文件的命名，不允许输入特殊字符
        /// \/:*?"<>|
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_Password(object sender, KeyPressEventArgs e)
        {
            //不可以有以下特殊字符
            // \/:*?"<>|
            // \\
            // \|
            // ""
            Regex meRgx = new Regex(@"[\\/:*?""<>\|]");
            if (meRgx.IsMatch(e.KeyChar.ToString()))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 文本框输入限制(禁止输入中文及中文符号)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_EnglishOnly(object sender, KeyPressEventArgs e)
        {
            int mycharNum = System.Text.Encoding.GetEncoding("GBK").GetBytes(e.KeyChar.ToString()).Length;
            if (mycharNum > 1)          //禁止输入中文及中文符号
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 只允许输入中文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_ChineseOnly(object sender, KeyPressEventArgs e)
        {
            Regex meRgx = new Regex(@"[\u4e00-\u9fa5]");
            if ((meRgx.IsMatch(e.KeyChar.ToString()) == false) && (e.KeyChar != 8))
            {
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// 禁止输入回车
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyPress_NoEnter(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)        //禁止输入回车
            {
                e.Handled = true;
                return;
            }
        }

        #endregion

        #region keyup

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_Integer(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsInteger(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴整数
        /// 限制长度5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_Integer_len5(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 5))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴整数
        /// 限制长度10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_Integer_len10(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 10))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositive(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len2(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 2))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len3(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 3))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len4(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 4))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len5(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 5))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len6(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 6))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制7
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len7(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 7))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴正整数
        /// 长度限制10
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_IntegerPositive_len10(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsIntegerPositiveLength(((TextBox)sender).Text + Clipboard.GetText(), 0, 10))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴有理数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_RationalNumber(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsNumber(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴十六进制字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_HEX(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsHEX(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴十六进制字符串
        /// 长度限制为2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_HEX_len2(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    string str = ((TextBox)sender).Text + Clipboard.GetText();
                    if (str.Length <= 2 && Regex_IsHEX(str))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴电话号码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_MobilePhone(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsPhoneNumber(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴日期时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_DateTime(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsDateTime(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴用户名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_UserName(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsUserName(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴文件名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_FileName(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (!Regex_IsFileName(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 不允许粘贴中文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_EnglishOnly(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (!Regex_IsChineseCharacter(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        /// <summary>
        /// ctrl+c, ctrl+v,ctrl+x
        /// 只允许粘贴中文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void KeyUp_ControlXCV_ChineseOnly(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.C))
            {
                Clipboard.SetText(((TextBox)sender).SelectedText.Trim()); //Ctrl+C 复制
            }
            if (e.KeyData == (Keys.Control | Keys.V))
            {
                if (Clipboard.ContainsText())
                {
                    if (Regex_IsChineseCharacter(((TextBox)sender).Text + Clipboard.GetText()))
                    {
                        ((TextBox)sender).SelectedText = Clipboard.GetText(); //Ctrl+V 粘贴
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
            if (e.KeyData == (Keys.Control | Keys.X))
            {
                if (Clipboard.ContainsText())
                {
                    ((TextBox)sender).Cut();
                }
            }
        }

        #endregion

        #region Leave

        /// <summary>
        /// 离开时验证是否是时间格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Leave_Time(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text == null || ((TextBox)sender).Text == "")
                return;

            if (!Regex_IsDateTime(((TextBox)sender).Text))
            {
                MessageBox.Show("输入字符或格式有误，重新输入，如00:00:00？", "提示");
            }
        }

        /// <summary>
        /// 离开时验证是否为有理数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Leave_RationalNumber(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text == null || ((TextBox)sender).Text == "")
                return;

            if (!Regex_IsNumber(((TextBox)sender).Text))
            {
                MessageBox.Show("输入格式有误，请重新输入，如12.4？", "提示");
            }
        }

        /// <summary>
        /// 离开时验证是否为十六进制字符串
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Leave_HEX(object sender, EventArgs e)
        {
            if (((TextBox)sender).Text == null || ((TextBox)sender).Text == "")
                return;

            if (!Regex_IsHEX(((TextBox)sender).Text))
            {
                MessageBox.Show("输入格式有误，请重新输入，如0F FF FF？", "提示");
            }
        }

        #endregion

        #region 正则表达式

        #region 匹配方法

        /// <summary>
        /// 验证字符串是否匹配正则表达式描述的规则
        /// </summary>
        /// <param name="inputStr">待验证的字符串</param>
        /// <param name="patternStr">正则表达式字符串</param>
        /// <returns>是否匹配</returns>
        private static bool IsMatch(string inputStr, string patternStr)
        {
            return IsMatch(inputStr, patternStr, false, false);
        }

        /// <summary>
        /// 验证字符串是否匹配正则表达式描述的规则
        /// </summary>
        /// <param name="inputStr">待验证的字符串</param>
        /// <param name="patternStr">正则表达式字符串</param>
        /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>
        /// <returns>是否匹配</returns>
        private static bool IsMatch(string inputStr, string patternStr, bool ifIgnoreCase)
        {
            return IsMatch(inputStr, patternStr, ifIgnoreCase, false);
        }

        /// <summary>
        /// 验证字符串是否匹配正则表达式描述的规则
        /// </summary>
        /// <param name="inputStr">待验证的字符串</param>
        /// <param name="patternStr">正则表达式字符串</param>
        /// <param name="ifIgnoreCase">匹配时是否不区分大小写</param>
        /// <param name="ifValidateWhiteSpace">是否验证空白字符串</param>
        /// <returns>是否匹配</returns>
        private static bool IsMatch(string inputStr, string patternStr, bool ifIgnoreCase, bool ifValidateWhiteSpace)
        {
            if (!ifValidateWhiteSpace && string.IsNullOrWhiteSpace(inputStr))//.NET 4.0 新增IsNullOrWhiteSpace 方法，便于对用户做处理
            {
                return false;//如果不要求验证空白字符串而此时传入的待验证字符串为空白字符串，则不匹配
            }

            Regex regex = null;
            if (ifIgnoreCase)
            {
                regex = new Regex(patternStr, RegexOptions.IgnoreCase);//指定不区分大小写的匹配
            }
            else
            {
                regex = new Regex(patternStr);
            }

            return regex.IsMatch(inputStr);
        }

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证数字(double类型)
        /// [可以包含负号和小数点]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsNumber(string input)
        {
            //string pattern = @"^-?\d+$|^(-?\d+)(\.\d+)?$";
            //return IsMatch(input, pattern);
            double d = 0;
            if (double.TryParse(input, out d))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsInteger(string input)
        {
            //string pattern = @"^-?\d+$";
            //return IsMatch(input, pattern);
            int data = 0;
            if (int.TryParse(input, out data))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证非负整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIntegerNotNagtive(string input)
        {
            //string pattern = @"^\d+$";
            //return IsMatch(input, pattern);
            int data = -1;
            if (int.TryParse(input, out data) && data >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证正整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIntegerPositive(string input)
        {
            //string pattern = @"^[0-9]*[1-9][0-9]*$";
            //return IsMatch(input, pattern);
            int data = 0;
            if (int.TryParse(input, out data) && data >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证小数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsDecimal(string input)
        {
            string pattern = @"^([-+]?[1-9]\d*\.\d+|-?0\.\d*[1-9]\d*)$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证是否是十六进制字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool Regex_IsHEX(string input)
        {
            string pattern = @"^[A-Fa-f0-9]{2}(\s[A-Fa-f0-9]{2})*(\s?)$";// @"([^A-Fa-f0-9]\s+?)+";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证只包含英文字母
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsEnglishCharacter(string input)
        {
            string pattern = @"^[A-Za-z]+$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证只包含数字和英文字母
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIntegerAndEnglishCharacter(string input)
        {
            string pattern = @"^[0-9A-Za-z]+$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证只包含汉字
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsChineseCharacter(string input)
        {
            string pattern = @"^[\u4e00-\u9fa5]+$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 判断字符串是否由数字、字母和下划线组成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool Regex_IsUserName(string input)
        {
            Regex regex = new Regex(@"[a-zA-Z0-9_]+$");
            return regex.IsMatch(input);
        }

        /// <summary>
        /// 判断字符串是否包含\/:*?"<>|
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool Regex_IsFileName(string input)
        {
            Regex regex = new Regex(@"[\\/:*?""<>\|]");
            return regex.IsMatch(input);
        }

        /// <summary>
        /// 验证数字长度范围（数字前端的0计长度）
        /// [若要验证固定长度，可传入相同的两个长度数值]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIntegerLength(string input, int lengthBegin, int lengthEnd)
        {
            //string pattern = @"^\d{" + lengthBegin + "," + lengthEnd + "}$";
            //return IsMatch(input, pattern);
            if (input.Length >= lengthBegin && input.Length <= lengthEnd)
            {
                int data;
                if (int.TryParse(input, out data))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证正整数长度范围（数字前端的0计长度）
        /// [若要验证固定长度，可传入相同的两个长度数值]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIntegerPositiveLength(string input, int lengthBegin, int lengthEnd)
        {
            //string pattern = @"^\d{" + lengthBegin + "," + lengthEnd + "}$";
            //return IsMatch(input, pattern);
            if (input.Length >= lengthBegin && input.Length <= lengthEnd)
            {
                int data;
                if (int.TryParse(input, out data) && data >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证字符串包含内容
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="withEnglishCharacter">是否包含英文字母</param>
        /// <param name="withNumber">是否包含数字</param>
        /// <param name="withChineseCharacter">是否包含汉字</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsStringInclude(string input, bool withEnglishCharacter, bool withNumber, bool withChineseCharacter)
        {
            if (!withEnglishCharacter && !withNumber && !withChineseCharacter)
            {
                return false;//如果英文字母、数字和汉字都没有，则返回false
            }

            StringBuilder patternString = new StringBuilder();
            patternString.Append("^[");

            //验证是否包含英文字母
            if (withEnglishCharacter)
            {
                patternString.Append("a-zA-Z");
            }
            //验证是否包含数字
            if (withNumber)
            {
                patternString.Append("0-9");
            }
            //验证是否包含汉字
            if (withChineseCharacter)
            {
                patternString.Append(@"\u4E00-\u9FA5");
            }
            patternString.Append("]+$");

            return IsMatch(input, patternString.ToString());
        }

        /// <summary>
        /// 验证字符串长度范围
        /// [若要验证固定长度，可传入相同的两个长度数值]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsStringLength(string input, int lengthBegin, int lengthEnd)
        {
            //string pattern = @"^.{" + lengthBegin + "," + lengthEnd + "}$";
            //return IsMatch(input, pattern);
            if (input.Length >= lengthBegin && input.Length <= lengthEnd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证字符串长度范围（字符串内只包含数字或英文字母）
        /// [若要验证固定长度，可传入相同的两个长度数值]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsStringLengthOnlyNumberAndEnglishCharacter(string input, int lengthBegin, int lengthEnd)
        {
            string pattern = @"^[0-9a-zA-z]{" + lengthBegin + "," + lengthEnd + "}$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证字符串长度范围
        /// [若要验证固定长度，可传入相同的两个长度数值]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="withEnglishCharacter">是否包含英文字母</param>
        /// <param name="withNumber">是否包含数字</param>
        /// <param name="withChineseCharacter">是否包含汉字</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsStringLengthByInclude(string input, bool withEnglishCharacter, bool withNumber, bool withChineseCharacter, int lengthBegin, int lengthEnd)
        {
            if (!withEnglishCharacter && !withNumber && !withChineseCharacter)
            {
                return false;//如果英文字母、数字和汉字都没有，则返回false
            }
            StringBuilder patternString = new StringBuilder();
            patternString.Append("^[");
            if (withEnglishCharacter)
            {
                patternString.Append("a-zA-Z");
            }
            if (withNumber)
            {
                patternString.Append("0-9");
            }
            if (withChineseCharacter)
            {
                patternString.Append(@"\u4E00-\u9FA5");
            }
            patternString.Append("]{" + lengthBegin + "," + lengthEnd + "}$");

            return IsMatch(input, patternString.ToString());
        }

        /// <summary>
        /// 验证字符串字节数长度范围
        /// [若要验证固定长度，可传入相同的两个长度数值；每个汉字为两个字节长度]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="lengthBegin">长度范围起始值（含）</param>
        /// <param name="lengthEnd">长度范围结束值（含）</param>
        /// <returns></returns>
        public static bool Regex_IsStringByteLength(string input, int lengthBegin, int lengthEnd)
        {
            //int byteLength = Regex.Replace(input, @"[^\x00-\xff]", "ok").Length;
            //if (byteLength >= lengthBegin && byteLength <= lengthEnd)
            //{
            //    return true;
            //}
            //return false;
            int byteLength = Encoding.Default.GetByteCount(input);
            if (byteLength >= lengthBegin && byteLength <= lengthEnd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证日期
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsDateTime(string input)
        {
            DateTime date;
            if (DateTime.TryParse(input, out date))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证固定电话号码
        /// [3位或4位区号；区号可以用小括号括起来；区号可以省略；区号与本地号间可以用减号或空格隔开；可以有3位数的分机号，分机号前要加减号]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsTelePhone(string input)
        {
            string pattern = @"^(((0\d2|0\d{2})[- ]?)?\d{8}|((0\d3|0\d{3})[- ]?)?\d{7})(-\d{3})?$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证手机号码
        /// [可匹配"(+86)013325656352"，括号可以省略，+号可以省略，(+86)可以省略，11位手机号前的0可以省略；11位手机号第二位数可以是3、4、5、8中的任意一个]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsMobilePhone(string input)
        {
            string pattern = @"^((\+)?86|((\+)?86)?)0?1[3458]\d{9}$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证电话号码（可以是固定电话号码或手机号码）
        /// [固定电话：[3位或4位区号；区号可以用小括号括起来；区号可以省略；区号与本地号间可以用减号或空格隔开；可以有3位数的分机号，分机号前要加减号]]
        /// [手机号码：[可匹配"(+86)013325656352"，括号可以省略，+号可以省略，(+86)可以省略，手机号前的0可以省略；手机号第二位数可以是3、4、5、8中的任意一个]]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsPhoneNumber(string input)
        {
            string pattern = @"^((\+)?86|((\+)?86)?)0?1[3458]\d{9}$|^(((0\d2|0\d{2})[- ]?)?\d{8}|((0\d3|0\d{3})[- ]?)?\d{7})(-\d{3})?$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证邮政编码
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsZipCode(string input)
        {
            //string pattern = @"^\d{6}$";
            //return IsMatch(input, pattern);
            if (input.Length != 6)
                return false;
            int data;
            if (int.TryParse(input, out data))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 验证电子邮箱
        /// [@字符前可以包含字母、数字、下划线和点号；@字符后可以包含字母、数字、下划线和点号；@字符后至少包含一个点号且点号不能是最后一个字符；最后一个点号后只能是字母或数字]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsEmail(string input)
        {
            //邮箱名以数字或字母开头；邮箱名可由字母、数字、点号、减号、下划线组成；邮箱名（@前的字符）长度为3～18个字符；邮箱名不能以点号、减号或下划线结尾；不能出现连续两个或两个以上的点号、减号。
            //string pattern = @"^[a-zA-Z0-9]((?<!(\.\.|--))[a-zA-Z0-9\._-]){1,16}[a-zA-Z0-9]@([0-9a-zA-Z][0-9a-zA-Z-]{0,62}\.)+([0-9a-zA-Z][0-9a-zA-Z-]{0,62})\.?|((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
            string pattern = @"^([\w-\.]+)@([\w-\.]+)(\.[a-zA-Z0-9]+)$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证网址（可以匹配IPv4地址但没对IPv4地址进行格式验证；IPv6暂时没做匹配）
        /// [允许省略"://"；可以添加端口号；允许层级；允许传参；域名中至少一个点号且此点号前要有内容]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsURL(string input)
        {
            //每级域名由字母、数字和减号构成（第一个字母不能是减号），不区分大小写，单个域长度不超过63，完整的域名全长不超过256个字符。在DNS系统中，全名是以一个点“.”来结束的，例如“www.nit.edu.cn.”。没有最后的那个点则表示一个相对地址。
            //没有例如"http://"的前缀，没有传参的匹配
            //string pattern = @"^([0-9a-zA-Z][0-9a-zA-Z-]{0,62}\.)+([0-9a-zA-Z][0-9a-zA-Z-]{0,62})\.?$";

            //string pattern = @"^(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&%_\./-~-]*)?$";
            string pattern = @"^([a-zA-Z]+://)?([\w-\.]+)(\.[a-zA-Z0-9]+)(:\d{0,5})?/?([\w-/]*)\.?([a-zA-Z]*)\??(([\w-]*=[\w%]*&?)*)$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 验证IPv4地址
        /// [第一位和最后一位数字不能是0或255；允许用0补位]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIPv4(string input)
        {
            //string pattern = @"^(25[0-4]|2[0-4]\d]|[01]?\d{2}|[1-9])\.(25[0-5]|2[0-4]\d]|[01]?\d?\d)\.(25[0-5]|2[0-4]\d]|[01]?\d?\d)\.(25[0-4]|2[0-4]\d]|[01]?\d{2}|[1-9])$";
            //return IsMatch(input, pattern);
            string[] IPs = input.Split('.');
            if (IPs.Length != 4)
            {
                return false;
            }

            int data = -1;
            for (int i = 0; i < IPs.Length; i++)
            {
                if (i == 0 || i == 3)
                {
                    if (int.TryParse(IPs[i], out data) && data > 0 && data < 255)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (int.TryParse(IPs[i], out data) && data >= 0 && data <= 255)
                    {
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 验证IPv6地址
        /// [可用于匹配任何一个合法的IPv6地址]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIPv6(string input)
        {
            string pattern = @"^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$";
            return IsMatch(input, pattern);
        }

        /// <summary>
        /// 身份证上数字对应的地址
        /// </summary>
        //enum IDAddress
        //{
        //    北京 = 11, 天津 = 12, 河北 = 13, 山西 = 14, 内蒙古 = 15, 辽宁 = 21, 吉林 = 22, 黑龙江 = 23, 上海 = 31, 江苏 = 32, 浙江 = 33,
        //    安徽 = 34, 福建 = 35, 江西 = 36, 山东 = 37, 河南 = 41, 湖北 = 42, 湖南 = 43, 广东 = 44, 广西 = 45, 海南 = 46, 重庆 = 50, 四川 = 51,
        //    贵州 = 52, 云南 = 53, 西藏 = 54, 陕西 = 61, 甘肃 = 62, 青海 = 63, 宁夏 = 64, 新疆 = 65, 台湾 = 71, 香港 = 81, 澳门 = 82, 国外 = 91
        //}

        /// <summary>
        /// 验证一代身份证号（15位数）
        /// [长度为15位的数字；匹配对应省份地址；生日能正确匹配]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIDCard15(string input)
        {
            //验证是否可以转换为15位整数
            long data = 0;

            if (!long.TryParse(input, out data) || data.ToString().Length != 15)
            {
                return false;
            }

            //验证省份是否匹配
            //1~6位为地区代码，其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码。
            string address = "11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91,";
            if (!address.Contains(input.Remove(2) + ","))
            {
                return false;
            }

            //验证生日是否匹配
            string birthdate = input.Substring(6, 6).Insert(4, "/").Insert(2, "/");
            DateTime date;
            if (!DateTime.TryParse(birthdate, out date))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证二代身份证号（18位数，GB11643-1999标准）
        /// [长度为18位；前17位为数字，最后一位(校验码)可以为大小写x；匹配对应省份地址；生日能正确匹配；校验码能正确匹配]
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIDCard18(string input)
        {
            //验证是否可以转换为正确的整数
            long data = 0;
            if (!long.TryParse(input.Remove(17), out data) || data.ToString().Length != 17 || !long.TryParse(input.Replace('x', '0').Replace('X', '0'), out data))
            {
                return false;
            }

            //验证省份是否匹配
            //1~6位为地区代码，其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码。
            string address = "11,12,13,14,15,21,22,23,31,32,33,34,35,36,37,41,42,43,44,45,46,50,51,52,53,54,61,62,63,64,65,71,81,82,91,";
            if (!address.Contains(input.Remove(2) + ","))
            {
                return false;
            }

            //验证生日是否匹配
            string birthdate = input.Substring(6, 8).Insert(6, "/").Insert(4, "/");
            DateTime date;
            if (!DateTime.TryParse(birthdate, out date))
            {
                return false;
            }

            //校验码验证
            //校验码：
            //（1）十七位数字本体码加权求和公式
            //S = Sum(Ai * Wi), i = 0, ... , 16 ，先对前17位数字的权求和
            //Ai:表示第i位置上的身份证号码数字值
            //Wi:表示第i位置上的加权因子
            //Wi: 7 9 10 5 8 4 2 1 6 3 7 9 10 5 8 4 2
            //（2）计算模
            //Y = mod(S, 11)
            //（3）通过模得到对应的校验码
            //Y: 0 1 2 3 4 5 6 7 8 9 10
            //校验码: 1 0 X 9 8 7 6 5 4 3 2
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = input.Remove(17).ToCharArray();
            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int index = -1;
            Math.DivRem(sum, 11, out index);
            if (arrVarifyCode[index] != input.Substring(17, 1).ToLower())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证身份证号（不区分一二代身份证号）
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsIDCard(string input)
        {
            if (input.Length == 18)
            {
                return Regex_IsIDCard18(input);
            }
            else if (input.Length == 15)
            {
                return Regex_IsIDCard15(input);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证经度
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsLongitude(string input)
        {
            //范围为 - 180～180，小数位数必须是1到5位
            //string pattern = @"^[-\+]?((1[0-7]\d{1}|0?\d{1,2})\.\d{1,5}|180\.0{1,5})$";
            //return IsMatch(input, pattern);
            float lon;
            if (float.TryParse(input, out lon) && lon >= -180 && lon <= 180)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证纬度
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns>是否匹配</returns>
        public static bool Regex_IsLatitude(string input)
        {
            //范围为 - 90～90，小数位数必须是1到5位
            //string pattern = @"^[-\+]?([0-8]?\d{1}\.\d{1,5}|90\.0{1,5})$";
            //return IsMatch(input, pattern);
            float lat;
            if (float.TryParse(input, out lat) && lat >= -90 && lat <= 90)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
    }
}
