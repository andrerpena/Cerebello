using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace CerebelloWebRole.Code
{
    public class UrlBuilder
    {
        //string domain; // não foi necessário até agora
        string pagePath;
        string pageName;
        NameValueCollection pageParams;
        public UrlBuilder(HttpRequestBase request)
            : this(request, new String[] { })
        {

        }

        public UrlBuilder(HttpRequestBase request, String[] ignoredParams)
        {
            string rawUrl = request.Url.AbsolutePath;
            this.pagePath = rawUrl.Substring(0, rawUrl.LastIndexOf('/'));
            this.pageParams = new NameValueCollection(request.QueryString);

            if (ignoredParams != null)
                foreach (String ignoredParam in ignoredParams)
                    this.pageParams.Remove(ignoredParam);

            if (this.pageParams.Count > 0)
            {
                int start = rawUrl.LastIndexOf('/');
                if (rawUrl.Contains('?'))
                    this.pageName = rawUrl.Substring(start, rawUrl.IndexOf('?') - start);
                else
                    this.pageName = rawUrl.Substring(start);
            }
            else
            {
                this.pageName = rawUrl.Substring(rawUrl.LastIndexOf('/'));
            }
        }

        public void SetParam(string key, string value, string defaultValue)
        {
            this.pageParams.Remove(key);
            if (value != defaultValue)
                this.pageParams.Add(key, value);
        }

        public void SetParam(string key, string value)
        {
            this.pageParams.Remove(key);
            this.pageParams.Add(key, value);
        }

        public string GetParam(string key)
        {
            return this.pageParams.Get(key);
        }

        public void RemoveParam(string key)
        {
            this.pageParams.Remove(key);
        }

        public void RemoveParamRange(IEnumerable<string> keys)
        {
            foreach (var item in keys)
                this.RemoveParam(item);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(1000);
            var keys = this.pageParams.AllKeys;
            bool hasParams = false;
            for (int it = 0; it < this.pageParams.Count; it++)
            {
                string curKey = keys[it];
                string[] curValues = this.pageParams.GetValues(it);
                for (int itVal = 0; itVal < curValues.Length; itVal++)
                {
                    builder.Append(hasParams ? "&" : "?");
                    builder.Append(curKey);
                    builder.Append('=');
                    builder.Append(curValues[itVal]);
                    hasParams = true;
                }
            }
            string queryString = builder.ToString();
            return this.pagePath + this.pageName + queryString;
        }

        public static string AppendQuery(string url, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return url;

            if (url.Contains("?"))
                return url + "&" + query;

            return url + "?" + query;
        }

        public static string GetListQueryParams(string paramName, List<string> list)
        {
            if (list == null)
                return null;

            return string.Join(
                "&",
                list
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => string.Format("{0}={1}", paramName, HttpUtility.UrlEncode(s))));
        }
    }
}
