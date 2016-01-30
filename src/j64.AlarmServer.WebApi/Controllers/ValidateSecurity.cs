using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer.WebApi.Controllers
{
    public class ValidateSecurity
    {
        /// <summary>
        /// This is a lame attempt at adding some type of security to the site.
        /// I was going to just allow traffic from certain IP addresses however
        /// I don't seem to have a way to do that.  Kestrel does not seem to 
        /// return the address nor does it seem to have a way to restrict access
        /// from certain IP addresses.  
        /// 
        /// Ideally we would just have a policy or authorization attribute on the 
        /// controllers but that would require a full blown security provider to
        /// set up the users identity.  And, if i did that how would the hub be able
        /// to xmit those credentials???
        /// 
        /// Maybe I should just make it xmit a token that i send when the smart app
        /// is created.  hmm.  that might work out but it is still pretty easy to 
        /// exploit!
        /// </summary>
        /// <returns></returns>
        public static bool AllowLocalLan()
        {
                return true;
        }
    }
}
