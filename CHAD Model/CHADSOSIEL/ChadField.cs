namespace CHADSOSIEL
{
    public class ChadField
    {
        public int Number { get; set; }

        public string Plant { get; internal set; }

        public double NumberOfAcres { get; set; }

        public int FieldHistoryCrop { get; set; }

        public int FieldHistoryNonCrop { get; set; }
    }
}