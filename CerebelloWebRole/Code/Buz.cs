namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Buziness rules constants.
    /// </summary>
    public static class Buz
    {
        public static class Trial
        {
            /// <summary> 50 </summary>
            public const int TRIAL_PATIENTS_LIMIT = 50;

            /// <summary> 20 </summary>
            public const int TRIAL_FILES_LIMIT = 20;

            /// <summary> 10 </summary>
            public const int TRIAL_FILES_MAXSIZE_MB = 10;
        }

        public static class Pro
        {
            /// <summary> 92.00m </summary>
            public const decimal PRICE_MONTH = 89.00m;

            /// <summary> 262.20m </summary>
            public const decimal PRICE_QUARTER = PRICE_MONTH * (1 - DISCOUNT_QUARTER / 100m) * 3;

            /// <summary> 496.80m </summary>
            public const decimal PRICE_SEMESTER = PRICE_MONTH * (1 - DISCOUNT_SEMESTER / 100m) * 6;

            /// <summary> 938.40m </summary>
            public const decimal PRICE_YEAR = PRICE_MONTH * (1 - DISCOUNT_YEAR / 100m) * 12;

            /// <summary> 5 </summary>
            public const int DISCOUNT_QUARTER = 5;

            /// <summary> 10 </summary>
            public const int DISCOUNT_SEMESTER = 10;

            /// <summary> 15 </summary>
            public const int DISCOUNT_YEAR = 15;

            /// <summary> 39 </summary>
            public const int DOCTOR_PRICE = 39;
        }

        public static class ProOld
        {
            /// <summary> 119 </summary>
            public const int PRICE_MONTH = 119;

            /// <summary> 339 (113 × 3) </summary>
            public const int PRICE_QUARTER = 3 * 113;

            /// <summary> 642 (107 × 6) </summary>
            public const int PRICE_SEMESTER = 6 * 107;

            /// <summary> 1068 (89 × 12) </summary>
            public const int PRICE_YEAR = 12 * 89;

            /// <summary> 5 </summary>
            public const int DISCOUNT_QUARTER = 5;

            /// <summary> 10 </summary>
            public const int DISCOUNT_SEMESTER = 10;

            /// <summary> 25 </summary>
            public const int DISCOUNT_YEAR = 25;
        }
    }
}
