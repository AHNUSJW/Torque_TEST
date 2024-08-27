namespace DBHelper
{
    public class DBHelper
    {
        //连接用的字符串
        private string connStr;

        //数据库连接字符串
        public string ConnStr
        {
            get { return this.connStr; }
            set { this.connStr = value; }
        }
    }
}
