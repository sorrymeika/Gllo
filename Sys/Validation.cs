using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Collections.Specialized;

namespace Gllo.Sys
{
    public class Validation
    {
        private IDictionary<string, string> errors;
        private NameValueCollection form;
        private bool isHttpPost = true;
        private bool validateResult = true;

        private static readonly IDictionary<string, bool> boolConvert = new Dictionary<string, bool>()
        {
            {"on", true},
            {"off", false},
            {"", false},
            {"1", true},
            {"0", false},
            {"True", true},
            {"False", false},
            {"true", true},
            {"false", false},
        };

        public bool IsHttpPost
        {
            get
            {
                return isHttpPost;
            }
            set
            {
                form = value ? System.Web.HttpContext.Current.Request.Form : System.Web.HttpContext.Current.Request.QueryString;
                isHttpPost = value;
            }
        }

        public Validation(bool isHttpPost)
        {
            this.IsHttpPost = isHttpPost;
            this.errors = new Dictionary<string, string>();
        }

        public Validation()
            : this(true)
        {
        }

        public static ValidationResult Validate(string input, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null)
        {
            ValidationResult vr = new ValidationResult();

            if (emptyAble == false && string.IsNullOrEmpty(input))
            {
                vr.success = false;
                vr.msg = emptyText;
            }
            else if (regex != null && !string.IsNullOrEmpty(input) && !System.Text.RegularExpressions.Regex.IsMatch(input, regex))
            {
                vr.success = false;
                vr.msg = regexText;
            }
            else if (compare != null && !string.Equals(compare, input, StringComparison.OrdinalIgnoreCase))
            {
                vr.success = false;
                vr.msg = compareText;
            }
            else
                vr.success = true;

            return vr;
        }

        public bool HasError
        {
            get { return !this.validateResult; }
        }

        public void ClearErrors()
        {
            this.errors.Clear();
        }

        public IDictionary<string, string> GetErrors()
        {
            return this.errors;
        }

        public string GetError()
        {
            foreach (var kv in this.errors)
            {
                return kv.Value;
            }
            return null;
        }

        public string Get(string name, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null)
        {
            string value = form[name];
            ValidationResult vr = Validate(value, emptyAble, emptyText, regex, regexText, compare, compareText);
            if (!vr.success)
            {
                validateResult = false;
                errors.Add(name, vr.msg);
            }
            return value;
        }

        public int GetInt(string name, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null, int defaultValue = 0)
        {
            string value = form[name];
            ValidationResult vr = Validate(value, emptyAble, emptyText, regex, regexText, compare, compareText);
            if (!vr.success)
            {
                validateResult = false;
                errors.Add(name, vr.msg);
            }
            else
            {
                try
                {
                    return int.Parse(value);
                }
                catch
                {
                    errors.Add(name, "转换为Int类型时出现错误");
                }
            }
            return defaultValue;
        }

        public decimal GetDecimal(string name, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null)
        {
            string value = form[name];
            ValidationResult vr = Validate(value, emptyAble, emptyText, regex, regexText, compare, compareText);
            if (!vr.success)
            {
                validateResult = false;
                errors.Add(name, vr.msg);
            }
            else
            {
                try
                {
                    return decimal.Parse(value);
                }
                catch
                {
                    errors.Add(name, "转换为Decimal类型时出现错误");
                }
            }
            return 0;
        }

        public bool GetBool(string name, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null)
        {
            string value = form[name];
            ValidationResult vr = Validate(value, emptyAble, emptyText, regex, regexText, compare, compareText);
            if (!vr.success)
            {
                validateResult = false;
                errors.Add(name, vr.msg);
            }
            else
            {
                try
                {
                    return boolConvert[value];
                }
                catch
                {
                    errors.Add(name, "转换为布尔类型时出现错误");
                }
            }
            return false;
        }

        public DateTime GetDateTime(string name, bool emptyAble = true, string emptyText = null, string regex = null, string regexText = null, string compare = null, string compareText = null)
        {
            string value = form[name];
            ValidationResult vr = Validate(value, emptyAble, emptyText, regex, regexText, compare, compareText);
            if (!vr.success)
            {
                validateResult = false;
                errors.Add(name, vr.msg);
            }
            else
            {
                try
                {
                    return DateTime.Parse(value);
                }
                catch
                {
                    errors.Add(name, "转换为时间类型时出现错误");
                }
            }
            return DateTime.MinValue;
        }
    }
}
