namespace Platform.BusinessLogic.ReportingServices.PrintForms
{
    public class PrintFormBase
    {
        public int DocId { get; set; }

        public PrintFormBase(int docId)
        {
            DocId = docId;
        }
    }
}
