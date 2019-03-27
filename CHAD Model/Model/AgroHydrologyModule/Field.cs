using System.Collections.Generic;
using System.Linq;

namespace CHAD.Model.AgroHydrologyModule
{
    public class Field
    {
        #region Fields

        private readonly List<FieldSeason> _fieldSeasons;

        #endregion

        #region Constructors

        public Field(int fieldNumber, decimal fieldSize)
        {
            FieldNum = fieldNumber;
            FieldSize = fieldSize;

            _fieldSeasons = new List<FieldSeason>
            {
                new FieldSeason(-4) {Plant = Plant.Nothing, Harvestable = 0},
                new FieldSeason(-3) {Plant = Plant.Nothing, Harvestable = 0},
                new FieldSeason(-2) {Plant = Plant.Nothing, Harvestable = 0},
                new FieldSeason(-1) {Plant = Plant.Nothing, Harvestable = 0},
                new FieldSeason(0) {Plant = Plant.Nothing, Harvestable = 0}
            };
        }

        #endregion

        #region Public Interface

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Field) obj);
        }

        public int FieldNum { get; }
        public decimal FieldSize { get; }

        public int GetCropNumberSeasons()
        {
            return GetLastFive().Count(pf => pf != Plant.Nothing && pf != Plant.Alfalfa);
        }

        public override int GetHashCode()
        {
            return FieldNum;
        }

        public int GetNonCropNumberSeasons()
        {
            return GetLastFive().Count(pf => pf == Plant.Nothing || pf == Plant.Alfalfa);
        }

        public static bool operator ==(Field left, Field right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Field left, Field right)
        {
            return !Equals(left, right);
        }

        public Plant Plant => _fieldSeasons.Last().Plant;

        #endregion

        #region All other members

        private bool Equals(Field other)
        {
            return FieldNum == other.FieldNum;
        }

        private List<Plant> GetLastFive()
        {
            var lastFive = new List<Plant>();

            for (var i = 1; i <= 5; i++) lastFive.Add(_fieldSeasons[_fieldSeasons.Count - i].Plant);

            return lastFive;
        }

        #endregion
    }
}