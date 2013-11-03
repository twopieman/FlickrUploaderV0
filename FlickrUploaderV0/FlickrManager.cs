using FlickrNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlickrUploaderV0
{
    class FlickrManager
    {
        private static string Key = ConfigurationManager.AppSettings["FlickrKey"];
        private static string Secret = ConfigurationManager.AppSettings["FlickrSecret"];

        private static OAuthAccessToken oAuthToken;

        public static OAuthAccessToken OAuthToken
        {
            get { return oAuthToken; }
            set { oAuthToken = value; }
        }

        public static Flickr GetInstance()
        {
            return new Flickr(Key, Secret);
        }

        public static Flickr GetAuthInstance()
        {
            var f = new Flickr(Key, Secret);
            f.OAuthAccessToken = OAuthToken.Token;
            f.OAuthAccessTokenSecret = OAuthToken.TokenSecret;
            return f;
        }
    }
}
