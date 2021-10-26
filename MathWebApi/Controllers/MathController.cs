using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MathWebApi.Controllers
{
    public class MathController : ApiController
    {
        private List<Uri> FindPeers()
        {
            List<Uri> peerUris = new List<Uri>();

            string peersAppSetting = System.Configuration.ConfigurationManager.AppSettings["peers"];
            string[] peers = peersAppSetting.Split(' ');
            foreach (var peer in peers)
            {
                Uri peerUri = null;
                if (Uri.TryCreate(peer, UriKind.RelativeOrAbsolute, out peerUri))
                {
                    peerUris.Add(peerUri); 
                }
            }

            return peerUris;
        }

        public IHttpActionResult GetSum([FromUri] int[] operand)
        {
            List<Uri> peers = FindPeers();

            int result = operand.Length > 0 ? operand[0] : 0;

            for (int i = 1; i < operand.Length; i++)
            {
                result += operand[i];
            }

            return Ok(result);
        }
    }
}
