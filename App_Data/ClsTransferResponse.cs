﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsTransferResponse
    {
        public string status { get; set; }
        public string result { get; set; }
        public string descriptionStatus { get; set; }
        public string resultMessage { get; set; }
        public string transactionId { get; set; }
    }
}
