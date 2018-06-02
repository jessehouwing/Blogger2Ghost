using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Blogger2Ghost.Ghost
{
    public class Setting
    {
        /*
         "settings": [
                    {
                        <id>tag:blogger.com,1999:blog-5657211614364435356.settings.BLOG_NAME</id>
                        "id": "5b0d772b0b214700bf78faf3",
                        "key": "title",
                        "value": "Scrum Bug",
                        "type": "blog"
                    }
                    {
                        <id>tag:blogger.com,1999:blog-5657211614364435356.settings.BLOG_DESCRIPTION</id>
    
                        "id": "5b0d772b0b214700bf78faf4",
                        "key": "description",
                        "value": "Thoughts, stories and ideas.",
                        "type": "blog"
                    },
                    {
                        <id>tag:blogger.com,1999:blog-5657211614364435356.settings.BLOG_LOCALE</id>
                        "id": "5b0d772b0b214700bf78faf8",
                        "key": "default_locale",
                        "value": "en",
                        "type": "blog"
                    },
                    {
                        <id>tag:blogger.com,1999:blog-5657211614364435356.settings.BLOG_TIME_ZONE</id>
                        "id": "5b0d772b0b214700bf78faf9",
                        "key": "active_timezone",
                        "value": "Europe/Amsterdam",
                        "type": "blog"
                    }
                ],
         */

        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SettingsType Type { get; set; }
    }

    public enum SettingsType
    {
        Private,
        App,
        Theme,
        Blog,
    }
}
