namespace CSF_PayACHServive 
{
    public class ClsTransferResponse
    {
        public int codTransaction { get; set; }
        public string statusRequest { get; set; }
        public string result { get; set; }
        public string descriptionStatus { get; set; }
        public int transactionId { get; set; }
        public ClsStatus statusResponse { get; set; }
    }
}