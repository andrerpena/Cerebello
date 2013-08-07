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
            { "FreePlan", "FreePlan" },
            { "UnlimitedPlan", "UnlimitedPlan" },
        };

        public static readonly Dictionary<string, string> CorrectContractCase = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "TrialContract", "TrialContract" },
            { "ProfessionalContract", "ProfessionalContract" },
            { "FreeContract", "FreeContract" },
            { "UnlimitedContract", "UnlimitedContract" },
        };

        public static readonly Dictionary<string, string> ContractToPlan = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "TrialContract", "TrialPlan" },
            { "ProfessionalContract", "ProfessionalPlan" },
            { "FreeContract", "FreePlan" },
            { "UnlimitedContract", "UnlimitedPlan" },
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

        public class ProData
        {
            /// <summary> Gets or sets the price per month for one doctor. </summary>
            public decimal PRICE_MONTH { get; set; }

            /// <summary> Gets or sets the total price per quarter for one doctor. </summary>
            public decimal PRICE_QUARTER { get; set; }

            /// <summary> Gets or sets the total price per semester for one doctor. </summary>
            public decimal PRICE_SEMESTER { get; set; }

            /// <summary> Gets or sets the total price per year for one doctor. </summary>
            public decimal PRICE_YEAR { get; set; }

            /// <summary> Gets or sets the quarter discount. </summary>
            public int DISCOUNT_QUARTER { get; set; }

            /// <summary> Gets or sets the semester discount. </summary>
            public int DISCOUNT_SEMESTER { get; set; }

            /// <summary> Gets or sets the year discount. </summary>
            public int DISCOUNT_YEAR { get; set; }

            /// <summary> Gets or sets the price per each additional doctor, excluding the first one. </summary>
            public int DOCTOR_PRICE { get; set; }

            /// <summary> Gets or sets the maximum number of days to pay the billing. </summary>
            public int MAX_DAYS_TO_PAY_BILLING { get; set; }

            /// <summary> Gets or sets the price of unlimited account. </summary>
            public decimal UNLIMITED_PRICE { get; set; }
        }

        /// <summary>
        /// Professional plan constants.
        /// </summary>
        public class ProPtBr
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

            /// <summary> Equals 30 </summary>
            public const int MAX_DAYS_TO_PAY_BILLING = 30;

            /// <summary> Equals 9990.00m </summary>
            public const decimal UNLIMITED_PRICE = 9990.00m;

            public readonly static ProData Data = new ProData
                {
                    PRICE_MONTH = PRICE_MONTH,
                    PRICE_QUARTER = PRICE_QUARTER,
                    PRICE_SEMESTER = PRICE_SEMESTER,
                    PRICE_YEAR = PRICE_YEAR,
                    DISCOUNT_QUARTER = DISCOUNT_QUARTER,
                    DISCOUNT_SEMESTER = DISCOUNT_SEMESTER,
                    DISCOUNT_YEAR = DISCOUNT_YEAR,
                    DOCTOR_PRICE = DOCTOR_PRICE,
                    MAX_DAYS_TO_PAY_BILLING = MAX_DAYS_TO_PAY_BILLING,
                    UNLIMITED_PRICE = UNLIMITED_PRICE,
                };
        }

        /// <summary>
        /// Professional plan constants.
        /// </summary>
        public static class ProEnUs
        {
            /// <summary> Equals 20.00m </summary>
            public const decimal PRICE_MONTH = 20.00m;

            /// <summary> Equals 57.00m </summary>
            public const decimal PRICE_QUARTER = (int)(PRICE_MONTH * (100 - DISCOUNT_QUARTER) / 10) / 10m * 3;

            /// <summary> Equals 108.00m </summary>
            public const decimal PRICE_SEMESTER = (int)(PRICE_MONTH * (100 - DISCOUNT_SEMESTER) / 10) / 10m * 6;

            /// <summary> Equals 204.00m </summary>
            public const decimal PRICE_YEAR = (int)(PRICE_MONTH * (100 - DISCOUNT_YEAR) / 10) / 10m * 12;

            /// <summary> Equals 5 </summary>
            public const int DISCOUNT_QUARTER = 5;

            /// <summary> Equals 10 </summary>
            public const int DISCOUNT_SEMESTER = 10;

            /// <summary> Equals 15 </summary>
            public const int DISCOUNT_YEAR = 15;

            /// <summary> Equals 10 </summary>
            public const int DOCTOR_PRICE = (int)(PRICE_MONTH / 2);

            /// <summary> Equals 30 </summary>
            public const int MAX_DAYS_TO_PAY_BILLING = 30;

            /// <summary> Equals 5000.00m </summary>
            public const decimal UNLIMITED_PRICE = 5000.00m;

            public readonly static ProData Data = new ProData
                {
                    PRICE_MONTH = PRICE_MONTH,
                    PRICE_QUARTER = PRICE_QUARTER,
                    PRICE_SEMESTER = PRICE_SEMESTER,
                    PRICE_YEAR = PRICE_YEAR,
                    DISCOUNT_QUARTER = DISCOUNT_QUARTER,
                    DISCOUNT_SEMESTER = DISCOUNT_SEMESTER,
                    DISCOUNT_YEAR = DISCOUNT_YEAR,
                    DOCTOR_PRICE = DOCTOR_PRICE,
                    MAX_DAYS_TO_PAY_BILLING = MAX_DAYS_TO_PAY_BILLING,
                    UNLIMITED_PRICE = UNLIMITED_PRICE,
                };
        }

        public static ProData Pro = ProEnUs.Data;
    }
}
