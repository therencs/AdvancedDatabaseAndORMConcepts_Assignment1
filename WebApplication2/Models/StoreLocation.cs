namespace WebApplication2.Models
{
    public class StoreLocation
    {
        public Guid StoreNumber { get; set; }

        private readonly List<string> Provinces = new List<string>
        {
            "British Columbia",
            "Alberta",
            "Saskatchewan",
            "Manitoba",
            "Ontario",
            "Quebec",
            "Newfoundland",
            "Nova Scotia",
            "Prince Edward Island",
            "New Brunswick"
        };

        private string _streetNameAndNumber;

        public string StreetNameAndNumber
        {
            get
            {
                return _streetNameAndNumber;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value)); 
                } else
                {
                    _streetNameAndNumber = value;
                }
            }
        }

        private string _province;

        public string Province
        {
            get
            {
                return _province;
            }

            set 
            {
                if (String.IsNullOrEmpty(value) || !Provinces.Contains(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                } else
                {
                    _province = value;
                }


            }

        }



        public HashSet<StoreLaptop> StoreLaptops { get; set; } = new HashSet<StoreLaptop>();

        public int Quantity { get; set; }

        public StoreLocation()
        {
            if (StoreNumber == Guid.Empty)
            {
                StoreNumber = Guid.NewGuid();
            }
            
        }

        public StoreLocation(string streetnameandnumber, string province, int quantity)
        {
            _streetNameAndNumber = streetnameandnumber;
            _province = province;
            Quantity = quantity;
            
        }
    }
}
