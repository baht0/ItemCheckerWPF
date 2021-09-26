using System;

namespace ItemChecker.Net
{
    public class Delete
    {
        public static String FetchRequest(string url)
        {
            string js_fetch = @"
                fetch('" + url + @"', {
                        method: 'DELETE'
                    }).then(response => response.json()).then(data => console.log(data));";

            return js_fetch;
        }
    }
}
