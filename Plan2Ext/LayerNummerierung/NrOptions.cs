// ReSharper disable IdentifierTypo
namespace Plan2Ext.LayerNummerierung
{
    public class NrOptions
    {

        private string _prefix = "";
        public string Prefix { get { return _prefix; } set { _prefix = value; } }
        // ReSharper disable once UnusedMember.Global
        public void SetPrefix(string topName)
        {
            Form.txtPrefix.Text = topName;
            _prefix = topName;
        }

        private string _suffix = "";
        public string Suffix { get { return _suffix; } set { _suffix = value; } }
        // ReSharper disable once UnusedMember.Global
        public void SetSuffix(string topName)
        {
            Form.txtSuffix.Text = topName;
            _suffix = topName;
        }

        private string _number = "01";
        public string Number { get { return _number; } set { _number = value; } }
        public void SetNumber(string num)
        {
            Form.txtNumber.Text = num;
            _number = num;
        }

        public NrControl Form { private get; set; }

        internal void ResetNr()
        {
            if (_number.Length == 0) _number = "01";
            else
            {
                var num = "1";
                _number = num.PadLeft(_number.Length, '0');
            }
        }
    }
}
