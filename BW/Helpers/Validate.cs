using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BW.Helpers
{
    public class Validate
    {
        public bool ValidateString(string _value)
        {
            string RegularExpressions = "^[^&]+$";

            Match m = Regex.Match(_value, RegularExpressions);

            if (m.Success)
                return true;
            else
                return false;
        }
    }
}