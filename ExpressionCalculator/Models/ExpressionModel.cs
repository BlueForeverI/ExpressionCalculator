using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ExpressionCalculator.Models
{
    [DataContract]
    public class ExpressionModel
    {
        [DataMember]
        public string Expression { get; set; }

        [DataMember]
        public double Result { get; set; }
    }
}