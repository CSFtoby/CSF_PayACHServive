namespace CSF_PayACHServive
{
    public class ClsCreditCardRequest
    {
        /// <summary>
        /// consulta o pago
        /// </summary>
        public string operation { get; set; }
        public string number { get; set; }
        public decimal amount { get; set; }
        public string id { get; set; }
    }
}