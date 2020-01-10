﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net;

namespace ArchiveLoader
{
    class SeApiClient
    {
        string apiurl;
        string site;        
        WebClient wc;

        public SeApiClient(string url,string sitename)
        {
            this.apiurl = url;
            this.site = sitename;            
            wc = new WebClient();            
            wc.Headers["Accept-Encoding"] = "GZIP";
        }

        public string DoRequest(string url)
        {
            byte[] bytes = wc.DownloadData(url);
            byte[] bytes_decompressed;
            using (MemoryStream original = new MemoryStream(bytes))
            {
                using (MemoryStream decompressed = new MemoryStream())
                {
                    using (GZipStream decompressionStream = new GZipStream(original, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressed);
                        bytes_decompressed = decompressed.ToArray();
                    }
                }
            }

            string apires = Encoding.UTF8.GetString(bytes_decompressed);
            return apires;
        }

        public Dictionary<int, object> LoadPostsRange(int start, int end)
        {
            StringBuilder request = new StringBuilder(500);
            request.Append(apiurl + "posts/");

            for (int i = start; i <= end; i++)
            {
                if (i > start) request.Append(";");
                request.Append(i.ToString());
            }
            request.Append("?site="+this.site);
            request.Append("&filter=withbody&pagesize=100");
                        
            string apires = DoRequest(request.ToString());
            
            dynamic data = JSON.Parse(apires);
            IEnumerable<object> items;
            items = JSON.ToCollection(data.items);

            Dictionary<int, object> posts = new Dictionary<int, object>();

            foreach (dynamic item in items)
            {
                posts[item.post_id] = item;
            }

            return posts;            
        }

        public string LoadQuestion(int id)
        {
            StringBuilder request = new StringBuilder(500);
            request.Append(apiurl + "questions/");
            request.Append(id.ToString());            
            request.Append("?site=" + this.site);
            request.Append("&filter=withbody");

            string apires = DoRequest(request.ToString());

            dynamic data = JSON.Parse(apires);
            IEnumerable<object> items;
            items = JSON.ToCollection(data.items);
            object retval = items.FirstOrDefault();

            if (retval != null) return JSON.Stringify(retval);
            else return null;
        }

        public Dictionary<int, string> LoadQuestionAnswers(int id)
        {
            StringBuilder request = new StringBuilder(500);
            request.Append(apiurl + "questions/");
            request.Append(id.ToString());
            request.Append("/answers?site=" + this.site);
            request.Append("&filter=withbody&pagesize=100");

            string apires = DoRequest(request.ToString());

            dynamic data = JSON.Parse(apires);
            IEnumerable<object> items;
            items = JSON.ToCollection(data.items);

            Dictionary<int, string> posts = new Dictionary<int, string>();

            foreach (dynamic item in items)
            {
                posts[item.answer_id] = JSON.Stringify(item);
            }

            return posts;
        }
    }
}
