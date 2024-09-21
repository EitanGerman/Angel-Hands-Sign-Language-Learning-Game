using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.CommonTypes
{
    [System.Serializable]
    public class WordAttempt
    {
        public int times_required; //how many times the user was required to present the sign (required = succeded + failed + skipped)
        public int times_failed; //how many times the user was required to present the sign and failed
        public int times_succeded; //how many times the user was required to present the sign and succeded
        public int times_skipped; //skipped 
    }
}
