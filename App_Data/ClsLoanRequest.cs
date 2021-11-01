namespace CSF_PayACHServive
{
    public class ClsLoanRequest
    {
        public string numOperation { get; set; }
        /// <summary>
        /// S = Consulta, N = Aplicación
        /// </summary>
        public string consult { get; set; }
        /// <summary>
        /// N = Abono Normal, E = Abono a Capital, C = Cancelación Total, null = si es consulta
        /// </summary>
        public string tipeOfPay { get; set; }
        public decimal amount { get; set; }
    }
}