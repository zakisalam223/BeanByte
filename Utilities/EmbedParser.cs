using System.Text.RegularExpressions;
using forum_aspcore.Stores;

namespace forum_aspcore.Utilities
{
    public class EmbedParser
    {
        public static string ParseContent(string content, MongoFileStore fileStore)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Parse bold text
            content = Regex.Replace(content, @"\[b\](.*?)\[/b\]", "<strong>$1</strong>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Parse italic text
            content = Regex.Replace(content, @"\[i\](.*?)\[/i\]", "<em>$1</em>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Parse underlined text
            content = Regex.Replace(content, @"\[u\](.*?)\[/u\]", "<u>$1</u>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Parse quotes
            content = Regex.Replace(content, @"\[quote\](.*?)\[/quote\]", "<blockquote class=\"border-l-4 border-gray-300 pl-4 my-4\">$1</blockquote>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Parse code blocks
            var codePattern = @"\[CODE(?: lang=""(\w+)"")?\](.*?)\[/CODE\]";

            var codeReplacement = "<pre><code class=\"language-$1\">$2</code></pre>";

            content = Regex.Replace(content, codePattern, codeReplacement, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            content = Regex.Replace(content, @"class=""language-""", "class=\"\"", RegexOptions.Singleline);

            // Parse URLs into clickable links
            var urlPattern = @"(https?:\/\/[^\s<]+[^<.,:;""\'\]\s]|www\.[^\s<]+[^<.,:;""\'\]\s])";
            content = Regex.Replace(content, urlPattern, match =>
            {
                var url = match.Value;
                var displayUrl = url;
                
                if (url.StartsWith("www."))
                {
                    url = "https://" + url;
                }

                // Check if is file download link
                if (url.Contains("/File/Download/"))
                {
                    var fileId = url.Split("/").Last();
                    var file = fileStore.GetFileByIdAsync(fileId).Result;
                    
                    // if file isn't verified yet then user can't click download link
                    if (file != null && !file.Status)
                    {
                        return $"<span class=\"line-through text-red-500 cursor-not-allowed\" title=\"File is not verified\">{displayUrl}</span>";
                    }
                }

                return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"text-blue-600 hover:text-blue-800 underline\">{displayUrl}</a>";
            });

            return content;
        }
    }
}
