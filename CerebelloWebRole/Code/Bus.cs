using System;
using System.Collections.Generic;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Business rules constants.
    /// </summary>
    public static class Bus
    {
        public static readonly Dictionary<string, string> CorrectPlanCase = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "TrialPlan", "TrialPlan" },
            { "ProfessionalPlan", "ProfessionalPlan" },
        };

        public static readonly Dictionary<string, string> CorrectContractCase = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "TrialContract", "TrialContract" },
            { "ProfessionalContract", "ProfessionalContract" },
        };

        /// <summary>
        /// Trial plan constants.
        /// </summary>
        public static class Trial
        {
            /// <summary> Equals 50 </summary>
            public const int TRIAL_PATIENTS_LIMIT = 50;

            /// <summary> Equals 20 </summary>
            public const int TRIAL_FILES_LIMIT = 20;

            /// <summary> Equals 10 </summary>
            public const int TRIAL_FILES_MAXSIZE_MB = 10;
        }

        /// <summary>
        /// Professional plan constants.
        /// </summary>
        public static class Pro
        {
            /// <summary> Equals 58.70m </summary>
            public const decimal PRICE_MONTH = 58.70m;

            /// <summary> Equals 253.65m </summary>
            public const decimal PRICE_QUARTER = (int)(PRICE_MONTH * (100 - DISCOUNT_QUARTER) / 10) / 10m * 3;

            /// <summary> Equals 496.80m </summary>
            public const decimal PRICE_SEMESTER = (int)(PRICE_MONTH * (100 - DISCOUNT_SEMESTER) / 10) / 10m * 6;

            /// <summary> Equals 938.40m </summary>
            public const decimal PRICE_YEAR = (int)(PRICE_MONTH * (100 - DISCOUNT_YEAR) / 10) / 10m * 12;

            /// <summary> Equals 5 </summary>
            public const int DISCOUNT_QUARTER = 5;

            /// <summary> Equals 10 </summary>
            public const int DISCOUNT_SEMESTER = 10;

            /// <summary> Equals 15 </summary>
            public const int DISCOUNT_YEAR = 15;

            /// <summary> Equals 29 </summary>
            public const int DOCTOR_PRICE = (int)(PRICE_MONTH / 2);
        }
    }
}
