namespace CHADSOSIEL
{
    public class ChadField
    {
        public ChadField(int number)
        {
            Number = number;
        }

        public int Number { get; }

        public string Plant { get; internal set; }

        public double NumberOfAcres { get; set; }

        public int FieldHistoryCrop { get; set; }

        public int FieldHistoryNonCrop { get; set; }
    }
}