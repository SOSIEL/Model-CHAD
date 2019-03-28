using System.Collections.Generic;
using System.Linq;

namespace CHAD.Model.AgroHydrologyModule
{
    public class FieldHistory
    {
        #region Fields

        private readonly List<FieldSeason> _fieldSeasons;

        #endregion

        #region Constructors

        public FieldHistory(Field field)
        {
            Field = field;

            _fieldSeasons = new List<FieldSeason>
            {
                new FieldSeason(-4, Plant.Nothing) {Harvestable = 0},
                new FieldSeason(-3, Plant.Nothing) {Harvestable = 0},
                new FieldSeason(-2, Plant.Nothing) {Harvestable = 0},
                new FieldSeason(-1, Plant.Nothing) {Harvestable = 0},
                new FieldSeason(0, Plant.Nothing) {Harvestable = 0}
            };
        }

        #endregion

        #region Public Interface

        public void AddNewSeason(FieldSeason fieldSeason)
        {
            _fieldSeasons.Add(fieldSeason);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FieldHistory) obj);
        }

        public Field Field { get; }

        public int GetCropNumberSeasons()
        {
            return GetLastFive().Count(pf => pf != Plant.Nothing && pf != Plant.Alfalfa);
        }

        public override int GetHashCode()
        {
            return Field.FieldNumber;
        }

        public int GetNonCropNumberSeasons()
        {
            return GetLastFive().Count(pf => pf == Plant.Nothing || pf == Plant.Alfalfa);
        }

        public static bool operator ==(FieldHistory left, FieldHistory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FieldHistory left, FieldHistory right)
        {
            return !Equals(left, right);
        }

        public Plant Plant => _fieldSeasons.Last().Plant;

        #endregion

        #region All other members

        private bool Equals(FieldHistory other)
        {
            return Field.FieldNumber == other.Field.FieldNumber;
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